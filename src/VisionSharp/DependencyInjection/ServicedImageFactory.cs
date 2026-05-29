using VisionSharp.Abstractions.Interfaces;
using VisionSharp.Core;

namespace VisionSharp.DependencyInjection;

/// <summary>
/// <see cref="IImageFactory"/> implementation that resolves engine instances via DI,
/// enabling lifetime management (Transient, Scoped) through the container.
/// </summary>
internal sealed class ServicedImageFactory : IImageFactory
{
    private static readonly HttpClient _httpClient = new();
    private readonly Func<IImageEngine> _engineFactory;

    public ServicedImageFactory(Func<IImageEngine> engineFactory)
    {
        _engineFactory = engineFactory;
    }

    /// <inheritdoc/>
    public IImageBuilder OpenAsync(string path)
        => new ImageBuilder(_engineFactory, (e, ct) => e.LoadFromPathAsync(path, ct));

    /// <inheritdoc/>
    public IImageBuilder OpenAsync(Stream stream)
        => new ImageBuilder(_engineFactory, (e, ct) => e.LoadFromStreamAsync(stream, ct));

    /// <inheritdoc/>
    public IImageBuilder OpenAsync(byte[] bytes)
        => new ImageBuilder(_engineFactory, (e, ct) => e.LoadFromBytesAsync(bytes, ct));

    /// <inheritdoc/>
    public IImageBuilder OpenBase64Async(string base64)
        => new ImageBuilder(_engineFactory, (e, ct) => e.LoadFromBase64Async(base64, ct));

    /// <inheritdoc/>
    public IImageBuilder OpenAsync(Uri url)
    {
        ArgumentNullException.ThrowIfNull(url);
        return new ImageBuilder(_engineFactory, async (e, ct) =>
        {
            var bytes = await _httpClient.GetByteArrayAsync(url, ct);
            await e.LoadFromBytesAsync(bytes, ct);
        });
    }
}
