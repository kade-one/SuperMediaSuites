using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using SuperMediaSuites.Services;

namespace SuperMediaSuites;

/// <summary>
/// Application entry point. Configures logging, enforces single-instance behavior (a second
/// launch restores the running window and exits), applies the persisted theme, and shows the
/// main window.
/// </summary>
public partial class App : Application
{
    /// <summary>Window title used by <see cref="FindWindow"/> to locate the running instance.</summary>
    private static string WindowTitle => Helpers.Ui.AppName;

    private static Mutex? _mutex;
    private Window? _window;

    public App()
    {
        Logging.Configure();
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        bool createdNew;
        // "Local\\" scopes the mutex to the current user session (multi-user servers need this).
        _mutex = new Mutex(true, @"Local\SuperMediaSuites_SingleInstance_8F3D2A", out createdNew);

        if (!createdNew)
        {
            // Another instance is already running — wake it and exit this one.
            RestoreExistingInstance();
            Application.Current.Exit();
            return;
        }

        _window = new MainWindow();
        _window.Activate();

        // Launched silently by Windows login (registry auto-start): start hidden in the tray.
        var isStartup = Array.IndexOf(Environment.GetCommandLineArgs(), "--startup") >= 0;
        if (isStartup && _window is MainWindow main)
        {
            main.TrayService.MinimizeToTray();
        }
    }

    /// <summary>Find the running instance by its exact window title and signal it to restore.</summary>
    private static void RestoreExistingInstance()
    {
        try
        {
            var hwnd = FindWindow(null, WindowTitle);
            if (hwnd == IntPtr.Zero) return;

            var msg = RegisterWindowMessage(TrayService.RestoreMessageName);
            if (msg != 0)
            {
                PostMessage(hwnd, msg, IntPtr.Zero, IntPtr.Zero);
            }

            // Fallback in case the subclassed instance is slow to handle the posted message.
            ShowWindow(hwnd, 9); // SW_RESTORE
            ShowWindow(hwnd, 5); // SW_SHOW
            SetForegroundWindow(hwnd);
        }
        catch
        {
            // Non-fatal: worst case the user activates the existing window manually.
        }
    }

    #region Win32 P/Invoke for instance restoration

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern uint RegisterWindowMessage(string lpString);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    #endregion
}
