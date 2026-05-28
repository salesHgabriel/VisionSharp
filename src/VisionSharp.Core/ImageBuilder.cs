using VisionSharp.Abstractions.Enums;
using VisionSharp.Abstractions.Interfaces;
using VisionSharp.Abstractions.Options;
using VisionSharp.Core.Exceptions;
using VisionSharp.Core.Pipeline;

namespace VisionSharp.Core;

/// <summary>
/// Fluent image builder that accumulates operations into a lazy pipeline.
/// Operations are applied in order when a terminal method is called.
/// </summary>
public sealed class ImageBuilder : IImageBuilder
{
    private readonly Func<IImageEngine> _engineFactory;
    private readonly Func<IImageEngine, CancellationToken, Task> _loader;
    private readonly List<ImageOperation> _pipeline = [];
    private ImageFormat _outputFormat = ImageFormat.Original;
    private int _outputQuality = 90;

    public ImageBuilder(Func<IImageEngine> engineFactory, Func<IImageEngine, CancellationToken, Task> loader)
    {
        _engineFactory = engineFactory;
        _loader = loader;
    }

    // ── Resize ────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public IImageBuilder Resize(int width, int height, ResizeOptions? options = null)
    {
        var opts = options ?? new ResizeOptions();
        return AddSync("Resize", e => e.ApplyResize(width, height, opts));
    }

    /// <inheritdoc/>
    public IImageBuilder ResizeDown(int width, int height)
        => AddSync("ResizeDown", e => e.ApplyResize(width, height, new ResizeOptions { AllowUpscale = false }));

    /// <inheritdoc/>
    public IImageBuilder Scale(double factor)
    {
        if (factor <= 0) throw InvalidResizeOperationException.InvalidScaleFactor(factor);
        return AddSync("Scale", e =>
        {
            int w = (int)Math.Round(e.Width * factor);
            int h = (int)Math.Round(e.Height * factor);
            e.ApplyResize(w, h, new ResizeOptions());
        });
    }

    /// <inheritdoc/>
    public IImageBuilder ScaleDown(double factor)
    {
        if (factor <= 0) throw InvalidResizeOperationException.InvalidScaleFactor(factor);
        return AddSync("ScaleDown", e =>
        {
            if (factor >= 1.0) return;
            int w = (int)Math.Round(e.Width * factor);
            int h = (int)Math.Round(e.Height * factor);
            e.ApplyResize(w, h, new ResizeOptions());
        });
    }

    // ── Cover / Contain ───────────────────────────────────────────────────────

    /// <inheritdoc/>
    public IImageBuilder Cover(int width, int height)
        => AddSync("Cover", e => e.ApplyCover(width, height));

    /// <inheritdoc/>
    public IImageBuilder CoverDown(int width, int height)
        => AddSync("CoverDown", e => e.ApplyCover(width, height, onlyIfLarger: true));

    /// <inheritdoc/>
    public IImageBuilder Contain(int width, int height)
        => AddSync("Contain", e => e.ApplyContain(width, height));

    /// <inheritdoc/>
    public IImageBuilder ContainDown(int width, int height)
        => AddSync("ContainDown", e => e.ApplyContain(width, height, onlyIfLarger: true));

    // ── Crop ──────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public IImageBuilder Crop(int width, int height)
        => AddSync("Crop", e => e.ApplyCrop(0, 0, -width, -height));

    /// <inheritdoc/>
    public IImageBuilder Crop(int x, int y, int width, int height)
        => AddSync("Crop", e => e.ApplyCrop(x, y, width, height));

    // ── Canvas ────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public IImageBuilder ResizeCanvas(int width, int height, string backgroundColor = "#00000000")
        => AddSync("ResizeCanvas", e => e.ApplyResizeCanvas(width, height, backgroundColor));

    /// <inheritdoc/>
    public IImageBuilder ResizeCanvasRelative(int widthOffset, int heightOffset)
        => AddSync("ResizeCanvasRelative", e =>
            e.ApplyResizeCanvas(e.Width + widthOffset, e.Height + heightOffset, "#00000000"));

    // ── Effects ───────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public IImageBuilder Blur(int radius)
        => AddSync("Blur", e => e.ApplyBlur(radius));

    /// <inheritdoc/>
    public IImageBuilder GaussianBlur(float sigma)
        => AddSync("GaussianBlur", e => e.ApplyGaussianBlur(sigma));

    /// <inheritdoc/>
    public IImageBuilder Brightness(float value)
        => AddSync("Brightness", e => e.ApplyBrightness(value));

    /// <inheritdoc/>
    public IImageBuilder Contrast(float value)
        => AddSync("Contrast", e => e.ApplyContrast(value));

    /// <inheritdoc/>
    public IImageBuilder Grayscale()
        => AddSync("Grayscale", e => e.ApplyGrayscale());

    /// <inheritdoc/>
    public IImageBuilder Sepia()
        => AddSync("Sepia", e => e.ApplySepia());

    /// <inheritdoc/>
    public IImageBuilder Sharpen()
        => AddSync("Sharpen", e => e.ApplySharpen());

    /// <inheritdoc/>
    public IImageBuilder Pixelate(int size)
        => AddSync("Pixelate", e => e.ApplyPixelate(size));

    /// <inheritdoc/>
    public IImageBuilder Rotate(float degrees)
        => AddSync("Rotate", e => e.ApplyRotate(degrees));

    /// <inheritdoc/>
    public IImageBuilder FlipHorizontal()
        => AddSync("FlipHorizontal", e => e.ApplyFlipHorizontal());

    /// <inheritdoc/>
    public IImageBuilder FlipVertical()
        => AddSync("FlipVertical", e => e.ApplyFlipVertical());

