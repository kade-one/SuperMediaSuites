using System.IO;
using System.Text.Json;

namespace SuperMediaSuites.Services;

/// <summary>
/// Persisted application settings. Stored as JSON in
/// %LOCALAPPDATA%\SuperMediaSuites\settings.json, which works for the unpackaged
/// (portable) build without needing the application-data capability.
/// </summary>
public sealed class AppSettings
{
    private static readonly string AppDataFolder = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SuperMediaSuites");

    private static readonly string SettingsFile = System.IO.Path.Combine(AppDataFolder, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public string DefaultOutputFolder { get; set; } = string.Empty;
    public bool OpenOutputFolderAfter { get; set; } = true;
    public bool MinimizeToTray { get; set; } = true;
    public int ParallelJobs { get; set; } = 3;

    /// <summary>"System", "Dark", or "Light". Drives the root RequestedTheme.</summary>
    public string Theme { get; set; } = "System";

    /// <summary>Whether the app launches into the tray on Windows login (registry auto-start).</summary>
    public bool RunAtStartup { get; set; }

    private static string DefaultDocumentsOutput()
    {
        var docs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return System.IO.Path.Combine(docs, "SuperMediaSuites Output");
    }

    public static AppSettings Load()
    {
        AppSettings? loaded = null;
        try
        {
            if (File.Exists(SettingsFile))
            {
                loaded = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(SettingsFile));
            }
        }
        catch
        {
            // Corrupt settings file — fall through to defaults rather than crash.
        }

        var settings = loaded ?? new AppSettings();
        if (string.IsNullOrWhiteSpace(settings.DefaultOutputFolder))
        {
            settings.DefaultOutputFolder = DefaultDocumentsOutput();
        }
        return settings;
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(AppDataFolder);
            File.WriteAllText(SettingsFile, JsonSerializer.Serialize(this, JsonOptions));
        }
        catch
        {
            // Best-effort persistence; not fatal.
        }
    }
}
