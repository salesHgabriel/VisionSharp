using VisionSharp.Abstractions.Enums;
using VisionSharp.Abstractions.Options;

namespace VisionSharp.Abstractions.Interfaces;

/// <summary>
/// Low-level contract for graphics engine implementations (ImageSharp, SkiaSharp, Magick.NET …).
/// Each engine instance is stateful: it holds a single loaded image and applies
/// mutations in place. Engines must be disposed after use.
/// </summary>
public interface IImageEngine : IAsyncDisposable
{
    /// <summary>Width of the currently loaded image in pixels.</summary>
    int Width { get; }

    /// <summary>Height of the currently loaded image in pixels.</summary>
    int Height { get; }

    // ── Loading ───────────────────────────────────────────────────────────────

    /// <summary>Loads an image from a file path.</summary>
    Task LoadFromPathAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>Loads an image from an open stream.</summary>
    Task LoadFromStreamAsync(Stream stream, CancellationToken cancellationToken = default);

    /// <summary>Loads an image from a byte array.</summary>
    Task LoadFromBytesAsync(byte[] bytes, CancellationToken cancellationToken = default);

    /// <summary>Loads an image from a Base64-encoded string.</summary>
    Task LoadFromBase64Async(string base64, CancellationToken cancellationToken = default);

    // ── Geometry ──────────────────────────────────────────────────────────────

    void ApplyResize(int width, int height, ResizeOptions options);
    void ApplyCover(int width, int height, bool onlyIfLarger = false);
    void ApplyContain(int width, int height, bool onlyIfLarger = false);
    void ApplyCrop(int x, int y, int width, int height);
    void ApplyResizeCanvas(int width, int height, string backgroundColor);

    // ── Effects ───────────────────────────────────────────────────────────────

    void ApplyBlur(int radius);
    void ApplyGaussianBlur(float sigma);
    void ApplyBrightness(float value);
    void ApplyContrast(float value);
    void ApplyGrayscale();
    void ApplySepia();
    void ApplySharpen();
    void ApplyPixelate(int size);
    void ApplyRotate(float degrees);
    void ApplyFlipHorizontal();
    void ApplyFlipVertical();
    void ApplyOpacity(float value);

    // ── Watermark ─────────────────────────────────────────────────────────────

    Task ApplyWatermarkAsync(string imagePath, WatermarkPosition position, float opacity, CancellationToken cancellationToken = default);
    void ApplyWatermarkText(string text, TextWatermarkOptions options);

    // ── Drawing ───────────────────────────────────────────────────────────────

    void ApplyDrawLine(float x1, float y1, float x2, float y2, DrawingOptions options);
    void ApplyDrawRectangle(float x, float y, float width, float height, DrawingOptions options);
    void ApplyDrawCircle(float centerX, float centerY, float radius, DrawingOptions options);
    void ApplyDrawPolygon(IEnumerable<(float X, float Y)> vertices, DrawingOptions options);
    void ApplyDrawText(string text, float x, float y, DrawingOptions options);
    Task ApplyImageInsideShapeAsync(string imagePath, ShapeType shapeType, float x, float y, float width, float height, CancellationToken cancellationToken = default);

    // ── Format ────────────────────────────────────────────────────────────────

    void SetOutputFormat(ImageFormat format, int quality);

    // ── Output ────────────────────────────────────────────────────────────────

    Task SaveToPathAsync(string path, CancellationToken cancellationToken = default);
    Task<byte[]> GetBytesAsync(CancellationToken cancellationToken = default);
    Task<Stream> GetStreamAsync(CancellationToken cancellationToken = default);
    Task<string> GetBase64Async(CancellationToken cancellationToken = default);
    int GetWidth();
    int GetHeight();
}
