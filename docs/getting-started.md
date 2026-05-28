# Getting Started with VisionSharp

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

## Installation

```bash
dotnet add package VisionSharp
```

## Your first image operation

```csharp
using VisionSharp;
using VisionSharp.Abstractions.Enums;

var image = await ImageFactory
    .OpenAsync("input.jpg")
    .Resize(800, 600)
    .SaveAsync("output.jpg");

Console.WriteLine($"Saved: {image.Width}×{image.Height}");
```

## Understanding the pipeline

`ImageFactory.OpenAsync(...)` returns an `IImageBuilder`. Each method call on the
builder **adds an operation to a queue** — no processing happens yet. The image is
loaded and all operations are applied when you call a **terminal method**:

| Terminal method | What it does |
|----------------|--------------|
| `SaveAsync(path)` | Processes the pipeline and writes to disk |
| `ToBytesAsync()` | Processes and returns `byte[]` |
| `ToBase64Async()` | Processes and returns a Base64 string |
| `ToStreamAsync()` | Processes and returns a `MemoryStream` |

## Fluent chaining

```csharp
var bytes = await ImageFactory
    .OpenAsync("photo.jpg")
    .Cover(500, 500)          // crop-fill to square
    .GaussianBlur(0.5f)       // subtle blur
    .Brightness(0.05f)        // slight brightness boost
    .ToJpeg(85)               // encode as JPEG at 85% quality
    .ToBytesAsync();          // ← pipeline executes here
```

## Multiple outputs from the same source

Each call to a terminal method creates a **new engine instance** and re-applies
the pipeline from scratch. If you need multiple outputs efficiently, process once
and use `SaveToMultipleAsync`:

```csharp
using VisionSharp.Extensions;

await ImageFactory
    .OpenAsync("photo.jpg")
    .Resize(800, 600)
    .SaveToMultipleAsync(new[] { "out.jpg", "out.webp", "out.png" });
```

## Error handling

```csharp
using VisionSharp.Core.Exceptions;

try
{
    await ImageFactory.OpenAsync("photo.jpg").Resize(-1, 600).ToBytesAsync();
}
catch (InvalidResizeOperationException ex)
{
    Console.WriteLine($"Bad resize: {ex.Message}");
}
catch (ImageProcessingException ex)
{
    Console.WriteLine($"Operation '{ex.OperationName}' failed: {ex.Message}");
}
```

## Next steps

- [Resize guide](resize.md)
- [Watermark guide](watermark.md)
- [Effects guide](effects.md)
- [Architecture overview](architecture.md)
