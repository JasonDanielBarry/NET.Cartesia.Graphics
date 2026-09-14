using Cartesia.Core.Curve;
using Cartesia.Core.Geometry;
using Cartesia.Core.Shapes;
using Cartesia.Render.Entities.Curve;
using Cartesia.Render.Entities.Geometry;
using Cartesia.Render.Entities.Shape;
using Cartesia.Render.Rendering;
using Cartesia.Tests.Helpers;
using SkiaSharp;

namespace Cartesia.Tests.Render;

public sealed class GraphicEntityContractTests
{
    [Fact]
    public void Ctor_NullFill_ParamNameIsFillIn()
    {
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
            new GraphicPolyline([new Point(0, 0)], null!, new Pen(2, SKColors.Red, [])));

        Assert.Equal("fillIn", ex.ParamName);
    }

    [Fact]
    public void Ctor_NullStroke_ParamNameIsStrokeIn()
    {
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() =>
            new GraphicLine(new Point(0, 0), new Point(1, 1), null!));

        Assert.Equal("strokeIn", ex.ParamName);
    }

    [Fact]
    public void ShapeDraw_WipesCallerTransform_LineDraw_PreservesIt()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var line = new GraphicLine(new Point(100, 200), new Point(300, 200), new Pen(5, SKColors.Blue, []));

        using SKSurface surface = TestHelpers.CreateSurface();
        line.Precompute(mapper);
        surface.Canvas.Translate(10, 0);
        line.Draw(surface.Canvas);

        // GraphicLine.Draw never touches the matrix: caller transform survives.
        TestHelpers.Near(10, surface.Canvas.TotalMatrix.TransX, 1e-6);
    }

    [Fact]
    public void PolylineDraw_DoesNotTouchMatrix()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var polyline = new GraphicPolyline(
            [new Point(100, 200), new Point(300, 200)],
            new Brush(SKColors.Transparent), new Pen(5, SKColors.Green, []));

        using SKSurface surface = TestHelpers.CreateSurface();
        polyline.Precompute(mapper);
        surface.Canvas.Translate(10, 0);
        polyline.Draw(surface.Canvas);

        TestHelpers.Near(10, surface.Canvas.TotalMatrix.TransX, 1e-6);
    }

    [Fact]
    public void PolygonDraw_DoesNotTouchMatrix()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var polygon = new GraphicPolygon(
            [new Point(100, 100), new Point(300, 100), new Point(200, 300)],
            new Brush(SKColors.Yellow), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        polygon.Precompute(mapper);
        surface.Canvas.Translate(10, 0);
        polygon.Draw(surface.Canvas);

        TestHelpers.Near(10, surface.Canvas.TotalMatrix.TransX, 1e-6);
    }

    [Fact]
    public void KnownIssue_PrecomputeNullMapper_ThrowsNullReference()
    {
        var polyline = new GraphicPolyline(
            [new Point(0, 0), new Point(1, 1)],
            new Brush(SKColors.Transparent), new Pen(2, SKColors.Red, []));

        Assert.Throws<NullReferenceException>(() => polyline.Precompute(null!));
    }

    [Fact]
    public void KnownIssue_DrawNullCanvas_ThrowsNullReference()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var line = new GraphicLine(new Point(0, 0), new Point(1, 1), new Pen(2, SKColors.Red, []));
        line.Precompute(mapper);

        Assert.Throws<NullReferenceException>(() => line.Draw(null!));
    }

    [Fact]
    public void AllEntityTypes_PrecomputeThenDraw_DoNotThrow()
    {
        var mapper = TestHelpers.SquareMapper(400);
        Cartesia.Render.Entities.Base.GraphicEntity[] entities =
        [
            // Bezier first: its P0 butt pixel (100,300) sits inside the ellipse
            // fill below, which overpaints it; its apex (200,150) stays clear.
            new GraphicBezierCurve(new BezierCurveProperties(
                new Point(100, 100), new Point(100, 300),
                new Point(300, 300), new Point(300, 100)), new Pen(2, SKColors.Red, [])),
            new GraphicLine(new Point(0, 0), new Point(10, 10), new Pen(2, SKColors.Red, [])),
            new GraphicPolyline([new Point(0, 0), new Point(10, 10)], new Brush(SKColors.Transparent), new Pen(2, SKColors.Red, [])),
            new GraphicPolygon([new Point(0, 0), new Point(10, 0), new Point(5, 8)], new Brush(SKColors.Yellow), new Pen(2, SKColors.Red, [])),
            new GraphicRectangle(
                new RectangleProperties(20, 10, 0, 0, 0, HorizontalAlignment.Centre, VerticalAlignment.Centre, new Point(200, 200)),
                new Brush(SKColors.Blue), Pen.None),
            new GraphicEllipse(
                new EllipseProperties(20, 10, 0, HorizontalAlignment.Centre, VerticalAlignment.Centre, new Point(100, 100)),
                new Brush(SKColors.Blue), Pen.None),
            new GraphicArc(
                new ArcProperties(30, 30, -90, 90, 0, new Point(300, 300)),
                new Pen(2, SKColors.Red, [])),
        ];

        using SKSurface surface = TestHelpers.CreateSurface();

        foreach (Cartesia.Render.Entities.Base.GraphicEntity entity in entities)
        {
            entity.Precompute(mapper);
            var ex = Record.Exception(() => entity.Draw(surface.Canvas));
            Assert.Null(ex);
        }

        // Spot pixels: rectangle centre, ellipse centre, arc right rim all painted.
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 200));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 100, 300));
        TestHelpers.AssertRed(TestHelpers.Sample(surface, 330, 100));
        // Bezier apex world (200,250) -> canvas (200,150) painted.
        TestHelpers.AssertRed(TestHelpers.Sample(surface, 200, 150));
    }
}
