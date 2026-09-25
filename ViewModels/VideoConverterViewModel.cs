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

/// <summary>Tab 1 — Video Converter (Xabe.FFmpeg wrapping the bundled ffmpeg.exe).</summary>
public partial class VideoConverterViewModel : ConverterViewModelBase
{
    protected override string ToolName => "Video";
    protected override IReadOnlyCollection<string> SupportedExtensions { get; } = new HashSet<string>
    {
        "mp4", "mkv", "mov", "avi", "webm", "wmv", "flv"
    };

    private static readonly string[] Formats =
        { "mp4", "mkv", "webm", "mov", "avi" };
    private static readonly string[] ResolutionPresets =
        { "Original", "4K", "1080p", "720p", "480p", "360p" };

    [ObservableProperty] private string _targetFormat = "mp4";
    [ObservableProperty] private string _resolutionPreset = "Original";
    [ObservableProperty] private int _crf = 23;

    public IList<string> OutputFormats => Formats;
    public IList<string> Resolutions => ResolutionPresets;

    protected override async Task ConvertItemAsync(ConversionItem item, CancellationToken cancellationToken)
    {
        if (!FFmpegLocator.IsAvailable())
        {
            throw new FileNotFoundException(
                "ffmpeg.exe was not found. Place it in the app's ffmpeg\\ folder (see README).");
        }

        FFmpegLocator.Locate(); // ensure Xabe.FFmpeg.SetExecutablesPath has been called

        var info = await FFmpeg.GetMediaInfo(item.SourcePath, cancellationToken);
        item.OutputPath = ResolveOutput(item, TargetFormat);

        var streams = new List<IStream>();
        var video = info.VideoStreams.FirstOrDefault();
        var audio = info.AudioStreams.FirstOrDefault();
        if (video != null) streams.Add(video);
        if (audio != null) streams.Add(audio);

        if (video != null && ResolutionPreset != "Original")
        {
            var (w, h) = ResolutionToSize(ResolutionPreset, video.Width, video.Height);
            // Keep aspect ratio: scale so the longer side matches the target, width/height divisible by 2.
            streams[0] = video.SetSize(w - (w % 2), h - (h % 2));
        }

        var conversion = FFmpeg.Conversions.New()
            .AddStream(streams)
            .SetOutput(item.OutputPath)
            .SetOverwriteOutput(true)
            .UseMultiThread(true);

        // -crf is a quality knob for the H.264/H.265/VP9 encoders; the other containers use
        // different default codecs where the option has no meaning.
        if (TargetFormat is "mp4" or "mkv" or "mov" or "webm")
        {
            conversion = conversion.AddParameter($"-crf {Math.Clamp(Crf, 0, 51)}", ParameterPosition.PostInput);
        }

        conversion.OnProgress += (_, args) =>
        {
            var pct = Math.Clamp(args.Percent, 0, 100);
            Ui.Run(() => item.Progress = pct);
        };

        await conversion.Start(cancellationToken);
    }

    private static (int width, int height) ResolutionToSize(string preset, int srcW, int srcH)
    {
        var longer = Math.Max(srcW, srcH);
        if (longer == 0) return (srcW, srcH);
        int target = preset switch
        {
            "360p" => 640,
            "480p" => 854,
            "720p" => 1280,
            "1080p" => 1920,
            "4K" => 3840,
            _ => longer
        };
        var scale = (double)target / longer;
        var w = (int)Math.Round(srcW * scale);
        var h = (int)Math.Round(srcH * scale);
        return (w == 0 ? 1 : w, h == 0 ? 1 : h);
    }
}
