using Microsoft.UI.Xaml;

namespace SuperMediaSuites.Services;

/// <summary>
/// Applies the persisted UI theme (System/Dark/Light) to the app's root element at runtime.
/// WinUI 3 desktop apps cannot switch <see cref="Application.RequestedTheme"/> after startup, so
/// we drive <see cref="FrameworkElement.RequestedTheme"/> on the window's root element instead,
/// which re-themes the entire live visual tree (and the Mica backdrop follows it).
/// </summary>
public static class ThemeHelper
{
    private static FrameworkElement? _root;

    /// <summary>Register the root element so theme changes can be pushed to it later.</summary>
    public static void Register(FrameworkElement root)
    {
        _root = root;
        ApplyCurrent();
    }

    /// <summary>Apply the theme currently saved in settings (called at startup).</summary>
    public static void ApplyCurrent()
    {
        if (_root == null) return;
        _root.RequestedTheme = ToElementTheme(ServiceLocator.Settings.Theme);
    }

    /// <summary>Persist a new theme string to settings and apply it immediately.</summary>
    public static void SetTheme(string theme)
    {
        var settings = ServiceLocator.Settings;
        settings.Theme = Normalize(theme);
        settings.Save();
        if (_root != null)
        {
            _root.RequestedTheme = ToElementTheme(settings.Theme);
        }
    }

    /// <summary>Map a stored theme string to an <see cref="ElementTheme"/> ("System" → Default).</summary>
    public static ElementTheme ToElementTheme(string theme) => Normalize(theme) switch
    {
        "Dark" => ElementTheme.Dark,
        "Light" => ElementTheme.Light,
        _ => ElementTheme.Default,
    };

    private static string Normalize(string? theme) => string.IsNullOrWhiteSpace(theme) ? "System" : theme;
}
