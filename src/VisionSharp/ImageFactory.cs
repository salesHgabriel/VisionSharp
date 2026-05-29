using VisionSharp.Abstractions.Interfaces;
using VisionSharp.Core;
using VisionSharp.Core.Engines;

namespace VisionSharp;

/// <summary>
/// Static entry point for the VisionSharp fluent pipeline.
/// All <c>Open*</c> methods return a lazy <see cref="IImageBuilder"/>; no I/O occurs
/// until you call a terminal method such as <see cref="IImageBuilder.SaveAsync"/>.
/// </summary>
/// <example>
/// <code>
/// var image = await ImageFactory
///     .OpenAsync("photo.jpg")
///     .Resize(800, 600)
///     .Blur(2)
///     .Watermark("logo.png", WatermarkPosition.BottomRight)
///     .SaveAsync("output/photo.jpg");
/// </code>
/// </example>
public static class ImageFactory
{
    private static Func<IImageEngine> _engineFactory = () => new ImageSharpEngine();
    private static readonly HttpClient _httpClient = new();

    /// <summary>
    /// Overrides the engine factory used to create engine instances.
    /// Call this once at application startup when using a custom engine.
    /// </summary>
    public static void UseEngine(Func<IImageEngine> factory)
        => _engineFactory = factory ?? throw new ArgumentNullException(nameof(factory));

    /// <summary>Opens an image from a file path. Loading is deferred to the terminal call.</summary>
    public static IImageBuilder OpenAsync(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        return new ImageBuilder(_engineFactory, (engine, ct) => engine.LoadFromPathAsync(path, ct));
    }

    /// <summary>Opens an image from a stream. The stream must remain open until a terminal method is called.</summary>
    public static IImageBuilder OpenAsync(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);
        return new ImageBuilder(_engineFactory, (engine, ct) => engine.LoadFromStreamAsync(stream, ct));
    }

    /// <summary>Opens an image from a byte array.</summary>
    public static IImageBuilder OpenAsync(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        return new ImageBuilder(_engineFactory, (engine, ct) => engine.LoadFromBytesAsync(bytes, ct));
    }

    /// <summary>Opens an image from a Base64-encoded string.</summary>
    public static IImageBuilder OpenBase64Async(string base64)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(base64);
        return new ImageBuilder(_engineFactory, (engine, ct) => engine.LoadFromBase64Async(base64, ct));
    }

    /// <summary>Opens an image from a URL. The download is deferred to the terminal call.</summary>
    public static IImageBuilder OpenAsync(Uri url)
    {
        ArgumentNullException.ThrowIfNull(url);
        return new ImageBuilder(_engineFactory, async (engine, ct) =>
        {
            var bytes = await _httpClient.GetByteArrayAsync(url, ct);
            await engine.LoadFromBytesAsync(bytes, ct);
        });
    }
}
