namespace VisionSharp.Abstractions.Interfaces;

/// <summary>
/// Represents a single, named processing step that can be applied to an image engine.
/// Implement this interface to create custom processors that plug into the VisionSharp pipeline.
/// </summary>
public interface IImageProcessor
{
    /// <summary>Human-readable name for diagnostics and logging.</summary>
    string Name { get; }

    /// <summary>Applies this processor to the provided engine context.</summary>
    Task ProcessAsync(IImageEngine engine, CancellationToken cancellationToken = default);
}
