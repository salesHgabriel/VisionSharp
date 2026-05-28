using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors;

namespace VisionSharp.Core.Engines;

/// <summary>
/// Applies an alpha mask to an <see cref="Image{Rgba32}"/>: pixels on the mask
/// that are transparent make the corresponding target pixel transparent.
/// </summary>
internal sealed class AlphaMaskProcessor : IImageProcessor
{
    private readonly Image<Rgba32> _mask;

    public AlphaMaskProcessor(Image<Rgba32> mask)
    {
        _mask = mask;
    }

    public IImageProcessor<TPixel> CreatePixelSpecificProcessor<TPixel>(
        Configuration configuration,
        Image<TPixel> source,
        Rectangle sourceRectangle)
        where TPixel : unmanaged, IPixel<TPixel>
    {
        return new AlphaMaskPixelProcessor<TPixel>(configuration, source, sourceRectangle, _mask);
    }
}

internal sealed class AlphaMaskPixelProcessor<TPixel> : ImageProcessor<TPixel>
    where TPixel : unmanaged, IPixel<TPixel>
{
    private readonly Image<Rgba32> _mask;

    public AlphaMaskPixelProcessor(
        Configuration configuration,
        Image<TPixel> source,
        Rectangle sourceRectangle,
        Image<Rgba32> mask)
        : base(configuration, source, sourceRectangle)
    {
        _mask = mask;
    }

    protected override void OnFrameApply(ImageFrame<TPixel> source)
    {
        int w = Math.Min(source.Width, _mask.Width);
        int h = Math.Min(source.Height, _mask.Height);

        var maskFrame = _mask.Frames.RootFrame;

        for (int y = 0; y < h; y++)
        {
            var srcRow = source.PixelBuffer.DangerousGetRowSpan(y);
            var maskRow = maskFrame.PixelBuffer.DangerousGetRowSpan(y);

            for (int x = 0; x < w; x++)
            {
                var srcPixel = srcRow[x].ToScaledVector4();
                float maskAlpha = maskRow[x].A / 255f;
                srcPixel.W *= maskAlpha;
                srcRow[x].FromScaledVector4(srcPixel);
            }
        }
    }
}
