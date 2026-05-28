namespace VisionSharp.Core.Exceptions;

/// <summary>Thrown when a watermark operation cannot be completed.</summary>
public sealed class WatermarkException : Exception
{
    public WatermarkException(string message) : base(message) { }

    public WatermarkException(string message, Exception innerException)
        : base(message, innerException) { }
}
