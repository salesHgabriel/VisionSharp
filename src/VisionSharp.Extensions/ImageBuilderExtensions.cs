using VisionSharp.Abstractions.Enums;
using VisionSharp.Abstractions.Interfaces;

namespace VisionSharp.Extensions;

/// <summary>General-purpose extensions that enrich the <see cref="IImageBuilder"/> API.</summary>
public static class ImageBuilderExtensions
{
    /// <summary>
    /// Conditionally applies a transformation. The <paramref name="configure"/> delegate
    /// is only invoked when <paramref name="condition"/> is <c>true</c>.
    /// </summary>
    /// <example>
    /// <code>
    /// builder.When(isGrayscale, b => b.Grayscale())
    /// </code>
    /// </example>
    public static IImageBuilder When(
        this IImageBuilder builder,
        bool condition,
        Func<IImageBuilder, IImageBuilder> configure)
        => condition ? configure(builder) : builder;

    /// <summary>
    /// Applies a series of transformations from a collection of configurators.
    /// Useful for dynamic effect pipelines loaded from configuration.
    /// </summary>
    public static IImageBuilder Apply(
        this IImageBuilder builder,
        IEnumerable<Func<IImageBuilder, IImageBuilder>> operations)
    {
        foreach (var op in operations)
            builder = op(builder);
        return builder;
    }

    /// <summary>
    /// Saves the same processed image to multiple paths (e.g., JPEG + WebP).
    /// All paths are written concurrently.
    /// </summary>
    public static async Task SaveToMultipleAsync(
        this IImageBuilder builder,
        IEnumerable<string> paths,
        CancellationToken cancellationToken = default)
    {
        var bytes = await builder.ToBytesAsync(cancellationToken).ConfigureAwait(false);

        await Parallel.ForEachAsync(paths, cancellationToken, async (path, ct) =>
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            await File.WriteAllBytesAsync(path, bytes, ct).ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Applies a square crop (centered) and then covers to the given dimension.
    /// Useful for generating profile pictures.
    /// </summary>
    public static IImageBuilder ProfilePicture(this IImageBuilder builder, int size)
        => builder.Cover(size, size);

    /// <summary>Creates a social-media-ready open-graph image (1200×630).</summary>
    public static IImageBuilder OpenGraph(this IImageBuilder builder)
        => builder.Cover(1200, 630).ToJpeg(85);

    /// <summary>Creates a thumbnail at the given max dimension, preserving aspect ratio.</summary>
    public static IImageBuilder AsThumbnail(this IImageBuilder builder, int maxDimension = 256)
        => builder.ContainDown(maxDimension, maxDimension);

    /// <summary>
    /// Adds a semi-transparent dark overlay — useful for making text readable over photos.
    /// </summary>
    public static IImageBuilder DarkOverlay(this IImageBuilder builder, float opacity = 0.4f)
        => builder.DrawRectangle(0, 0, int.MaxValue, int.MaxValue, opts =>
        {
            opts.FillColor = "#000000";
            opts.StrokeWidth = 0;
            opts.Opacity = opacity;
        });
}
