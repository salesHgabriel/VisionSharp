namespace VisionSharp.Core.Exceptions;

/// <summary>Thrown when a resize operation specifies invalid dimensions or parameters.</summary>
public sealed class InvalidResizeOperationException : Exception
{
    public InvalidResizeOperationException(string message) : base(message) { }

    public InvalidResizeOperationException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>Creates an exception for non-positive dimension values.</summary>
    public static InvalidResizeOperationException NegativeOrZeroDimension(int width, int height)
        => new($"Resize dimensions must be positive. Got width={width}, height={height}.");

    /// <summary>Creates an exception when scale factor is out of range.</summary>
    public static InvalidResizeOperationException InvalidScaleFactor(double factor)
        => new($"Scale factor must be greater than zero. Got {factor}.");
}
