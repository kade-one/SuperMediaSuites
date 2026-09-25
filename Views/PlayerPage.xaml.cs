using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SuperMediaSuites.ViewModels;
using Windows.Media.Core;
using Windows.Storage;

namespace SuperMediaSuites.Views;

/// <summary>Player tab: loads the selected video into the MediaElement.</summary>
public sealed partial class PlayerPage : UserControl
{
    public PlayerViewModel ViewModel { get; } = new PlayerViewModel();

    public PlayerPage()
    {
        InitializeComponent();
        Loaded += async (_, _) => await ViewModel.ScanDefaultsAsync();
    }

    private async void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ViewModel.SelectedVideo is not { } entry)
        {
            PlayerMedia.Source = null;
            return;
        }

        try
        {
            var file = await StorageFile.GetFileFromPathAsync(entry.SourcePath);
            PlayerMedia.Source = MediaSource.CreateFromStorageFile(file);
            PlayerMedia.MediaPlayer?.Play();
        }
        catch
        {
            PlayerMedia.Source = null;
        }
    }
}
