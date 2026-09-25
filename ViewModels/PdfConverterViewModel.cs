using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageMagick;
using PDFtoImage;
using PdfSharpCore;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using SkiaSharp;
using SuperMediaSuites.Helpers;
using SuperMediaSuites.Models;
using SuperMediaSuites.Services;

namespace SuperMediaSuites.ViewModels;

public enum PdfMode
{
    PdfToImages,
    ImagesToPdf,
    MergePdfs,
    SplitPdf
}

/// <summary>
/// Tab 6 — PDF Converter. Mode A rasterizes PDFs to images (PDFtoImage); Mode B assembles images
/// into one PDF; Merge and Split are bonus tools (PdfSharpCore). No network.
/// </summary>
public partial class PdfConverterViewModel : ConverterViewModelBase
{
    protected override string ToolName => "PDF";

    private static readonly string[] PdfInputs = { "pdf" };
    private static readonly string[] ImageInputs = { "jpg", "jpeg", "png", "webp", "bmp", "tiff", "tif", "heic" };
    private static readonly string[] OutputImageFormats = { "jpg", "png", "tiff", "gif", "webp" };
    private static readonly int[] DpiOptions = { 96, 150, 300, 600 };

    private IReadOnlyCollection<string> _currentExtensions = PdfInputs;

    protected override IReadOnlyCollection<string> SupportedExtensions => _currentExtensions;

    [ObservableProperty] private PdfMode _mode = PdfMode.PdfToImages;
    [ObservableProperty] private string _imageFormat = "png";          // Mode A output
    [ObservableProperty] private int _dpi = 96;                         // Mode A: 96/150/300/600
    [ObservableProperty] private string _pageRange = "1-";              // Mode A / Split
    [ObservableProperty] private string _pageSize = "Fit";              // Mode B (Fit/A4/Letter)
    [ObservableProperty] private string _orientation = "Auto";          // Mode B (Auto/Portrait/Landscape)
    [ObservableProperty] private string _password = "";                 // Mode B
    [ObservableProperty] private bool _compress;                        // Mode B

    public IList<string> ImageFormats => OutputImageFormats;
    public IList<int> AvailableDpis => DpiOptions;
    public IList<string> PageSizes { get; } = new[] { "Fit", "A4", "Letter" };
    public IList<string> Orientations { get; } = new[] { "Auto", "Portrait", "Landscape" };
    public IList<string> ModeNames { get; } = new[] { "PDF → Images", "Images → PDF", "Merge PDFs", "Split PDF" };

    public bool IsModeA => Mode == PdfMode.PdfToImages;
    public bool IsModeB => Mode == PdfMode.ImagesToPdf;
    public bool IsMerge => Mode == PdfMode.MergePdfs;
    public bool IsSplit => Mode == PdfMode.SplitPdf;

    /// <summary>Index mirror of <see cref="Mode"/> for RadioButtons.SelectedIndex binding.</summary>
    public int ModeIndex
    {
        get => (int)Mode;
        set => Mode = (PdfMode)value;
    }

    partial void OnModeChanged(PdfMode value)
    {
        _currentExtensions = value switch
        {
            PdfMode.ImagesToPdf => ImageInputs,
            _ => PdfInputs
        };
        Items.Clear();
        OnPropertyChanged(nameof(IsModeA));
        OnPropertyChanged(nameof(IsModeB));
        OnPropertyChanged(nameof(IsMerge));
        OnPropertyChanged(nameof(IsSplit));
        OnPropertyChanged(nameof(ModeIndex));
    }

    // ---- Mode A: PDF → Images (per-item, runs on the shared BatchProcessor) ---------------------------

    protected override async Task ConvertItemAsync(ConversionItem item, CancellationToken cancellationToken)
    {
        if (Mode != PdfMode.PdfToImages)
        {
            throw new InvalidOperationException("Use the dedicated command for this PDF mode.");
        }

        var options = new RenderOptions { Dpi = Math.Clamp(Dpi, 72, 1200) };

        var pageCount = Conversion.GetPageCount(item.SourcePath, "");
        var pages = ParsePageRange(PageRange, pageCount);

        if (pages.Count == 0) pages = Enumerable.Range(1, pageCount).ToList();

        var outDir = UseCustomOutputFolder
            ? OutputFolder!
            : (Path.GetDirectoryName(item.SourcePath) ?? item.SourcePath);
        Directory.CreateDirectory(outDir);

        var baseName = Path.GetFileNameWithoutExtension(item.SourcePath);
        var format = ImageFormat;
        const int quality = 90;

        var written = 0;
        foreach (var page in pages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var bitmap = Conversion.ToImage(item.SourcePath, (Index)(page - 1), "", options);
            var pagePath = Path.Combine(outDir, $"{baseName}_p{page:000}.{format}");
            EncodePage(bitmap, pagePath, format, quality);

            if (written == 0) item.OutputPath = pagePath;
            written++;
            Ui.Run(() => item.Progress = (int)Math.Round((double)written / pages.Count * 100));
        }

        await Task.CompletedTask;
    }

