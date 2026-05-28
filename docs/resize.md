# Resize Operations

## Resize to exact dimensions

Stretches the image to the target size, ignoring aspect ratio.

```csharp
await ImageFactory.OpenAsync("photo.jpg")
    .Resize(800, 600)
    .SaveAsync("output.jpg");
```

## ResizeDown — never upscale

Only resizes if the image is already larger than the target.

```csharp
await ImageFactory.OpenAsync("small.jpg")
    .ResizeDown(800, 600)  // no-op if smaller than 800×600
    .SaveAsync("output.jpg");
```

## Scale — proportional factor

```csharp
await ImageFactory.OpenAsync("photo.jpg")
    .Scale(0.5)        // 50% of original size
    .SaveAsync("half.jpg");
```

```csharp
await ImageFactory.OpenAsync("photo.jpg")
    .ScaleDown(0.5)    // 50% only if it reduces the image
    .SaveAsync("half.jpg");
```

## Cover — crop-fill (CSS object-fit: cover)

Scales and crops so the image exactly fills the target dimensions.
Aspect ratio is preserved; excess area is cropped from the center.

```csharp
await ImageFactory.OpenAsync("photo.jpg")
    .Cover(500, 500)   // square crop, fills entire 500×500
    .SaveAsync("square.jpg");
```

```csharp
// Only cover if the image is larger than the target
await ImageFactory.OpenAsync("photo.jpg")
    .CoverDown(500, 500)
    .SaveAsync("square.jpg");
```

## Contain — fit within bounds (CSS object-fit: contain)

Scales so the entire image fits within the target.
No cropping; the result may be smaller than the target on one axis.

```csharp
await ImageFactory.OpenAsync("photo.jpg")
    .Contain(300, 300)  // max 300px on longest side
    .SaveAsync("thumbnail.jpg");
```

## Canvas resize

Adds or removes padding without scaling the image content.

```csharp
// Expand canvas (adds transparent padding)
await ImageFactory.OpenAsync("photo.jpg")
    .ResizeCanvas(1000, 1000, "#00000000")
    .SaveAsync("padded.png");

// Adjust by offset
await ImageFactory.OpenAsync("photo.jpg")
    .ResizeCanvasRelative(widthOffset: 100, heightOffset: 50)
    .SaveAsync("padded.jpg");
```

## Custom ResizeOptions

```csharp
using VisionSharp.Abstractions.Options;
using VisionSharp.Abstractions.Enums;

await ImageFactory.OpenAsync("photo.jpg")
    .Resize(800, 600, new ResizeOptions
    {
        Mode = ResizeMode.Pad,     // pad instead of stretch
        AllowUpscale = false,       // never enlarge
        PadColor = "#FFFFFF"        // white padding
    })
    .SaveAsync("output.jpg");
```
