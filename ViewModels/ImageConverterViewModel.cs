using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using ImageMagick;
using SuperMediaSuites.Helpers;
using SuperMediaSuites.Models;
using SuperMediaSuites.Services;

namespace SuperMediaSuites.ViewModels;

/// <summary>Tab 3 — Image Converter (Magick.NET, Q8/AnyCPU).</summary>
public partial class ImageConverterViewModel : ConverterViewModelBase
{
    protected override string ToolName => "Image";
    protected override IReadOnlyCollection<string> SupportedExtensions { get; } = new HashSet<string>
    {
        "jpg", "jpeg", "png", "webp", "bmp", "gif", "tiff", "tif", "heic", "heif", "avif", "ico"
    };

    private static readonly string[] OutputFormatsList = { "jpg", "png", "webp", "bmp", "gif", "tiff", "ico" };
    private static readonly string[] ResizeModes = { "None", "Scale %", "Width × Height" };

    [ObservableProperty] private string _targetFormat = "png";
    [ObservableProperty] private int _quality = 90;
    [ObservableProperty] private string _resizeMode = "None";
    [ObservableProperty] private int _scalePercent = 100;
    [ObservableProperty] private int _targetWidth = 1920;
    [ObservableProperty] private int _targetHeight = 1080;
    [ObservableProperty] private string _backgroundColor = "#FFFFFF";
    [ObservableProperty] private bool _preserveExif = true;

    public IList<string> OutputFormats => OutputFormatsList;
    public IList<string> AvailableResizeModes => ResizeModes;
    public bool IsResizeScale => ResizeMode == "Scale %";
    public bool IsResizeDimensions => ResizeMode == "Width × Height";

    partial void OnResizeModeChanged(string value)
    {
        OnPropertyChanged(nameof(IsResizeScale));
        OnPropertyChanged(nameof(IsResizeDimensions));
    }

    protected override async Task ConvertItemAsync(ConversionItem item, CancellationToken cancellationToken)
    {
        using var image = new MagickImage(item.SourcePath);
        item.OutputPath = ResolveOutput(item, TargetFormat);

        var format = ImageFormatMap.ToMagick(TargetFormat);
        if (format != default) image.Format = format;

        image.Quality = (uint)Math.Clamp(Quality, 1, 100);

        switch (ResizeMode)
        {
            case "Scale %" when ScalePercent > 0:
                image.Resize(new Percentage(Math.Clamp(ScalePercent, 1, 400)));
                break;
            case "Width × Height" when TargetWidth > 0 && TargetHeight > 0:
                image.Resize((uint)TargetWidth, (uint)TargetHeight);
                break;
        }

        // Flatten transparency onto the chosen background for formats without alpha.
        if (format == MagickFormat.Jpeg || format == MagickFormat.Bmp)
        {
            var bg = ImageFormatMap.ParseColor(BackgroundColor) ?? MagickColors.White;
            image.BackgroundColor = bg;
            image.Alpha(AlphaOption.Remove);
        }

        if (!PreserveExif)
        {
            image.Strip();
        }

        Ui.Run(() => item.Progress = 50);
        await Task.Run(() => image.Write(item.OutputPath), cancellationToken);
        Ui.Run(() => item.Progress = 99);
    }
}
