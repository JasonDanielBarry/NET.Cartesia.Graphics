using Cartesia.Core.Geometry;
using Cartesia.Render.Entities.Geometry;
using Cartesia.Render.Rendering;
using Cartesia.Tests.Helpers;
using SkiaSharp;

namespace Cartesia.Tests.Render;

public sealed class GraphicLineTests
{
    private static Pen Stroke(double thickness = 5) => new(thickness, SKColors.Blue, []);

    [Fact]
    public void BoundingBox_SpansBothEndpoints()
    {
        var line = new GraphicLine(new Point(50, 100), new Point(200, 250), Stroke());

        var box = line.BoundingBox();

        TestHelpers.Near(new Point(50, 100), box.BottomLeft);
        TestHelpers.Near(new Point(200, 250), box.TopRight);
    }

    [Fact]
    public void BoundingBox_ReversedEndpoints_Normalizes()
    {
        var line = new GraphicLine(new Point(200, 250), new Point(50, 100), Stroke());

        var box = line.BoundingBox();

        TestHelpers.Near(new Point(50, 100), box.BottomLeft);
        TestHelpers.Near(new Point(200, 250), box.TopRight);
    }

    [Fact]
    public void BoundingBox_DegeneratePoint_ZeroSize()
    {
        var line = new GraphicLine(new Point(7, 7), new Point(7, 7), Stroke());

        var box = line.BoundingBox();

        TestHelpers.Near(0, box.Width);
        TestHelpers.Near(0, box.Height);
    }

    [Fact]
    public void Ctor_NullStroke_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new GraphicLine(new Point(0, 0), new Point(1, 1), null!));
    }

    [Fact]
    public void PrecomputeThenDraw_PaintsLinePixels()
    {
        var mapper = TestHelpers.SquareMapper(400);
        // World (50,350)->canvas (50,50); world (350,50)->canvas (350,350). Midpoint canvas (200,200).
        var line = new GraphicLine(new Point(50, 350), new Point(350, 50), Stroke());

        using SKSurface surface = TestHelpers.CreateSurface();
        line.Precompute(mapper);
        line.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void Draw_OffLinePixels_StayBackground()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var line = new GraphicLine(new Point(50, 350), new Point(350, 50), Stroke());

        using SKSurface surface = TestHelpers.CreateSurface();
        line.Precompute(mapper);
        line.Draw(surface.Canvas);

        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 50));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 50, 350));
    }

    [Fact]
    public void Draw_HorizontalLine_PaintsRow()
    {
        var mapper = TestHelpers.SquareMapper(400);
        // World y=200 -> canvas y=200.
        var line = new GraphicLine(new Point(100, 200), new Point(300, 200), Stroke());

        using SKSurface surface = TestHelpers.CreateSurface();
        line.Precompute(mapper);
        line.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 150, 200));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 250, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 150, 210));
    }

    [Fact]
    public void Draw_VerticalLine_PaintsColumn()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var line = new GraphicLine(new Point(200, 100), new Point(200, 300), Stroke());

        using SKSurface surface = TestHelpers.CreateSurface();
        line.Precompute(mapper);
        line.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 150));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 250));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 210, 200));
    }

    [Fact]
    public void Draw_DashedStroke_PaintsSomethingWithoutThrowing()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var line = new GraphicLine(new Point(100, 200), new Point(300, 200), new Pen(5, SKColors.Red, [10, 10, 20, 20]));

        using SKSurface surface = TestHelpers.CreateSurface();
        line.Precompute(mapper);

        var ex = Record.Exception(() => line.Draw(surface.Canvas));

        Assert.Null(ex);
    }

    [Fact]
    public void Draw_WithoutPrecompute_DoesNotThrow_PaintsNothing()
    {
        // Endpoints default to (0,0); SKPaint exists from ctor, so no crash.
        // Zero-length line with default butt caps paints no pixel.
        var line = new GraphicLine(new Point(50, 350), new Point(350, 50), Stroke());

        using SKSurface surface = TestHelpers.CreateSurface();

        var ex = Record.Exception(() => line.Draw(surface.Canvas));

        Assert.Null(ex);
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 0, 0));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 1, 1));
    }

    [Fact]
    public void Draw_PenNone_PaintsNothing()
    {
        // GraphicLine.Draw has no IsVisible gate, but a transparent zero-width paint changes no pixel.
        var mapper = TestHelpers.SquareMapper(400);
        var line = new GraphicLine(new Point(50, 350), new Point(350, 50), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        line.Precompute(mapper);
        line.Draw(surface.Canvas);

        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 100, 100));
    }

    [Fact]
    public void BoundingBox_HorizontalLine_ZeroHeight()
    {
        var line = new GraphicLine(new Point(100, 200), new Point(300, 200), Stroke());

        var box = line.BoundingBox();

        TestHelpers.Near(200, box.Width);
        TestHelpers.Near(0, box.Height);
    }

    [Fact]
    public void BoundingBox_VerticalLine_ZeroWidth()
    {
        var line = new GraphicLine(new Point(200, 100), new Point(200, 300), Stroke());

        var box = line.BoundingBox();

        TestHelpers.Near(0, box.Width);
        TestHelpers.Near(200, box.Height);
    }

    [Fact]
    public void Draw_DashedStroke_PaintsDashesAndGaps()
    {
        var mapper = TestHelpers.SquareMapper(400);
        // Canvas row y=200, dash [10,10] phase 0: x 100..110 on, 110..120 off, 120..130 on.
        var line = new GraphicLine(
            new Point(100, 200), new Point(300, 200), new Pen(5, SKColors.Red, [10, 10]));

        using SKSurface surface = TestHelpers.CreateSurface();
        line.Precompute(mapper);
        line.Draw(surface.Canvas);

        TestHelpers.AssertRed(TestHelpers.Sample(surface, 105, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 115, 200));
        TestHelpers.AssertRed(TestHelpers.Sample(surface, 125, 200));
    }

    [Fact]
    public void Precompute_NullMapper_Throws()
    {
        var line = new GraphicLine(new Point(0, 0), new Point(1, 1), Stroke());

        Assert.ThrowsAny<Exception>(() => line.Precompute(null!));
    }

    [Fact]
    public void Draw_NullCanvas_Throws()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var line = new GraphicLine(new Point(0, 0), new Point(1, 1), Stroke());
        line.Precompute(mapper);

        Assert.ThrowsAny<Exception>(() => line.Draw(null!));
    }

    [Fact]
    public void Precompute_Twice_SecondWins()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var line = new GraphicLine(new Point(100, 200), new Point(300, 200), Stroke());

        using SKSurface surface = TestHelpers.CreateSurface();
        line.Precompute(mapper);
        line.Precompute(mapper);
        line.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 200));
    }
}
