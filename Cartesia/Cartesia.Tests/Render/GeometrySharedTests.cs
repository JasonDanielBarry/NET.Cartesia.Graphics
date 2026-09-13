using Cartesia.Core.Geometry;
using Cartesia.Render.Entities.Utilities;
using Cartesia.Tests.Helpers;
using SkiaSharp;

namespace Cartesia.Tests.Render;

public sealed class GeometrySharedTests
{
    [Fact]
    public void BuildSKPath_Open_ReturnsPathWithAllPoints()
    {
        var mapper = TestHelpers.SquareMapper(400);
        Point[] vertices = [new Point(50, 200), new Point(200, 350), new Point(300, 250)];

        using SKPath path = GeometryShared.BuildSKPath(false, vertices, new SKPathBuilder(), mapper);

        Assert.NotNull(path);
        Assert.Equal(3, path.PointCount);
    }

    [Fact]
    public void BuildSKPath_Closed_ReturnsPathWithAllPoints()
    {
        var mapper = TestHelpers.SquareMapper(400);
        Point[] vertices = [new Point(50, 300), new Point(200, 450), new Point(300, 350)];

        using SKPath path = GeometryShared.BuildSKPath(true, vertices, new SKPathBuilder(), mapper);

        Assert.NotNull(path);
        Assert.Equal(3, path.PointCount);
    }

    [Fact]
    public void BuildSKPath_MapsVerticesThroughMapper()
    {
        var mapper = TestHelpers.SquareMapper(400);
        Point[] vertices = [new Point(10, 20), new Point(30, 40)];

        using SKPath path = GeometryShared.BuildSKPath(false, vertices, new SKPathBuilder(), mapper);

        // World (10,20) -> canvas (10,380); world (30,40) -> canvas (30,360).
        SKPoint p0 = path.GetPoint(0);
        SKPoint p1 = path.GetPoint(1);

        Assert.Equal(10, p0.X);
        Assert.Equal(380, p0.Y);
        Assert.Equal(30, p1.X);
        Assert.Equal(360, p1.Y);
    }

    [Fact]
    public void BuildSKPath_SingleVertex_ProducesSinglePointPath()
    {
        var mapper = TestHelpers.SquareMapper(400);

        using SKPath path = GeometryShared.BuildSKPath(false, [new Point(7, 9)], new SKPathBuilder(), mapper);

        Assert.Equal(1, path.PointCount);
    }

    [Fact]
    public void BuildSKPath_EmptyVertices_Throws()
    {
        var mapper = TestHelpers.SquareMapper(400);

        Assert.Throws<IndexOutOfRangeException>(() =>
            GeometryShared.BuildSKPath(false, [], new SKPathBuilder(), mapper));
    }

    [Fact]
    public void BuildSKPath_BuilderIsReused_SecondBuildResetsFirst()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var builder = new SKPathBuilder();

        using SKPath first = GeometryShared.BuildSKPath(false, [new Point(0, 0), new Point(1, 1)], builder, mapper);
        using SKPath second = GeometryShared.BuildSKPath(false, [new Point(5, 5), new Point(6, 6), new Point(7, 7)], builder, mapper);

        Assert.Equal(2, first.PointCount);
        Assert.Equal(3, second.PointCount);
    }

    [Fact]
    public void BuildSKPath_OpenVsClosed_SameBounds()
    {
        var mapper = TestHelpers.SquareMapper(400);
        Point[] vertices = [new Point(50, 300), new Point(200, 450), new Point(300, 350)];

        using SKPath open = GeometryShared.BuildSKPath(false, vertices, new SKPathBuilder(), mapper);
        using SKPath closed = GeometryShared.BuildSKPath(true, vertices, new SKPathBuilder(), mapper);

        Assert.Equal(open.Bounds, closed.Bounds);
    }

    [Fact]
    public void BuildSKPath_PathsAreDrawable()
    {
        var mapper = TestHelpers.SquareMapper(400);
        Point[] vertices = [new Point(50, 200), new Point(200, 350), new Point(300, 250)];

        using SKSurface surface = TestHelpers.CreateSurface();
        using SKPath path = GeometryShared.BuildSKPath(false, vertices, new SKPathBuilder(), mapper);
        using SKPaint paint = new() { Color = SKColors.Red, Style = SKPaintStyle.Stroke, StrokeWidth = 3 };

        var ex = Record.Exception(() => surface.Canvas.DrawPath(path, paint));

        Assert.Null(ex);
    }

    [Fact]
    public void BuildSKPath_CloseFlag_LeavesPointSequenceUnchanged()
    {
        // SKPath exposes no closed-state query, so Close() is proven by pixels:
        // GraphicPolygon paints its closing edge, GraphicPolyline does not (see those suites).
        // Here pin that closing adds no points and keeps bounds.
        var mapper = TestHelpers.SquareMapper(400);
        Point[] vertices = [new Point(50, 300), new Point(200, 450), new Point(300, 350)];

        using SKPath open = GeometryShared.BuildSKPath(false, vertices, new SKPathBuilder(), mapper);
        using SKPath closed = GeometryShared.BuildSKPath(true, vertices, new SKPathBuilder(), mapper);

        Assert.Equal(open.PointCount, closed.PointCount);
        Assert.Equal(open.Bounds, closed.Bounds);
        for (int i = 0; i < vertices.Length; i++)
            Assert.Equal(open.GetPoint(i), closed.GetPoint(i));
    }

    [Fact]
    public void BuildSKPath_Detach_ReturnsFreshInstances()
    {
        var mapper = TestHelpers.SquareMapper(400);
        Point[] vertices = [new Point(0, 0), new Point(1, 1)];
        var builder = new SKPathBuilder();

        using SKPath first = GeometryShared.BuildSKPath(false, vertices, builder, mapper);
        using SKPath second = GeometryShared.BuildSKPath(false, vertices, builder, mapper);

        Assert.NotSame(first, second);
    }

    [Fact]
    public void BuildSKPath_SingleVertexClosed_NoThrow()
    {
        var mapper = TestHelpers.SquareMapper(400);

        using SKPath path = GeometryShared.BuildSKPath(true, [new Point(7, 9)], new SKPathBuilder(), mapper);

        Assert.Equal(1, path.PointCount);
    }

    [Fact]
    public void BuildSKPath_NullVertices_Throws()
    {
        var mapper = TestHelpers.SquareMapper(400);

        Assert.ThrowsAny<Exception>(() =>
            GeometryShared.BuildSKPath(false, null!, new SKPathBuilder(), mapper));
    }

    [Fact]
    public void BuildSKPath_NullBuilder_Throws()
    {
        var mapper = TestHelpers.SquareMapper(400);

        Assert.ThrowsAny<Exception>(() =>
            GeometryShared.BuildSKPath(false, [new Point(0, 0)], null!, mapper));
    }

    [Fact]
    public void BuildSKPath_NullMapper_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            GeometryShared.BuildSKPath(false, [new Point(0, 0)], new SKPathBuilder(), null!));
    }

    [Fact]
    public void BuildSKPath_ClosedNullVertices_Throws()
    {
        var mapper = TestHelpers.SquareMapper(400);

        Assert.ThrowsAny<Exception>(() =>
            GeometryShared.BuildSKPath(true, null!, new SKPathBuilder(), mapper));
    }
}
