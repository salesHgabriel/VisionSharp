using VisionSharp.Abstractions.Enums;

namespace VisionSharp.Abstractions.Options;

/// <summary>Options controlling image resize behaviour.</summary>
public sealed class ResizeOptions
{
    /// <summary>How the image is scaled to fit the target dimensions.</summary>
    public ResizeMode Mode { get; set; } = ResizeMode.Stretch;

    /// <summary>Whether to allow upscaling. When <c>false</c>, the image is never enlarged.</summary>
    public bool AllowUpscale { get; set; } = true;

    /// <summary>Whether to maintain the original aspect ratio.</summary>
    public bool PreserveAspectRatio { get; set; } = false;

    /// <summary>Background color used when padding is required (hex or named color).</summary>
    public string PadColor { get; set; } = "#00000000";
}
