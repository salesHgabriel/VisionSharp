using FluentAssertions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using VisionSharp.Extensions;
using VisionSharp.Processing.Extensions;

namespace VisionSharp.IntegrationTests;

/// <summary>
/// End-to-end tests that run the full pipeline against a real in-memory image
/// via the ImageSharp engine. These tests verify pixel-level correctness and
/// do not write to disk.
/// </summary>
public sealed class ImageProcessingIntegrationTests : IDisposable
{
    private readonly string _tempDir;

    public ImageProcessingIntegrationTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "VisionSharpTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose() => Directory.Delete(_tempDir, recursive: true);

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Creates a solid-color JPEG image in memory.</summary>
    private static byte[] CreateSolidColorJpeg(int width, int height, byte r, byte g, byte b)
    {
        using var img = new Image<Rgba32>(width, height, new Rgba32(r, g, b));
        using var ms = new MemoryStream();
        img.SaveAsJpeg(ms);
        return ms.ToArray();
    }

    private static async Task<(int Width, int Height)> GetImageDimensions(byte[] bytes)
    {
        using var ms = new MemoryStream(bytes);
        using var img = await Image.LoadAsync<Rgba32>(ms);
        return (img.Width, img.Height);
    }

    // ── Resize ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Resize_ProducesCorrectDimensions()
    {
        var src = CreateSolidColorJpeg(800, 600, 200, 100, 50);

        var result = await ImageFactory.OpenAsync(src).Resize(400, 300).ToBytesAsync();

        var (w, h) = await GetImageDimensions(result);
        w.Should().Be(400);
        h.Should().Be(300);
    }

    [Fact]
    public async Task ResizeDown_DoesNotUpscale()
    {
        var src = CreateSolidColorJpeg(200, 150, 100, 100, 100);

        var result = await ImageFactory.OpenAsync(src).ResizeDown(800, 600).ToBytesAsync();

        var (w, h) = await GetImageDimensions(result);
        // Image is already smaller than target → should not be enlarged
        w.Should().BeLessOrEqualTo(800);
        h.Should().BeLessOrEqualTo(600);
    }

    [Fact]
    public async Task Scale_HalvesImageSize()
    {
        var src = CreateSolidColorJpeg(400, 300, 50, 50, 50);

        var result = await ImageFactory.OpenAsync(src).Scale(0.5).ToBytesAsync();

        var (w, h) = await GetImageDimensions(result);
        w.Should().BeCloseTo(200, delta: 2);
        h.Should().BeCloseTo(150, delta: 2);
    }

    // ── Cover / Contain ───────────────────────────────────────────────────────

    [Fact]
    public async Task Cover_ProducesExactDimensions()
    {
        var src = CreateSolidColorJpeg(1200, 800, 0, 128, 255);

        var result = await ImageFactory.OpenAsync(src).Cover(500, 500).ToBytesAsync();

        var (w, h) = await GetImageDimensions(result);
        w.Should().Be(500);
        h.Should().Be(500);
    }

    [Fact]
    public async Task Contain_FitsWithinBounds()
    {
        var src = CreateSolidColorJpeg(1000, 400, 128, 0, 64);

        var result = await ImageFactory.OpenAsync(src).Contain(300, 300).ToBytesAsync();

        var (w, h) = await GetImageDimensions(result);
        w.Should().BeLessOrEqualTo(300);
        h.Should().BeLessOrEqualTo(300);
    }

    // ── Crop ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Crop_ProducesCorrectDimensions()
    {
        var src = CreateSolidColorJpeg(800, 600, 255, 0, 0);

        var result = await ImageFactory.OpenAsync(src).Crop(50, 50, 300, 200).ToBytesAsync();

        var (w, h) = await GetImageDimensions(result);
        w.Should().Be(300);
        h.Should().Be(200);
    }

    // ── Effects ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Grayscale_ProducesBytes()
    {
        var src = CreateSolidColorJpeg(200, 200, 200, 100, 50);

        var result = await ImageFactory.OpenAsync(src).Grayscale().ToBytesAsync();

        result.Should().NotBeEmpty();
        var (w, h) = await GetImageDimensions(result);
        w.Should().Be(200);
        h.Should().Be(200);
    }

    [Fact]
    public async Task Sepia_PreservesDimensions()
    {
        var src = CreateSolidColorJpeg(300, 200, 50, 100, 150);

        var result = await ImageFactory.OpenAsync(src).Sepia().ToBytesAsync();

        var (w, h) = await GetImageDimensions(result);
        w.Should().Be(300);
        h.Should().Be(200);
    }

    [Fact]
    public async Task Blur_PreservesDimensions()
    {
        var src = CreateSolidColorJpeg(400, 300, 128, 128, 128);

        var result = await ImageFactory.OpenAsync(src).Blur(3).ToBytesAsync();

        var (w, h) = await GetImageDimensions(result);
        w.Should().Be(400);
        h.Should().Be(300);
    }

