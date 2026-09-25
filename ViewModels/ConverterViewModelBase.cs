using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using SuperMediaSuites.Helpers;
using SuperMediaSuites.Models;
using SuperMediaSuites.Services;
using Windows.Storage;

namespace SuperMediaSuites.ViewModels;

/// <summary>
/// Shared logic for the file→file converter tabs (Video / Audio / Image / HEIC, and the
/// image-side of PDF). Implements: drag/drop + picker file intake, output-path resolution,
/// running conversions through <see cref="BatchProcessor{TWork}"/> with aggregate progress,
/// cancellation, and marshalling UI updates back to the dispatcher. Subclasses only implement
/// <see cref="ConvertItemAsync"/> and expose their format/option properties.
/// </summary>
public abstract partial class ConverterViewModelBase : ObservableObject
{
    protected AppSettings Settings { get; } = ServiceLocator.Settings;
    public StatusService Status { get; } = new StatusService();

    public ObservableCollection<ConversionItem> Items { get; } = new();

    [ObservableProperty] private string _outputFolder = string.Empty;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _footerNote = "All processing is done locally on your device.";

    private CancellationTokenSource? _cts;

    /// <summary>Lowercase, no-dot input extensions this tab accepts (e.g. "mp4", "mkv").</summary>
    protected abstract IReadOnlyCollection<string> SupportedExtensions { get; }

    /// <summary>Tab title used in logging.</summary>
    protected abstract string ToolName { get; }

    /// <summary>True when the user picked a specific output folder (otherwise source folder + suffix is used).</summary>
    protected bool UseCustomOutputFolder => !string.IsNullOrWhiteSpace(OutputFolder);

    public int ActiveCount => Items.Count;
    public bool CanConvert => !IsBusy && ActiveCount > 0;
    public bool CanCancel => IsBusy;
    public bool HasAnyItems => Items.Count > 0;

    protected ConverterViewModelBase()
    {
        OutputFolder = Settings.DefaultOutputFolder ?? string.Empty;
        Items.CollectionChanged += (_, _) => RaiseQueueState();
    }

    // ---- Intake -------------------------------------------------------------

    /// <summary>Add files received from the picker or a drag/drop operation.</summary>
    public virtual async Task AddPathsAsync(IEnumerable<string> paths)
    {
        var supported = new HashSet<string>(SupportedExtensions, StringComparer.OrdinalIgnoreCase);
        foreach (var path in paths)
        {
            if (string.IsNullOrWhiteSpace(path)) continue;
            var ext = Path.GetExtension(path).TrimStart('.').ToLowerInvariant();
            if (!supported.Contains(ext)) continue;
            if (Items.Any(i => string.Equals(i.SourcePath, path, StringComparison.OrdinalIgnoreCase))) continue;

            var item = new ConversionItem(path);
            Items.Add(item);
        }
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task AddFilesAsync()
    {
        var hwnd = Ui.MainWindowHandle;
        if (hwnd == IntPtr.Zero) return;
        var files = await FileDialogs.PickFilesAsync(hwnd, SupportedExtensions, "Add files");
        if (files == null || files.Count == 0) return;
        await AddPathsAsync(files.Select(f => f.Path));
    }

    [RelayCommand]
    private void RemoveItem(ConversionItem? item)
    {
        if (item != null && Items.Contains(item))
        {
            Items.Remove(item);
        }
    }

    [RelayCommand]
    private void ClearAll()
    {
        if (IsBusy) return;
        Items.Clear();
    }

    [RelayCommand]
    private async Task ChooseOutputFolderAsync()
    {
        var hwnd = Ui.MainWindowHandle;
        if (hwnd == IntPtr.Zero) return;
        var folder = await FileDialogs.PickFolderAsync(hwnd);
        if (folder != null)
        {
            OutputFolder = folder.Path;
        }
    }

    // ---- Queue state --------------------------------------------------------

    private void RaiseQueueState()
    {
        OnPropertyChanged(nameof(ActiveCount));
        OnPropertyChanged(nameof(CanConvert));
        OnPropertyChanged(nameof(HasAnyItems));
    }

    partial void OnIsBusyChanged(bool value) => RaiseQueueState();

    // ---- Output path --------------------------------------------------------

    protected string ResolveOutput(ConversionItem item, string targetExtensionWithoutDot)
    {
        string dir;
        string suffix;
        if (UseCustomOutputFolder)
        {
            dir = OutputFolder!;
            suffix = string.Empty;
        }
        else
        {
            dir = Path.GetDirectoryName(item.SourcePath) ?? item.SourcePath;
            suffix = "_converted";
        }

        var baseName = Path.GetFileNameWithoutExtension(item.SourcePath);
        var ext = "." + targetExtensionWithoutDot.TrimStart('.').ToLowerInvariant();
        var candidate = Path.Combine(dir, baseName + suffix + ext);

        var index = 1;
        while (File.Exists(candidate))
        {
            candidate = Path.Combine(dir, $"{baseName}{suffix} ({index}){ext}");
            index++;
        }
        return candidate;
    }

    // ---- Conversion ---------------------------------------------------------

    /// <summary>Perform one conversion. Set item.OutputPath and item.Progress; throw on failure.</summary>
    protected abstract Task ConvertItemAsync(ConversionItem item, CancellationToken cancellationToken);

    [RelayCommand]
    private async Task ConvertAsync()
    {
        if (IsBusy) return;

        var active = Items.ToList();
        if (active.Count == 0) return;

        Directory.CreateDirectory(UseCustomOutputFolder ? OutputFolder! : active[0].SourcePath);

        _cts = new CancellationTokenSource();
        IsBusy = true;
        Status.Reset();
        Status.IsBusy = true;
        Status.Report(0, active.Count);

        var processor = new BatchProcessor<ConversionItem>
        {
            MaxDegreeOfParallelism = Math.Clamp(Settings.ParallelJobs, 1, 8)
        };
        processor.ItemCompleted += (_, e) => Ui.Run(() =>
        {
            e.Item.Status = ConversionStatus.Completed;
            e.Item.Progress = 100;
            e.Item.Message = null;
        });
        processor.ItemFailed += (_, e) => Ui.Run(() =>
        {
            e.Item.Status = ConversionStatus.Failed;
            e.Item.Message = e.Error?.Message ?? "Conversion error";
        });

        try
        {
            await processor.RunAsync(
                active,
                async (item, ct) =>
                {
                    Ui.Run(() => item.Status = ConversionStatus.Processing);
                    await ConvertItemAsync(item, ct);
                },
                (completed, total) => Ui.Run(() => Status.Report(completed, total)),
                _cts.Token);

            Status.StatusMessage = "Completed.";
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
            global::Serilog.Log.Error(ex, "{Tool}: batch finished with errors", ToolName);
        }
        finally
        {
            foreach (var item in Items.Where(i => i.Status == ConversionStatus.Processing))
            {
                item.Status = ConversionStatus.Canceled;
            }
            Status.IsBusy = false;
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        _cts?.Cancel();
    }

    [RelayCommand]
    public async Task OpenOutputFolderAsync()
    {
        var first = Items.FirstOrDefault(i => !string.IsNullOrEmpty(i.OutputPath));        var target = first?.OutputPath;
        if (string.IsNullOrEmpty(target)) return;
        try
        {
            var folder = Path.GetDirectoryName(target);
            if (!string.IsNullOrEmpty(folder))
            {
                var folderItem = await StorageFolder.GetFolderFromPathAsync(folder);
                await Windows.System.Launcher.LaunchFolderAsync(folderItem);
            }
        }
        catch
        {
            // Opening the folder is best-effort.
        }
    }
}
