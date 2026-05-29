namespace VisionSharp.Abstractions.Interfaces;

/// <summary>
/// Factory that creates <see cref="IImageBuilder"/> instances from various sources.
/// The builder returned is lazy: no I/O occurs until a terminal method is called.
/// </summary>
public interface IImageFactory
{
    /// <summary>Opens an image from a file path.</summary>
    IImageBuilder OpenAsync(string path);

    /// <summary>Opens an image from a stream.</summary>
    IImageBuilder OpenAsync(Stream stream);

    /// <summary>Opens an image from a byte array.</summary>
    IImageBuilder OpenAsync(byte[] bytes);

    /// <summary>Opens an image from a Base64-encoded string.</summary>
    IImageBuilder OpenBase64Async(string base64);

    /// <summary>Opens an image from a URL. The download is deferred to the terminal call.</summary>
    IImageBuilder OpenAsync(Uri url);
}
