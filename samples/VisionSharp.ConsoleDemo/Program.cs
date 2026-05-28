using System.Diagnostics;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using VisionSharp;
using VisionSharp.Abstractions.Enums;
using VisionSharp.Drawing.Extensions;
using VisionSharp.Extensions;
using VisionSharp.Formats.Extensions;
using VisionSharp.Processing.Extensions;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Directory.CreateDirectory("output");

PrintBanner();

string imagePath = AskImagePath();
Directory.CreateDirectory(Path.GetDirectoryName(imagePath)!);

if (!File.Exists(imagePath))
{
    Console.WriteLine($"\n  Generating sample image at {imagePath}...");
    await CreateSampleImageAsync(imagePath, 800, 600);
}

await CreateSampleImageAsync("images/logo.png", 100, 50);

while (true)
{
    PrintMenu();
    string? input = Console.ReadLine()?.Trim();

    if (string.IsNullOrEmpty(input)) continue;

    if (input.Equals("q", StringComparison.OrdinalIgnoreCase) ||
        input.Equals("exit", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("\n  Bye!\n");
        break;
    }

    if (input == "0")
    {
        await RunAllDemosAsync(imagePath);
        continue;
    }

    if (input == "i")
    {
        imagePath = AskImagePath();
        if (!File.Exists(imagePath))
        {
            Console.WriteLine($"  Generating sample image at {imagePath}...");
            await CreateSampleImageAsync(imagePath, 800, 600);
        }
        Console.WriteLine($"  Active image: {imagePath}");
        continue;
    }

    if (int.TryParse(input, out int choice) && choice >= 1 && choice <= 15)
        await RunDemoAsync(choice, imagePath);
    else
        WriteError("Invalid option. Type a number from the menu, 0 for all, i to change image, or q to quit.");
}

// ── Menu & helpers ────────────────────────────────────────────────────────────

static void PrintBanner()
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine();
    Console.WriteLine("  ╔══════════════════════════════════════════╗");
    Console.WriteLine("  ║          VisionSharp  Interactive Demo   ║");
    Console.WriteLine("  ╚══════════════════════════════════════════╝");
    Console.ResetColor();
    Console.WriteLine();
}

static void PrintMenu()
{
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.WriteLine("  ─── Operations ──────────────────────────────");
    Console.ResetColor();
    Console.WriteLine("   1  Resize + Blur + Watermark (logo)");
    Console.WriteLine("   2  Cover 500×500");
    Console.WriteLine("   3  Contain 300×300");
    Console.WriteLine("   4  Crop 400×300");
    Console.WriteLine("   5  Grayscale + Sharpen");
    Console.WriteLine("   6  Sepia");
    Console.WriteLine("   7  Pixelate");
    Console.WriteLine("   8  Flip horizontal + Rotate 15°");
    Console.WriteLine("   9  Text watermark");
    Console.WriteLine("  10  Draw shapes");
    Console.WriteLine("  11  Export WebP");
    Console.WriteLine("  12  Base64 round-trip");
    Console.WriteLine("  13  Stream I/O");
    Console.WriteLine("  14  Vintage preset");
    Console.WriteLine("  15  Black & White preset");
    Console.ForegroundColor = ConsoleColor.DarkCyan;
    Console.WriteLine("  ─────────────────────────────────────────────");
    Console.ResetColor();
    Console.WriteLine("   0  Run all");
    Console.WriteLine("   i  Change active image");
    Console.WriteLine("   q  Quit");
    Console.WriteLine();
    Console.Write("  > ");
}

static string AskImagePath()
{
    Console.WriteLine();
    Console.Write("  Image path (Enter for default images/photo.jpg): ");
    string? raw = Console.ReadLine()?.Trim();
    return string.IsNullOrEmpty(raw) ? "images/photo.jpg" : raw;
}

static async Task RunAllDemosAsync(string imagePath)
{
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("  Running all demos…");
    Console.ResetColor();

    var sw = Stopwatch.StartNew();
    for (int i = 1; i <= 15; i++)
        await RunDemoAsync(i, imagePath);

    sw.Stop();
    Console.WriteLine();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"  All done in {sw.ElapsedMilliseconds} ms. Files saved to ./output/");
    Console.ResetColor();
}

