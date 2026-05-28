using VisionSharp.Abstractions.Interfaces;

namespace VisionSharp.Processing.Extensions;

/// <summary>
/// Additional processing shortcuts built on top of the core <see cref="IImageBuilder"/> API.
/// </summary>
public static class ProcessingBuilderExtensions
{
    /// <summary>
    /// Applies a warm color tint by boosting red/yellow channels and reducing blue.
    /// Useful for golden-hour photo effects.
    /// </summary>
    public static IImageBuilder WarmTone(this IImageBuilder builder)
        => builder.Brightness(0.05f).Contrast(0.1f).Sepia();

    /// <summary>
    /// Applies a cool color treatment — slight blue shift with desaturation.
    /// </summary>
    public static IImageBuilder CoolTone(this IImageBuilder builder)
        => builder.Grayscale().Contrast(0.05f);

    /// <summary>
    /// Vintage look: sepia + slight blur + reduced contrast.
    /// </summary>
    public static IImageBuilder Vintage(this IImageBuilder builder)
        => builder.Sepia().GaussianBlur(0.5f).Contrast(-0.05f).Brightness(0.05f);

    /// <summary>
    /// High-contrast black-and-white conversion.
    /// </summary>
    public static IImageBuilder BlackAndWhite(this IImageBuilder builder)
        => builder.Grayscale().Contrast(0.2f).Sharpen();

    /// <summary>
    /// Applies a light soft-focus glow by adding a subtle Gaussian blur without losing detail.
    /// </summary>
    public static IImageBuilder SoftFocus(this IImageBuilder builder)
        => builder.GaussianBlur(1.2f).Brightness(0.03f);

    /// <summary>
    /// Thumbnail preset: resizes to fit in 150×150 while preserving aspect ratio.
    /// </summary>
    public static IImageBuilder Thumbnail(this IImageBuilder builder, int size = 150)
        => builder.ContainDown(size, size);
}