    /// <inheritdoc/>
    public IImageBuilder Opacity(float value)
        => AddSync("Opacity", e => e.ApplyOpacity(value));

    // ── Watermark ─────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public IImageBuilder Watermark(string imagePath, WatermarkPosition position, float opacity = 0.7f)
        => AddAsync("Watermark", (e, ct) => e.ApplyWatermarkAsync(imagePath, position, opacity, ct));

    /// <inheritdoc/>
    public IImageBuilder WatermarkText(string text, Action<TextWatermarkOptions>? configure = null)
    {
        var opts = new TextWatermarkOptions();
        configure?.Invoke(opts);
        return AddSync("WatermarkText", e => e.ApplyWatermarkText(text, opts));
    }

    // ── Drawing ───────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public IImageBuilder DrawLine(float x1, float y1, float x2, float y2, Action<DrawingOptions>? configure = null)
    {
        var opts = Build(configure);
        return AddSync("DrawLine", e => e.ApplyDrawLine(x1, y1, x2, y2, opts));
    }

    /// <inheritdoc/>
    public IImageBuilder DrawRectangle(float x, float y, float width, float height, Action<DrawingOptions>? configure = null)
    {
        var opts = Build(configure);
        return AddSync("DrawRectangle", e => e.ApplyDrawRectangle(x, y, width, height, opts));
    }

    /// <inheritdoc/>
    public IImageBuilder DrawCircle(float centerX, float centerY, float radius, Action<DrawingOptions>? configure = null)
    {
        var opts = Build(configure);
        return AddSync("DrawCircle", e => e.ApplyDrawCircle(centerX, centerY, radius, opts));
    }

    /// <inheritdoc/>
    public IImageBuilder DrawPolygon(IEnumerable<(float X, float Y)> vertices, Action<DrawingOptions>? configure = null)
    {
        var opts = Build(configure);
        var points = vertices.ToList();
        return AddSync("DrawPolygon", e => e.ApplyDrawPolygon(points, opts));
    }

    /// <inheritdoc/>
    public IImageBuilder DrawText(string text, float x, float y, Action<DrawingOptions>? configure = null)
    {
        var opts = Build(configure);
        return AddSync("DrawText", e => e.ApplyDrawText(text, x, y, opts));
    }

    /// <inheritdoc/>
    public IImageBuilder AddImageInsideShape(string imagePath, ShapeType shapeType, float x, float y, float width, float height)
        => AddAsync("AddImageInsideShape", (e, ct) => e.ApplyImageInsideShapeAsync(imagePath, shapeType, x, y, width, height, ct));

    // ── Format ────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public IImageBuilder ToJpeg(int quality = 90)
    {
        _outputFormat = ImageFormat.Jpeg;
        _outputQuality = quality;
        return this;
    }

    /// <inheritdoc/>
    public IImageBuilder ToPng()
    {
        _outputFormat = ImageFormat.Png;
        return this;
    }

    /// <inheritdoc/>
    public IImageBuilder ToWebp(int quality = 80)
    {
        _outputFormat = ImageFormat.Webp;
        _outputQuality = quality;
        return this;
    }

    /// <inheritdoc/>
    public IImageBuilder ToGif()
    {
        _outputFormat = ImageFormat.Gif;
        return this;
    }

    /// <inheritdoc/>
    public IImageBuilder ToBmp()
    {
        _outputFormat = ImageFormat.Bmp;
        return this;
    }

    // ── Terminal operations ───────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task<IImage> SaveAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var engine = await ExecutePipelineAsync(cancellationToken).ConfigureAwait(false);
        await engine.SaveToPathAsync(path, cancellationToken).ConfigureAwait(false);
        return new VisionImage(engine.GetWidth(), engine.GetHeight(), _outputFormat,
            await engine.GetBytesAsync(cancellationToken).ConfigureAwait(false));
    }

    /// <inheritdoc/>
    public async Task<byte[]> ToBytesAsync(CancellationToken cancellationToken = default)
    {
        await using var engine = await ExecutePipelineAsync(cancellationToken).ConfigureAwait(false);
        return await engine.GetBytesAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<string> ToBase64Async(CancellationToken cancellationToken = default)
    {
        await using var engine = await ExecutePipelineAsync(cancellationToken).ConfigureAwait(false);
        return await engine.GetBase64Async(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<Stream> ToStreamAsync(CancellationToken cancellationToken = default)
    {
        await using var engine = await ExecutePipelineAsync(cancellationToken).ConfigureAwait(false);
        return await engine.GetStreamAsync(cancellationToken).ConfigureAwait(false);
    }

    // ── Pipeline execution ────────────────────────────────────────────────────

    private async Task<IImageEngine> ExecutePipelineAsync(CancellationToken cancellationToken)
    {
        var engine = _engineFactory();
        try
        {
            await _loader(engine, cancellationToken).ConfigureAwait(false);

            if (_outputFormat != ImageFormat.Original)
                engine.SetOutputFormat(_outputFormat, _outputQuality);

            foreach (var op in _pipeline)
            {
                try
                {
                    await op.ExecuteAsync(engine, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    throw new ImageProcessingException(op.Name, $"Operation '{op.Name}' failed: {ex.Message}", ex);
                }
            }

            return engine;
        }
        catch
        {
            await engine.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private ImageBuilder AddSync(string name, Action<IImageEngine> action)
    {
        _pipeline.Add(ImageOperation.Sync(name, action));
        return this;
    }

    private ImageBuilder AddAsync(string name, Func<IImageEngine, CancellationToken, Task> action)
    {
        _pipeline.Add(ImageOperation.Async(name, action));
        return this;
    }

    private static DrawingOptions Build(Action<DrawingOptions>? configure)
    {
        var opts = new DrawingOptions();
        configure?.Invoke(opts);
        return opts;
    }
}
