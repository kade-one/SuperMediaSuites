using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SuperMediaSuites.Helpers;
using SuperMediaSuites.Services;

namespace SuperMediaSuites.ViewModels;

/// <summary>Tab 8 — Settings: defaults, appearance, startup, and about box.</summary>
public partial class SettingsViewModel : ObservableObject
{
    private readonly AppSettings _settings = ServiceLocator.Settings;

    public string AppName => Helpers.Ui.AppName;
    public int[] ParallelJobOptions { get; } = { 1, 2, 3, 4, 5, 6, 7, 8 };
    public string[] ThemeOptions { get; } = { "System", "Dark", "Light" };
    public string AppVersion { get; }

    [ObservableProperty] private string _defaultOutputFolder;
    [ObservableProperty] private bool _openOutputFolderAfter;
    [ObservableProperty] private bool _minimizeToTray;
    [ObservableProperty] private int _parallelJobs;
    [ObservableProperty] private string _theme;
    [ObservableProperty] private bool _runAtStartup;

    public SettingsViewModel()
    {
        DefaultOutputFolder = _settings.DefaultOutputFolder ?? string.Empty;
        OpenOutputFolderAfter = _settings.OpenOutputFolderAfter;
        MinimizeToTray = _settings.MinimizeToTray;
        ParallelJobs = Math.Clamp(_settings.ParallelJobs, 1, 8);

        // Theme: source of truth is settings.json; the registry is the source of truth for startup.
        _theme = string.IsNullOrWhiteSpace(_settings.Theme) ? "System" : _settings.Theme;
        _runAtStartup = new StartupManager().IsAutoStartEnabled;

        AppVersion = ReadAppVersion();
    }

    partial void OnDefaultOutputFolderChanged(string value) { _settings.DefaultOutputFolder = value; _settings.Save(); }
    partial void OnOpenOutputFolderAfterChanged(bool value) { _settings.OpenOutputFolderAfter = value; _settings.Save(); }
    partial void OnMinimizeToTrayChanged(bool value) { _settings.MinimizeToTray = value; _settings.Save(); }
    partial void OnParallelJobsChanged(int value) { _settings.ParallelJobs = Math.Clamp(value, 1, 8); _settings.Save(); }

    partial void OnThemeChanged(string value) => ThemeHelper.SetTheme(value);

    partial void OnRunAtStartupChanged(bool value)
    {
        new StartupManager().SetAutoStart(value);
        _settings.RunAtStartup = value;
        _settings.Save();
    }

    [RelayCommand]
    private async Task ChooseOutputFolderAsync()
    {
        var hwnd = Ui.MainWindowHandle;
        if (hwnd == IntPtr.Zero) return;
        var folder = await FileDialogs.PickFolderAsync(hwnd);
        if (folder != null) DefaultOutputFolder = folder.Path;
    }

    private static string ReadAppVersion()
    {
        var version = typeof(SettingsViewModel).Assembly.GetName().Version;
        return version == null ? "1.0.0.0" : $"{version.Major}.{version.Minor}.{version.Build}";
    }
}
