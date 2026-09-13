using Cartesia.Core.Geometry;
using Cartesia.Render.Entities.Geometry;
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
    public void AllEntityTypes_PrecomputeThenDraw_DoNotThrow()
    {
        var mapper = TestHelpers.SquareMapper(400);
        Cartesia.Render.Entities.Base.GraphicEntity[] entities =
        [
            new GraphicLine(new Point(0, 0), new Point(10, 10), new Pen(2, SKColors.Red, [])),
            new GraphicPolyline([new Point(0, 0), new Point(10, 10)], new Brush(SKColors.Transparent), new Pen(2, SKColors.Red, [])),
            new GraphicPolygon([new Point(0, 0), new Point(10, 0), new Point(5, 8)], new Brush(SKColors.Yellow), new Pen(2, SKColors.Red, [])),
        ];

        using SKSurface surface = TestHelpers.CreateSurface();

        foreach (Cartesia.Render.Entities.Base.GraphicEntity entity in entities)
        {
            entity.Precompute(mapper);
            var ex = Record.Exception(() => entity.Draw(surface.Canvas));
            Assert.Null(ex);
        }
    }
}
