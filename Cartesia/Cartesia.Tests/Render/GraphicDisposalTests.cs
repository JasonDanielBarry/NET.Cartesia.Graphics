using System.Reflection;
using Cartesia.Core.Curve;
using Cartesia.Core.Geometry;
using Cartesia.Core.Mapping;
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

public sealed class GraphicDisposalTests
{
    private static IntPtr HandleOf(object skObject) =>
        (IntPtr)skObject.GetType().GetProperty("Handle", BindingFlags.Public | BindingFlags.Instance)!.GetValue(skObject)!;

    private static T Field<T>(object target, string name) where T : class
    {
        Type? t = target.GetType();
        while (t is not null)
        {
            FieldInfo? f = t.GetField(name, BindingFlags.NonPublic | BindingFlags.Instance);
            if (f is not null)
                return (T)f.GetValue(target)!;
            t = t.BaseType;
        }
        throw new MissingFieldException(name);
    }

    private static SKPaint FillOf(GraphicEntity e) => Field<SKPaint>(e, "_fillPaint");
    private static SKPaint StrokeOf(GraphicEntity e) => Field<SKPaint>(e, "_strokePaint");
    private static bool IsDisposedFlag(GraphicEntity e)
    {
        Type? t = typeof(GraphicEntity);
        FieldInfo f = t.GetField("_isDisposed", BindingFlags.NonPublic | BindingFlags.Instance)!;
        return (bool)f.GetValue(e)!;
    }

    private static GraphicLine NewLine() =>
        new(new Point(50, 350), new Point(350, 50), new Pen(5, SKColors.Blue, []));
    private static GraphicPolyline NewPolyline() =>
        new([new Point(100, 200), new Point(300, 200)], Brush.None, new Pen(5, SKColors.Green, []));
    private static GraphicPolygon NewStrokeOnlyPolygon() =>
        new([new Point(100, 100), new Point(300, 100), new Point(200, 300)], Brush.None, new Pen(5, SKColors.Blue, []));
    private static GraphicPolygon NewFillPolygon() =>
        new([new Point(100, 100), new Point(300, 100), new Point(200, 300)], new Brush(SKColors.Yellow), new Pen(5, SKColors.Blue, []));
    private static GraphicRectangle NewRectangle() =>
        new(new RectangleProperties(100, 60, 0, 0, 0, HorizontalAlignment.Centre, VerticalAlignment.Centre, new Point(200, 200)), new Brush(SKColors.Blue), new Pen(5, SKColors.Red, []));
    private static GraphicEllipse NewStrokeOnlyEllipse() =>
        new(new EllipseProperties(100, 60, 0, HorizontalAlignment.Centre, VerticalAlignment.Centre, new Point(200, 200)), Brush.None, new Pen(5, SKColors.Red, []));
    private static GraphicEllipse NewFillEllipse() =>
        new(new EllipseProperties(100, 60, 0, HorizontalAlignment.Centre, VerticalAlignment.Centre, new Point(200, 200)), new Brush(SKColors.Blue), new Pen(5, SKColors.Red, []));
    private static GraphicArc NewArc() =>
        new(new ArcProperties(100, 100, -90, 90, 0, new Point(200, 200)), new Pen(5, SKColors.Red, []));
    private static GraphicBezierCurve NewBezier() =>
        new(new BezierCurveProperties(new Point(100, 100), new Point(100, 300), new Point(300, 300), new Point(300, 100)), new Pen(5, SKColors.Red, []));

    private static GraphicEntity[] AllKinds() =>
    [
        NewLine(),
        NewPolyline(),
        NewStrokeOnlyPolygon(),
        NewRectangle(),
        NewFillEllipse(),
        NewArc(),
        NewBezier(),
    ];

    [Fact]
    public void AllEntityTypes_ImplementIDisposable()
    {
        foreach (GraphicEntity e in AllKinds())
        {
            Assert.Contains(typeof(IDisposable), e.GetType().GetInterfaces());
            e.Dispose();
        }
        Assert.Contains(typeof(IDisposable), typeof(GraphicEntity).GetInterfaces());
    }

    [Fact]
    public void Ctor_CreatesBothPaintsEagerly()
    {
        foreach (GraphicEntity e in AllKinds())
        {
            Assert.NotEqual(IntPtr.Zero, HandleOf(FillOf(e)));
            Assert.NotEqual(IntPtr.Zero, HandleOf(StrokeOf(e)));
            e.Dispose();
        }
    }

