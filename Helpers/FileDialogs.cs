using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace SuperMediaSuites.Helpers;

/// <summary>WinUI pickers require an owner window handle; these wrap that for the single-window app.</summary>
public static class FileDialogs
{
    public static async Task<IReadOnlyList<StorageFile>?> PickFilesAsync(IntPtr hwnd, IEnumerable<string> extensions, string title)
    {
        var picker = new FileOpenPicker
        {
            ViewMode = PickerViewMode.List,
            SuggestedStartLocation = PickerLocationId.ComputerFolder,
            CommitButtonText = "Add"
        };
        picker.SettingsIdentifier = "addfiles";

        var any = false;
        foreach (var ext in extensions)
        {
            var normalized = ext.Trim().TrimStart('.');
            if (string.IsNullOrEmpty(normalized)) continue;
            picker.FileTypeFilter.Add("." + normalized.ToLowerInvariant());
            any = true;
        }
        if (!any)
        {
            picker.FileTypeFilter.Add("*");
        }

        InitializeWithWindow.Initialize(picker, hwnd);
        var files = await picker.PickMultipleFilesAsync();
        return files;
    }

    public static async Task<StorageFolder?> PickFolderAsync(IntPtr hwnd)
    {
        var picker = new FolderPicker();
        picker.FileTypeFilter.Add("*");
        InitializeWithWindow.Initialize(picker, hwnd);
        return await picker.PickSingleFolderAsync();
    }
}
