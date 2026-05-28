using VisionSharp.Abstractions.Enums;

namespace VisionSharp.Abstractions.Interfaces;

/// <summary>Encodes a raw image to a specific output format.</summary>
public interface IImageEncoder
{
    /// <summary>The format this encoder produces.</summary>
    ImageFormat Format { get; }

    /// <summary>Encodes the image held by <paramref name="engine"/> to a byte array.</summary>
    Task<byte[]> EncodeAsync(IImageEngine engine, CancellationToken cancellationToken = default);

    /// <summary>Encodes the image held by <paramref name="engine"/> and writes it to <paramref name="stream"/>.</summary>
    Task EncodeToStreamAsync(IImageEngine engine, Stream stream, CancellationToken cancellationToken = default);
}
