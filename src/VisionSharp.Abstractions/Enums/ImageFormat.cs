namespace VisionSharp.Abstractions.Enums;

/// <summary>Supported output image formats.</summary>
public enum ImageFormat
{
    /// <summary>JPEG format — lossy, best for photos.</summary>
    Jpeg,
    /// <summary>PNG format — lossless, supports transparency.</summary>
    Png,
    /// <summary>WebP format — modern, smaller file size.</summary>
    Webp,
    /// <summary>GIF format — supports animation.</summary>
    Gif,
    /// <summary>BMP format — uncompressed bitmap.</summary>
    Bmp,
    /// <summary>Preserve original format from source.</summary>
    Original
}
