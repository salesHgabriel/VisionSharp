# Crop

## Centered crop

When only `width` and `height` are provided (two-argument overload), VisionSharp
crops from the center of the image.

```csharp
await ImageFactory.OpenAsync("photo.jpg")
    .Crop(400, 300)       // center-crop to 400×300
    .SaveAsync("cropped.jpg");
```

## Crop from specific coordinates

```csharp
await ImageFactory.OpenAsync("photo.jpg")
    .Crop(x: 50, y: 100, width: 600, height: 400)
    .SaveAsync("cropped.jpg");
```

Coordinates are automatically clamped to the image bounds — no out-of-range errors.

## Combined with resize

```csharp
await ImageFactory.OpenAsync("photo.jpg")
    .Resize(1200, 900)    // first enlarge
    .Crop(800, 600)       // then center-crop
    .SaveAsync("output.jpg");
```

## Cover is crop + scale

`Cover` is conceptually equivalent to a scale-then-crop that fills the target
dimensions exactly, equivalent to CSS `object-fit: cover`:

```csharp
await ImageFactory.OpenAsync("photo.jpg")
    .Cover(500, 500)      // scale + crop to exactly 500×500
    .SaveAsync("square.jpg");
```