    [Fact]
    public void Dispose_IsIdempotent_AllKinds()
    {
        foreach (GraphicEntity e in AllKinds())
        {
            var ex = Record.Exception(() => { e.Dispose(); e.Dispose(); e.Dispose(); });
            Assert.Null(ex);
            Assert.True(IsDisposedFlag(e));
        }
    }

    [Fact]
    public void UsingPattern_SetsDisposedFlag()
    {
        GraphicLine line;
        using (line = NewLine())
        {
            Assert.False(IsDisposedFlag(line));
        }
        Assert.True(IsDisposedFlag(line));
    }

    [Fact]
    public void Dispose_SetsIsDisposedFlag()
    {
        var line = NewLine();
        Assert.False(IsDisposedFlag(line));
        line.Dispose();
        Assert.True(IsDisposedFlag(line));
    }

    [Fact]
    public void BoundingBox_AfterDispose_StillWorks_AllKinds()
    {
        var line = NewLine();
        var before = line.BoundingBox();
        line.Dispose();
        var after = line.BoundingBox();
        TestHelpers.Near(before.BottomLeft, after.BottomLeft);
        TestHelpers.Near(before.TopRight, after.TopRight);

        var poly = NewStrokeOnlyPolygon();
        var pb = poly.BoundingBox();
        poly.Dispose();
        var pa = poly.BoundingBox();
        TestHelpers.Near(pb.BottomLeft, pa.BottomLeft);

        var rect = NewRectangle();
        var rb = rect.BoundingBox();
        rect.Dispose();
        TestHelpers.Near(rb.BottomLeft, rect.BoundingBox().BottomLeft);

        var ell = NewFillEllipse();
        var eb = ell.BoundingBox();
        ell.Dispose();
        TestHelpers.Near(eb.BottomLeft, ell.BoundingBox().BottomLeft);

        var arc = NewArc();
        var ab = arc.BoundingBox();
        arc.Dispose();
        TestHelpers.Near(ab.BottomLeft, arc.BoundingBox().BottomLeft);

        var bez = NewBezier();
        var bb = bez.BoundingBox();
        bez.Dispose();
        TestHelpers.Near(bb.BottomLeft, bez.BoundingBox().BottomLeft);

        var pl = NewPolyline();
        var lb = pl.BoundingBox();
        pl.Dispose();
        TestHelpers.Near(lb.BottomLeft, pl.BoundingBox().BottomLeft);
    }

    [Fact]
    public void CoreTypes_DoNotImplementIDisposable()
    {
        Assert.False(typeof(Box).GetInterfaces().Contains(typeof(IDisposable)));
        Assert.False(typeof(Point).GetInterfaces().Contains(typeof(IDisposable)));
        Assert.False(typeof(WorldToCanvasMapper).GetInterfaces().Contains(typeof(IDisposable)));
        Assert.False(typeof(Cartesia.Core.Math.AffineTransform).GetInterfaces().Contains(typeof(IDisposable)));
    }

    [Fact]
    public void Pen_Brush_Renderer_DoNotImplementIDisposable()
    {
        Assert.False(typeof(Pen).GetInterfaces().Contains(typeof(IDisposable)));
        Assert.False(typeof(Brush).GetInterfaces().Contains(typeof(IDisposable)));
        Assert.False(typeof(GraphicRenderer).GetInterfaces().Contains(typeof(IDisposable)));
    }

    [Fact]
    public void DisposeResources_IsVirtual_And_OverridesExist()
    {
        var baseMethod = typeof(GraphicEntity).GetMethod("DisposeResources", BindingFlags.NonPublic | BindingFlags.Instance)!;
        Assert.True(baseMethod.IsVirtual);
        foreach (Type t in new[] { typeof(GraphicPolyline), typeof(GraphicPolygon), typeof(GraphicBezierCurve), typeof(GraphicRectangle) })
        {
            var m = t.GetMethod("DisposeResources", BindingFlags.NonPublic | BindingFlags.Instance)!;
            Assert.Equal(t, m.DeclaringType);
        }
        foreach (Type t in new[] { typeof(GraphicLine), typeof(GraphicEllipse), typeof(GraphicArc) })
        {
            var m = t.GetMethod("DisposeResources", BindingFlags.NonPublic | BindingFlags.Instance)!;
            Assert.Equal(typeof(GraphicEntity), m.DeclaringType);
        }
    }

