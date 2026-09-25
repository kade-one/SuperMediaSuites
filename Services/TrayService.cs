using System.Runtime.InteropServices;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using WinRT.Interop;

namespace SuperMediaSuites.Services;

/// <summary>
/// Drives window show/hide/restore from the tray and intercepts the single-instance
/// "restore me" message posted by a second launch. The actual <c>TaskbarIcon</c> is
/// declared in <c>MainWindow.xaml</c> (XAML Option A); this service owns the Win32
/// subclassing + window control.
/// See winui3_singleton_startup_instruction.md (Part 1, step 3) and tray_icon_winui3_instruction.md.
/// </summary>
public sealed class TrayService : IDisposable
{
    /// <summary>Custom window message a second instance posts to wake this one. Shared with <c>App</c>.</summary>
    public const string RestoreMessageName = "SuperMediaSuitesRestoreMessage_7F4E2D";

    private const int SwRestore = 9;
    private const int SwShow = 5;
    private const uint SubclassId = 101;

    private Window? _window;
    private DispatcherQueue? _dispatcher;
    private IntPtr _hwnd;
    private uint _restoreMessageId;
    private SubclassProc? _subclassProc;
    private bool _disposed;

    /// <summary>Attach the subclass hook so the restore message is handled on this window.</summary>
    public void Initialize(Window window)
    {
        _window = window;
        _hwnd = WindowNative.GetWindowHandle(window);
        _dispatcher = DispatcherQueue.GetForCurrentThread();

        try
        {
            _restoreMessageId = RegisterWindowMessage(RestoreMessageName);
            if (_restoreMessageId != 0 && _hwnd != IntPtr.Zero)
            {
                // Keep the delegate alive (field) so it isn't GC'd while native code holds it.
                _subclassProc = new SubclassProc(OnSubclassMessage);
                SetWindowSubclass(_hwnd, _subclassProc, SubclassId, IntPtr.Zero);
            }
        }
        catch
        {
            // Non-fatal: subclassing is best-effort. The tray menu still works.
        }
    }

    private IntPtr OnSubclassMessage(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, uint uIdSubclass, IntPtr dwRefData)
    {
        if (_restoreMessageId != 0 && uMsg == _restoreMessageId)
        {
            ShowAndActivate();
            return IntPtr.Zero;
        }
        return DefSubclassProc(hWnd, uMsg, wParam, lParam);
    }

    /// <summary>Restore, show and bring the window to the foreground (marshaled to the UI thread).</summary>
    public void ShowAndActivate()
    {
        var window = _window;
        if (window == null) return;

        void Run()
        {
            try
            {
                // WinUI Window has no Show(); AppWindow.Show() makes a hidden window visible again.
                window.AppWindow.Show();
                window.Activate();
                if (_hwnd != IntPtr.Zero)
                {
                    ShowWindow(_hwnd, SwRestore);
                    ShowWindow(_hwnd, SwShow);
                    SetForegroundWindow(_hwnd);
                }
            }
            catch
            {
                // Window operations must never crash the tray callback.
            }
        }

        if (_dispatcher != null && !_dispatcher.HasThreadAccess)
        {
            _dispatcher.TryEnqueue(Run);
        }
        else
        {
            Run();
        }
    }

    /// <summary>Hide the window into the tray (used by --startup launch and close-to-tray).</summary>
    public void MinimizeToTray()
    {
        if (_window == null) return;
        try
        {
            _window.AppWindow.Hide();
        }
        catch
        {
            // AppWindow may be unavailable in some hosting contexts; non-fatal.
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            if (_subclassProc != null && _hwnd != IntPtr.Zero)
            {
                RemoveWindowSubclass(_hwnd, _subclassProc, SubclassId);
                _subclassProc = null;
            }
        }
        catch
        {
            // Always clean up subclass hooks to avoid access violations on process exit.
        }
    }

    #region Win32 P/Invoke

    private delegate IntPtr SubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, uint uIdSubclass, IntPtr dwRefData);

    [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
    private static extern bool SetWindowSubclass(IntPtr hWnd, SubclassProc pfnSubclass, uint uIdSubclass, IntPtr dwRefData);

    [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
    private static extern bool RemoveWindowSubclass(IntPtr hWnd, SubclassProc pfnSubclass, uint uIdSubclass);

    [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr DefSubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern uint RegisterWindowMessage(string lpString);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    #endregion
}
