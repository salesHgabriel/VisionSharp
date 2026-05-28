using VisionSharp.Abstractions.Enums;
using VisionSharp.Abstractions.Options;

namespace VisionSharp.Abstractions.Interfaces;

/// <summary>
/// Fluent builder that accumulates image transformation operations and executes
/// them lazily when a terminal method (<see cref="SaveAsync"/>, <see cref="ToBytesAsync"/>,
/// <see cref="ToBase64Async"/>, or <see cref="ToStreamAsync"/>) is called.
/// </summary>
public interface IImageBuilder
{
    // ── Resize ────────────────────────────────────────────────────────────────

    /// <summary>Resizes the image to the specified dimensions, stretching if needed.</summary>
    IImageBuilder Resize(int width, int height, ResizeOptions? options = null);

    /// <summary>Resizes the image only if it is larger than the target.</summary>
    IImageBuilder ResizeDown(int width, int height);

    /// <summary>Scales the image uniformly by the given factor.</summary>
    IImageBuilder Scale(double factor);

    /// <summary>Scales the image uniformly by the given factor, only if it would reduce the size.</summary>
    IImageBuilder ScaleDown(double factor);

    // ── Cover / Contain ───────────────────────────────────────────────────────

    /// <summary>
    /// Scales and crops the image to exactly fill the target dimensions while
    /// preserving aspect ratio (CSS <c>object-fit: cover</c>).
    /// </summary>
    IImageBuilder Cover(int width, int height);

    /// <summary>Cover, but only when the image is larger than the target.</summary>
    IImageBuilder CoverDown(int width, int height);

    /// <summary>
    /// Scales the image to fit within the target dimensions while preserving
    /// aspect ratio (CSS <c>object-fit: contain</c>).
    /// </summary>
    IImageBuilder Contain(int width, int height);

    /// <summary>Contain, but only when the image is larger than the target.</summary>
    IImageBuilder ContainDown(int width, int height);

    // ── Crop ──────────────────────────────────────────────────────────────────

    /// <summary>Crops to the specified size from the center of the image.</summary>
    IImageBuilder Crop(int width, int height);

    /// <summary>Crops a rectangle starting at (<paramref name="x"/>, <paramref name="y"/>).</summary>
    IImageBuilder Crop(int x, int y, int width, int height);

    // ── Canvas ────────────────────────────────────────────────────────────────

    /// <summary>Resizes the canvas (adds or removes padding) without scaling the image.</summary>
    IImageBuilder ResizeCanvas(int width, int height, string backgroundColor = "#00000000");

    /// <summary>Adjusts the canvas size by the given pixel offsets.</summary>
    IImageBuilder ResizeCanvasRelative(int widthOffset, int heightOffset);

    // ── Effects ───────────────────────────────────────────────────────────────

    /// <summary>Applies a box blur with the specified radius.</summary>
    IImageBuilder Blur(int radius);

    /// <summary>Applies a Gaussian blur with the specified sigma.</summary>
    IImageBuilder GaussianBlur(float sigma);

    /// <summary>Adjusts image brightness. Value range: -1.0 (black) to 1.0 (white), 0 = unchanged.</summary>
    IImageBuilder Brightness(float value);

    /// <summary>Adjusts image contrast. Value range: -1.0 to 1.0, 0 = unchanged.</summary>
    IImageBuilder Contrast(float value);

    /// <summary>Converts the image to grayscale.</summary>
    IImageBuilder Grayscale();

    /// <summary>Applies a sepia tone effect.</summary>
    IImageBuilder Sepia();

    /// <summary>Sharpens the image.</summary>
    IImageBuilder Sharpen();

    /// <summary>Applies a pixelation effect with the given block size.</summary>
    IImageBuilder Pixelate(int size);

    /// <summary>Rotates the image clockwise by the specified number of degrees.</summary>
    IImageBuilder Rotate(float degrees);

    /// <summary>Flips the image horizontally.</summary>
    IImageBuilder FlipHorizontal();

    /// <summary>Flips the image vertically.</summary>
    IImageBuilder FlipVertical();

    /// <summary>Sets the overall opacity of the image (0.0 – 1.0).</summary>
    IImageBuilder Opacity(float value);

    // ── Watermark ─────────────────────────────────────────────────────────────

    /// <summary>Overlays an image watermark at the specified position.</summary>
    IImageBuilder Watermark(string imagePath, WatermarkPosition position, float opacity = 0.7f);

    /// <summary>Renders a text watermark using the supplied options.</summary>
    IImageBuilder WatermarkText(string text, Action<TextWatermarkOptions>? configure = null);

    // ── Drawing ───────────────────────────────────────────────────────────────

    /// <summary>Draws a line between two points.</summary>
    IImageBuilder DrawLine(float x1, float y1, float x2, float y2, Action<DrawingOptions>? configure = null);

    /// <summary>Draws a rectangle.</summary>
    IImageBuilder DrawRectangle(float x, float y, float width, float height, Action<DrawingOptions>? configure = null);

    /// <summary>Draws a circle with the specified center and radius.</summary>
    IImageBuilder DrawCircle(float centerX, float centerY, float radius, Action<DrawingOptions>? configure = null);

    /// <summary>Draws a filled polygon from the given vertex array.</summary>
    IImageBuilder DrawPolygon(IEnumerable<(float X, float Y)> vertices, Action<DrawingOptions>? configure = null);

    /// <summary>Renders text at the specified coordinates.</summary>
    IImageBuilder DrawText(string text, float x, float y, Action<DrawingOptions>? configure = null);

    /// <summary>Clips a secondary image into a geometric shape and composites it onto the canvas.</summary>
    IImageBuilder AddImageInsideShape(string imagePath, ShapeType shapeType, float x, float y, float width, float height);

    // ── Format ────────────────────────────────────────────────────────────────

    /// <summary>Sets the output format to JPEG with the given quality (1–100).</summary>
    IImageBuilder ToJpeg(int quality = 90);

    /// <summary>Sets the output format to PNG.</summary>
    IImageBuilder ToPng();

    /// <summary>Sets the output format to WebP with the given quality (1–100).</summary>
    IImageBuilder ToWebp(int quality = 80);

    /// <summary>Sets the output format to GIF.</summary>
    IImageBuilder ToGif();

    /// <summary>Sets the output format to BMP.</summary>
    IImageBuilder ToBmp();

    // ── Terminal operations ───────────────────────────────────────────────────

    /// <summary>Executes the pipeline and saves the result to <paramref name="path"/>.</summary>
    Task<IImage> SaveAsync(string path, CancellationToken cancellationToken = default);

    /// <summary>Executes the pipeline and returns the result as a byte array.</summary>
    Task<byte[]> ToBytesAsync(CancellationToken cancellationToken = default);

    /// <summary>Executes the pipeline and returns the result as a Base64 string.</summary>
    Task<string> ToBase64Async(CancellationToken cancellationToken = default);

    /// <summary>Executes the pipeline and returns the result as a <see cref="Stream"/>.</summary>
    Task<Stream> ToStreamAsync(CancellationToken cancellationToken = default);
}
