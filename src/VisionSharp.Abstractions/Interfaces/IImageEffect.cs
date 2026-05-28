namespace VisionSharp.Abstractions.Interfaces;

/// <summary>
/// A visual effect that can be applied to an image engine.
/// Effects differ from processors in that they produce purely aesthetic changes
/// with no structural modifications (resize, crop, etc.).
/// </summary>
public interface IImageEffect
{
    /// <summary>Effect display name.</summary>
    string Name { get; }

    /// <summary>Applies this effect to the provided engine.</summary>
    void Apply(IImageEngine engine);
}
