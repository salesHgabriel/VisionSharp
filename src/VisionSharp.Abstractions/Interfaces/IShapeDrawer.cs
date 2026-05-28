using VisionSharp.Abstractions.Enums;
using VisionSharp.Abstractions.Options;

namespace VisionSharp.Abstractions.Interfaces;

/// <summary>Draws 2-D vector primitives onto an image engine.</summary>
public interface IShapeDrawer
{
    void DrawLine(IImageEngine engine, float x1, float y1, float x2, float y2, DrawingOptions options);
    void DrawRectangle(IImageEngine engine, float x, float y, float width, float height, DrawingOptions options);
    void DrawCircle(IImageEngine engine, float cx, float cy, float radius, DrawingOptions options);
    void DrawPolygon(IImageEngine engine, IEnumerable<(float X, float Y)> vertices, DrawingOptions options);
    void DrawText(IImageEngine engine, string text, float x, float y, DrawingOptions options);
    Task DrawImageInsideShapeAsync(IImageEngine engine, string imagePath, ShapeType shapeType, float x, float y, float width, float height, CancellationToken cancellationToken = default);
}
