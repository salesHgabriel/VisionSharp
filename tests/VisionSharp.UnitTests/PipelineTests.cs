using VisionSharp.Abstractions.Interfaces;
using VisionSharp.Abstractions.Options;
using VisionSharp.Core;

namespace VisionSharp.UnitTests;

public sealed class PipelineTests
{
    private static (ImageBuilder Builder, Mock<IImageEngine> Mock) BuilderWithMock()
    {
        var m = new Mock<IImageEngine>(MockBehavior.Loose);
        m.Setup(e => e.GetWidth()).Returns(100);
        m.Setup(e => e.GetHeight()).Returns(100);
        m.Setup(e => e.GetBytesAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);
        m.Setup(e => e.LoadFromPathAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        m.Setup(e => e.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var b = new ImageBuilder(() => m.Object, (e, ct) => e.LoadFromPathAsync("t.jpg", ct));
        return (b, m);
    }

    [Fact]
    public async Task EmptyPipeline_ReturnsBytes_WithoutError()
    {
        var (builder, _) = BuilderWithMock();

        var bytes = await builder.ToBytesAsync();

        bytes.Should().NotBeNull();
    }

    [Fact]
    public async Task MultipleSameOperation_ExecutesAllInstances()
    {
        var (builder, mock) = BuilderWithMock();
        mock.Setup(e => e.ApplyBlur(It.IsAny<int>()));

        await builder.Blur(1).Blur(2).Blur(3).ToBytesAsync();

        mock.Verify(e => e.ApplyBlur(It.IsAny<int>()), Times.Exactly(3));
    }

    [Fact]
    public async Task Pipeline_DisposesEngineAfterExecution()
    {
        var (builder, mock) = BuilderWithMock();

        await builder.ToBytesAsync();

        mock.Verify(e => e.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task Pipeline_DisposesEngineEvenOnError()
    {
        var m = new Mock<IImageEngine>(MockBehavior.Loose);
        m.Setup(e => e.LoadFromPathAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FileNotFoundException("test.jpg"));
        m.Setup(e => e.DisposeAsync()).Returns(ValueTask.CompletedTask);

        var builder = new ImageBuilder(() => m.Object, (e, ct) => e.LoadFromPathAsync("t.jpg", ct));

        var act = async () => await builder.ToBytesAsync();

        await act.Should().ThrowAsync<FileNotFoundException>();
        m.Verify(e => e.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task Pipeline_WrapsEngineExceptionsInImageProcessingException()
    {
        var m = new Mock<IImageEngine>(MockBehavior.Loose);
        m.Setup(e => e.LoadFromPathAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        m.Setup(e => e.DisposeAsync()).Returns(ValueTask.CompletedTask);
        m.Setup(e => e.GetBytesAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);
        m.Setup(e => e.GetWidth()).Returns(100);
        m.Setup(e => e.GetHeight()).Returns(100);
        m.Setup(e => e.ApplyResize(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<ResizeOptions>()))
            .Throws(new InvalidOperationException("engine failure"));

        var builder = new ImageBuilder(() => m.Object, (e, ct) => e.LoadFromPathAsync("t.jpg", ct));

        var act = async () => await builder.Resize(100, 100).ToBytesAsync();

        await act.Should().ThrowAsync<Core.Exceptions.ImageProcessingException>()
            .WithMessage("*Resize*");
    }

    [Fact]
    public async Task Base64_ReturnsNonEmptyString_ForValidImage()
    {
        var m = new Mock<IImageEngine>(MockBehavior.Loose);
        m.Setup(e => e.LoadFromBytesAsync(It.IsAny<byte[]>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        m.Setup(e => e.DisposeAsync()).Returns(ValueTask.CompletedTask);
        m.Setup(e => e.GetWidth()).Returns(1);
        m.Setup(e => e.GetHeight()).Returns(1);
        m.Setup(e => e.GetBase64Async(It.IsAny<CancellationToken>())).ReturnsAsync("abc123==");

        var bytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xD9 }; // minimal JPEG
        var builder = new ImageBuilder(() => m.Object, (e, ct) => e.LoadFromBytesAsync(bytes, ct));

        var base64 = await builder.ToBase64Async();

        base64.Should().NotBeNullOrEmpty();
    }
}
