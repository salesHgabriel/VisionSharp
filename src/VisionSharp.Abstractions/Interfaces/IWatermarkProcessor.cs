using VisionSharp.Abstractions.Enums;
using VisionSharp.Abstractions.Options;

namespace VisionSharp.Abstractions.Interfaces;

/// <summary>Handles watermark compositing (image and text) onto a target image engine.</summary>
public interface IWatermarkProcessor
{
    /// <summary>Composites a watermark image at the given position with the given opacity.</summary>
    Task ApplyImageWatermarkAsync(
        IImageEngine engine,
        string watermarkPath,
        WatermarkPosition position,
        float opacity,
        CancellationToken cancellationToken = default);

    /// <summary>Renders a text watermark onto the image.</summary>
    void ApplyTextWatermark(IImageEngine engine, string text, TextWatermarkOptions options);
}
