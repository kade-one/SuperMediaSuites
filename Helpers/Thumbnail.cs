using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using ImageMagick;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;

namespace SuperMediaSuites.Helpers;

/// <summary>Builds small preview thumbnails for the HEIC queue list.</summary>
public static class Thumbnail
{
    /// <summary>Decode an image, shrink it, and return PNG bytes (null on failure).</summary>
    public static byte[]? FromFile(string path, int size)
    {
        try
        {
            using var image = new MagickImage(path);
            image.Thumbnail((uint)size, (uint)size);
            image.Format = MagickFormat.Png;
            return image.ToByteArray();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Create a bindable BitmapImage from raw image bytes (call on the UI thread).</summary>
    public static async Task<BitmapImage?> ToBitmapAsync(byte[]? bytes)
    {
        if (bytes == null || bytes.Length == 0) return null;
        try
        {
            var bitmap = new BitmapImage();
            using var stream = new InMemoryRandomAccessStream();
            using (var writer = new DataWriter(stream.GetOutputStreamAt(0)))
            {
                writer.WriteBytes(bytes);
                await writer.StoreAsync();
            }
            stream.Seek(0);
            await bitmap.SetSourceAsync(stream);
            return bitmap;
        }
        catch
        {
            return null;
        }
    }
}