static async Task RunDemoAsync(int choice, string imagePath)
{
    var sw = Stopwatch.StartNew();

    try
    {
        switch (choice)
        {
            case 1:
                Console.Write("  [1] Resize → Blur → Watermark → Save … ");
                var r = await ImageFactory
                    .OpenAsync(imagePath)
                    .Resize(1200, 800)
                    .Blur(2)
                    .Watermark("images/logo.png", WatermarkPosition.BottomRight, opacity: 0.8f)
                    .DrawText("VisionSharp", x: 20, y: 30, opts => { opts.FontSize = 32; opts.StrokeColor = "#FFFFFF"; })
                    .ToJpeg(90)
                    .SaveAsync("output/demo_resize_blur_watermark.jpg");
                WriteOk($"{r.Width}×{r.Height} → output/demo_resize_blur_watermark.jpg", sw);
                break;

            case 2:
                Console.Write("  [2] Cover 500×500 → PNG … ");
                await ImageFactory.OpenAsync(imagePath).Cover(500, 500).ToPng().SaveAsync("output/demo_cover.png");
                WriteOk("output/demo_cover.png", sw);
                break;

            case 3:
                Console.Write("  [3] Contain 300×300 … ");
                await ImageFactory.OpenAsync(imagePath).Contain(300, 300).SaveAsync("output/demo_contain.jpg");
                WriteOk("output/demo_contain.jpg", sw);
                break;

            case 4:
                Console.Write("  [4] Crop 400×300 … ");
                await ImageFactory.OpenAsync(imagePath).Crop(0, 0, 400, 300).SaveAsync("output/demo_crop.jpg");
                WriteOk("output/demo_crop.jpg", sw);
                break;

            case 5:
                Console.Write("  [5] Grayscale + Sharpen … ");
                await ImageFactory.OpenAsync(imagePath).Grayscale().Sharpen().SaveAsync("output/demo_grayscale.jpg");
                WriteOk("output/demo_grayscale.jpg", sw);
                break;

            case 6:
                Console.Write("  [6] Sepia … ");
                await ImageFactory.OpenAsync(imagePath).Sepia().SaveAsync("output/demo_sepia.jpg");
                WriteOk("output/demo_sepia.jpg", sw);
                break;

            case 7:
                Console.Write("  [7] Pixelate … ");
                int px = AskInt("  Pixel size (default 15): ", 15, 2, 64);
                await ImageFactory.OpenAsync(imagePath).Pixelate(px).SaveAsync("output/demo_pixelate.jpg");
                WriteOk("output/demo_pixelate.jpg", sw);
                break;

            case 8:
                Console.Write("  [8] Flip + Rotate … ");
                float angle = AskFloat("  Rotation angle in degrees (default 15): ", 15f);
                await ImageFactory.OpenAsync(imagePath).FlipHorizontal().Rotate(angle).SaveAsync("output/demo_flip_rotate.jpg");
                WriteOk("output/demo_flip_rotate.jpg", sw);
                break;

            case 9:
                Console.Write("  [9] Text watermark … ");
                Console.Write("\n  Watermark text (default '© VisionSharp 2026'): ");
                string wText = Console.ReadLine()?.Trim() is { Length: > 0 } t ? t : "© VisionSharp 2026";
                await ImageFactory.OpenAsync(imagePath)
                    .WatermarkText(wText, opts =>
                    {
                        opts.Position = WatermarkPosition.BottomRight;
                        opts.FontSize = 18;
                        opts.Color = "#FFFFFF";
                        opts.Opacity = 0.85f;
                        opts.PaddingX = 12;
                        opts.PaddingY = 12;
                    })
                    .SaveAsync("output/demo_text_watermark.jpg");
                WriteOk("output/demo_text_watermark.jpg", sw);
                break;

            case 10:
                Console.Write("  [10] Drawing shapes … ");
                await ImageFactory.OpenAsync(imagePath)
                    .DrawBorderRectangle(20, 20, 200, 150, strokeColor: "#FF0000", strokeWidth: 3)
                    .DrawFilledCircle(400, 150, 60, fillColor: "#0000FFAA")
                    .DrawLine(0, 0, 800, 600, opts => { opts.StrokeColor = "#FFFF00"; opts.StrokeWidth = 2; })
                    .DrawBoldText("Hello VisionSharp!", x: 50, y: 250, color: "#FFFFFF", fontSize: 28)
                    .SaveAsync("output/demo_drawing.jpg");
                WriteOk("output/demo_drawing.jpg", sw);
                break;

            case 11:
                Console.Write("  [11] WebP export … ");
                int quality = AskInt("  Quality 1-100 (default 85): ", 85, 1, 100);
                await ImageFactory.OpenAsync(imagePath).Resize(640, 480).ToWebp(quality).SaveAsync("output/demo_webp.webp");
                WriteOk("output/demo_webp.webp", sw);
                break;

            case 12:
                Console.Write("  [12] Base64 round-trip … ");
                var b64 = await ImageFactory.OpenAsync(imagePath).Resize(200, 150).ToBase64Async();
                await ImageFactory.OpenAsync(Convert.FromBase64String(b64)).SaveAsync("output/demo_from_base64.jpg");
                WriteOk($"Base64 length: {b64.Length} chars → output/demo_from_base64.jpg", sw);
                break;

            case 13:
                Console.Write("  [13] Stream I/O … ");
                await using (var src = File.OpenRead(imagePath))
                {
                    var outStream = await ImageFactory.OpenAsync(src).Contain(400, 300).ToStreamAsync();
                    await using var dst = File.Create("output/demo_stream.jpg");
                    await outStream.CopyToAsync(dst);
                }
                WriteOk("output/demo_stream.jpg", sw);
                break;

            case 14:
                Console.Write("  [14] Vintage preset … ");
                await ImageFactory.OpenAsync(imagePath).Vintage().SaveAsync("output/demo_vintage.jpg");
                WriteOk("output/demo_vintage.jpg", sw);
                break;

            case 15:
                Console.Write("  [15] Black & White preset … ");
                await ImageFactory.OpenAsync(imagePath).BlackAndWhite().SaveAsync("output/demo_bw.jpg");
                WriteOk("output/demo_bw.jpg", sw);
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine();
        WriteError($"Error: {ex.Message}");
    }
}

static void WriteOk(string msg, Stopwatch? sw = null)
{
    Console.ForegroundColor = ConsoleColor.Green;
    string timing = sw is not null ? $"  ({sw.ElapsedMilliseconds} ms)" : "";
    Console.WriteLine($"OK{timing}");
    Console.ResetColor();
    Console.WriteLine($"     {msg}");
}

static void WriteError(string msg)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"  ! {msg}");
    Console.ResetColor();
}

static int AskInt(string prompt, int defaultValue, int min, int max)
{
    Console.WriteLine();
    Console.Write($"  {prompt}");
    string? raw = Console.ReadLine()?.Trim();
    if (int.TryParse(raw, out int v) && v >= min && v <= max) return v;
    return defaultValue;
}

static float AskFloat(string prompt, float defaultValue)
{
    Console.WriteLine();
    Console.Write($"  {prompt}");
    string? raw = Console.ReadLine()?.Trim();
    if (float.TryParse(raw, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float v)) return v;
    return defaultValue;
}

// ── Helper: generate a synthetic test image ───────────────────────────────────
static async Task CreateSampleImageAsync(string path, int width, int height)
{
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    if (File.Exists(path)) return;

    using var img = new SixLabors.ImageSharp.Image<Rgba32>(width, height, new Rgba32(70, 130, 180));
    var ext = Path.GetExtension(path).ToLowerInvariant();
    await using var fs = File.Create(path);

    if (ext == ".png")
        await img.SaveAsync(fs, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
    else
        await img.SaveAsync(fs, new JpegEncoder { Quality = 90 });
}
