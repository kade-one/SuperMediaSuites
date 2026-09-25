using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SuperMediaSuites.Helpers;
using SuperMediaSuites.Models;
using SuperMediaSuites.Services;
using Xabe.FFmpeg;

namespace SuperMediaSuites.ViewModels;

/// <summary>Tab 5 — CD Ripper. Rips every track, optionally naming files from a MusicBrainz lookup.</summary>
public partial class CdRipperViewModel : ObservableObject
{
    private static readonly string[] OutputFormatsList = { "mp3", "flac" };

    private readonly AppSettings _settings = ServiceLocator.Settings;
    public StatusService Status { get; } = new StatusService();

    public ObservableCollection<CdDrive> Drives { get; } = new();
    public ObservableCollection<CdTrack> Tracks { get; } = new();

    [ObservableProperty] private CdDrive? _selectedDrive;
    [ObservableProperty] private string _outputFolder = string.Empty;
    [ObservableProperty] private string _targetFormat = "mp3";
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _footerNote = "All processing is done locally on your device. (The optional metadata lookup is the only network call.)";

    private int _leadOutSector;
    private CdMetadata? _metadata;
    private CancellationTokenSource? _cts;

    public IList<string> OutputFormats => OutputFormatsList;
    public bool HasTracks => Tracks.Count > 0;
    public bool CanRip => !IsBusy && SelectedDrive != null && Tracks.Count > 0;

