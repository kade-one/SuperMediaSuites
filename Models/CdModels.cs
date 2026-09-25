using CommunityToolkit.Mvvm.ComponentModel;

namespace SuperMediaSuites.Models;

/// <summary>One audio track on a CD, as shown in the CD Ripper queue.</summary>
public partial class CdTrack : ObservableObject
{
    public int Number { get; }
    public int StartSector { get; }
    public int EndSector { get; }

    public CdTrack(int number, int startSector, int endSector, string durationText, string title)
    {
        Number = number;
        StartSector = startSector;
        EndSector = endSector;
        DurationText = durationText;
        Title = title;
    }

    public int SectorCount => EndSector - StartSector;

    /// <summary>Duration in m:ss (or the raw string if unknown).</summary>
    [ObservableProperty] private string _durationText = "0:00";

    /// <summary>Title (generic "Track 01" unless a MusicBrainz lookup provided a real one).</summary>
    [ObservableProperty] private string _title;

    [ObservableProperty] private CdTrackStatus _status = CdTrackStatus.Idle;
    [ObservableProperty] private int _progress;
    [ObservableProperty] private string? _message;
    [ObservableProperty] private string? _outputPath;

    partial void OnStatusChanged(CdTrackStatus value) => OnPropertyChanged(nameof(StatusText));
    partial void OnProgressChanged(int value) => OnPropertyChanged(nameof(StatusText));

    public string StatusText => Status switch
    {
        CdTrackStatus.Idle => "Ready",
        CdTrackStatus.Processing => Progress > 0 ? $"Ripping… {Progress}%" : "Ripping…",
        CdTrackStatus.Completed => "Done",
        CdTrackStatus.Failed => string.IsNullOrWhiteSpace(Message) ? "Failed" : $"Failed: {Message}",
        _ => Status.ToString()
    };
}

public enum CdTrackStatus
{
    Idle,
    Processing,
    Completed,
    Failed
}

/// <summary>One detected optical drive.</summary>
public sealed class CdDrive
{
    public char Letter { get; }
    public string VolumeLabel { get; }
    public string Display => $"{Letter}: {VolumeLabel}";

    public CdDrive(char letter, string volumeLabel)
    {
        Letter = letter;
        VolumeLabel = volumeLabel;
    }
}
