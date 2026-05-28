# Effects

## Blur

```csharp
// Box blur (integer radius)
builder.Blur(5)

// Gaussian blur (float sigma — smoother)
builder.GaussianBlur(2.5f)
```

## Color adjustments

```csharp
// Brightness: -1.0 (black) → 0 (unchanged) → 1.0 (white)
builder.Brightness(0.1f)

// Contrast: -1.0 (flat) → 0 (unchanged) → 1.0 (max contrast)
builder.Contrast(0.15f)
```

## Tone effects

```csharp
builder.Grayscale()   // convert to black-and-white
builder.Sepia()       // warm brownish sepia tone
```

## Sharpening

```csharp
builder.Sharpen()     // Gaussian unsharp mask
```

## Pixelate

```csharp
builder.Pixelate(10)  // block size in pixels
```

## Transformations

```csharp
builder.Rotate(45f)         // clockwise degrees (float)
builder.FlipHorizontal()    // mirror left–right
builder.FlipVertical()      // mirror top–bottom
```

## Opacity

```csharp
builder.Opacity(0.5f)  // 0.0 = fully transparent, 1.0 = fully opaque
```

## Preset effects (VisionSharp.Processing)

```csharp
using VisionSharp.Processing.Extensions;

builder.Vintage()      // sepia + blur + soft contrast
builder.BlackAndWhite() // grayscale + contrast + sharpen
builder.WarmTone()     // golden-hour feel
builder.CoolTone()     // desaturated cool look
builder.SoftFocus()    // dreamy Gaussian glow
builder.Thumbnail()    // contain within 150px
```

## Custom effect chains

```csharp
await ImageFactory.OpenAsync("photo.jpg")
    .Grayscale()
    .GaussianBlur(0.3f)
    .Contrast(0.1f)
    .Sharpen()
    .Brightness(0.05f)
    .SaveAsync("styled.jpg");
```