    [Fact]
    public void KnownIssue_NoFinalizer_SuppressFinalizeIsNoop()
    {
        var fin = typeof(GraphicEntity).GetMethod("Finalize", BindingFlags.NonPublic | BindingFlags.Instance)!;
        Assert.Equal(typeof(object), fin.DeclaringType);
    }

    [Fact]
    public void ShouldBe_Dispose_DisposesStrokePaint_AllKinds()
    {
        foreach (GraphicEntity entity in AllKinds())
        {
            entity.Dispose();
            Assert.Equal(IntPtr.Zero, HandleOf(StrokeOf(entity)));
        }
    }

    [Fact]
    public void KnownIssue_Dispose_LeavesStrokePaintAlive_Line()
    {
        var line = NewLine();
        line.Dispose();
        Assert.Equal(IntPtr.Zero, HandleOf(FillOf(line)));
        Assert.NotEqual(IntPtr.Zero, HandleOf(StrokeOf(line)));
    }

    [Fact]
    public void KnownIssue_Dispose_LeavesStrokePaintAlive_Polyline()
    {
        var e = NewPolyline();
        e.Dispose();
        Assert.Equal(IntPtr.Zero, HandleOf(FillOf(e)));
        Assert.NotEqual(IntPtr.Zero, HandleOf(StrokeOf(e)));
    }

    [Fact]
    public void KnownIssue_Dispose_LeavesStrokePaintAlive_Polygon()
    {
        var e = NewStrokeOnlyPolygon();
        e.Dispose();
        Assert.Equal(IntPtr.Zero, HandleOf(FillOf(e)));
        Assert.NotEqual(IntPtr.Zero, HandleOf(StrokeOf(e)));
    }

    [Fact]
    public void KnownIssue_Dispose_LeavesStrokePaintAlive_Rectangle()
    {
        var e = NewRectangle();
        e.Dispose();
        Assert.Equal(IntPtr.Zero, HandleOf(FillOf(e)));
        Assert.NotEqual(IntPtr.Zero, HandleOf(StrokeOf(e)));
    }

    [Fact]
    public void KnownIssue_Dispose_LeavesStrokePaintAlive_Ellipse()
    {
        var e = NewFillEllipse();
        e.Dispose();
        Assert.Equal(IntPtr.Zero, HandleOf(FillOf(e)));
        Assert.NotEqual(IntPtr.Zero, HandleOf(StrokeOf(e)));
    }

    [Fact]
    public void KnownIssue_Dispose_LeavesStrokePaintAlive_Arc()
    {
        var e = NewArc();
        e.Dispose();
        Assert.Equal(IntPtr.Zero, HandleOf(FillOf(e)));
        Assert.NotEqual(IntPtr.Zero, HandleOf(StrokeOf(e)));
    }

    [Fact]
    public void KnownIssue_Dispose_LeavesStrokePaintAlive_Bezier()
    {
        var e = NewBezier();
        e.Dispose();
        Assert.Equal(IntPtr.Zero, HandleOf(FillOf(e)));
        Assert.NotEqual(IntPtr.Zero, HandleOf(StrokeOf(e)));
    }

    [Fact]
    public void KnownIssue_DoubleDispose_OfSameFillPaint_IsSilent()
    {
        var line = NewLine();
        var ex = Record.Exception(() => line.Dispose());
        Assert.Null(ex);
        Assert.Equal(IntPtr.Zero, HandleOf(FillOf(line)));
    }

