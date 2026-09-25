using System;
using ImageMagick;

namespace SuperMediaSuites.Helpers;

/// <summary>Maps the string format tokens used in the UI to Magick.NET format enums.</summary>
public static class ImageFormatMap
{
    public static MagickFormat ToMagick(string token) => token.Trim().ToLowerInvariant() switch
    {
        "jpg" or "jpeg" => MagickFormat.Jpeg,
        "png" => MagickFormat.Png,
        "webp" => MagickFormat.WebP,
        "bmp" => MagickFormat.Bmp,
        "gif" => MagickFormat.Gif,
        "tiff" or "tif" => MagickFormat.Tiff,
        "ico" => MagickFormat.Ico,
        "avif" => MagickFormat.Avif,
        "heic" or "heif" => MagickFormat.Heic,
        _ => default // unmapped: let Magick.NET infer from the output file extension
    };

    public static ImageMagick.MagickColor? ParseColor(string hex)
    {
        try
        {
            return string.IsNullOrWhiteSpace(hex) ? null : new MagickColor(hex.Trim());
        }
        catch (Exception)
        {
            return null;
        }
    }
}
