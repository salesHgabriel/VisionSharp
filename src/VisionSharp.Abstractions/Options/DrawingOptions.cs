namespace VisionSharp.Abstractions.Options;

/// <summary>Options that control stroke and fill rendering for drawing primitives.</summary>
public sealed class DrawingOptions
{
    /// <summary>Stroke color in CSS hex notation.</summary>
    public string StrokeColor { get; set; } = "#000000";

    /// <summary>Fill color in CSS hex notation. Use <c>#00000000</c> for transparent (no fill).</summary>
    public string FillColor { get; set; } = "#00000000";

    /// <summary>Stroke thickness in pixels.</summary>
    public float StrokeWidth { get; set; } = 1f;

    /// <summary>Opacity of the drawn shape (0.0 – 1.0).</summary>
    public float Opacity { get; set; } = 1f;

    /// <summary>Font family for text drawing operations.</summary>
    public string FontFamily { get; set; } = "Arial";

    /// <summary>Font size in points for text drawing operations.</summary>
    public float FontSize { get; set; } = 16f;

    /// <summary>Whether to anti-alias drawn shapes.</summary>
    public bool Antialias { get; set; } = true;
}
