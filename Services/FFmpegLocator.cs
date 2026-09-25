using System;
using System.Diagnostics;
using System.IO;
using Xabe.FFmpeg;

namespace SuperMediaSuites.Services;

/// <summary>
/// Locates the bundled <c>ffmpeg.exe</c> and <c>ffprobe.exe</c> (never downloads at runtime).
/// Always checks in the same .exe directory and its subdirectories.
/// Once found, points <c>Xabe.FFmpeg</c> at it via
/// <see cref="Xabe.FFmpeg.FFmpeg.SetExecutablesPath"/>.
/// </summary>
public static class FFmpegLocator
{
    private static string? _cached;
    private static bool _initialized;

    public static string? Locate()
    {
        if (_cached != null) return _cached;

        var dir = FindBinariesFolder();
        if (dir != null)
        {
            _cached = Path.Combine(dir, "ffmpeg.exe");
            EnsureSet();
            return _cached;
        }

        return null;
    }

    public static bool IsAvailable()
    {
        return Locate() != null;
    }

    public static bool IsFfprobeAvailable()
    {
        var ffmpegPath = Locate();
        if (ffmpegPath == null) return false;

        var dir = Path.GetDirectoryName(ffmpegPath);
        if (string.IsNullOrEmpty(dir)) return false;

        return File.Exists(Path.Combine(dir, "ffprobe.exe"));
    }

    private static string? FindBinariesFolder()
    {
        try
        {
            var baseDir = AppContext.BaseDirectory;
            if (string.IsNullOrEmpty(baseDir) || !Directory.Exists(baseDir)) return null;

            // 1. Direct check of AppContext.BaseDirectory/ffmpeg
            var ffmpegSub = Path.Combine(baseDir, "ffmpeg");
            if (Directory.Exists(ffmpegSub) &&
                File.Exists(Path.Combine(ffmpegSub, "ffmpeg.exe")) &&
                File.Exists(Path.Combine(ffmpegSub, "ffprobe.exe")))
            {
                return ffmpegSub;
            }

            // 2. Direct check of AppContext.BaseDirectory
            if (File.Exists(Path.Combine(baseDir, "ffmpeg.exe")) &&
                File.Exists(Path.Combine(baseDir, "ffprobe.exe")))
            {
                return baseDir;
            }

            // 3. Scan all subdirectories recursively
            foreach (var dir in Directory.EnumerateDirectories(baseDir, "*", SearchOption.AllDirectories))
            {
                if (File.Exists(Path.Combine(dir, "ffmpeg.exe")) &&
                    File.Exists(Path.Combine(dir, "ffprobe.exe")))
                {
                    return dir;
                }
            }

            // 4. Per-user fallback: %LOCALAPPDATA%\SuperMediaSuites\bin
            var perUser = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SuperMediaSuites", "bin");
            if (Directory.Exists(perUser) &&
                File.Exists(Path.Combine(perUser, "ffmpeg.exe")) &&
                File.Exists(Path.Combine(perUser, "ffprobe.exe")))
            {
                return perUser;
            }

            // 5. Last resort: the system PATH (where.exe ffmpeg)
            var onPath = FindOnPath();
            if (onPath != null) return onPath;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[FFmpegLocator] Error scanning directories: {ex.Message}");
        }

        return null;
    }

    /// <summary>Probe the system PATH for a complete ffmpeg + ffprobe pair.</summary>
    private static string? FindOnPath()
    {
        try
        {
            var path = Environment.GetEnvironmentVariable("PATH");
            if (string.IsNullOrEmpty(path)) return null;

            foreach (var entry in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                var dir = entry.Trim().Trim('"');
                if (dir.Length == 0) continue;
                try
                {
                    if (File.Exists(Path.Combine(dir, "ffmpeg.exe")) &&
                        File.Exists(Path.Combine(dir, "ffprobe.exe")))
                    {
                        return dir;
                    }
                }
                catch
                {
                    // Unreadable PATH entry — keep looking.
                }
            }
        }
        catch
        {
            // Best-effort lookup only.
        }
        return null;
    }

    private static void EnsureSet()
    {
        if (_initialized || _cached == null) return;
        try
        {
            var dir = Path.GetDirectoryName(_cached);
            if (!string.IsNullOrEmpty(dir))
            {
                FFmpeg.SetExecutablesPath(dir);
                _initialized = true;
            }
        }
        catch
        {
            // If Xabe rejects the path we still return the located binary; callers can invoke it directly.
        }
    }
}
