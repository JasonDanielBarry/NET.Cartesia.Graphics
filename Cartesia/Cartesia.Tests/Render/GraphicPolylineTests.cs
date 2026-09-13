using Cartesia.Core.Geometry;
using Cartesia.Render.Entities.Geometry;
using Cartesia.Render.Rendering;
using Cartesia.Tests.Helpers;
using SkiaSharp;

namespace Cartesia.Tests.Render;

public sealed class GraphicPolylineTests
{
    private static readonly Brush AnyFill = new(SKColors.Transparent);

    private static Pen Stroke(double thickness = 5) => new(thickness, SKColors.Green, []);

    [Fact]
    public void BoundingBox_SpansAllVertices()
    {
        var line = new GraphicPolyline(
            [new Point(50, 200), new Point(200, 350), new Point(300, 250)], AnyFill, Stroke());

        var box = line.BoundingBox();

        TestHelpers.Near(new Point(50, 200), box.BottomLeft);
        TestHelpers.Near(new Point(300, 350), box.TopRight);
    }

    [Fact]
    public void Ctor_CopiesVertices_MutatingSourceHasNoEffect()
    {
        Point[] source = [new Point(0, 0), new Point(10, 10)];
        var line = new GraphicPolyline(source, AnyFill, Stroke());

        source[1] = new Point(999, 999);

        TestHelpers.Near(new Point(10, 10), line.BoundingBox().TopRight);
    }

    [Fact]
    public void Ctor_NullFill_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new GraphicPolyline([new Point(0, 0)], null!, Stroke()));
    }

    [Fact]
    public void Ctor_NullStroke_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new GraphicPolyline([new Point(0, 0)], AnyFill, null!));
    }

    [Fact]
    public void PrecomputeThenDraw_StrokesPathPixels()
    {
        var mapper = TestHelpers.SquareMapper(400);
        // World y=200 -> canvas y=200 horizontal segment from x=100..300.
        var line = new GraphicPolyline(
            [new Point(100, 200), new Point(300, 200)], AnyFill, Stroke());

        using SKSurface surface = TestHelpers.CreateSurface();
        line.Precompute(mapper);
        line.Draw(surface.Canvas);

        TestHelpers.AssertGreenISH(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void Draw_IgnoresFill_OpenPathInteriorUnpainted()
    {
        var mapper = TestHelpers.SquareMapper(400);
        // V shape; interior point below the V stays background because only the stroke paints.
        var line = new GraphicPolyline(
            [new Point(100, 100), new Point(200, 300), new Point(300, 100)],
            new Brush(SKColors.Red), Stroke(3));

        using SKSurface surface = TestHelpers.CreateSurface();
        line.Precompute(mapper);
        line.Draw(surface.Canvas);

        // On-path vertex paints...
        TestHelpers.AssertGreenISH(TestHelpers.SampleWorld(surface, mapper, new Point(200, 300)));
        // ...but the enclosed interior does not fill.
        TestHelpers.AssertWhite(TestHelpers.SampleWorld(surface, mapper, new Point(200, 150)));
    }

    [Fact]
    public void Draw_InvisibleStroke_PaintsNothing()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var line = new GraphicPolyline(
            [new Point(100, 200), new Point(300, 200)], AnyFill, Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        line.Precompute(mapper);
        line.Draw(surface.Canvas);

        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void Draw_WithoutPrecompute_Throws()
    {
        var line = new GraphicPolyline([new Point(0, 0), new Point(1, 1)], AnyFill, Stroke());

        using SKSurface surface = TestHelpers.CreateSurface();

        Assert.ThrowsAny<Exception>(() => line.Draw(surface.Canvas));
    }

    [Fact]
    public void Draw_MultiSegment_AllSegmentsPainted()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var line = new GraphicPolyline(
            [new Point(50, 350), new Point(200, 50), new Point(350, 350)], AnyFill, Stroke());

        using SKSurface surface = TestHelpers.CreateSurface();
        line.Precompute(mapper);
        line.Draw(surface.Canvas);

        // Apex world (200,50) -> canvas (200,350).
        TestHelpers.AssertGreenISH(TestHelpers.Sample(surface, 200, 350));
    }

    [Fact]
    public void BoundingBox_SingleVertex_ZeroSize()
    {
        var line = new GraphicPolyline([new Point(5, 5)], AnyFill, Stroke());

        var box = line.BoundingBox();

        TestHelpers.Near(0, box.Width);
        TestHelpers.Near(0, box.Height);
    }
}
