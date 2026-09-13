using Cartesia.Core.Geometry;
using Cartesia.Render.Entities.Geometry;
using Cartesia.Render.Rendering;
using Cartesia.Tests.Helpers;
using SkiaSharp;

namespace Cartesia.Tests.Render;

public sealed class GraphicPolygonTests
{
    private static Point[] Triangle => [new Point(100, 100), new Point(300, 100), new Point(200, 300)];

    // Centroid world (200, ~167) -> canvas (200, ~233): solid interior on 400-viewport.
    private static readonly Point Interior = new(200, 167);

    [Fact]
    public void BoundingBox_SpansAllVertices()
    {
        var polygon = new GraphicPolygon(Triangle, new Brush(SKColors.Yellow), new Pen(3, SKColors.Purple, []));

        var box = polygon.BoundingBox();

        TestHelpers.Near(new Point(100, 100), box.BottomLeft);
        TestHelpers.Near(new Point(300, 300), box.TopRight);
    }

    [Fact]
    public void Ctor_CopiesVertices_MutatingSourceHasNoEffect()
    {
        Point[] source = [new Point(0, 0), new Point(10, 10), new Point(10, 0)];
        var polygon = new GraphicPolygon(source, new Brush(SKColors.Yellow), Pen.None);

        source[1] = new Point(999, 999);

        TestHelpers.Near(new Point(10, 10), polygon.BoundingBox().TopRight);
    }

    [Fact]
    public void Ctor_NullFill_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new GraphicPolygon(Triangle, null!, Pen.None));
    }

    [Fact]
    public void Ctor_NullStroke_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new GraphicPolygon(Triangle, new Brush(SKColors.Yellow), null!));
    }

    [Fact]
    public void Draw_FillOnly_PaintsInterior()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var polygon = new GraphicPolygon(Triangle, new Brush(SKColors.Yellow), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        polygon.Precompute(mapper);
        polygon.Draw(surface.Canvas);

        TestHelpers.AssertYellow(TestHelpers.SampleWorld(surface, mapper, Interior));
    }

    [Fact]
    public void Draw_StrokeOnly_PaintsBorderNotInterior()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var polygon = new GraphicPolygon(Triangle, Brush.None, new Pen(5, SKColors.Blue, []));

        using SKSurface surface = TestHelpers.CreateSurface();
        polygon.Precompute(mapper);
        polygon.Draw(surface.Canvas);

        // Base edge world y=100 -> canvas y=300; midpoint (200,300) is on the border.
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 300));
        TestHelpers.AssertWhite(TestHelpers.SampleWorld(surface, mapper, Interior));
    }

    [Fact]
    public void Draw_FillAndStroke_PaintsBoth()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var polygon = new GraphicPolygon(
            Triangle, new Brush(SKColors.Yellow), new Pen(5, SKColors.Blue, []));

        using SKSurface surface = TestHelpers.CreateSurface();
        polygon.Precompute(mapper);
        polygon.Draw(surface.Canvas);

        TestHelpers.AssertYellow(TestHelpers.SampleWorld(surface, mapper, Interior));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 300));
    }

    [Fact]
    public void Draw_NeitherVisible_PaintsNothing()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var polygon = new GraphicPolygon(Triangle, Brush.None, Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        polygon.Precompute(mapper);
        polygon.Draw(surface.Canvas);

        TestHelpers.AssertWhite(TestHelpers.SampleWorld(surface, mapper, Interior));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 300));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 10, 10));
    }

    [Fact]
    public void Draw_ExteriorPixels_StayBackground()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var polygon = new GraphicPolygon(Triangle, new Brush(SKColors.Yellow), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        polygon.Precompute(mapper);
        polygon.Draw(surface.Canvas);

        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 20, 20));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 380, 20));
    }

    [Fact]
    public void Draw_WithoutPrecompute_Throws()
    {
        var polygon = new GraphicPolygon(Triangle, new Brush(SKColors.Yellow), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();

        Assert.ThrowsAny<Exception>(() => polygon.Draw(surface.Canvas));
    }

    [Fact]
    public void Draw_ClosedPath_BaseEdgePainted()
    {
        // Closing edge (300,100)->(100,100) must exist: base midpoint painted with stroke-only.
        var mapper = TestHelpers.SquareMapper(400);
        var polygon = new GraphicPolygon(Triangle, Brush.None, new Pen(5, SKColors.Blue, []));

        using SKSurface surface = TestHelpers.CreateSurface();
        polygon.Precompute(mapper);
        polygon.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 300));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 150, 300));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 250, 300));
    }

    [Fact]
    public void Draw_ClosedPath_ClosingEdgePainted()
    {
        // Closing edge (200,300)-(100,100) midpoint world (150,200) -> canvas (150,200).
        // The open polyline leaves this white (see GraphicPolylineTests).
        var mapper = TestHelpers.SquareMapper(400);
        var polygon = new GraphicPolygon(Triangle, Brush.None, new Pen(5, SKColors.Blue, []));

        using SKSurface surface = TestHelpers.CreateSurface();
        polygon.Precompute(mapper);
        polygon.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 150, 200));
    }

    [Fact]
    public void BoundingBox_SingleVertex_ZeroSize()
    {
        var polygon = new GraphicPolygon([new Point(5, 5)], new Brush(SKColors.Yellow), Pen.None);

        var box = polygon.BoundingBox();

        TestHelpers.Near(0, box.Width);
        TestHelpers.Near(0, box.Height);
    }

    [Fact]
    public void BoundingBox_EmptyVertices_InvertedSentinelBox()
    {
        var polygon = new GraphicPolygon([], new Brush(SKColors.Yellow), Pen.None);

        TestHelpers.Near(-2e9, polygon.BoundingBox().Width);
    }

    [Fact]
    public void Precompute_EmptyVertices_ThrowsIndexOutOfRange()
    {
        var polygon = new GraphicPolygon([], new Brush(SKColors.Yellow), Pen.None);

        Assert.Throws<IndexOutOfRangeException>(() => polygon.Precompute(TestHelpers.SquareMapper(400)));
    }

    [Fact]
    public void Ctor_NullVertices_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new GraphicPolygon(null!, new Brush(SKColors.Yellow), Pen.None));
    }

    [Fact]
    public void Draw_TwoVertices_StrokesSegmentWithoutFill()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var polygon = new GraphicPolygon(
            [new Point(100, 300), new Point(300, 100)],
            new Brush(SKColors.Yellow), new Pen(5, SKColors.Blue, []));

        var box = polygon.BoundingBox();
        TestHelpers.Near(new Point(100, 100), box.BottomLeft);
        TestHelpers.Near(new Point(300, 300), box.TopRight);

        using SKSurface surface = TestHelpers.CreateSurface();
        polygon.Precompute(mapper);

        var ex = Record.Exception(() => polygon.Draw(surface.Canvas));

        Assert.Null(ex);
        // Segment world (100,300)-(300,100) passes through world (200,200) -> canvas (200,200).
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void Draw_SingleVertex_PaintsNothingWithoutThrowing()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var polygon = new GraphicPolygon([new Point(200, 200)], new Brush(SKColors.Yellow), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        polygon.Precompute(mapper);

        var ex = Record.Exception(() => polygon.Draw(surface.Canvas));

        Assert.Null(ex);
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void Precompute_NullMapper_Throws()
    {
        var polygon = new GraphicPolygon(Triangle, new Brush(SKColors.Yellow), Pen.None);

        Assert.ThrowsAny<Exception>(() => polygon.Precompute(null!));
    }
}
