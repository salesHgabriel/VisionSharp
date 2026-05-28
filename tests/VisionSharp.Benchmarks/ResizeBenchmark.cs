using BenchmarkDotNet.Attributes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace VisionSharp.Benchmarks;

/// <summary>Benchmarks for the most commonly invoked resize operations.</summary>
[SimpleJob]
[MemoryDiagnoser]
[HtmlExporter]
[MarkdownExporterAttribute.GitHub]
public class ResizeBenchmark
{
    private byte[] _imageBytes = [];

    [GlobalSetup]
    public void Setup()
    {
        using var img = new Image<Rgba32>(1920, 1080, new Rgba32(100, 149, 237));
        using var ms = new MemoryStream();
        img.SaveAsJpeg(ms);
        _imageBytes = ms.ToArray();
    }

    [Benchmark(Baseline = true, Description = "Resize 1920×1080 → 800×600")]
    public async Task<byte[]> Resize_1920x1080_To_800x600()
        => await ImageFactory.OpenAsync(_imageBytes).Resize(800, 600).ToBytesAsync();

    [Benchmark(Description = "Resize 1920×1080 → 400×300")]
    public async Task<byte[]> Resize_1920x1080_To_400x300()
        => await ImageFactory.OpenAsync(_imageBytes).Resize(400, 300).ToBytesAsync();

    [Benchmark(Description = "Cover 1920×1080 → 500×500")]
    public async Task<byte[]> Cover_1920x1080_To_500x500()
        => await ImageFactory.OpenAsync(_imageBytes).Cover(500, 500).ToBytesAsync();

    [Benchmark(Description = "Contain 1920×1080 → 640×480")]
    public async Task<byte[]> Contain_1920x1080_To_640x480()
        => await ImageFactory.OpenAsync(_imageBytes).Contain(640, 480).ToBytesAsync();

    [Benchmark(Description = "Scale × 0.5")]
    public async Task<byte[]> Scale_Half()
        => await ImageFactory.OpenAsync(_imageBytes).Scale(0.5).ToBytesAsync();
}
