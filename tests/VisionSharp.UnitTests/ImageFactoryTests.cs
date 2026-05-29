using FluentAssertions;
using VisionSharp.Abstractions.Interfaces;
using VisionSharp.Core;

namespace VisionSharp.UnitTests;

public sealed class ImageFactoryTests
{
    [Fact]
    public void OpenAsync_WithPath_ReturnsImageBuilder()
    {
        var builder = ImageFactory.OpenAsync("photo.jpg");

        builder.Should().NotBeNull();
        builder.Should().BeAssignableTo<IImageBuilder>();
    }

    [Fact]
    public void OpenAsync_WithStream_ReturnsImageBuilder()
    {
        using var stream = new MemoryStream();
        var builder = ImageFactory.OpenAsync(stream);

        builder.Should().NotBeNull();
        builder.Should().BeAssignableTo<IImageBuilder>();
    }

    [Fact]
    public void OpenAsync_WithBytes_ReturnsImageBuilder()
    {
        var builder = ImageFactory.OpenAsync(Array.Empty<byte>());

        builder.Should().NotBeNull();
        builder.Should().BeAssignableTo<IImageBuilder>();
    }

    [Fact]
    public void OpenBase64Async_WithBase64String_ReturnsImageBuilder()
    {
        var base64 = Convert.ToBase64String(new byte[] { 1, 2, 3 });
        var builder = ImageFactory.OpenBase64Async(base64);

        builder.Should().NotBeNull();
        builder.Should().BeAssignableTo<IImageBuilder>();
    }

    [Fact]
    public void OpenAsync_NullPath_ThrowsArgumentException()
    {
        var act = () => ImageFactory.OpenAsync((string)null!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void OpenAsync_EmptyPath_ThrowsArgumentException()
    {
        var act = () => ImageFactory.OpenAsync(string.Empty);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void OpenAsync_NullStream_ThrowsArgumentNullException()
    {
        var act = () => ImageFactory.OpenAsync((Stream)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void OpenAsync_NullBytes_ThrowsArgumentNullException()
    {
        var act = () => ImageFactory.OpenAsync((byte[])null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void OpenAsync_ReturnsSameTypeAsImageBuilder()
    {
        var builder = ImageFactory.OpenAsync("image.jpg");

        builder.Should().BeOfType<ImageBuilder>();
    }

    [Fact]
    public void OpenAsync_WithUri_ReturnsImageBuilder()
    {
        var uri = new Uri("https://example.com/photo.jpg");
        var builder = ImageFactory.OpenAsync(uri);

        builder.Should().NotBeNull();
        builder.Should().BeAssignableTo<IImageBuilder>();
    }

    [Fact]
    public void OpenAsync_WithUri_ReturnsSameTypeAsImageBuilder()
    {
        var uri = new Uri("https://example.com/photo.jpg");
        var builder = ImageFactory.OpenAsync(uri);

        builder.Should().BeOfType<ImageBuilder>();
    }

    [Fact]
    public void OpenAsync_NullUri_ThrowsArgumentNullException()
    {
        var act = () => ImageFactory.OpenAsync((Uri)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void OpenAsync_WithRelativeUri_ReturnsImageBuilder()
    {
        var uri = new Uri("/images/photo.jpg", UriKind.Relative);
        var builder = ImageFactory.OpenAsync(uri);

        builder.Should().NotBeNull();
        builder.Should().BeAssignableTo<IImageBuilder>();
    }
}
