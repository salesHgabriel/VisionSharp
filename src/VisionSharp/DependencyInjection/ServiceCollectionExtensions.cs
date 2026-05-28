using Microsoft.Extensions.DependencyInjection;
using VisionSharp.Abstractions.Interfaces;
using VisionSharp.Core.Engines;

namespace VisionSharp.DependencyInjection;

/// <summary>Extension methods for registering VisionSharp with the .NET DI container.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers VisionSharp services and returns a builder for selecting the graphics engine.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddVisionSharp()
    ///         .UseImageSharp();
    /// </code>
    /// </example>
    public static VisionSharpBuilder AddVisionSharp(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddTransient<IImageFactory, ServicedImageFactory>();
        services.AddTransient<IImageEngine, ImageSharpEngine>();

        return new VisionSharpBuilder(services);
    }
}
