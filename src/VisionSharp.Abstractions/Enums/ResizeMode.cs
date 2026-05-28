namespace VisionSharp.Abstractions.Enums;

/// <summary>Controls how an image is resized relative to its bounding box.</summary>
public enum ResizeMode
{
    /// <summary>Stretches the image to exactly fill the target dimensions, ignoring aspect ratio.</summary>
    Stretch,
    /// <summary>Scales proportionally so the image fits within the target, leaving empty space (contain).</summary>
    Max,
    /// <summary>Scales proportionally and crops to exactly fill the target (cover).</summary>
    Crop,
    /// <summary>Scales proportionally using the minimum scaling factor.</summary>
    Min,
    /// <summary>Pads the image to the target size without cropping.</summary>
    Pad
}
