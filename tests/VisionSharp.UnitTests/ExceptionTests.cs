using FluentAssertions;
using VisionSharp.Core.Exceptions;

namespace VisionSharp.UnitTests;

public sealed class ExceptionTests
{
    [Fact]
    public void ImageProcessingException_HasCorrectOperationName()
    {
        var ex = new ImageProcessingException("Resize", "Something went wrong");

        ex.OperationName.Should().Be("Resize");
        ex.Message.Should().Be("Something went wrong");
    }

    [Fact]
    public void ImageFormatNotSupportedException_ContainsFormat()
    {
        var ex = new ImageFormatNotSupportedException(".tiff");

        ex.FormatIdentifier.Should().Be(".tiff");
        ex.Message.Should().Contain(".tiff");
    }

    [Fact]
    public void InvalidResizeOperationException_NegativeDimension_HasMessage()
    {
        var ex = InvalidResizeOperationException.NegativeOrZeroDimension(-10, 300);

        ex.Message.Should().Contain("-10");
        ex.Message.Should().Contain("300");
    }

    [Fact]
    public void InvalidResizeOperationException_InvalidScaleFactor_HasMessage()
    {
        var ex = InvalidResizeOperationException.InvalidScaleFactor(0);

        ex.Message.Should().Contain("0");
    }

    [Fact]
    public void WatermarkException_PreservesMessage()
    {
        var ex = new WatermarkException("Watermark file missing");

        ex.Message.Should().Be("Watermark file missing");
    }
}
