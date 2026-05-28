using VisionSharp.Abstractions.Enums;
using VisionSharp.Abstractions.Interfaces;

namespace VisionSharp.Core;

/// <summary>
/// Immutable result image returned by terminal pipeline operations.
/// Holds the encoded bytes in memory for subsequent conversions or saves.
/// </summary>
public sealed class VisionImage : IImage
{
    private readonly byte[] _data;
    private bool _disposed;

    /// <inheritdoc/>
    public int Width { get; }

    /// <inheritdoc/>
    public int Height { get; }

    /// <inheritdoc/>
    public ImageFormat Format { get; }

    internal VisionImage(int width, int height, ImageFormat format, byte[] data)
    {
        Width = width;
        Height = height;
        Format = format;
        _data = data;
    }

    /// <inheritdoc/>
    public Task<byte[]> ToBytesAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return Task.FromResult(_data.ToArray());
    }

    /// <inheritdoc/>
    public Task<Stream> ToStreamAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        Stream stream = new MemoryStream(_data, writable: false);
        return Task.FromResult(stream);
    }

    /// <inheritdoc/>
    public Task<string> ToBase64Async(CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        return Task.FromResult(Convert.ToBase64String(_data));
    }

    /// <inheritdoc/>
    public async Task SaveAsync(string path, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var dir = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

        await File.WriteAllBytesAsync(path, _data, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        _disposed = true;
        return ValueTask.CompletedTask;
    }

    private void ThrowIfDisposed()
        => ObjectDisposedException.ThrowIf(_disposed, this);
}
