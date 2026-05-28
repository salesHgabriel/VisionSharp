using BenchmarkDotNet.Attributes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace VisionSharp.Benchmarks;

/// <summary>Benchmarks comparing encoding performance across different output formats.</summary>
[SimpleJob]
[MemoryDiagnoser]
[HtmlExporter]
public class EncodeBenchmark
{
    private byte[] _imageBytes = [];

    [GlobalSetup]
    public void Setup()
    {
        using var img = new Image<Rgba32>(800, 600, new Rgba32(50, 100, 150));
        using var ms = new MemoryStream();
        img.SaveAsJpeg(ms);
        _imageBytes = ms.ToArray();
    }

    [Benchmark(Baseline = true, Description = "Encode JPEG q=90")]
    public async Task<byte[]> EncodeJpeg_Q90()
        => await ImageFactory.OpenAsync(_imageBytes).ToJpeg(90).ToBytesAsync();

    [Benchmark(Description = "Encode JPEG q=60")]
    public async Task<byte[]> EncodeJpeg_Q60()
        => await ImageFactory.OpenAsync(_imageBytes).ToJpeg(60).ToBytesAsync();

    [Benchmark(Description = "Encode PNG")]
    public async Task<byte[]> EncodePng()
        => await ImageFactory.OpenAsync(_imageBytes).ToPng().ToBytesAsync();

    [Benchmark(Description = "Encode WebP q=80")]
    public async Task<byte[]> EncodeWebP_Q80()
        => await ImageFactory.OpenAsync(_imageBytes).ToWebp(80).ToBytesAsync();

    [Benchmark(Description = "Encode WebP q=50")]
    public async Task<byte[]> EncodeWebP_Q50()
        => await ImageFactory.OpenAsync(_imageBytes).ToWebp(50).ToBytesAsync();
}
