using VisionSharp.Abstractions.Enums;

namespace VisionSharp.Abstractions.Options;

/// <summary>Options for rendering a text watermark onto an image.</summary>
public sealed class TextWatermarkOptions
{
    /// <summary>Font family name (must be installed on the host system).</summary>
    public string FontFamily { get; set; } = "Arial";

    /// <summary>Font size in points.</summary>
    public float FontSize { get; set; } = 24f;

    /// <summary>Text color in CSS hex notation, e.g. <c>#FFFFFF</c> or <c>#FFFFFFCC</c>.</summary>
    public string Color { get; set; } = "#FFFFFF";

    /// <summary>Overall opacity of the watermark (0.0 – 1.0).</summary>
    public float Opacity { get; set; } = 0.7f;

    /// <summary>Watermark placement within the image.</summary>
    public WatermarkPosition Position { get; set; } = WatermarkPosition.BottomRight;

    /// <summary>Horizontal padding from the placement edge, in pixels.</summary>
    public int PaddingX { get; set; } = 10;

    /// <summary>Vertical padding from the placement edge, in pixels.</summary>
    public int PaddingY { get; set; } = 10;

    /// <summary>Whether to render the text bold.</summary>
    public bool Bold { get; set; } = false;

    /// <summary>Whether to render the text italic.</summary>
    public bool Italic { get; set; } = false;
}
