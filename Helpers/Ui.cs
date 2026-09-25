using System;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;

namespace SuperMediaSuites.Helpers;

/// <summary>
/// UI-thread helpers. The main window's <see cref="DispatcherQueue"/> and native handle are
/// captured at startup so background work (BatchProcessor callbacks) can marshal updates back.
/// </summary>
public static class Ui
{
    /// <summary>Product name shown in the title bar, tray tooltip and dialogs.</summary>
    public const string AppName = "Super Media Suites";

    public static DispatcherQueue? MainQueue { get; set; }
    public static IntPtr MainWindowHandle { get; set; }
    public static XamlRoot? XamlRoot { get; set; }

    /// <summary>Run an action on the UI thread; if already on it, run inline.</summary>
    public static void Run(Action action)
    {
        if (MainQueue == null || MainQueue.HasThreadAccess)
        {
            action();
        }
        else
        {
            MainQueue.TryEnqueue(() => action());
        }
    }
}
