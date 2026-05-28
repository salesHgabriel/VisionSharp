using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using VisionSharp.Abstractions.Enums;
using VisionSharp.Abstractions.Interfaces;

// Aliases to resolve naming conflicts with SixLabors namespaces
using IOPath = System.IO.Path;
using ISResizeMode = SixLabors.ImageSharp.Processing.ResizeMode;
using ISResizeOptions = SixLabors.ImageSharp.Processing.ResizeOptions;
using ISEncoder = SixLabors.ImageSharp.Formats.IImageEncoder;
using VSResizeOptions = VisionSharp.Abstractions.Options.ResizeOptions;
using VSDrawingOptions = VisionSharp.Abstractions.Options.DrawingOptions;
using VSTextWatermarkOptions = VisionSharp.Abstractions.Options.TextWatermarkOptions;
using VSImageProcessingException = VisionSharp.Core.Exceptions.ImageProcessingException;
using VSWatermarkException = VisionSharp.Core.Exceptions.WatermarkException;
using VSInvalidResizeException = VisionSharp.Core.Exceptions.InvalidResizeOperationException;

namespace VisionSharp.Core.Engines;

/// <summary>
/// Image engine backed by <a href="https://github.com/SixLabors/ImageSharp">SixLabors.ImageSharp</a>.
/// This is the default engine shipped with VisionSharp.
/// </summary>
public sealed class ImageSharpEngine : IImageEngine
{
    private Image<Rgba32>? _image;
    private ImageFormat _outputFormat = ImageFormat.Jpeg;
    private int _outputQuality = 90;
    private bool _disposed;

    // ── IImageEngine properties ───────────────────────────────────────────────

    /// <inheritdoc/>
    public int Width => EnsureLoaded().Width;

    /// <inheritdoc/>
    public int Height => EnsureLoaded().Height;

    // ── Loading ───────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task LoadFromPathAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        if (!File.Exists(path))
            throw new FileNotFoundException($"Image file not found: {path}", path);

