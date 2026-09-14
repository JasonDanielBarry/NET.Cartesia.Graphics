using Cartesia.Core.Curve;
using Cartesia.Core.Geometry;
using Cartesia.Core.Shapes;
using Cartesia.Render.Entities.Base;
using Cartesia.Render.Entities.Curve;
using Cartesia.Render.Entities.Geometry;
using Cartesia.Render.Entities.Shape;
using Cartesia.Render.Renderer;
using Cartesia.Render.Rendering;
using Cartesia.Tests.Helpers;
using SkiaSharp;

namespace Cartesia.Tests.Render;

public sealed class GraphicRendererTests
{
    private static GraphicLine BlueDiagonal() =>
        new(new Point(50, 350), new Point(350, 50), new Pen(5, SKColors.Blue, []));

    private static GraphicPolygon YellowTriangle() =>
        new([new Point(100, 100), new Point(300, 100), new Point(200, 300)],
            new Brush(SKColors.Yellow), Pen.None);

    private static void RenderOn(IReadOnlyList<GraphicEntity> entities, SKSurface surface, int size = 400)
    {
        var renderer = new GraphicRenderer();
        renderer.SetEntities(entities);
        renderer.Render(TestHelpers.SquareMapper(size), surface.Canvas);
    }

    [Fact]
    public void Render_EmptyEntities_DoesNotThrow()
    {
        using SKSurface surface = TestHelpers.CreateSurface();

        var ex = Record.Exception(() => RenderOn([], surface));

        Assert.Null(ex);
    }

