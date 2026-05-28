namespace VisionSharp.Abstractions.Interfaces;

/// <summary>Decodes image data from a stream into a format the engine can operate on.</summary>
public interface IImageDecoder
{
    /// <summary>Returns <c>true</c> if this decoder can handle the given MIME type.</summary>
    bool CanDecode(string mimeType);

    /// <summary>Loads image data from <paramref name="stream"/> into <paramref name="engine"/>.</summary>
    Task DecodeAsync(IImageEngine engine, Stream stream, CancellationToken cancellationToken = default);
}