    [Fact]
    public void ShouldBe_Line_DrawAfterDispose_DoesNotPaint()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var line = NewLine();
        line.Dispose();
        line.Precompute(mapper);
        using SKSurface surface = TestHelpers.CreateSurface();
        line.Draw(surface.Canvas);
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void KnownIssue_Line_DrawAfterDispose_StillPaints()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var line = NewLine();
        line.Dispose();
        line.Precompute(mapper);
        using SKSurface surface = TestHelpers.CreateSurface();
        line.Draw(surface.Canvas);
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void ShouldBe_Polyline_StrokeOnly_DrawAfterDispose_DoesNotPaint()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewPolyline();
        e.Precompute(mapper);
        e.Dispose();
        using SKSurface surface = TestHelpers.CreateSurface();
        e.Draw(surface.Canvas);
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void KnownIssue_Polyline_StrokeOnly_DrawAfterDispose_StillPaints()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewPolyline();
        e.Precompute(mapper);
        e.Dispose();
        using SKSurface surface = TestHelpers.CreateSurface();
        e.Draw(surface.Canvas);
        TestHelpers.AssertGreenISH(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void ShouldBe_Polygon_StrokeOnly_DrawAfterDispose_DoesNotPaint()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewStrokeOnlyPolygon();
        e.Precompute(mapper);
        e.Dispose();
        using SKSurface surface = TestHelpers.CreateSurface();
        e.Draw(surface.Canvas);
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 300));
    }

    [Fact]
    public void KnownIssue_Polygon_StrokeOnly_DrawAfterDispose_StillPaints()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewStrokeOnlyPolygon();
        e.Precompute(mapper);
        e.Dispose();
        using SKSurface surface = TestHelpers.CreateSurface();
        e.Draw(surface.Canvas);
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 300));
    }

    [Fact]
    public void ShouldBe_Bezier_StrokeOnly_DrawAfterDispose_DoesNotPaint()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewBezier();
        e.Precompute(mapper);
        e.Dispose();
        using SKSurface surface = TestHelpers.CreateSurface();
        e.Draw(surface.Canvas);
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 150));
    }

    [Fact]
    public void KnownIssue_Bezier_StrokeOnly_DrawAfterDispose_StillPaints()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewBezier();
        e.Precompute(mapper);
        e.Dispose();
        using SKSurface surface = TestHelpers.CreateSurface();
        e.Draw(surface.Canvas);
        TestHelpers.AssertRed(TestHelpers.Sample(surface, 200, 150));
    }

    [Fact]
    public void ShouldBe_Arc_DrawAfterDispose_DoesNotPaint()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewArc();
        e.Dispose();
        e.Precompute(mapper);
        using SKSurface surface = TestHelpers.CreateSurface();
        e.Draw(surface.Canvas);
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 299, 200));
    }

    [Fact]
    public void KnownIssue_Arc_DrawAfterDispose_StillPaints()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewArc();
        e.Dispose();
        e.Precompute(mapper);
        using SKSurface surface = TestHelpers.CreateSurface();
        e.Draw(surface.Canvas);
        TestHelpers.AssertRed(TestHelpers.Sample(surface, 299, 200));
    }

    [Fact]
    public void ShouldBe_Ellipse_StrokeOnly_DrawAfterDispose_DoesNotPaint()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewStrokeOnlyEllipse();
        e.Dispose();
        e.Precompute(mapper);
        using SKSurface surface = TestHelpers.CreateSurface();
        e.Draw(surface.Canvas);
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 170));
    }

    [Fact]
    public void KnownIssue_Ellipse_StrokeOnly_DrawAfterDispose_StillPaints()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewStrokeOnlyEllipse();
        e.Dispose();
        e.Precompute(mapper);
        using SKSurface surface = TestHelpers.CreateSurface();
        e.Draw(surface.Canvas);
        TestHelpers.AssertRed(TestHelpers.Sample(surface, 200, 170));
    }

    [Fact]
    public void KnownIssue_Polyline_Draw_DisposesPath_SingleUse()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewPolyline();
        e.Precompute(mapper);
        SKPath path = Field<SKPath>(e, "_path");
        Assert.NotEqual(IntPtr.Zero, HandleOf(path));
        using SKSurface surface = TestHelpers.CreateSurface();
        e.Draw(surface.Canvas);
        Assert.Equal(IntPtr.Zero, HandleOf(path));
    }

    [Fact]
    public void KnownIssue_Polygon_Draw_DisposesPath_SingleUse()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewStrokeOnlyPolygon();
        e.Precompute(mapper);
        SKPath path = Field<SKPath>(e, "_path");
        Assert.NotEqual(IntPtr.Zero, HandleOf(path));
        using SKSurface surface = TestHelpers.CreateSurface();
        e.Draw(surface.Canvas);
        Assert.Equal(IntPtr.Zero, HandleOf(path));
    }

    [Fact]
    public void KnownIssue_Bezier_Draw_DisposesPath_SingleUse()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewBezier();
        e.Precompute(mapper);
        SKPath path = Field<SKPath>(e, "_curvePath");
        Assert.NotEqual(IntPtr.Zero, HandleOf(path));
        using SKSurface surface = TestHelpers.CreateSurface();
        e.Draw(surface.Canvas);
        Assert.Equal(IntPtr.Zero, HandleOf(path));
    }

    [Fact]
    public void ShouldBe_Draw_PreservesPathForRedraw_Polyline()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewPolyline();
        e.Precompute(mapper);
        using SKSurface surface = TestHelpers.CreateSurface();
        e.Draw(surface.Canvas);
        TestHelpers.AssertGreenISH(TestHelpers.Sample(surface, 200, 200));
        SKPath path = Field<SKPath>(e, "_path");
        Assert.NotEqual(IntPtr.Zero, HandleOf(path));
    }

    [Fact]
    public void ShouldBe_Draw_PreservesPathForRedraw_Polygon()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewStrokeOnlyPolygon();
        e.Precompute(mapper);
        using SKSurface surface = TestHelpers.CreateSurface();
        e.Draw(surface.Canvas);
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 300));
        SKPath path = Field<SKPath>(e, "_path");
        Assert.NotEqual(IntPtr.Zero, HandleOf(path));
    }

    [Fact]
    public void ShouldBe_Draw_PreservesPathForRedraw_Bezier()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewBezier();
        e.Precompute(mapper);
        using SKSurface surface = TestHelpers.CreateSurface();
        e.Draw(surface.Canvas);
        TestHelpers.AssertRed(TestHelpers.Sample(surface, 200, 150));
        SKPath path = Field<SKPath>(e, "_curvePath");
        Assert.NotEqual(IntPtr.Zero, HandleOf(path));
    }

    [Fact]
    public void KnownIssue_DrawTwiceWithoutPrecompute_WouldUseDisposedPath()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewBezier();
        e.Precompute(mapper);
        using SKSurface surface = TestHelpers.CreateSurface();
        e.Draw(surface.Canvas);
        Assert.Equal(IntPtr.Zero, HandleOf(Field<SKPath>(e, "_curvePath")));
    }

    [Fact]
    public void ShouldBe_Precompute_Twice_DisposesFirstPath_Polyline()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewPolyline();
        e.Precompute(mapper);
        SKPath first = Field<SKPath>(e, "_path");
        e.Precompute(mapper);
        Assert.Equal(IntPtr.Zero, HandleOf(first));
        using SKSurface surface = TestHelpers.CreateSurface();
        e.Draw(surface.Canvas);
    }

    [Fact]
    public void KnownIssue_Precompute_Twice_LeaksFirstPath_Polyline()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewPolyline();
        e.Precompute(mapper);
        SKPath first = Field<SKPath>(e, "_path");
        e.Precompute(mapper);
        SKPath second = Field<SKPath>(e, "_path");
        Assert.NotSame(first, second);
        Assert.NotEqual(IntPtr.Zero, HandleOf(first));
        Assert.NotEqual(IntPtr.Zero, HandleOf(second));
        using SKSurface surface = TestHelpers.CreateSurface();
        e.Draw(surface.Canvas);
    }

    [Fact]
    public void ShouldBe_Precompute_Twice_DisposesFirstPath_Bezier()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewBezier();
        e.Precompute(mapper);
        SKPath first = Field<SKPath>(e, "_curvePath");
        e.Precompute(mapper);
        Assert.Equal(IntPtr.Zero, HandleOf(first));
        using SKSurface surface = TestHelpers.CreateSurface();
        e.Draw(surface.Canvas);
    }

    [Fact]
    public void KnownIssue_Precompute_Twice_LeaksFirstPath_Bezier()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewBezier();
        e.Precompute(mapper);
        SKPath first = Field<SKPath>(e, "_curvePath");
        e.Precompute(mapper);
        SKPath second = Field<SKPath>(e, "_curvePath");
        Assert.NotSame(first, second);
        Assert.NotEqual(IntPtr.Zero, HandleOf(first));
        using SKSurface surface = TestHelpers.CreateSurface();
        e.Draw(surface.Canvas);
    }

    [Fact]
    public void ShouldBe_Precompute_Twice_DisposesFirstPath_Polygon()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewStrokeOnlyPolygon();
        e.Precompute(mapper);
        SKPath first = Field<SKPath>(e, "_path");
        e.Precompute(mapper);
        Assert.Equal(IntPtr.Zero, HandleOf(first));
        using SKSurface surface = TestHelpers.CreateSurface();
        e.Draw(surface.Canvas);
    }

    [Fact]
    public void KnownIssue_Precompute_Twice_LeaksFirstPath_Polygon()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewStrokeOnlyPolygon();
        e.Precompute(mapper);
        SKPath first = Field<SKPath>(e, "_path");
        e.Precompute(mapper);
        Assert.NotEqual(IntPtr.Zero, HandleOf(first));
        using SKSurface surface = TestHelpers.CreateSurface();
        e.Draw(surface.Canvas);
    }

    [Fact]
    public void ShouldBe_Dispose_DisposesPendingPath_Polyline()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewPolyline();
        e.Precompute(mapper);
        SKPath pending = Field<SKPath>(e, "_path");
        e.Dispose();
        Assert.Equal(IntPtr.Zero, HandleOf(pending));
    }

    [Fact]
    public void KnownIssue_Dispose_LeavesPendingPathAlive_Polyline()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewPolyline();
        e.Precompute(mapper);
        SKPath pending = Field<SKPath>(e, "_path");
        e.Dispose();
        Assert.NotEqual(IntPtr.Zero, HandleOf(pending));
    }

    [Fact]
    public void ShouldBe_Dispose_DisposesPendingPath_Polygon()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewStrokeOnlyPolygon();
        e.Precompute(mapper);
        SKPath pending = Field<SKPath>(e, "_path");
        e.Dispose();
        Assert.Equal(IntPtr.Zero, HandleOf(pending));
    }

    [Fact]
    public void KnownIssue_Dispose_LeavesPendingPathAlive_Polygon()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewStrokeOnlyPolygon();
        e.Precompute(mapper);
        SKPath pending = Field<SKPath>(e, "_path");
        e.Dispose();
        Assert.NotEqual(IntPtr.Zero, HandleOf(pending));
    }

    [Fact]
    public void ShouldBe_Dispose_DisposesPendingPath_Bezier()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewBezier();
        e.Precompute(mapper);
        SKPath pending = Field<SKPath>(e, "_curvePath");
        e.Dispose();
        Assert.Equal(IntPtr.Zero, HandleOf(pending));
    }

    [Fact]
    public void KnownIssue_Dispose_LeavesPendingPathAlive_Bezier()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewBezier();
        e.Precompute(mapper);
        SKPath pending = Field<SKPath>(e, "_curvePath");
        e.Dispose();
        Assert.NotEqual(IntPtr.Zero, HandleOf(pending));
    }

    [Fact]
    public void Dispose_DisposesBuilder_Polyline()
    {
        var e = NewPolyline();
        Assert.NotEqual(IntPtr.Zero, HandleOf(Field<SKPathBuilder>(e, "_pathBuilder")));
        e.Dispose();
        Assert.Equal(IntPtr.Zero, HandleOf(Field<SKPathBuilder>(e, "_pathBuilder")));
    }

    [Fact]
    public void Dispose_DisposesBuilder_Polygon()
    {
        var e = NewStrokeOnlyPolygon();
        e.Dispose();
        Assert.Equal(IntPtr.Zero, HandleOf(Field<SKPathBuilder>(e, "_pathBuilder")));
    }

    [Fact]
    public void Dispose_DisposesBuilder_Bezier()
    {
        var e = NewBezier();
        e.Dispose();
        Assert.Equal(IntPtr.Zero, HandleOf(Field<SKPathBuilder>(e, "_pathBuilder")));
    }

    [Fact]
    public void KnownIssue_DisposedBuilder_PrecomputeWouldCrash_Polyline()
    {
        var e = NewPolyline();
        e.Dispose();
        Assert.Equal(IntPtr.Zero, HandleOf(Field<SKPathBuilder>(e, "_pathBuilder")));
    }

    [Fact]
    public void KnownIssue_DisposedBuilder_PrecomputeWouldCrash_Bezier()
    {
        var e = NewBezier();
        e.Dispose();
        Assert.Equal(IntPtr.Zero, HandleOf(Field<SKPathBuilder>(e, "_pathBuilder")));
    }

    [Fact]
    public void Dispose_DisposesRoundRect_Rectangle()
    {
        var e = NewRectangle();
        Assert.NotEqual(IntPtr.Zero, HandleOf(Field<SKRoundRect>(e, "_roundRect")));
        e.Dispose();
        Assert.Equal(IntPtr.Zero, HandleOf(Field<SKRoundRect>(e, "_roundRect")));
    }

    [Fact]
    public void KnownIssue_DisposedRoundRect_PrecomputeWouldCrash_Rectangle()
    {
        var e = NewRectangle();
        e.Dispose();
        Assert.Equal(IntPtr.Zero, HandleOf(Field<SKRoundRect>(e, "_roundRect")));
    }

    [Fact]
    public void KnownIssue_DisposedPaint_DrawWouldCrash_RectangleFill()
    {
        var e = NewRectangle();
        e.Dispose();
        Assert.Equal(IntPtr.Zero, HandleOf(FillOf(e)));
        Assert.NotEqual(IntPtr.Zero, HandleOf(StrokeOf(e)));
    }

    [Fact]
    public void Renderer_DoesNotDisposeEntities_LineStaysAlive()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var line = NewLine();
        var renderer = new GraphicRenderer();
        renderer.SetEntities([line]);
        using SKSurface surface = TestHelpers.CreateSurface();
        renderer.Render(mapper, surface.Canvas);
        Assert.NotEqual(IntPtr.Zero, HandleOf(FillOf(line)));
        Assert.NotEqual(IntPtr.Zero, HandleOf(StrokeOf(line)));
        Assert.False(IsDisposedFlag(line));
        line.Dispose();
    }

    [Fact]
    public void Renderer_RenderTwice_PolylineRepaintsWithoutThrowing()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var e = NewPolyline();
        var renderer = new GraphicRenderer();
        renderer.SetEntities([e]);
        using SKSurface first = TestHelpers.CreateSurface();
        renderer.Render(mapper, first.Canvas);
        using SKSurface second = TestHelpers.CreateSurface();
        var ex = Record.Exception(() => renderer.Render(mapper, second.Canvas));
        Assert.Null(ex);
        TestHelpers.AssertGreenISH(TestHelpers.Sample(second, 200, 200));
        e.Dispose();
    }

    [Fact]
    public void ShouldBe_Renderer_RenderDisposedLine_Throws()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var line = NewLine();
        line.Dispose();
        var renderer = new GraphicRenderer();
        renderer.SetEntities([line]);
        using SKSurface surface = TestHelpers.CreateSurface();
        Assert.ThrowsAny<Exception>(() => renderer.Render(mapper, surface.Canvas));
    }

    [Fact]
    public void KnownIssue_Renderer_RendersDisposedLine_WithoutThrowing()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var line = NewLine();
        line.Dispose();
        var renderer = new GraphicRenderer();
        renderer.SetEntities([line]);
        using SKSurface surface = TestHelpers.CreateSurface();
        var ex = Record.Exception(() => renderer.Render(mapper, surface.Canvas));
        Assert.Null(ex);
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void Pen_Brush_ToSKPaint_CallerOwnsDisposal()
    {
        var pen = new Pen(2, SKColors.Red, []);
        using SKPaint paint = (SKPaint)typeof(Pen).GetMethod("ToSKPaint", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(pen, null)!;
        Assert.Equal(SKColors.Red, paint.Color);

        var brush = new Brush(SKColors.Blue);
        using SKPaint fill = (SKPaint)typeof(Brush).GetMethod("ToSKPaint", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(brush, null)!;
        Assert.Equal(SKColors.Blue, fill.Color);
    }

    [Fact]
    public void Ellipse_Arc_Line_HaveNoBuilderOrRoundRectFields()
    {
        Assert.Null(typeof(GraphicLine).GetField("_pathBuilder", BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.Null(typeof(GraphicEllipse).GetField("_pathBuilder", BindingFlags.NonPublic | BindingFlags.Instance));
        Assert.Null(typeof(GraphicArc).GetField("_roundRect", BindingFlags.NonPublic | BindingFlags.Instance));
    }
}
