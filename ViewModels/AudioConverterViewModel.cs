using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using SuperMediaSuites.Helpers;
using SuperMediaSuites.Models;
using SuperMediaSuites.Services;
using Xabe.FFmpeg;

namespace SuperMediaSuites.ViewModels;

/// <summary>Tab 2 — Audio Converter (reuses the bundled ffmpeg via Xabe.FFmpeg).</summary>
public partial class AudioConverterViewModel : ConverterViewModelBase
{
    protected override string ToolName => "Audio";
    protected override IReadOnlyCollection<string> SupportedExtensions { get; } = new HashSet<string>
    {
        "mp3", "wav", "flac", "aac", "m4a", "ogg", "wma"
    };

    private static readonly string[] OutputFormatsList = { "mp3", "wav", "flac", "aac", "ogg" };
    private static readonly int[] Bitrates = { 96, 128, 192, 256, 320 };

    [ObservableProperty] private string _targetFormat = "mp3";
    [ObservableProperty] private int _bitrate = 192;
    [ObservableProperty] private bool _trimEnabled;
    [ObservableProperty] private string _trimStart = "00:00:00";
    [ObservableProperty] private string _trimEnd = "";

    public IList<string> OutputFormats => OutputFormatsList;
    public IList<int> BitrateOptions => Bitrates;

    protected override async Task ConvertItemAsync(ConversionItem item, CancellationToken cancellationToken)
    {
        if (!FFmpegLocator.IsAvailable())
        {
            throw new FileNotFoundException(
                "ffmpeg.exe was not found. Place it in the app's ffmpeg\\ folder (see README).");
        }

        FFmpegLocator.Locate();

        var info = await FFmpeg.GetMediaInfo(item.SourcePath, cancellationToken);
        item.OutputPath = ResolveOutput(item, TargetFormat);

        var audio = info.AudioStreams.FirstOrDefault()
            ?? throw new System.IO.InvalidDataException("No audio stream found.");

        var conversion = FFmpeg.Conversions.New()
            .AddStream(audio)
            .SetOutput(item.OutputPath)
            .SetOverwriteOutput(true)
            .UseMultiThread(true);

        conversion = conversion.AddParameter($"-b:a {Math.Clamp(Bitrate, 32, 320)}k", ParameterPosition.PostInput);
        if (TrimEnabled)
        {
            if (TryParseTimestamp(TrimStart, out _))
                conversion = conversion.AddParameter($"-ss {TrimStart.Trim()}", ParameterPosition.PostInput);
            if (TryParseTimestamp(TrimEnd, out _))
                conversion = conversion.AddParameter($"-to {TrimEnd.Trim()}", ParameterPosition.PostInput);
        }

        conversion.OnProgress += (_, args) =>
        {
            var pct = Math.Clamp(args.Percent, 0, 100);
            Ui.Run(() => item.Progress = pct);
        };

        await conversion.Start(cancellationToken);
    }

    private static bool TryParseTimestamp(string value, out string normalized)
    {
        normalized = value ?? string.Empty;
        if (string.IsNullOrWhiteSpace(value)) return false;
        // Accept HH:MM:SS or MM:SS or seconds.
        var parts = value.Split(':');
        foreach (var part in parts)
        {
            if (!double.TryParse(part.Trim(), out _)) return false;
        }
        return parts.Length is 2 or 3;
    }
}
