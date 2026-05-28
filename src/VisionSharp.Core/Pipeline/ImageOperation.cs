using VisionSharp.Abstractions.Interfaces;

namespace VisionSharp.Core.Pipeline;

/// <summary>
/// A deferred image operation stored in the processing pipeline.
/// The delegate is executed against the engine when a terminal method is called.
/// </summary>
internal sealed class ImageOperation
{
    private readonly string _name;
    private readonly Func<IImageEngine, CancellationToken, Task> _action;

    public ImageOperation(string name, Func<IImageEngine, CancellationToken, Task> action)
    {
        _name = name;
        _action = action;
    }

    /// <summary>Human-readable name for diagnostics.</summary>
    public string Name => _name;

    /// <summary>Executes this operation against the provided engine.</summary>
    public Task ExecuteAsync(IImageEngine engine, CancellationToken cancellationToken)
        => _action(engine, cancellationToken);

    // ── Factory helpers ───────────────────────────────────────────────────────

    public static ImageOperation Sync(string name, Action<IImageEngine> action)
        => new(name, (engine, _) =>
        {
            action(engine);
            return Task.CompletedTask;
        });

    public static ImageOperation Async(string name, Func<IImageEngine, CancellationToken, Task> action)
        => new(name, action);
}
