using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SuperMediaSuites.Helpers;
using SuperMediaSuites.Models;

namespace SuperMediaSuites.ViewModels;

/// <summary>
/// Player tab: lists video files from a folder and feeds the selected one to the MediaElement.
/// Defaults to scanning the user's Videos and Downloads folders.
/// </summary>
public partial class PlayerViewModel : ObservableObject
{
    private static readonly string[] VideoExtensions =
    {
        "mp4", "mkv", "mov", "avi", "webm", "wmv", "flv", "m4v", "mpg", "mpeg", "ts", "3gp"
    };

    public ObservableCollection<VideoEntry> Videos { get; } = new();

    [ObservableProperty] private VideoEntry? _selectedVideo;
    [ObservableProperty] private string _currentFolder = "Videos + Downloads";

    /// <summary>Scan the default Videos + Downloads folders.</summary>
    [RelayCommand]
    public async Task ScanDefaultsAsync()
    {
        var videos = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
        var downloads = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

        CurrentFolder = "Videos + Downloads";
        await ScanFoldersAsync(new[] { videos, downloads });
    }

    /// <summary>Pick a folder and list its videos.</summary>
    [RelayCommand]
    public async Task ChooseFolderAsync()
    {
        var hwnd = Ui.MainWindowHandle;
        if (hwnd == System.IntPtr.Zero) return;
        var folder = await FileDialogs.PickFolderAsync(hwnd);
        if (folder == null) return;

        CurrentFolder = folder.Path;
        await ScanFoldersAsync(new[] { folder.Path });
    }

    private async Task ScanFoldersAsync(IEnumerable<string> folders)
    {
        var found = new List<VideoEntry>();
        await Task.Run(() =>
        {
            foreach (var folder in folders)
            {
                if (!Directory.Exists(folder)) continue;
                foreach (var ext in VideoExtensions)
                {
                    string[] files;
                    try
                    {
                        files = Directory.GetFiles(folder, "*." + ext, SearchOption.TopDirectoryOnly);
                    }
                    catch
                    {
                        continue;
                    }
                    foreach (var file in files)
                    {
                        found.Add(new VideoEntry(file));
                    }
                }
            }
        });

        Videos.Clear();
        foreach (var entry in found)
        {
            Videos.Add(entry);
        }
    }
}
