using FluentAssertions;
using Moq;
using VisionSharp.Abstractions.Enums;
using VisionSharp.Abstractions.Interfaces;
using VisionSharp.Abstractions.Options;
using VisionSharp.Core;
using VisionSharp.Extensions;

namespace VisionSharp.UnitTests;

public sealed class ImageBuilderTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Creates an <see cref="ImageBuilder"/> wired to a mock engine.</summary>
    private static (ImageBuilder builder, Mock<IImageEngine> engineMock) CreateBuilder()
    {
        var mock = new Mock<IImageEngine>();
        mock.Setup(e => e.Width).Returns(800);
        mock.Setup(e => e.Height).Returns(600);
        mock.Setup(e => e.GetWidth()).Returns(800);
        mock.Setup(e => e.GetHeight()).Returns(600);
        mock.Setup(e => e.GetBytesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<byte>());
        mock.Setup(e => e.GetBase64Async(It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty);
        mock.Setup(e => e.GetStreamAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new MemoryStream());
        mock.Setup(e => e.LoadFromPathAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        mock.Setup(e => e.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var builder = new ImageBuilder(
            () => mock.Object,
            (e, ct) => e.LoadFromPathAsync("test.jpg", ct));

        return (builder, mock);
    }

    // ── Resize ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Resize_CallsApplyResize_WithCorrectDimensions()
    {
        var (builder, mock) = CreateBuilder();

        await builder.Resize(400, 300).ToBytesAsync();

        mock.Verify(e => e.ApplyResize(400, 300, It.IsAny<ResizeOptions>()), Times.Once);
    }

    [Fact]
    public async Task ResizeDown_SetsAllowUpscaleToFalse()
    {
        var (builder, mock) = CreateBuilder();

        await builder.ResizeDown(400, 300).ToBytesAsync();

        mock.Verify(e => e.ApplyResize(
            400, 300,
            It.Is<ResizeOptions>(o => !o.AllowUpscale)),
            Times.Once);
    }

    [Fact]
    public async Task Scale_ComputesDerivedDimensions()
    {
        var (builder, mock) = CreateBuilder();

        // Engine reports 800×600; scaling by 0.5 → 400×300
        await builder.Scale(0.5).ToBytesAsync();

        mock.Verify(e => e.ApplyResize(400, 300, It.IsAny<ResizeOptions>()), Times.Once);
    }

    [Fact]
    public void Scale_ThrowsForNegativeFactor()
    {
        var (builder, _) = CreateBuilder();

        var act = () => builder.Scale(-1);

        act.Should().Throw<Core.Exceptions.InvalidResizeOperationException>();
    }

    // ── Cover / Contain ───────────────────────────────────────────────────────

    [Fact]
    public async Task Cover_CallsApplyCover()
    {
        var (builder, mock) = CreateBuilder();

        await builder.Cover(500, 500).ToBytesAsync();

        mock.Verify(e => e.ApplyCover(500, 500, false), Times.Once);
    }

    [Fact]
    public async Task CoverDown_CallsApplyCoverWithOnlyIfLarger()
    {
        var (builder, mock) = CreateBuilder();

        await builder.CoverDown(500, 500).ToBytesAsync();

        mock.Verify(e => e.ApplyCover(500, 500, true), Times.Once);
    }

    [Fact]
    public async Task Contain_CallsApplyContain()
    {
        var (builder, mock) = CreateBuilder();

        await builder.Contain(300, 300).ToBytesAsync();

        mock.Verify(e => e.ApplyContain(300, 300, false), Times.Once);
    }

    // ── Crop ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Crop_WithXY_CallsApplyCropWithCorrectArgs()
    {
        var (builder, mock) = CreateBuilder();

        await builder.Crop(10, 20, 200, 150).ToBytesAsync();

        mock.Verify(e => e.ApplyCrop(10, 20, 200, 150), Times.Once);
    }

    // ── Effects ───────────────────────────────────────────────────────────────

    [Fact]
    public async Task Blur_CallsApplyBlur()
    {
        var (builder, mock) = CreateBuilder();

        await builder.Blur(5).ToBytesAsync();

        mock.Verify(e => e.ApplyBlur(5), Times.Once);
    }

    [Fact]
    public async Task GaussianBlur_CallsApplyGaussianBlur()
    {
        var (builder, mock) = CreateBuilder();

        await builder.GaussianBlur(2.5f).ToBytesAsync();

        mock.Verify(e => e.ApplyGaussianBlur(2.5f), Times.Once);
    }

    [Fact]
    public async Task Grayscale_CallsApplyGrayscale()
    {
        var (builder, mock) = CreateBuilder();

        await builder.Grayscale().ToBytesAsync();

        mock.Verify(e => e.ApplyGrayscale(), Times.Once);
    }

    [Fact]
    public async Task Sepia_CallsApplySepia()
    {
        var (builder, mock) = CreateBuilder();

        await builder.Sepia().ToBytesAsync();

        mock.Verify(e => e.ApplySepia(), Times.Once);
    }

    [Fact]
    public async Task Sharpen_CallsApplySharpen()
    {
        var (builder, mock) = CreateBuilder();

        await builder.Sharpen().ToBytesAsync();

        mock.Verify(e => e.ApplySharpen(), Times.Once);
    }

    [Fact]
    public async Task Pixelate_CallsApplyPixelate()
    {
        var (builder, mock) = CreateBuilder();

        await builder.Pixelate(10).ToBytesAsync();

        mock.Verify(e => e.ApplyPixelate(10), Times.Once);
    }

    [Fact]
    public async Task Rotate_CallsApplyRotate()
    {
        var (builder, mock) = CreateBuilder();

        await builder.Rotate(45f).ToBytesAsync();

        mock.Verify(e => e.ApplyRotate(45f), Times.Once);
    }

    [Fact]
    public async Task FlipHorizontal_CallsApplyFlipHorizontal()
    {
        var (builder, mock) = CreateBuilder();

        await builder.FlipHorizontal().ToBytesAsync();

        mock.Verify(e => e.ApplyFlipHorizontal(), Times.Once);
    }

    [Fact]
    public async Task FlipVertical_CallsApplyFlipVertical()
    {
        var (builder, mock) = CreateBuilder();

        await builder.FlipVertical().ToBytesAsync();

        mock.Verify(e => e.ApplyFlipVertical(), Times.Once);
    }

    [Fact]
    public async Task Opacity_CallsApplyOpacity()
    {
        var (builder, mock) = CreateBuilder();

        await builder.Opacity(0.5f).ToBytesAsync();

        mock.Verify(e => e.ApplyOpacity(0.5f), Times.Once);
    }

    // ── Format ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task ToJpeg_SetsOutputFormatOnEngine()
    {
        var (builder, mock) = CreateBuilder();

        await builder.ToJpeg(85).ToBytesAsync();

        mock.Verify(e => e.SetOutputFormat(ImageFormat.Jpeg, 85), Times.Once);
    }

    [Fact]
    public async Task ToPng_SetsOutputFormatPng()
    {
        var (builder, mock) = CreateBuilder();

        await builder.ToPng().ToBytesAsync();

        mock.Verify(e => e.SetOutputFormat(ImageFormat.Png, It.IsAny<int>()), Times.Once);
    }

    [Fact]
    public async Task ToWebp_SetsOutputFormatWebp()
    {
        var (builder, mock) = CreateBuilder();

        await builder.ToWebp(75).ToBytesAsync();

        mock.Verify(e => e.SetOutputFormat(ImageFormat.Webp, 75), Times.Once);
    }

    // ── Pipeline ordering ─────────────────────────────────────────────────────

    [Fact]
    public async Task Pipeline_ExecutesOperationsInOrder()
    {
        var callOrder = new List<string>();
        var mock = new Mock<IImageEngine>(MockBehavior.Loose);
        mock.Setup(e => e.GetWidth()).Returns(400);
        mock.Setup(e => e.GetHeight()).Returns(300);
        mock.Setup(e => e.GetBytesAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);
        mock.Setup(e => e.LoadFromPathAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        mock.Setup(e => e.DisposeAsync()).Returns(ValueTask.CompletedTask);
        mock.Setup(e => e.ApplyResize(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ResizeOptions>()))
            .Callback<int, int, ResizeOptions>((_, _, _) => callOrder.Add("Resize"));
        mock.Setup(e => e.ApplyGrayscale()).Callback(() => callOrder.Add("Grayscale"));
        mock.Setup(e => e.ApplySharpen()).Callback(() => callOrder.Add("Sharpen"));

        var builder = new ImageBuilder(() => mock.Object, (e, ct) => e.LoadFromPathAsync("t.jpg", ct));

        await builder.Resize(400, 300).Grayscale().Sharpen().ToBytesAsync();

        callOrder.Should().Equal("Resize", "Grayscale", "Sharpen");
    }

    // ── Watermark ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task Watermark_CallsApplyWatermarkAsync()
    {
        var (builder, mock) = CreateBuilder();
        mock.Setup(e => e.ApplyWatermarkAsync(It.IsAny<string>(), It.IsAny<WatermarkPosition>(), It.IsAny<float>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await builder.Watermark("logo.png", WatermarkPosition.BottomRight, 0.8f).ToBytesAsync();

        mock.Verify(e => e.ApplyWatermarkAsync("logo.png", WatermarkPosition.BottomRight, 0.8f, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── When extension ────────────────────────────────────────────────────────

    [Fact]
    public async Task When_True_AppliesTransformation()
    {
        var (builder, mock) = CreateBuilder();

        await builder.When(true, b => b.Grayscale()).ToBytesAsync();

        mock.Verify(e => e.ApplyGrayscale(), Times.Once);
    }

    [Fact]
    public async Task When_False_SkipsTransformation()
    {
        var (builder, mock) = CreateBuilder();

        await builder.When(false, b => b.Grayscale()).ToBytesAsync();

        mock.Verify(e => e.ApplyGrayscale(), Times.Never);
    }
}
