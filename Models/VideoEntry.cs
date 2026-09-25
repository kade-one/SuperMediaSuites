using System.IO;

namespace SuperMediaSuites.Models;

/// <summary>One video file shown in the Player tab's list.</summary>
public sealed class VideoEntry
{
    public string SourcePath { get; }
    public string FileName { get; }
    public string Folder { get; }

    public VideoEntry(string sourcePath)
    {
        SourcePath = sourcePath;
        FileName = Path.GetFileName(sourcePath);
        Folder = Path.GetDirectoryName(sourcePath) ?? string.Empty;
    }
}