    private static void EncodePage(SKBitmap bitmap, string path, string format, int quality)
    {
        var fmt = format.ToLowerInvariant();
        if (fmt == "tiff" || fmt == "tif")
        {
            byte[] pngBytes;
            using (var image = SKImage.FromBitmap(bitmap))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            {
                using var ms = new MemoryStream();
                data.SaveTo(ms);
                pngBytes = ms.ToArray();
            }
            using var magick = new MagickImage(pngBytes) { Format = MagickFormat.Tiff };
            magick.Write(path);
            return;
        }

        var skFormat = fmt switch
        {
            "jpg" or "jpeg" => SKEncodedImageFormat.Jpeg,
            "webp" => SKEncodedImageFormat.Webp,
            "gif" => SKEncodedImageFormat.Gif,
            _ => SKEncodedImageFormat.Png
        };

        using (var img = SKImage.FromBitmap(bitmap))
        using (var data = img.Encode(skFormat, quality))
        using (var fs = File.OpenWrite(path))
        {
            data.SaveTo(fs);
        }
    }

    // ---- Mode B: Images → PDF (single output) ---------------------------------------------------------

    [RelayCommand]
    private async Task ImagesToPdfAsync()
    {
        if (IsBusy || Mode != PdfMode.ImagesToPdf) return;
        var active = Items.ToList();
        if (active.Count == 0) return;

        var outPath = ResolveBatchOutput("combined", "pdf");
        Directory.CreateDirectory(Path.GetDirectoryName(outPath)!);

        await RunSingleAsync(active, async ct =>
        {
            var doc = new PdfDocument();
            var done = 0;
            foreach (var item in active)
            {
                ct.ThrowIfCancellationRequested();
                var prepared = await Task.Run(() => PrepareImageForPdf(item.SourcePath), ct);
                try
                {
                    var ximg = XImage.FromFile(prepared);
                    try
                    {
                        var page = doc.AddPage();

                        if (PageSize == "Fit")
                        {
                            page.Width = ximg.PointWidth;
                            page.Height = ximg.PointHeight;
                        }
                        else
                        {
                            page.Size = PageSize == "Letter" ? PdfSharpCore.PageSize.Letter : PdfSharpCore.PageSize.A4;
                            page.Orientation = ResolveOrientation(Orientation, ximg.PointWidth > ximg.PointHeight);
                        }

                        using (var gfx = XGraphics.FromPdfPage(page))
                        {
                            gfx.DrawImage(ximg, 0, 0, page.Width, page.Height);
                        }
                    }
                    finally
                    {
                        ximg.Dispose();
                    }
                }
                finally
                {
                    TryDelete(prepared);
                }

                done++;
                var captured = done;
                Ui.Run(() =>
                {
                    item.Progress = (int)Math.Round((double)captured / active.Count * 100);
                    Status.Report(captured, active.Count);
                });
            }

            if (!string.IsNullOrWhiteSpace(Password))
            {
                doc.SecuritySettings.UserPassword = Password;
            }

            doc.Save(outPath);
            Ui.Run(() =>
            {
                foreach (var item in active)
                {
                    item.OutputPath = outPath;
                    item.Status = ConversionStatus.Completed;
                    item.Progress = 100;
                }
            });
        }, "PDF created.");
    }

    private string PrepareImageForPdf(string source)
    {
        var temp = Path.Combine(Path.GetTempPath(), $"cs_pdf_{Guid.NewGuid():N}.png");
        using var image = new MagickImage(source);
        image.AutoOrient();
        if (Compress)
        {
            temp = Path.ChangeExtension(temp, ".jpg");
            image.BackgroundColor = MagickColors.White;
            image.Alpha(AlphaOption.Remove);
            image.Format = MagickFormat.Jpeg;
            image.Quality = 75;
        }
        image.Write(temp);
        return temp;
    }

    // ---- Merge PDFs -----------------------------------------------------------------------------------

    [RelayCommand]
    private async Task MergePdfsAsync()
    {
        if (IsBusy || Mode != PdfMode.MergePdfs) return;
        var active = Items.ToList();
        if (active.Count < 2)
        {
            Status.StatusMessage = "Add at least two PDFs to merge.";
            return;
        }

        var outPath = ResolveBatchOutput("merged", "pdf");
        await RunSingleAsync(active, async ct =>
        {
            var output = new PdfDocument();
            var done = 0;
            foreach (var item in active)
            {
                ct.ThrowIfCancellationRequested();
                var input = PdfReader.Open(item.SourcePath, PdfDocumentOpenMode.Import);
                foreach (var page in input.Pages)
                {
                    output.AddPage(page);
                }
                done++;
                var captured = done;
                Ui.Run(() => Status.Report(captured, active.Count));
            }
            output.Save(outPath);
            Ui.Run(() =>
            {
                foreach (var item in active)
                {
                    item.OutputPath = outPath;
                    item.Status = ConversionStatus.Completed;
                }
            });
            await Task.CompletedTask;
        }, "PDFs merged.");
    }

