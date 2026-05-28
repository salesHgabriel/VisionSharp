using BenchmarkDotNet.Attributes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace VisionSharp.Benchmarks;

/// <summary>Benchmarks for common effect operations on a 1080p image.</summary>
[SimpleJob]
[MemoryDiagnoser]
[HtmlExporter]
[MarkdownExporterAttribute.GitHub]
public class EffectsBenchmark
{
    private byte[] _imageBytes = [];

    [GlobalSetup]
    public void Setup()
    {
        using var img = new Image<Rgba32>(1920, 1080, new Rgba32(80, 120, 200));
        using var ms = new MemoryStream();
        img.SaveAsJpeg(ms);
        _imageBytes = ms.ToArray();
    }

    [Benchmark(Baseline = true, Description = "Baseline — no effects")]
    public async Task<byte[]> NoEffects()
        => await ImageFactory.OpenAsync(_imageBytes).ToBytesAsync();

    [Benchmark(Description = "Grayscale")]
    public async Task<byte[]> Grayscale()
        => await ImageFactory.OpenAsync(_imageBytes).Grayscale().ToBytesAsync();

    [Benchmark(Description = "Gaussian Blur σ=2")]
    public async Task<byte[]> GaussianBlur()
        => await ImageFactory.OpenAsync(_imageBytes).GaussianBlur(2f).ToBytesAsync();

    [Benchmark(Description = "Sepia")]
    public async Task<byte[]> Sepia()
        => await ImageFactory.OpenAsync(_imageBytes).Sepia().ToBytesAsync();

    [Benchmark(Description = "Sharpen")]
    public async Task<byte[]> Sharpen()
        => await ImageFactory.OpenAsync(_imageBytes).Sharpen().ToBytesAsync();

    [Benchmark(Description = "Resize + Blur + Grayscale")]
    public async Task<byte[]> ResizeBlurGrayscale()
        => await ImageFactory.OpenAsync(_imageBytes)
            .Resize(800, 600)
            .Blur(3)
            .Grayscale()
            .ToBytesAsync();
}