        DisposeImage();
        _image = await Image.LoadAsync<Rgba32>(path, cancellationToken).ConfigureAwait(false);
        InferOutputFormat(IOPath.GetExtension(path));
    }

    /// <inheritdoc/>
    public async Task LoadFromStreamAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        DisposeImage();
        _image = await Image.LoadAsync<Rgba32>(stream, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task LoadFromBytesAsync(byte[] bytes, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        using var ms = new MemoryStream(bytes, writable: false);
        await LoadFromStreamAsync(ms, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task LoadFromBase64Async(string base64, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(base64);
        var bytes = Convert.FromBase64String(base64);
        await LoadFromBytesAsync(bytes, cancellationToken).ConfigureAwait(false);
    }

    // ── Geometry ──────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public void ApplyResize(int width, int height, VSResizeOptions options)
    {
        if (width <= 0 || height <= 0)
            throw VSInvalidResizeException.NegativeOrZeroDimension(width, height);

        var img = EnsureLoaded();

        if (!options.AllowUpscale && img.Width <= width && img.Height <= height)
            return;

        var resizeMode = options.Mode switch
        {
            Abstractions.Enums.ResizeMode.Max  => ISResizeMode.Max,
            Abstractions.Enums.ResizeMode.Crop => ISResizeMode.Crop,
            Abstractions.Enums.ResizeMode.Min  => ISResizeMode.Min,
            Abstractions.Enums.ResizeMode.Pad  => ISResizeMode.Pad,
            _                                  => ISResizeMode.Stretch,
        };

        img.Mutate(ctx => ctx.Resize(new ISResizeOptions
        {
            Size = new Size(width, height),
            Mode = resizeMode,
            Sampler = KnownResamplers.Lanczos3
        }));
    }

    /// <inheritdoc/>
    public void ApplyCover(int width, int height, bool onlyIfLarger = false)
    {
        var img = EnsureLoaded();
        if (onlyIfLarger && img.Width <= width && img.Height <= height) return;

        img.Mutate(ctx => ctx.Resize(new ISResizeOptions
        {
            Size = new Size(width, height),
            Mode = ISResizeMode.Crop,
            Sampler = KnownResamplers.Lanczos3,
            Position = AnchorPositionMode.Center
        }));
    }

    /// <inheritdoc/>
    public void ApplyContain(int width, int height, bool onlyIfLarger = false)
    {
        var img = EnsureLoaded();
        if (onlyIfLarger && img.Width <= width && img.Height <= height) return;

        img.Mutate(ctx => ctx.Resize(new ISResizeOptions
        {
            Size = new Size(width, height),
            Mode = ISResizeMode.Max,
            Sampler = KnownResamplers.Lanczos3
        }));
    }

    /// <inheritdoc/>
    public void ApplyCrop(int x, int y, int width, int height)
    {
        var img = EnsureLoaded();

        // Negative width/height signals center crop
        if (x == 0 && y == 0 && width <= 0 && height <= 0)
        {
            int cw = -width;
            int ch = -height;
            int cx = (img.Width - cw) / 2;
            int cy = (img.Height - ch) / 2;
            img.Mutate(ctx => ctx.Crop(new Rectangle(cx, cy, cw, ch)));
            return;
        }

        int safeX = Math.Clamp(x, 0, img.Width - 1);
        int safeY = Math.Clamp(y, 0, img.Height - 1);
        int safeW = Math.Min(width, img.Width - safeX);
        int safeH = Math.Min(height, img.Height - safeY);

        img.Mutate(ctx => ctx.Crop(new Rectangle(safeX, safeY, safeW, safeH)));
    }

    /// <inheritdoc/>
    public void ApplyResizeCanvas(int width, int height, string backgroundColor)
    {
        var img = EnsureLoaded();
        var color = ParseColor(backgroundColor);
        img.Mutate(ctx => ctx.Pad(width, height, color));
    }

    // ── Effects ───────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public void ApplyBlur(int radius)
    {
        if (radius <= 0) return;
        EnsureLoaded().Mutate(ctx => ctx.BoxBlur(radius));
    }

    /// <inheritdoc/>
    public void ApplyGaussianBlur(float sigma)
    {
        if (sigma <= 0f) return;
        EnsureLoaded().Mutate(ctx => ctx.GaussianBlur(sigma));
    }

    /// <inheritdoc/>
    public void ApplyBrightness(float value)
        => EnsureLoaded().Mutate(ctx => ctx.Brightness(1f + value));

    /// <inheritdoc/>
    public void ApplyContrast(float value)
        => EnsureLoaded().Mutate(ctx => ctx.Contrast(1f + value));

    /// <inheritdoc/>
    public void ApplyGrayscale()
        => EnsureLoaded().Mutate(ctx => ctx.Grayscale());

    /// <inheritdoc/>
    public void ApplySepia()
        => EnsureLoaded().Mutate(ctx => ctx.Sepia());

    /// <inheritdoc/>
    public void ApplySharpen()
        // GaussianSharpen is the sharpening implementation available in ImageSharp
        => EnsureLoaded().Mutate(ctx => ctx.GaussianSharpen());

    /// <inheritdoc/>
    public void ApplyPixelate(int size)
    {
        if (size <= 1) return;
        EnsureLoaded().Mutate(ctx => ctx.Pixelate(size));
    }

    /// <inheritdoc/>
    public void ApplyRotate(float degrees)
        => EnsureLoaded().Mutate(ctx => ctx.Rotate(degrees));

    /// <inheritdoc/>
    public void ApplyFlipHorizontal()
        => EnsureLoaded().Mutate(ctx => ctx.Flip(FlipMode.Horizontal));

    /// <inheritdoc/>
    public void ApplyFlipVertical()
        => EnsureLoaded().Mutate(ctx => ctx.Flip(FlipMode.Vertical));

    /// <inheritdoc/>
    public void ApplyOpacity(float value)
        => EnsureLoaded().Mutate(ctx => ctx.Opacity(Math.Clamp(value, 0f, 1f)));

    // ── Watermark ─────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task ApplyWatermarkAsync(
        string imagePath,
        WatermarkPosition position,
        float opacity,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(imagePath))
            throw new VSWatermarkException($"Watermark image not found: {imagePath}");

        var img = EnsureLoaded();

        using var watermark = await Image.LoadAsync<Rgba32>(imagePath, cancellationToken).ConfigureAwait(false);
        var (posX, posY) = ComputeWatermarkPosition(img.Width, img.Height, watermark.Width, watermark.Height, position);
        img.Mutate(ctx => ctx.DrawImage(watermark, new Point(posX, posY), Math.Clamp(opacity, 0f, 1f)));
    }

    /// <inheritdoc/>
    public void ApplyWatermarkText(string text, VSTextWatermarkOptions options)
    {
        var img = EnsureLoaded();

        var font = GetFont(options.FontFamily, options.FontSize, options.Bold, options.Italic);
        var rawColor = ParseColor(options.Color);
        var color = rawColor.WithAlpha(Math.Clamp(options.Opacity, 0f, 1f));

        var textOpts = new RichTextOptions(font) { Dpi = 96 };
        var textSize = TextMeasurer.MeasureSize(text, textOpts);

        var (x, y) = ComputeWatermarkPosition(
            img.Width, img.Height,
            (int)textSize.Width, (int)textSize.Height,
            options.Position, options.PaddingX, options.PaddingY);

        img.Mutate(ctx => ctx.DrawText(text, font, color, new PointF(x, y)));
    }

    // ── Drawing ───────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public void ApplyDrawLine(float x1, float y1, float x2, float y2, VSDrawingOptions options)
        => EnsureLoaded().Mutate(ctx => ctx.DrawLine(BuildPen(options), new PointF(x1, y1), new PointF(x2, y2)));

    /// <inheritdoc/>
    public void ApplyDrawRectangle(float x, float y, float width, float height, VSDrawingOptions options)
    {
        var rect = new RectangularPolygon(x, y, width, height);
        EnsureLoaded().Mutate(ctx =>
        {
            if (HasFill(options)) ctx.Fill(BuildBrush(options), rect);
            if (options.StrokeWidth > 0) ctx.Draw(BuildPen(options), rect);
        });
    }

    /// <inheritdoc/>
    public void ApplyDrawCircle(float cx, float cy, float radius, VSDrawingOptions options)
    {
        var circle = new EllipsePolygon(cx, cy, radius * 2, radius * 2);
        EnsureLoaded().Mutate(ctx =>
        {
            if (HasFill(options)) ctx.Fill(BuildBrush(options), circle);
            if (options.StrokeWidth > 0) ctx.Draw(BuildPen(options), circle);
        });
    }

    /// <inheritdoc/>
    public void ApplyDrawPolygon(IEnumerable<(float X, float Y)> vertices, VSDrawingOptions options)
    {
        var points = vertices.Select(v => new PointF(v.X, v.Y)).ToArray();
        if (points.Length < 3) return;

        var polygon = new Polygon(new LinearLineSegment(points));
        EnsureLoaded().Mutate(ctx =>
        {
            if (HasFill(options)) ctx.Fill(BuildBrush(options), polygon);
            if (options.StrokeWidth > 0) ctx.Draw(BuildPen(options), polygon);
        });
    }

    /// <inheritdoc/>
    public void ApplyDrawText(string text, float x, float y, VSDrawingOptions options)
    {
        var font = GetFont(options.FontFamily, options.FontSize, bold: false, italic: false);
        var color = ParseColor(options.StrokeColor);
        EnsureLoaded().Mutate(ctx => ctx.DrawText(text, font, color, new PointF(x, y)));
    }

    /// <inheritdoc/>
    public async Task ApplyImageInsideShapeAsync(
        string imagePath,
        ShapeType shapeType,
        float x, float y,
        float width, float height,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(imagePath))
            throw new VSImageProcessingException("AddImageInsideShape", $"Source image not found: {imagePath}");

        var baseImg = EnsureLoaded();
        using var sourceImg = await Image.LoadAsync<Rgba32>(imagePath, cancellationToken).ConfigureAwait(false);
        sourceImg.Mutate(ctx => ctx.Resize((int)width, (int)height));

        using var mask = new Image<Rgba32>((int)width, (int)height, Color.Transparent);
        mask.Mutate(ctx =>
        {
            IPath shape = shapeType switch
            {
                ShapeType.Circle  => new EllipsePolygon(width / 2f, height / 2f, width, height),
                ShapeType.Ellipse => new EllipsePolygon(width / 2f, height / 2f, width, height),
                _                 => new RectangularPolygon(0, 0, width, height),
            };
            ctx.Fill(Color.White, shape);
        });

        sourceImg.Mutate(ctx => ctx.ApplyProcessor(new AlphaMaskProcessor(mask)));
        baseImg.Mutate(ctx => ctx.DrawImage(sourceImg, new Point((int)x, (int)y), 1f));
    }

    // ── Format ────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public void SetOutputFormat(ImageFormat format, int quality)
    {
        _outputFormat = format;
        _outputQuality = Math.Clamp(quality, 1, 100);
    }

    // ── Output ────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public async Task SaveToPathAsync(string path, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        var dir = IOPath.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        var img = EnsureLoaded();
        var encoder = ResolveEncoder(path);
        await img.SaveAsync(path, encoder, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public async Task<byte[]> GetBytesAsync(CancellationToken cancellationToken = default)
    {
        using var ms = new MemoryStream();
        await GetStreamInternalAsync(ms, cancellationToken).ConfigureAwait(false);
        return ms.ToArray();
    }

    /// <inheritdoc/>
    public async Task<Stream> GetStreamAsync(CancellationToken cancellationToken = default)
    {
        var ms = new MemoryStream();
        await GetStreamInternalAsync(ms, cancellationToken).ConfigureAwait(false);
        ms.Position = 0;
        return ms;
    }

    /// <inheritdoc/>
    public async Task<string> GetBase64Async(CancellationToken cancellationToken = default)
    {
        var bytes = await GetBytesAsync(cancellationToken).ConfigureAwait(false);
        return Convert.ToBase64String(bytes);
    }

    /// <inheritdoc/>
    public int GetWidth() => EnsureLoaded().Width;

    /// <inheritdoc/>
    public int GetHeight() => EnsureLoaded().Height;

    // ── Disposal ──────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            DisposeImage();
            _disposed = true;
        }
        return ValueTask.CompletedTask;
    }

    // ── Private helpers ───────────────────────────────────────────────────────

    private Image<Rgba32> EnsureLoaded()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _image ?? throw new InvalidOperationException(
            "No image loaded. Call a Load method before applying operations.");
    }

    private void DisposeImage()
    {
        _image?.Dispose();
        _image = null;
    }

    private void InferOutputFormat(string extension)
    {
        _outputFormat = extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => ImageFormat.Jpeg,
            ".png"            => ImageFormat.Png,
            ".webp"           => ImageFormat.Webp,
            ".gif"            => ImageFormat.Gif,
            ".bmp"            => ImageFormat.Bmp,
            _                 => ImageFormat.Jpeg
        };
    }

    private ISEncoder ResolveEncoder(string? path = null)
    {
        var fmt = _outputFormat;
        if (fmt == ImageFormat.Original && path is not null)
        {
            InferOutputFormat(IOPath.GetExtension(path));
            fmt = _outputFormat;
        }

        return fmt switch
        {
            ImageFormat.Png  => new PngEncoder(),
            ImageFormat.Webp => new WebpEncoder { Quality = _outputQuality },
            ImageFormat.Gif  => new GifEncoder(),
            ImageFormat.Bmp  => new BmpEncoder(),
            _                => new JpegEncoder { Quality = _outputQuality }
        };
    }

    private async Task GetStreamInternalAsync(MemoryStream ms, CancellationToken cancellationToken)
    {
        var img = EnsureLoaded();
        await img.SaveAsync(ms, ResolveEncoder(), cancellationToken).ConfigureAwait(false);
    }

    private static Font GetFont(string familyName, float size, bool bold, bool italic)
    {
        FontStyle style = (bold, italic) switch
        {
            (true, true)  => FontStyle.BoldItalic,
            (true, false) => FontStyle.Bold,
            (false, true) => FontStyle.Italic,
            _             => FontStyle.Regular
        };

        if (SystemFonts.TryGet(familyName, out FontFamily family))
            return family.CreateFont(size, style);

        // Fallback to any available system font
        var fallback = SystemFonts.Families.FirstOrDefault();
        if (fallback != default)
            return fallback.CreateFont(size, style);

        // Last resort: use embedded font (ImageSharp bundles none, so create minimal)
        throw new InvalidOperationException(
            $"No system fonts available. Install at least one font, or provide a custom font file.");
    }

    private static (int X, int Y) ComputeWatermarkPosition(
        int baseWidth, int baseHeight,
        int markWidth, int markHeight,
        WatermarkPosition position,
        int paddingX = 10, int paddingY = 10)
    {
        return position switch
        {
            WatermarkPosition.TopLeft      => (paddingX, paddingY),
            WatermarkPosition.TopCenter    => ((baseWidth - markWidth) / 2, paddingY),
            WatermarkPosition.TopRight     => (baseWidth - markWidth - paddingX, paddingY),
            WatermarkPosition.MiddleLeft   => (paddingX, (baseHeight - markHeight) / 2),
            WatermarkPosition.Center       => ((baseWidth - markWidth) / 2, (baseHeight - markHeight) / 2),
            WatermarkPosition.MiddleRight  => (baseWidth - markWidth - paddingX, (baseHeight - markHeight) / 2),
            WatermarkPosition.BottomLeft   => (paddingX, baseHeight - markHeight - paddingY),
            WatermarkPosition.BottomCenter => ((baseWidth - markWidth) / 2, baseHeight - markHeight - paddingY),
            WatermarkPosition.BottomRight  => (baseWidth - markWidth - paddingX, baseHeight - markHeight - paddingY),
            _                              => (paddingX, paddingY)
        };
    }

    private static Color ParseColor(string hex)
    {
        if (Color.TryParse(hex, out var c)) return c;
        return Color.Black;
    }

    private static Pen BuildPen(VSDrawingOptions opts)
        => Pens.Solid(ParseColor(opts.StrokeColor).WithAlpha(opts.Opacity), opts.StrokeWidth);

    private static SolidBrush BuildBrush(VSDrawingOptions opts)
        => Brushes.Solid(ParseColor(opts.FillColor).WithAlpha(opts.Opacity));

    private static bool HasFill(VSDrawingOptions opts)
    {
        if (Color.TryParse(opts.FillColor, out var c))
            return c.ToPixel<Rgba32>().A > 0;
        return false;
    }
}
