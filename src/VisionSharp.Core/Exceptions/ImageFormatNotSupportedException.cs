namespace VisionSharp.Core.Exceptions;

/// <summary>Thrown when an image format is not supported by the active engine.</summary>
public sealed class ImageFormatNotSupportedException : Exception
{
    /// <summary>The unsupported format identifier (file extension, MIME type, etc.).</summary>
    public string FormatIdentifier { get; }

    public ImageFormatNotSupportedException(string formatIdentifier)
        : base($"The image format '{formatIdentifier}' is not supported by the active engine.")
    {
        FormatIdentifier = formatIdentifier;
    }

    public ImageFormatNotSupportedException(string formatIdentifier, Exception innerException)
        : base($"The image format '{formatIdentifier}' is not supported by the active engine.", innerException)
    {
        FormatIdentifier = formatIdentifier;
    }
}
