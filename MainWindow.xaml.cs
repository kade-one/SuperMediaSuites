using System.IO;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using SuperMediaSuites.Helpers;
using SuperMediaSuites.Services;

namespace SuperMediaSuites;

/// <summary>
/// Main shell: an 8-tab <c>TabView</c> (Video, Audio, Image, HEIC, CD Ripper, PDF, Player, Settings).
/// Uses the default window title bar; hosts the system-tray icon; applies the persisted theme to
/// the root element. Closing the window minimizes to the tray when that setting is on.
/// </summary>
public sealed partial class MainWindow : Window
{
    private readonly TrayService _trayService = new();
    private bool _reallyExit;

    /// <summary>Tray commands are created BEFORE InitializeComponent because compiled x:Bind is OneTime.</summary>
    public RelayCommand TrayOpenCommand { get; }
    public RelayCommand TrayExitCommand { get; }

    /// <summary>Exposed so <c>App</c> can minimize to the tray on <c>--startup</c> launches.</summary>
    public TrayService TrayService => _trayService;

    public MainWindow()
    {
        TrayOpenCommand = new RelayCommand(() => _trayService.ShowAndActivate());
        TrayExitCommand = new RelayCommand(RequestExit);

        InitializeComponent();

        Title = Ui.AppName;
        TrayIcon.ToolTipText = Ui.AppName;

        // Keep the window handle + dispatcher available for pickers and background marshaling.
        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        Ui.MainWindowHandle = hwnd;
        Ui.MainQueue = DispatcherQueue.GetForCurrentThread();

        try
        {
            uint dpi = GetDpiForWindow(hwnd);
            double scale = dpi > 0 ? dpi / 96.0 : 1.0;
            int width = (int)System.Math.Round(800 * scale);
            int height = (int)System.Math.Round(600 * scale);

            AppWindow.Resize(new Windows.Graphics.SizeInt32 { Width = width, Height = height });
            AppWindow.SetIcon(System.IO.Path.Combine(
                System.IO.Path.GetDirectoryName(typeof(MainWindow).Assembly.Location)!,
                "Assets", "AppIcon.ico"));
        }
        catch
        {
            // AppWindow may be unavailable in some hosting contexts; non-fatal.
        }

        ThemeHelper.Register(RootGrid);
        ApplyTrayIcon();
        _trayService.Initialize(this);

        RootGrid.Loaded += (s, e) => Ui.XamlRoot = RootGrid.XamlRoot;

        CheckFfmpegStatus();

        this.Closed += OnClosed;
        AppWindow.Closing += OnClosing;
    }

    /// <summary>Load the bundled .ico into the tray icon (file-path based, works unpackaged).</summary>
    private void ApplyTrayIcon()
    {
        try
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico");
            if (File.Exists(path))
            {
                TrayIcon.Icon = new System.Drawing.Icon(path);
            }
        }
        catch
        {
            // Tray falls back to a default icon; non-fatal.
        }
    }

    private void OnClosing(AppWindow sender, AppWindowClosingEventArgs args)
    {
        if (_reallyExit) return;

        // Close-to-tray: hide instead of quitting, unless the user explicitly exits via the tray.
        if (ServiceLocator.Settings.MinimizeToTray)
        {
            args.Cancel = true;
            _trayService.MinimizeToTray();
        }
    }

    private void RequestExit()
    {
        _reallyExit = true;
        Close();
    }

    private void OnClosed(object sender, WindowEventArgs args)
    {
        try { _trayService.Dispose(); } catch { /* best-effort cleanup */ }
        try { TrayIcon.Dispose(); } catch { /* best-effort cleanup */ }
    }

    private void CheckFfmpegStatus()
    {
        bool ffmpegAvailable = FFmpegLocator.IsAvailable();
        bool ffprobeAvailable = FFmpegLocator.IsFfprobeAvailable();
        string? ffmpegPath = FFmpegLocator.Locate();
        System.Diagnostics.Debug.WriteLine($"[FFmpeg Status] ffmpeg available: {ffmpegAvailable} (path: {ffmpegPath ?? "null"}), ffprobe available: {ffprobeAvailable}");

        bool missing = !ffmpegAvailable || !ffprobeAvailable;
        FfmpegWarningBar.IsOpen = missing;
    }

    private void OnRecheckFfmpegClick(object sender, RoutedEventArgs e)
    {
        CheckFfmpegStatus();
    }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(System.IntPtr hwnd);
}
