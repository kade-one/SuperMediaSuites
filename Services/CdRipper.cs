using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NAudio.Wave;
using SuperMediaSuites.Models;

namespace SuperMediaSuites.Services;

/// <summary>
/// Rips audio CDs. Track table-of-contents is read with the WinMM MCI cdaudio device and the
/// raw PCM is extracted sector-by-sector with <c>DeviceIoControl(IOCTL_CDROM_RAW_READ)</c>;
/// each track is written to WAV with NAudio and later encoded by ffmpeg (see CdRipperViewModel).
/// </summary>
public static class CdRipper
{
    private const uint GenericRead = 0x80000000;
    private const uint FileShareRead = 0x00000001;
    private const uint FileShareWrite = 0x00000002;
    private const uint OpenExisting = 3;
    private static readonly IntPtr InvalidHandleValue = new(-1);

    private const uint IoctlCdromReadToc = 0x00024000;
    private const uint IoctlCdromRawRead = 0x0002403E;

    private const int RawSectorSize = 2352;       // one Red Book audio sector
    private const int SectorsPerSecond = 75;
    private const int RawReadChunk = 20;          // sectors per DeviceIoControl call

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern IntPtr CreateFileW(string lpFileName, uint dwDesiredAccess, uint dwShareMode,
        IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeviceIoControl(IntPtr hDevice, uint dwIoControlCode,
        IntPtr lpInBuffer, uint nInBufferSize, IntPtr lpOutBuffer, uint nOutBufferSize,
        out uint lpBytesReturned, IntPtr lpOverlapped);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool CloseHandle(IntPtr hObject);

    [StructLayout(LayoutKind.Sequential)]
    private struct RawReadInfo
    {
        public uint DiskOffset;   // byte offset = LBA * 2048
        public uint SectorCount;
        public uint TrackMode;    // 2 = CDDA
    }

    /// <summary>Open the CD device for the given drive letter and read its table of contents.</summary>
    public static List<CdTrack> ReadToc(char driveLetter, out int leadOutSector)
    {
        leadOutSector = 0;
        var handle = OpenDrive(driveLetter);
        if (handle == InvalidHandleValue || handle == IntPtr.Zero)
        {
            throw new Win32Exception(Marshal.GetLastWin32Error(), $"Could not open CD-ROM drive {driveLetter}:.");
        }

        try
        {
            var tocBuffer = new byte[804]; // 4-byte header + 100 * 8-byte TRACK_DATA
            uint returned;
            var pinned = GCHandle.Alloc(tocBuffer, GCHandleType.Pinned);
            try
            {
                if (!DeviceIoControl(handle, IoctlCdromReadToc, IntPtr.Zero, 0,
                    pinned.AddrOfPinnedObject(), (uint)tocBuffer.Length, out returned, IntPtr.Zero))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "READ_TOC failed; the drive may contain no audio CD.");
                }
            }
            finally
            {
                pinned.Free();
            }

            var first = tocBuffer[2];
            var last = tocBuffer[3];
            var tracks = new List<CdTrack>();

            var trackOffsets = new int[100];
            var frameDurations = new int[100];

            for (var t = first; t <= last; t++)
            {
                trackOffsets[t] = ReadLba(tocBuffer, t);
            }
            leadOutSector = ReadLba(tocBuffer, 0xAA); // lead-out entry

            for (var t = first; t <= last; t++)
            {
                var start = trackOffsets[t];
                var end = t < last ? trackOffsets[t + 1] : leadOutSector;
                var frames = end - start;
                var seconds = frames / SectorsPerSecond;
                tracks.Add(new CdTrack(t, start, end, $"{seconds / 60:D1}:{seconds % 60:D2}", $"Track {t:D2}"));
            }

            return tracks;
        }
        finally
        {
            CloseHandle(handle);
        }
    }

    /// <summary>Extract one track's PCM to a WAV file, reporting per-sector progress.</summary>
    public static async Task RipTrackToWavAsync(char driveLetter, CdTrack track, string wavPath,
        Action<int> progress, CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            var handle = OpenDrive(driveLetter);
            if (handle == InvalidHandleValue || handle == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), $"Could not open CD-ROM drive {driveLetter}:.");
            }

            var totalSectors = track.SectorCount;
            var info = new RawReadInfo { TrackMode = 2 }; // CDDA
            var infoSize = Marshal.SizeOf<RawReadInfo>();
            var outBuffer = Marshal.AllocHGlobal(RawReadChunk * RawSectorSize);
            var inBuffer = Marshal.AllocHGlobal(infoSize);

            try
            {
                var format = new WaveFormat(44100, 16, 2);
                using var writer = new WaveFileWriter(wavPath, format);

                var remaining = totalSectors;
                var currentSector = track.StartSector;

                while (remaining > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var chunk = Math.Min(RawReadChunk, remaining);
                    info.DiskOffset = (uint)currentSector * 2048u;
                    info.SectorCount = (uint)chunk;

                    Marshal.StructureToPtr(info, inBuffer, false);

                    uint returned;
                    if (!DeviceIoControl(handle, IoctlCdromRawRead, inBuffer, (uint)infoSize,
                        outBuffer, (uint)(chunk * RawSectorSize), out returned, IntPtr.Zero))
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error(),
                            $"RAW_READ failed at sector {currentSector}. Digital audio extraction is not supported by this drive.");
                    }

                    var bytesRead = (int)returned;
                    var managed = new byte[bytesRead];
                    Marshal.Copy(outBuffer, managed, 0, bytesRead);
                    writer.Write(managed, 0, bytesRead);

                    currentSector += chunk;
                    remaining -= chunk;
                    var done = totalSectors - remaining;
                    progress((int)Math.Round((double)done / totalSectors * 90)); // 0–90% reserved for the rip
                }
            }
            finally
            {
                Marshal.FreeHGlobal(outBuffer);
                Marshal.FreeHGlobal(inBuffer);
                CloseHandle(handle);
            }
        }, cancellationToken);
    }

    private static IntPtr OpenDrive(char driveLetter)
    {
        var path = $@"\\.\{driveLetter}:";
        return CreateFileW(path, GenericRead, FileShareRead | FileShareWrite,
            IntPtr.Zero, OpenExisting, 0, IntPtr.Zero);
    }

    /// <summary>Read the LBA start sector for a track number from the raw TOC buffer.</summary>
    private static int ReadLba(byte[] toc, int trackNumber)
    {
        // TRACK_DATA layout in the buffer: reserved, control/adr, trackNumber, reserved, MSF[3]
        for (var i = 0; i < 100; i++)
        {
            var offset = 4 + i * 8;
            if (toc[offset + 2] == trackNumber)
            {
                var m = toc[offset + 5];
                var s = toc[offset + 6];
                var f = toc[offset + 7];
                return (m * 60 + s) * SectorsPerSecond + f;
            }
        }
        return 0;
    }
}
