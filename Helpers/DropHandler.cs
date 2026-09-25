using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI.Xaml;

namespace SuperMediaSuites.Helpers;

/// <summary>Shared drag/drop handling for the converter pages (keeps each code-behind thin).</summary>
public static class DropHandler
{
    public static void MarkCopy(DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            e.DragUIOverride.Caption = "Add files";
            e.DragUIOverride.IsCaptionVisible = true;
        }
    }

    public static async Task AcceptAsync(DragEventArgs e, Func<IEnumerable<string>, Task> addPaths)
    {
        if (!e.DataView.Contains(StandardDataFormats.StorageItems)) return;
        var deferral = e.GetDeferral();
        try
        {
            var items = await e.DataView.GetStorageItemsAsync();
            await addPaths(items.Select(i => i.Path));
        }
        finally
        {
            deferral.Complete();
        }
    }
}
