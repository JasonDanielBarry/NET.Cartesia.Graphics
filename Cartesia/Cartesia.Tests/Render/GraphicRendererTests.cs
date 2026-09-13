using Cartesia.Core.Geometry;
using Cartesia.Core.Shapes;
using Cartesia.Render.Entities.Base;
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
        ];

        using SKSurface surface = TestHelpers.CreateSurface(800, 800);
        var renderer = new GraphicRenderer();
        renderer.SetEntities(entities);

        var ex = Record.Exception(() => renderer.Render(mapper, surface.Canvas));

        Assert.Null(ex);
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

    private static void AssertChannelHigh(byte channel, string name) =>
        Assert.True(channel >= 200, $"Expected {name} >= 200 but was {channel}.");

    private static void AssertChannelLow(byte channel, string name) =>
        Assert.True(channel <= 55, $"Expected {name} <= 55 but was {channel}.");
}
