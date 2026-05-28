using VisionSharp.Abstractions.Enums;
using VisionSharp.Abstractions.Interfaces;
using VisionSharp.Abstractions.Options;

namespace VisionSharp.Drawing.Extensions;

/// <summary>
/// Convenience extensions on <see cref="IImageBuilder"/> that provide
/// strongly-typed drawing overloads with common defaults.
/// </summary>
public static class DrawingBuilderExtensions
{
    /// <summary>Draws a solid-color filled rectangle.</summary>
    public static IImageBuilder DrawFilledRectangle(
        this IImageBuilder builder,
        float x, float y, float width, float height,
        string fillColor,
        float opacity = 1f)
        => builder.DrawRectangle(x, y, width, height, opts =>
        {
            opts.FillColor = fillColor;
            opts.StrokeColor = fillColor;
            opts.StrokeWidth = 0;
            opts.Opacity = opacity;
        });

    /// <summary>Draws a solid-color filled circle.</summary>
    public static IImageBuilder DrawFilledCircle(
        this IImageBuilder builder,
        float cx, float cy, float radius,
        string fillColor,
        float opacity = 1f)
        => builder.DrawCircle(cx, cy, radius, opts =>
        {
            opts.FillColor = fillColor;
            opts.StrokeColor = fillColor;
            opts.StrokeWidth = 0;
            opts.Opacity = opacity;
        });

    /// <summary>Draws a stroke-only rectangle border.</summary>
    public static IImageBuilder DrawBorderRectangle(
        this IImageBuilder builder,
        float x, float y, float width, float height,
        string strokeColor,
        float strokeWidth = 2f)
        => builder.DrawRectangle(x, y, width, height, opts =>
        {
            opts.StrokeColor = strokeColor;
            opts.StrokeWidth = strokeWidth;
            opts.FillColor = "#00000000";
        });

    /// <summary>
    /// Clips a circular avatar image and composites it onto the canvas at the given position.
    /// </summary>
    public static IImageBuilder DrawCircularAvatar(
        this IImageBuilder builder,
        string avatarPath,
        float x, float y,
        float diameter)
        => builder.AddImageInsideShape(avatarPath, ShapeType.Circle, x, y, diameter, diameter);

    /// <summary>Renders bold text onto the image.</summary>
    public static IImageBuilder DrawBoldText(
        this IImageBuilder builder,
        string text, float x, float y,
        string color = "#000000",
        float fontSize = 24f,
        string fontFamily = "Arial")
        => builder.DrawText(text, x, y, opts =>
        {
            opts.StrokeColor = color;
            opts.FontFamily = fontFamily;
            opts.FontSize = fontSize;
        });
}