    // ---- Split PDF ------------------------------------------------------------------------------------

    [RelayCommand]
    private async Task SplitPdfAsync()
    {
        if (IsBusy || Mode != PdfMode.SplitPdf) return;
        var source = Items.FirstOrDefault();
        if (source == null)
        {
            Status.StatusMessage = "Add a PDF to split.";
            return;
        }

        var outPath = ResolveBatchOutput(Path.GetFileNameWithoutExtension(source.SourcePath) + "_split", "pdf");
        Status.Reset();
        Status.IsBusy = true;
        IsBusy = true;
        try
        {
            await Task.Run(() =>
            {
                using var input = PdfReader.Open(source.SourcePath, PdfDocumentOpenMode.Import);
                var pages = ParsePageRange(PageRange, input.Pages.Count);
                var output = new PdfDocument();
                foreach (var page in pages)
                {
                    output.AddPage(input.Pages[page - 1]);
                }
                output.Save(outPath);
            });

            source.OutputPath = outPath;
            source.Status = ConversionStatus.Completed;
            Status.Progress = 100;
            Status.StatusMessage = "PDF split.";
            if (Settings.OpenOutputFolderAfter) await OpenOutputFolderAsync();
        }
        catch (Exception ex)
        {
            source.Status = ConversionStatus.Failed;
            source.Message = ex.Message;
            Status.StatusMessage = "Split failed.";
            global::Serilog.Log.Error(ex, "PDF split failed");
        }
        finally
        {
            Status.IsBusy = false;
            IsBusy = false;
        }
    }

    // ---- Shared helpers -------------------------------------------------------------------------------

    private async Task RunSingleAsync(List<ConversionItem> active, Func<CancellationToken, Task> work, string doneMessage)
    {
        var cts = new CancellationTokenSource();
        Status.Reset();
        Status.IsBusy = true;
        IsBusy = true;
        Status.Report(0, active.Count);
        try
        {
            await Task.Run(() => work(cts.Token), cts.Token);
            Status.StatusMessage = doneMessage;
            if (Settings.OpenOutputFolderAfter && !string.IsNullOrEmpty(active[0].OutputPath))
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
            global::Serilog.Log.Error(ex, "{Tool}: single-batch finished with errors", ToolName);
        }
        finally
        {
            Status.IsBusy = false;
            IsBusy = false;
        }
    }

    private string ResolveBatchOutput(string baseName, string ext)
    {
        var dir = UseCustomOutputFolder ? OutputFolder! : Settings.DefaultOutputFolder!;
        Directory.CreateDirectory(dir);
        var candidate = Path.Combine(dir, $"{baseName}.{ext}");
        var index = 1;
        while (File.Exists(candidate))
        {
            candidate = Path.Combine(dir, $"{baseName} ({index}).{ext}");
            index++;
        }
        return candidate;
    }

    private static PageOrientation ResolveOrientation(string preference, bool isLandscapeImage)
    {
        return preference switch
        {
            "Portrait" => PageOrientation.Portrait,
            "Landscape" => PageOrientation.Landscape,
            _ => isLandscapeImage ? PageOrientation.Landscape : PageOrientation.Portrait
        };
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); } catch { /* best-effort temp cleanup */ }
    }

    /// <summary>Parse "1-3, 5, 8-10" into a sorted list of 1-based page numbers within [1, max].</summary>
    internal static List<int> ParsePageRange(string range, int max)
    {
        var result = new List<int>();
        if (string.IsNullOrWhiteSpace(range) || max <= 0) return result;

        foreach (var token in range.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (token.Contains('-'))
            {
                var parts = token.Split('-', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 &&
                    int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var lo) &&
                    int.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var hi))
                {
                    for (var p = lo; p <= hi; p++) if (p >= 1 && p <= max) result.Add(p);
                }
            }
            else if (token.EndsWith("-"))
            {
                if (int.TryParse(token.TrimEnd('-'), NumberStyles.Integer, CultureInfo.InvariantCulture, out var lo))
                {
                    for (var p = lo; p <= max; p++) if (p >= 1) result.Add(p);
                }
            }
            else if (int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out var single) && single >= 1 && single <= max)
            {
                result.Add(single);
            }
        }
        return result.Distinct().OrderBy(p => p).ToList();
    }
}
