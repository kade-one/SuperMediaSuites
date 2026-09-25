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

/// <summary>Tab 4 — HEIC Converter (Magick.NET, shared with Tab 3).</summary>
public partial class HeicConverterViewModel : ConverterViewModelBase
{
    protected override string ToolName => "HEIC";
    protected override IReadOnlyCollection<string> SupportedExtensions { get; } = new HashSet<string>
    {
        "heic", "heif"
    };

    private static readonly string[] OutputFormatsList = { "jpg", "png" };

    [ObservableProperty] private string _targetFormat = "jpg";
    [ObservableProperty] private int _quality = 90;
    [ObservableProperty] private bool _preserveExif = true;

    public IList<string> OutputFormats => OutputFormatsList;

    /// <summary>After items are added, generate a small preview thumbnail for each.</summary>
    public override async Task AddPathsAsync(IEnumerable<string> paths)
    {
        await base.AddPathsAsync(paths);

        foreach (var item in Items.Where(i => i.Preview == null).ToList())
        {
            var bytes = await Task.Run(() => Thumbnail.FromFile(item.SourcePath, 96));
            if (bytes != null)
            {
                var bitmap = await Thumbnail.ToBitmapAsync(bytes);
                Ui.Run(() => item.Preview = bitmap);
            }
        }
    }

    protected override async Task ConvertItemAsync(ConversionItem item, CancellationToken cancellationToken)
    {
        using var image = new MagickImage(item.SourcePath);
        item.OutputPath = ResolveOutput(item, TargetFormat);

        var format = ImageFormatMap.ToMagick(TargetFormat);
        if (format != default) image.Format = format;

        image.Quality = (uint)Math.Clamp(Quality, 1, 100);

        if (format == MagickFormat.Jpeg)
        {
            image.BackgroundColor = MagickColors.White;
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
