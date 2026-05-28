namespace VisionSharp.Core.Exceptions;

/// <summary>Thrown when an error occurs during image processing pipeline execution.</summary>
public sealed class ImageProcessingException : Exception
{
    /// <summary>The name of the operation that failed.</summary>
    public string OperationName { get; }

    public ImageProcessingException(string operationName, string message)
        : base(message)
    {
        OperationName = operationName;
    }

    public ImageProcessingException(string operationName, string message, Exception innerException)
        : base(message, innerException)
    {
        OperationName = operationName;
    }
}
