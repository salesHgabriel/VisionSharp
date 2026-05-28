using Microsoft.Extensions.DependencyInjection;
using VisionSharp.Abstractions.Interfaces;
using VisionSharp.Core.Engines;

namespace VisionSharp.DependencyInjection;

/// <summary>Builder returned by <c>AddVisionSharp()</c> for chaining engine registration.</summary>
public sealed class VisionSharpBuilder
{
    private readonly IServiceCollection _services;

    internal VisionSharpBuilder(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Registers the ImageSharp engine as the active <see cref="IImageEngine"/> provider.
    /// This is the default engine; call this only when you need to be explicit.
    /// </summary>
    public VisionSharpBuilder UseImageSharp()
    {
        _services.AddTransient<IImageEngine, ImageSharpEngine>();
        ImageFactory.UseEngine(() => new ImageSharpEngine());
        return this;
    }

    /// <summary>
    /// Registers a custom engine factory. Use this to plug in SkiaSharp, Magick.NET,
    /// or any other <see cref="IImageEngine"/> implementation.
    /// </summary>
    public VisionSharpBuilder UseEngine<TEngine>(Func<IServiceProvider, TEngine> factory)
        where TEngine : class, IImageEngine
    {
        _services.AddTransient<IImageEngine>(factory);
        _services.AddTransient<IImageFactory>(sp =>
            new ServicedImageFactory(() => sp.GetRequiredService<IImageEngine>()));
        return this;
    }
}
