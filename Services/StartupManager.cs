using Microsoft.Win32;

namespace SuperMediaSuites.Services;

/// <summary>
/// Registry-based "Run on Windows startup" (auto-start) for the unpackaged host.
/// Writes/deletes a value under HKCU\…\Run pointing at this executable with the
/// <c>--startup</c> argument, which the app uses to start hidden in the tray.
/// See winui3_singleton_startup_instruction.md, Part 2.
/// </summary>
public sealed class StartupManager
{
    private const string RunKeyName = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName = "SuperMediaSuites";

    /// <summary>True when the startup value exists and is non-empty.</summary>
    public bool IsAutoStartEnabled
    {
        get
        {
            try
            {
                using var key = Registry.CurrentUser.OpenSubKey(RunKeyName, false);
                if (key != null)
                {
                    var value = key.GetValue(AppName) as string;
                    return !string.IsNullOrEmpty(value);
                }
            }
            catch
            {
                // Unpackaged standard users under restricted policy may throw; treat as disabled.
            }
            return false;
        }
    }

    /// <summary>Register or unregister the app executable (with --startup) in the Run key.</summary>
    public void SetAutoStart(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RunKeyName, true);
            if (key == null) return;

            if (enable)
            {
                var exePath = GetExecutablePath();
                if (!string.IsNullOrEmpty(exePath))
                {
                    key.SetValue(AppName, $"\"{exePath}\" --startup");
                }
            }
            else
            {
                key.DeleteValue(AppName, false);
            }
        }
        catch
        {
            // Best-effort; a failed registry write must not crash the app.
        }
    }

    private static string GetExecutablePath()
    {
        try
        {
            var exePath = Environment.ProcessPath;
            if (string.IsNullOrEmpty(exePath))
            {
                return System.Reflection.Assembly.GetExecutingAssembly().Location;
            }
            return System.IO.Path.GetFullPath(exePath);
        }
        catch
        {
            return string.Empty;
        }
    }
}
