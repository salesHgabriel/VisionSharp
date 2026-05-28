using BenchmarkDotNet.Running;
using VisionSharp.Benchmarks;

Console.WriteLine("VisionSharp Benchmarks");
Console.WriteLine("══════════════════════");
Console.WriteLine();
Console.WriteLine("Available benchmark classes:");
Console.WriteLine("  [1] ResizeBenchmark    — resize variants on 1920×1080");
Console.WriteLine("  [2] EffectsBenchmark   — individual effects on 1920×1080");
Console.WriteLine("  [3] EncodeBenchmark    — format encoding comparison");
Console.WriteLine("  [4] All benchmarks");
Console.WriteLine();

if (args.Contains("--all") || args.Contains("-a"))
{
    BenchmarkRunner.Run<ResizeBenchmark>();
    BenchmarkRunner.Run<EffectsBenchmark>();
    BenchmarkRunner.Run<EncodeBenchmark>();
    return;
}

// Default: run all when launched normally
Console.WriteLine("Running all benchmarks (use --all flag to suppress this prompt)...");
Console.WriteLine();

BenchmarkRunner.Run<ResizeBenchmark>();
BenchmarkRunner.Run<EffectsBenchmark>();
BenchmarkRunner.Run<EncodeBenchmark>();