    [Fact]
    public void Render_SingleLine_PaintsPixels()
    {
        using SKSurface surface = TestHelpers.CreateSurface();
        RenderOn([BlueDiagonal()], surface);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void Render_MultipleEntities_AllPainted()
    {
        using SKSurface surface = TestHelpers.CreateSurface();
        RenderOn([BlueDiagonal(), YellowTriangle()], surface);

        // Diagonal at canvas (100,100) lies outside the triangle (apex x=200 at y=100);
        // (200,200) would be overpainted yellow since the triangle draws last.
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 100, 100));
        TestHelpers.AssertYellow(TestHelpers.SampleWorld(surface, TestHelpers.SquareMapper(), new Point(200, 167)));
    }

    [Fact]
    public void SetEntities_SnapshotsInput_LaterMutationsIgnored()
    {
        List<GraphicEntity> source = [BlueDiagonal()];
        var renderer = new GraphicRenderer();
        renderer.SetEntities(source);

        source.Add(YellowTriangle());

        using SKSurface surface = TestHelpers.CreateSurface();
        renderer.Render(TestHelpers.SquareMapper(), surface.Canvas);

        // Only the diagonal was snapshotted: triangle interior stays background.
        TestHelpers.AssertWhite(TestHelpers.SampleWorld(surface, TestHelpers.SquareMapper(), new Point(200, 167)));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void SetEntities_ReplacesPreviousEntities()
    {
        var renderer = new GraphicRenderer();
        renderer.SetEntities([BlueDiagonal()]);
        renderer.SetEntities([YellowTriangle()]);

        using SKSurface surface = TestHelpers.CreateSurface();
        renderer.Render(TestHelpers.SquareMapper(), surface.Canvas);

        TestHelpers.AssertYellow(TestHelpers.SampleWorld(surface, TestHelpers.SquareMapper(), new Point(200, 167)));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 100, 100));
    }

    [Fact]
    public void SetEntities_Null_ThrowsArgumentNullException()
    {
        var renderer = new GraphicRenderer();

        Assert.Throws<ArgumentNullException>(() => renderer.SetEntities(null!));
    }

    [Fact]
    public void Render_Twice_RepaintsBothTimes()
    {
        var renderer = new GraphicRenderer();
        renderer.SetEntities([BlueDiagonal()]);

        using SKSurface first = TestHelpers.CreateSurface();
        renderer.Render(TestHelpers.SquareMapper(), first.Canvas);
        using SKSurface second = TestHelpers.CreateSurface();
        renderer.Render(TestHelpers.SquareMapper(), second.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(first, 200, 200));
        TestHelpers.AssertBlue(TestHelpers.Sample(second, 200, 200));
    }

    [Fact]
    public void Render_PreservesDrawOrder_LaterEntityOverdraws()
    {
        var first = new GraphicPolygon(
            [new Point(100, 100), new Point(300, 100), new Point(300, 300), new Point(100, 300)],
            new Brush(SKColors.Yellow), Pen.None);
        var second = new GraphicPolygon(
            [new Point(150, 150), new Point(350, 150), new Point(350, 350), new Point(150, 350)],
            new Brush(SKColors.Cyan), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        RenderOn([first, second], surface);

        SKColor overlap = TestHelpers.SampleWorld(surface, TestHelpers.SquareMapper(), new Point(200, 200));
        AssertChannelHigh(overlap.Green, "Green");
        AssertChannelHigh(overlap.Blue, "Blue");
        AssertChannelLow(overlap.Red, "Red");
    }

    [Fact]
    public void Render_ManyEntities_ParallelPrecomputeCompletes()
    {
        var entities = new List<GraphicEntity>();
        for (int i = 0; i < 1000; i++)
        {
            entities.Add(new GraphicLine(
                new Point(i % 400, 0), new Point(i % 400, 400),
                new Pen(1, SKColors.Blue, [])));
        }

        using SKSurface surface = TestHelpers.CreateSurface();

        var ex = Record.Exception(() => RenderOn(entities, surface));

        Assert.Null(ex);
        // x=200 line (i=200,600) paints the column: every entity precomputed, none lost.
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void Render_FullScene_AllEntityKinds_NoThrow()
    {
        var mapper = TestHelpers.MapperFor(800, 800, 800, 800);
        GraphicEntity[] entities =
        [
            new GraphicArc(new ArcProperties(150, 100, -90, 90, 0, new Point(400, 600)), new Pen(3, SKColors.Red, [])),
            new GraphicEllipse(new EllipseProperties(150, 75, 15, HorizontalAlignment.Left, VerticalAlignment.Centre, new Point(600, 500)), new Brush(SKColors.Green), new Pen(5, SKColors.LightGray, [5, 5])),
            new GraphicLine(new Point(50, 100), new Point(200, 250), new Pen(3, SKColors.Blue, [])),
            new GraphicPolyline([new Point(50, 200), new Point(200, 350), new Point(300, 250)], new Brush(SKColors.Transparent), new Pen(3, SKColors.Green, [])),
            new GraphicPolygon([new Point(50, 300), new Point(200, 450), new Point(300, 350)], new Brush(SKColors.Yellow), new Pen(3, SKColors.Purple, [])),
            new GraphicRectangle(new RectangleProperties(150, 75, 15, 15, 30, HorizontalAlignment.Left, VerticalAlignment.Centre, new Point(600, 200)), new Brush(SKColors.Blue), new Pen(5, SKColors.OrangeRed, [])),
            // Bezier last: apex world (200,250) -> canvas (200,550) overpaints the
            // line endpoint there (red 5px over blue 3px); all other spot pixels
            // sit outside the bezier canvas footprint (x 100..300, y 100..300).
            new GraphicBezierCurve(new BezierCurveProperties(
                new Point(100, 100), new Point(100, 300),
                new Point(300, 300), new Point(300, 100)), new Pen(3, SKColors.Red, [])),
        ];

        using SKSurface surface = TestHelpers.CreateSurface(800, 800);
        var renderer = new GraphicRenderer();
        renderer.SetEntities(entities);

        var ex = Record.Exception(() => renderer.Render(mapper, surface.Canvas));

        Assert.Null(ex);

        // One pixel per entity kind proves each rendered (800-canvas, mapper above).
        surface.Canvas.Flush();
        using SKImage image = surface.Snapshot();
        using SKBitmap bitmap = SKBitmap.FromImage(image);
        // Line (50,100)-(200,250) midpoint world (125,175) -> canvas (125,625): blue.
        TestHelpers.AssertBlue(bitmap.GetPixel(125, 625));
        // Polyline segment-1 midpoint world (125,275) -> canvas (125,525): green
        // (vertex (200,450) sits inside the yellow polygon, so it cannot prove the polyline).
        TestHelpers.AssertGreenISH(bitmap.GetPixel(125, 525));
        // Polygon centroid world (183,367) -> canvas (183,433): yellow fill.
        TestHelpers.AssertYellow(bitmap.GetPixel(183, 433));
        // Rectangle box-centre world (675,200) rotated 30deg about handle (600,200)
        // -> world (665,237.5) -> canvas (665,562): blue fill.
        // (The handle itself is a box edge midpoint, unsafe to sample after rotation.)
        TestHelpers.AssertBlue(bitmap.GetPixel(665, 562));
        // Ellipse box-centre world (675,500) rotated 15deg about handle (600,500)
        // -> world (672.4,519.4) -> canvas (672,281): green fill.
        TestHelpers.AssertGreenISH(bitmap.GetPixel(672, 281));
        // Arc right half: world box (250,500)-(550,700) -> canvas x 250..550, y 100..300; rim (550,200) red.
        TestHelpers.AssertRed(bitmap.GetPixel(550, 200));
        // Bezier apex world (200,250) -> canvas (200,550): red.
        TestHelpers.AssertRed(bitmap.GetPixel(200, 550));
    }

    [Fact]
    public void Render_NullMapper_Throws()
    {
        var renderer = new GraphicRenderer();
        renderer.SetEntities([BlueDiagonal()]);

        using SKSurface surface = TestHelpers.CreateSurface();

        Assert.ThrowsAny<Exception>(() => renderer.Render(null!, surface.Canvas));
    }

    [Fact]
    public void Render_NullCanvas_Throws()
    {
        var renderer = new GraphicRenderer();
        renderer.SetEntities([BlueDiagonal()]);

        Assert.ThrowsAny<Exception>(() => renderer.Render(TestHelpers.SquareMapper(), null!));
    }

    [Fact]
    public void KnownIssue_RenderWithoutSetEntities_NeverTouchesNull()
    {
        // _entities is null! but _entityCount is 0, so neither loop body runs.
        using SKSurface surface = TestHelpers.CreateSurface();

        var ex = Record.Exception(() =>
            new GraphicRenderer().Render(TestHelpers.SquareMapper(), surface.Canvas));

        Assert.Null(ex);
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void KnownIssue_EmptyRender_MasksNullMapperAndCanvas()
    {
        // Zero-count loops never touch mapper/canvas: nulls are masked, not validated.
        var renderer = new GraphicRenderer();
        renderer.SetEntities([]);

        using SKSurface surface = TestHelpers.CreateSurface();

        Assert.Null(Record.Exception(() => renderer.Render(null!, surface.Canvas)));
        Assert.Null(Record.Exception(() => renderer.Render(TestHelpers.SquareMapper(), null!)));
    }

    [Fact]
    public void SetEntities_SnapshotsAgainstClearAndReplace()
    {
        List<GraphicEntity> source = [BlueDiagonal()];
        var renderer = new GraphicRenderer();
        renderer.SetEntities(source);

        source[0] = YellowTriangle();
        source.Clear();

        using SKSurface surface = TestHelpers.CreateSurface();
        renderer.Render(TestHelpers.SquareMapper(), surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 200));
        TestHelpers.AssertWhite(TestHelpers.SampleWorld(surface, TestHelpers.SquareMapper(), new Point(200, 167)));
    }

    [Fact]
    public void KnownIssue_ShapeDraw_WipesCallerTransformForLaterEntities()
    {
        // Caller translates, then shape.Draw calls ResetMatrix: the line draws UNSHIFTED.
        var rect = new GraphicRectangle(
            new RectangleProperties(100, 60, 0, 0, 0,
                HorizontalAlignment.Centre, VerticalAlignment.Centre, new Point(200, 200)),
            new Brush(SKColors.Blue), Pen.None);
        var renderer = new GraphicRenderer();
        renderer.SetEntities([rect, BlueDiagonal()]);

        using SKSurface surface = TestHelpers.CreateSurface();
        surface.Canvas.Translate(50, 0);
        renderer.Render(TestHelpers.SquareMapper(), surface.Canvas);

        // Unshifted diagonal passes (300,300); shifted would pass (100,50) instead.
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 300, 300));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 100, 50));
        SKMatrix m = surface.Canvas.TotalMatrix;
        TestHelpers.Near(1, m.ScaleX, 1e-6);
        TestHelpers.Near(0, m.TransX, 1e-6);
    }

    private static void AssertChannelHigh(byte channel, string name) =>
        Assert.True(channel >= 200, $"Expected {name} >= 200 but was {channel}.");

    private static void AssertChannelLow(byte channel, string name) =>
        Assert.True(channel <= 55, $"Expected {name} <= 55 but was {channel}.");
}
