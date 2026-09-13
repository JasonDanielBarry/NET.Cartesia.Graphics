using Cartesia.Core.Geometry;
using Cartesia.Core.Mapping;
using SkiaSharp;

namespace Cartesia.Tests.Helpers;

internal static class TestHelpers
{
    public const double Tol = 1e-9;

    public static void Near(double expected, double actual, double tol = Tol)
    {
        Assert.True(Math.Abs(expected - actual) <= tol, $"Expected {expected} +/- {tol} but was {actual}.");
    }

    public static void Near(Point expected, Point actual, double tol = Tol)
    {
        Near(expected.X, actual.X, tol);
        Near(expected.Y, actual.Y, tol);
    }

    public static WorldToCanvasMapper SquareMapper(int size = 400)
    {
        Box viewport = new([new Point(0, 0), new Point(size, size)]);
        return new WorldToCanvasMapper(size, size, viewport);
    }

    public static WorldToCanvasMapper MapperFor(int canvasW, int canvasH, double worldW, double worldH, double blX = 0, double blY = 0)
    {
        Box viewport = new([new Point(blX, blY), new Point(blX + worldW, blY + worldH)]);
        return new WorldToCanvasMapper(canvasW, canvasH, viewport);
    }

    public static SKSurface CreateSurface(int width = 400, int height = 400, SKColor? clear = null)
    {
        SKSurface? surface = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Rgba8888, SKAlphaType.Premul));
        Assert.True(surface is not null, "SKSurface.Create returned null.");
        surface!.Canvas.Clear(clear ?? SKColors.White);
        return surface;
    }

    public static SKColor Sample(SKSurface surface, int x, int y)
    {
        surface.Canvas.Flush();
        using SKImage image = surface.Snapshot();
        using SKBitmap bitmap = SKBitmap.FromImage(image);
        return bitmap.GetPixel(x, y);
    }

    public static SKColor SampleWorld(SKSurface surface, WorldToCanvasMapper mapper, Point world, int canvasW = 400, int canvasH = 400)
    {
        Point canvas = mapper.MapWorldToCanvas(world);
        int x = Math.Clamp((int)Math.Round(canvas.X), 0, canvasW - 1);
        int y = Math.Clamp((int)Math.Round(canvas.Y), 0, canvasH - 1);
        return Sample(surface, x, y);
    }

    public static void AssertChannelHigh(byte channel, string name = "channel")
    {
        Assert.True(channel >= 200, $"Expected {name} >= 200 but was {channel}.");
    }

    public static void AssertChannelLow(byte channel, string name = "channel")
    {
        Assert.True(channel <= 55, $"Expected {name} <= 55 but was {channel}.");
    }

    public static void AssertWhite(SKColor color)
    {
        AssertChannelHigh(color.Red, "Red");
        AssertChannelHigh(color.Green, "Green");
        AssertChannelHigh(color.Blue, "Blue");
    }

    public static void AssertRed(SKColor color)
    {
        AssertChannelHigh(color.Red, "Red");
        AssertChannelLow(color.Green, "Green");
        AssertChannelLow(color.Blue, "Blue");
    }

    public static void AssertBlue(SKColor color)
    {
        AssertChannelLow(color.Red, "Red");
        AssertChannelLow(color.Green, "Green");
        AssertChannelHigh(color.Blue, "Blue");
    }

    public static void AssertGreenISH(SKColor color)
    {
        // SKColors.Green is (0,128,0); SKColors.Lime is (0,255,0). Accept both.
        AssertChannelLow(color.Red, "Red");
        Assert.True(color.Green >= 100, $"Expected Green >= 100 but was {color.Green}.");
        AssertChannelLow(color.Blue, "Blue");
    }

    public static void AssertYellow(SKColor color)
    {
        AssertChannelHigh(color.Red, "Red");
        AssertChannelHigh(color.Green, "Green");
        AssertChannelLow(color.Blue, "Blue");
    }
}
