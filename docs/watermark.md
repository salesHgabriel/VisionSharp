# Watermark

## Image watermark

```csharp
using VisionSharp.Abstractions.Enums;

await ImageFactory.OpenAsync("photo.jpg")
    .Watermark("logo.png", WatermarkPosition.BottomRight, opacity: 0.8f)
    .SaveAsync("result.jpg");
```

### Available positions

```
TopLeft     TopCenter     TopRight
MiddleLeft    Center    MiddleRight
BottomLeft  BottomCenter  BottomRight
```

## Text watermark

```csharp
await ImageFactory.OpenAsync("photo.jpg")
    .WatermarkText("© My Company 2026", opts =>
    {
        opts.Position  = WatermarkPosition.BottomRight;
        opts.FontFamily = "Arial";
        opts.FontSize  = 20;
        opts.Color     = "#FFFFFF";   // white text
        opts.Opacity   = 0.85f;
        opts.Bold      = true;
        opts.PaddingX  = 15;
        opts.PaddingY  = 10;
    })
    .SaveAsync("result.jpg");
```

## Centered logo

```csharp
await ImageFactory.OpenAsync("banner.jpg")
    .Watermark("logo.png", WatermarkPosition.Center, opacity: 0.6f)
    .SaveAsync("branded.jpg");
```

## Multiple watermarks

Chain multiple calls to layer text and image watermarks:

```csharp
await ImageFactory.OpenAsync("photo.jpg")
    .Watermark("logo.png", WatermarkPosition.TopLeft)
    .WatermarkText("CONFIDENTIAL", opts =>
    {
        opts.Position = WatermarkPosition.Center;
        opts.FontSize = 48;
        opts.Color    = "#FF0000";
        opts.Opacity  = 0.25f;
        opts.Rotate   = 0; // future feature
    })
    .WatermarkText("© 2026", opts =>
    {
        opts.Position = WatermarkPosition.BottomRight;
        opts.FontSize = 14;
    })
    .SaveAsync("output.jpg");
```
