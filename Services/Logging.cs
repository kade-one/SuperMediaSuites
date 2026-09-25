using System;
using System.IO;
using Serilog;
using Serilog.Events;

namespace SuperMediaSuites.Services;

/// <summary>
/// Configures the global Serilog logger. Writes rolling daily files to
/// %LOCALAPPDATA%\SuperMediaSuites\Logs.
/// </summary>
public static class Logging
{
    private static readonly string LogDirectory = System.IO.Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SuperMediaSuites", "Logs");

    public static void Configure()
    {
        try
        {
            Directory.CreateDirectory(LogDirectory);
        }
        catch
        {
            // If we can't create the folder, Serilog will fall back to its own error handling.
        }

        var logFile = System.IO.Path.Combine(LogDirectory, "converter-.log");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                logFile,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 14,
                fileSizeLimitBytes: 10 * 1024 * 1024,
                rollOnFileSizeLimit: true,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Information("{AppName} logging initialised at {LogDir}", Helpers.Ui.AppName, LogDirectory);
    }
}