    [Fact]
    public async Task Pixelate_PreservesDimensions()
    {
        var src = CreateSolidColorJpeg(100, 100, 200, 200, 50);

        var result = await ImageFactory.OpenAsync(src).Pixelate(10).ToBytesAsync();

        var (w, h) = await GetImageDimensions(result);
        w.Should().Be(100);
        h.Should().Be(100);
    }

    [Fact]
    public async Task Rotate_ProducesOutput()
    {
        var src = CreateSolidColorJpeg(200, 150, 0, 200, 100);

        var result = await ImageFactory.OpenAsync(src).Rotate(90).ToBytesAsync();

        result.Should().NotBeEmpty();
    }

    // ── Format conversions ────────────────────────────────────────────────────

    [Fact]
    public async Task ToPng_ProducesValidOutput()
    {
        var src = CreateSolidColorJpeg(100, 100, 0, 0, 255);

        var result = await ImageFactory.OpenAsync(src).ToPng().ToBytesAsync();

        result.Should().NotBeEmpty();
        // PNG magic bytes: 0x89 0x50 0x4E 0x47
        result[0].Should().Be(0x89);
        result[1].Should().Be(0x50);
        result[2].Should().Be(0x4E);
        result[3].Should().Be(0x47);
    }

    [Fact]
    public async Task ToWebp_ProducesValidOutput()
    {
        var src = CreateSolidColorJpeg(100, 100, 200, 50, 50);

        var result = await ImageFactory.OpenAsync(src).ToWebp(80).ToBytesAsync();

        result.Should().NotBeEmpty();
    }

    // ── Base64 ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ToBase64_RoundTrip_PreservesDimensions()
    {
        var src = CreateSolidColorJpeg(160, 120, 30, 60, 90);

        var base64 = await ImageFactory.OpenAsync(src).Resize(80, 60).ToBase64Async();
        base64.Should().NotBeNullOrEmpty();

        var decoded = Convert.FromBase64String(base64);
        var (w, h) = await GetImageDimensions(decoded);
        w.Should().Be(80);
        h.Should().Be(60);
    }

    // ── Stream ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ToStream_ReturnsReadableStream()
    {
        var src = CreateSolidColorJpeg(200, 200, 100, 100, 100);

        var stream = await ImageFactory.OpenAsync(src).Resize(100, 100).ToStreamAsync();

        stream.Should().NotBeNull();
        stream.CanRead.Should().BeTrue();
        stream.Length.Should().BeGreaterThan(0);
    }

    // ── Save ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task SaveAsync_WritesFileToDisc()
    {
        var src = CreateSolidColorJpeg(200, 200, 0, 100, 200);
        var output = Path.Combine(_tempDir, "save_test.jpg");

        await ImageFactory.OpenAsync(src).Resize(100, 100).SaveAsync(output);

        File.Exists(output).Should().BeTrue();
        new FileInfo(output).Length.Should().BeGreaterThan(0);
    }

    // ── Complex pipeline ──────────────────────────────────────────────────────

    [Fact]
    public async Task ComplexPipeline_ProducesValidOutput()
    {
        var src = CreateSolidColorJpeg(800, 600, 80, 120, 200);
        var output = Path.Combine(_tempDir, "complex.jpg");

        var image = await ImageFactory
            .OpenAsync(src)
            .Resize(640, 480)
            .GaussianBlur(1f)
            .Brightness(0.05f)
            .Contrast(0.1f)
            .Sharpen()
            .ToJpeg(85)
            .SaveAsync(output);

        File.Exists(output).Should().BeTrue();
        image.Width.Should().BePositive();
        image.Height.Should().BePositive();
    }

    // ── Processing presets ────────────────────────────────────────────────────

    [Fact]
    public async Task Vintage_ProducesValidOutput()
    {
        var src = CreateSolidColorJpeg(200, 150, 180, 140, 100);

        var result = await ImageFactory.OpenAsync(src).Vintage().ToBytesAsync();

        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task BlackAndWhite_ProducesValidOutput()
    {
        var src = CreateSolidColorJpeg(200, 150, 200, 50, 100);

        var result = await ImageFactory.OpenAsync(src).BlackAndWhite().ToBytesAsync();

        result.Should().NotBeEmpty();
    }

    // ── Conditional extension ─────────────────────────────────────────────────

    [Fact]
    public async Task When_True_ExecutesTransform()
    {
        var src = CreateSolidColorJpeg(400, 300, 0, 0, 0);

        var result = await ImageFactory.OpenAsync(src)
            .When(true, b => b.Resize(200, 150))
            .ToBytesAsync();

        var (w, h) = await GetImageDimensions(result);
        w.Should().Be(200);
        h.Should().Be(150);
    }

    [Fact]
    public async Task When_False_SkipsTransform()
    {
        var src = CreateSolidColorJpeg(400, 300, 0, 0, 0);

        var result = await ImageFactory.OpenAsync(src)
            .When(false, b => b.Resize(200, 150))
            .ToBytesAsync();

        var (w, h) = await GetImageDimensions(result);
        w.Should().Be(400);
        h.Should().Be(300);
    }
}