    public CdRipperViewModel()
    {
        OutputFolder = _settings.DefaultOutputFolder ?? string.Empty;
        Tracks.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(HasTracks));
            OnPropertyChanged(nameof(CanRip));
        };
    }

    partial void OnSelectedDriveChanged(CdDrive? value) => OnPropertyChanged(nameof(CanRip));
    partial void OnIsBusyChanged(bool value) => OnPropertyChanged(nameof(CanRip));

    [RelayCommand]
    private void RefreshDrives()
    {
        Drives.Clear();
        foreach (var drive in DriveInfo.GetDrives().Where(d => d.DriveType == DriveType.CDRom && d.IsReady))
        {
            Drives.Add(new CdDrive(drive.Name[0], string.IsNullOrWhiteSpace(drive.VolumeLabel) ? "Audio CD" : drive.VolumeLabel));
        }
        if (SelectedDrive == null && Drives.Count > 0) SelectedDrive = Drives[0];
    }

    [RelayCommand]
    private async Task LoadTracksAsync()
    {
        if (SelectedDrive == null || IsBusy)
        {
            Status.StatusMessage = "Select a drive with an audio CD.";
            return;
        }

        Tracks.Clear();
        _metadata = null;
        Status.StatusMessage = "Reading CD…";
        try
        {
            var tracks = await Task.Run(() => CdRipper.ReadToc(SelectedDrive.Letter, out _leadOutSector));
            foreach (var t in tracks) Tracks.Add(t);
            Status.StatusMessage = $"Found {Tracks.Count} track(s).";
        }
        catch (Exception ex)
        {
            Status.StatusMessage = "No audio CD detected in the selected drive.";
            global::Serilog.Log.Warning(ex, "CD TOC read failed for {Drive}", SelectedDrive.Letter);
        }
    }

    [RelayCommand]
    private async Task ChooseOutputFolderAsync()
    {
        var hwnd = Ui.MainWindowHandle;
        if (hwnd == IntPtr.Zero) return;
        var folder = await FileDialogs.PickFolderAsync(hwnd);
        if (folder != null) OutputFolder = folder.Path;
    }

    [RelayCommand]
    private async Task RipAsync()
    {
        if (IsBusy || SelectedDrive == null || Tracks.Count == 0) return;

        var drive = SelectedDrive;
        Directory.CreateDirectory(string.IsNullOrWhiteSpace(OutputFolder) ? Path.GetTempPath() : OutputFolder);

        // Optional metadata fetch (the only network call in the app).
        if (_metadata == null)
        {
            Status.StatusMessage = "Looking up CD metadata…";
            try
            {
                var offsets = Tracks.Select(t => t.StartSector).ToArray();
                var discId = MusicBrainzClient.ComputeDiscId(Tracks[0].Number, Tracks[^1].Number, _leadOutSector, offsets);
                using var mb = new MusicBrainzClient();
                _metadata = await mb.LookupAsync(discId, CancellationToken.None);
                if (_metadata != null)
                {
                    foreach (var track in Tracks)
                    {
                        if (_metadata.TrackTitles.TryGetValue(track.Number, out var title)) track.Title = title;
                    }
                }
            }
            catch (Exception ex)
            {
                global::Serilog.Log.Warning(ex, "MusicBrainz lookup failed; falling back to generic names.");
            }
        }

        var active = Tracks.ToList();
        _cts = new CancellationTokenSource();
        IsBusy = true;
        Status.Reset();
        Status.IsBusy = true;
        Status.Report(0, active.Count);

        var processor = new BatchProcessor<CdTrack> { MaxDegreeOfParallelism = 1 }; // one optical drive → serial
        processor.ItemCompleted += (_, e) => Ui.Run(() =>
        {
            e.Item.Status = CdTrackStatus.Completed;
            e.Item.Progress = 100;
        });
        processor.ItemFailed += (_, e) => Ui.Run(() =>
        {
            e.Item.Status = CdTrackStatus.Failed;
            e.Item.Message = e.Error?.Message ?? "Rip error";
        });

        try
        {
            await processor.RunAsync(
                active,
                (track, ct) => RipOneAsync(drive.Letter, track, ct),
                (done, total) => Ui.Run(() => Status.Report(done, total)),
                _cts.Token);

            Status.StatusMessage = "Rip complete.";
            if (_settings.OpenOutputFolderAfter && !string.IsNullOrEmpty(active[0].OutputPath))
            {
                await OpenOutputFolderAsync();
            }
        }
        catch (OperationCanceledException)
        {
            Status.StatusMessage = "Canceled.";
        }
        catch (Exception ex)
        {
            Status.StatusMessage = "Finished with errors.";
            global::Serilog.Log.Error(ex, "CD rip finished with errors");
        }
        finally
        {
            Status.IsBusy = false;
            IsBusy = false;
        }
    }

    private async Task RipOneAsync(char driveLetter, CdTrack track, CancellationToken ct)
    {
        Ui.Run(() => track.Status = CdTrackStatus.Processing);

        var wavTemp = Path.Combine(Path.GetTempPath(), $"cs_track_{track.Number}_{Guid.NewGuid():N}.wav");
        try
        {
            await CdRipper.RipTrackToWavAsync(driveLetter, track, wavTemp,
                pct => Ui.Run(() => track.Progress = pct), ct);

            track.OutputPath = BuildOutputPath(track);
            await EncodeAsync(wavTemp, track.OutputPath, track, ct);
        }
        finally
        {
            try { File.Delete(wavTemp); } catch { /* temp cleanup best-effort */ }
        }
    }

    private async Task EncodeAsync(string wavPath, string outPath, CdTrack track, CancellationToken ct)
    {
        if (!FFmpegLocator.IsAvailable())
            throw new FileNotFoundException("ffmpeg.exe was not found (see README).");

        FFmpegLocator.Locate();

        var info = await FFmpeg.GetMediaInfo(wavPath, ct);
        var audio = info.AudioStreams.FirstOrDefault()
            ?? throw new InvalidDataException("No audio stream in ripped track.");

        var conversion = FFmpeg.Conversions.New()
            .AddStream(audio)
            .SetOutput(outPath)
            .SetOverwriteOutput(true)
            .UseMultiThread(true);

        if (TargetFormat == "mp3")
        {
            conversion = conversion.AddParameter("-b:a 320k", ParameterPosition.PostInput);
        }

        if (_metadata != null)
        {
            var title = _metadata.TrackTitles.TryGetValue(track.Number, out var t) ? t : $"Track {track.Number:D2}";
            conversion = conversion.AddParameter($"-metadata title=\"{Escape(title)}\"", ParameterPosition.PostInput);
            conversion = conversion.AddParameter($"-metadata artist=\"{Escape(_metadata.Artist)}\"", ParameterPosition.PostInput);
            conversion = conversion.AddParameter($"-metadata album=\"{Escape(_metadata.Album)}\"", ParameterPosition.PostInput);
            conversion = conversion.AddParameter($"-metadata track={track.Number}/{Tracks.Count}", ParameterPosition.PostInput);
        }

        conversion.OnProgress += (_, args) =>
        {
            // Encoding is the last 10% after the 0–90% rip.
            var pct = 90 + (int)Math.Round(args.Percent * 0.1);
            Ui.Run(() => track.Progress = Math.Clamp(pct, 90, 100));
        };

        await conversion.Start(ct);
    }

    private string BuildOutputPath(CdTrack track)
    {
        var root = string.IsNullOrWhiteSpace(OutputFolder) ? _settings.DefaultOutputFolder : OutputFolder;
        Directory.CreateDirectory(root!);

        string dir;
        string file;
        if (_metadata != null)
        {
            dir = Path.Combine(root!, $"{Sanitize(_metadata.Artist)} - {Sanitize(_metadata.Album)}");
            Directory.CreateDirectory(dir);
            file = $"{track.Number:D2}. {Sanitize(track.Title)}.{TargetFormat}";
        }
        else
        {
            dir = root!;
            file = $"{track.Title}.{TargetFormat}";
        }
        return Path.Combine(dir, file);
    }

    private static string Sanitize(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var clean = string.Join("_", value.Split(invalid)).Trim();
        return string.IsNullOrWhiteSpace(clean) ? "Unknown" : clean;
    }

    private static string Escape(string value) => value.Replace("\"", "\\\"");

    [RelayCommand]
    private void Cancel() => _cts?.Cancel();

    [RelayCommand]
    private async Task OpenOutputFolderAsync()
    {
        var first = Tracks.FirstOrDefault(t => !string.IsNullOrEmpty(t.OutputPath));
        if (first?.OutputPath == null) return;
        try
        {
            var folder = Path.GetDirectoryName(first.OutputPath);
            if (!string.IsNullOrEmpty(folder))
            {
                var folderItem = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(folder);
                await Windows.System.Launcher.LaunchFolderAsync(folderItem);
            }
        }
        catch { }
    }
}
