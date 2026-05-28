using VisionSharp.Abstractions.Interfaces;

namespace VisionSharp.Formats.Extensions;

/// <summary>
/// Shorthand extensions for common format-encode-and-export workflows.
/// </summary>
public static class FormatBuilderExtensions
{
    /// <summary>Encodes to JPEG and returns the bytes in one call.</summary>
    public static Task<byte[]> ToJpegBytesAsync(
        this IImageBuilder builder,
        int quality = 90,
        CancellationToken cancellationToken = default)
        => builder.ToJpeg(quality).ToBytesAsync(cancellationToken);

    /// <summary>Encodes to PNG and returns the bytes in one call.</summary>
    public static Task<byte[]> ToPngBytesAsync(
        this IImageBuilder builder,
        CancellationToken cancellationToken = default)
        => builder.ToPng().ToBytesAsync(cancellationToken);

    /// <summary>Encodes to WebP and returns the bytes in one call.</summary>
    public static Task<byte[]> ToWebpBytesAsync(
        this IImageBuilder builder,
        int quality = 80,
        CancellationToken cancellationToken = default)
        => builder.ToWebp(quality).ToBytesAsync(cancellationToken);

    /// <summary>Encodes to JPEG and returns a Base64 data-URI string.</summary>
    public static async Task<string> ToJpegDataUriAsync(
        this IImageBuilder builder,
        int quality = 90,
        CancellationToken cancellationToken = default)
    {
        var b64 = await builder.ToJpeg(quality).ToBase64Async(cancellationToken).ConfigureAwait(false);
        return $"data:image/jpeg;base64,{b64}";
    }

    /// <summary>Encodes to PNG and returns a Base64 data-URI string.</summary>
    public static async Task<string> ToPngDataUriAsync(
        this IImageBuilder builder,
        CancellationToken cancellationToken = default)
    {
        var b64 = await builder.ToPng().ToBase64Async(cancellationToken).ConfigureAwait(false);
        return $"data:image/png;base64,{b64}";
    }

    /// <summary>Encodes to WebP and returns a Base64 data-URI string.</summary>
    public static async Task<string> ToWebpDataUriAsync(
        this IImageBuilder builder,
        int quality = 80,
        CancellationToken cancellationToken = default)
    {
        var b64 = await builder.ToWebp(quality).ToBase64Async(cancellationToken).ConfigureAwait(false);
        return $"data:image/webp;base64,{b64}";
    }
}
