using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Media;

namespace SuperMediaSuites.Models;

/// <summary>
/// One row in a converter tab's file queue. Holds per-item status/progress so the
/// list can render each file's state independently while <see cref="Services.BatchProcessor{TWork}"/>
/// processes them in parallel.
/// </summary>
public partial class ConversionItem : ObservableObject
{
    public string SourcePath { get; }

    /// <summary>Display name shown in the queue list.</summary>
    public string FileName { get; }

    /// <summary>Optional human-friendly subtitle (dimensions, duration, size...).</summary>
    public string? Subtitle { get; init; }

    public ConversionItem(string sourcePath)
    {
        SourcePath = sourcePath;
        FileName = Path.GetFileName(sourcePath);
    }

    [ObservableProperty] private ConversionStatus _status = ConversionStatus.Queued;
    [ObservableProperty] private int _progress;
    [ObservableProperty] private string? _message;
    [ObservableProperty] private string? _outputPath;

    /// <summary>Optional preview thumbnail (HEIC tab). Null when not applicable.</summary>
    [ObservableProperty] private ImageSource? _preview;

    /// <summary>One-line status suitable for the queue cell.</summary>
    public string StatusText => Status switch
    {
        ConversionStatus.Queued => "Queued",
        ConversionStatus.Processing => Progress > 0 ? $"Processing… {Progress}%" : "Processing…",
        ConversionStatus.Completed => "Done",
        ConversionStatus.Failed => string.IsNullOrWhiteSpace(Message) ? "Failed" : $"Failed: {Message}",
        ConversionStatus.Canceled => "Canceled",
        _ => Status.ToString()
    };

    partial void OnStatusChanged(ConversionStatus value) => OnPropertyChanged(nameof(StatusText));
    partial void OnProgressChanged(int value) { if (Status == ConversionStatus.Processing) OnPropertyChanged(nameof(StatusText)); }
    partial void OnMessageChanged(string? value) { if (Status == ConversionStatus.Failed) OnPropertyChanged(nameof(StatusText)); }
}

public enum ConversionStatus
{
    Queued,
    Processing,
    Completed,
    Failed,
    Canceled
}
