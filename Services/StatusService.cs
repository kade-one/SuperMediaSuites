using CommunityToolkit.Mvvm.ComponentModel;

namespace SuperMediaSuites.Services;

/// <summary>
/// Bindable progress/status state shown at the bottom of every converter tab.
/// Each tab owns its own instance so tabs report progress independently.
/// </summary>
public partial class StatusService : ObservableObject
{
    [ObservableProperty] private int _progress;
    [ObservableProperty] private string _statusMessage = "Ready";
    [ObservableProperty] private bool _isBusy;

    public void Reset()
    {
        Progress = 0;
        StatusMessage = "Ready";
        IsBusy = false;
    }

    /// <summary>Update the aggregate progress bar (completed of total).</summary>
    public void Report(int completed, int total)
    {
        Progress = total <= 0 ? 0 : (int)Math.Round((double)completed / total * 100);
        StatusMessage = total <= 0 ? "Ready" : $"Converting… {completed}/{total}";
    }
}
