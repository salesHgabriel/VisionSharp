# Architecture

## Project layout

```
VisionSharp.sln
├── src/
│   ├── VisionSharp.Abstractions   — interfaces, enums, options (no deps)
│   ├── VisionSharp.Core           — ImageBuilder, ImageSharpEngine, pipeline
│   ├── VisionSharp.Drawing        — drawing extension methods
│   ├── VisionSharp.Processing     — effect presets
│   ├── VisionSharp.Formats        — format encoding helpers
│   ├── VisionSharp.Extensions     — utility extensions (When, Apply, …)
│   └── VisionSharp                — ImageFactory facade + DI extensions
├── samples/
│   └── VisionSharp.ConsoleDemo
└── tests/
    ├── VisionSharp.UnitTests      — mock-based, fast
    ├── VisionSharp.IntegrationTests — real engine, pixel-level
    └── VisionSharp.Benchmarks     — BenchmarkDotNet
```

## Dependency graph

```
Abstractions
     ↑
    Core  (contains ImageSharpEngine)
    ↑ ↑ ↑
Drawing  Processing  Formats  Extensions
    ↑         ↑           ↑        ↑
                VisionSharp (facade)
```

## Key types

| Type | Role |
|------|------|
| `IImageBuilder` | Fluent pipeline contract |
| `ImageBuilder` | Concrete builder — stores a `List<ImageOperation>` |
| `ImageOperation` | Delegates `(IImageEngine, CancellationToken) → Task` |
| `IImageEngine` | Engine contract (load, mutate, output) |
| `ImageSharpEngine` | Default engine backed by SixLabors.ImageSharp |
| `IImageFactory` | Creates `IImageBuilder` instances |
| `ImageFactory` | Static entry point (uses the registered engine factory) |
| `VisionImage` | Immutable result: width, height, format, raw bytes |

## Pipeline execution

```
ImageFactory.OpenAsync("photo.jpg")   ← returns ImageBuilder (no I/O yet)
    .Resize(800, 600)                  ← adds ResizeOperation to queue
    .Blur(2)                           ← adds BlurOperation
    .SaveAsync("out.jpg")              ← ← ← terminal method:
                                          1. Creates IImageEngine
                                          2. Calls engine.LoadFromPathAsync(...)
                                          3. Loops through operations, calling ExecuteAsync()
                                          4. Calls engine.SaveToPathAsync(...)
                                          5. Disposes engine
```

## Adding a custom engine

Implement `IImageEngine` from `VisionSharp.Abstractions`:

```csharp
public class SkiaSharpEngine : IImageEngine
{
    // ... implement all members
}
```

Register at startup:

```csharp
services.AddVisionSharp()
        .UseEngine(sp => new SkiaSharpEngine());
```

Or globally without DI:

```csharp
ImageFactory.UseEngine(() => new SkiaSharpEngine());
```

## Design patterns

| Pattern | Where |
|---------|-------|
| **Fluent API** | `IImageBuilder` chain |
| **Pipeline** | `ImageBuilder._pipeline` list |
| **Strategy** | `IImageEngine` swap |
| **Factory** | `ImageFactory`, `IImageFactory` |
| **Adapter** | `ImageSharpEngine` wrapping SixLabors API |
| **Builder** | `ImageBuilder` accumulates operations |
| **DI** | `ServiceCollectionExtensions`, `VisionSharpBuilder` |

## Lazy evaluation rationale

Operations are deferred until a terminal method for three reasons:

1. **Zero cost** for discarded builders (e.g., conditional branches that don't execute)
2. **Composability** — you can pass a half-built `IImageBuilder` around and add more steps
3. **Future optimization** — adjacent operations can be merged before execution
   (e.g., two consecutive resizes → one resize)
