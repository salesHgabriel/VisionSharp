using VisionSharp.Abstractions.Enums;

namespace VisionSharp.Abstractions.Interfaces;

/// <summary>
/// Represents an image that has completed processing. Provides access to its
/// dimensions, raw bytes, and serialized representations.
/// </summary>
public interface IImage : IAsyncDisposable
{
    /// <summary>Width of the processed image in pixels.</summary>
    int Width { get; }

    /// <summary>Height of the processed image in pixels.</summary>
    int Height { get; }

    /// <summary>The output format the image was encoded as.</summary>
    ImageFormat Format { get; }

    /// <summary>Returns the image as a raw byte array.</summary>
    Task<byte[]> ToBytesAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the image as a readable <see cref="Stream"/>.</summary>
    Task<Stream> ToStreamAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the image as a Base64-encoded string.</summary>
    Task<string> ToBase64Async(CancellationToken cancellationToken = default);

    /// <summary>Saves the image to the specified file path.</summary>
    Task SaveAsync(string path, CancellationToken cancellationToken = default);
}
