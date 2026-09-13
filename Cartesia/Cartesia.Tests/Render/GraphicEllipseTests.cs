using Cartesia.Core.Geometry;
using Cartesia.Core.Shapes;
using Cartesia.Render.Entities.Shape;
using Cartesia.Render.Rendering;
using Cartesia.Tests.Helpers;
using SkiaSharp;

namespace Cartesia.Tests.Render;

public sealed class GraphicEllipseTests
{
    private static EllipseProperties Props(
        double w = 100, double h = 60, double rot = 0,
        HorizontalAlignment ha = HorizontalAlignment.Centre,
        VerticalAlignment va = VerticalAlignment.Centre,
        double hx = 200, double hy = 200) =>
        new(w, h, rot, ha, va, new Point(hx, hy));

    [Fact]
    public void BoundingBox_UnrotatedCentre_MatchesHandleBox()
    {
        var ellipse = new GraphicEllipse(Props(), new Brush(SKColors.Blue), Pen.None);

        var box = ellipse.BoundingBox();

        TestHelpers.Near(new Point(150, 170), box.BottomLeft);
        TestHelpers.Near(new Point(250, 230), box.TopRight);
    }

    [Fact]
    public void BoundingBox_Rotated90AboutCentre_SwapsExtents()
    {
        var ellipse = new GraphicEllipse(Props(rot: 90), new Brush(SKColors.Blue), Pen.None);

        var box = ellipse.BoundingBox();

        TestHelpers.Near(new Point(170, 150), box.BottomLeft, 1e-9);
        TestHelpers.Near(new Point(230, 250), box.TopRight, 1e-9);
    }

    [Fact]
    public void BoundingBox_TopAlignment_ExtendsDownFromHandle()
    {
        var ellipse = new GraphicEllipse(
            Props(ha: HorizontalAlignment.Centre, va: VerticalAlignment.Top, hx: 0, hy: 0),
            new Brush(SKColors.Blue), Pen.None);

        var box = ellipse.BoundingBox();

        TestHelpers.Near(new Point(-50, -60), box.BottomLeft);
        TestHelpers.Near(new Point(50, 0), box.TopRight);
    }

    [Fact]
    public void Ctor_NullFill_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new GraphicEllipse(Props(), null!, Pen.None));
    }

    [Fact]
    public void Ctor_NullStroke_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new GraphicEllipse(Props(), new Brush(SKColors.Blue), null!));
    }

    [Fact]
    public void Draw_FillOnly_PaintsCentre()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var ellipse = new GraphicEllipse(Props(), new Brush(SKColors.Blue), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        ellipse.Precompute(mapper);
        ellipse.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void Draw_FillOnly_LeavesBoxCornersUnpainted()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var ellipse = new GraphicEllipse(Props(), new Brush(SKColors.Blue), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        ellipse.Precompute(mapper);
        ellipse.Draw(surface.Canvas);

        // ((152-200)/50)^2 + ((228-200)/30)^2 ~= 1.79 > 1: outside the oval.
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 152, 228));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 248, 172));
    }

    [Fact]
    public void Draw_FillOnly_PaintsAxisEndpoints()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var ellipse = new GraphicEllipse(Props(), new Brush(SKColors.Blue), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        ellipse.Precompute(mapper);
        ellipse.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 180));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 160, 200));
    }

    [Fact]
    public void Draw_StrokeOnly_PaintsRimNotCentre()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var ellipse = new GraphicEllipse(Props(), Brush.None, new Pen(5, SKColors.Red, []));

        using SKSurface surface = TestHelpers.CreateSurface();
        ellipse.Precompute(mapper);
        ellipse.Draw(surface.Canvas);

        TestHelpers.AssertRed(TestHelpers.Sample(surface, 200, 170));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void Draw_NeitherVisible_PaintsNothing()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var ellipse = new GraphicEllipse(Props(), Brush.None, Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        ellipse.Precompute(mapper);
        ellipse.Draw(surface.Canvas);

        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 170));
    }

    [Fact]
    public void Draw_Rotated90_FillsSwappedFootprint()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var ellipse = new GraphicEllipse(Props(rot: 90), new Brush(SKColors.Blue), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        ellipse.Precompute(mapper);
        ellipse.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 200));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 160));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 160, 200));
    }

    [Fact]
    public void BoundingBox_Minus90_Equals270()
    {
        var neg = new GraphicEllipse(Props(rot: -90), new Brush(SKColors.Blue), Pen.None);
        var pos = new GraphicEllipse(Props(rot: 270), new Brush(SKColors.Blue), Pen.None);

        TestHelpers.Near(neg.BoundingBox().BottomLeft, pos.BoundingBox().BottomLeft, 1e-9);
        TestHelpers.Near(neg.BoundingBox().TopRight, pos.BoundingBox().TopRight, 1e-9);
    }

    [Fact]
    public void BoundingBox_RightBottom_ExtendsNegativeFromHandle()
    {
        var ellipse = new GraphicEllipse(
            Props(ha: HorizontalAlignment.Right, va: VerticalAlignment.Bottom, hx: 5, hy: 5),
            new Brush(SKColors.Blue), Pen.None);

        var box = ellipse.BoundingBox();

        TestHelpers.Near(new Point(-95, 5), box.BottomLeft);
        TestHelpers.Near(new Point(5, 65), box.TopRight);
    }

    [Fact]
    public void Draw_FillAndStroke_PaintsBoth()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var ellipse = new GraphicEllipse(Props(), new Brush(SKColors.Yellow), new Pen(5, SKColors.Blue, []));

        using SKSurface surface = TestHelpers.CreateSurface();
        ellipse.Precompute(mapper);
        ellipse.Draw(surface.Canvas);

        TestHelpers.AssertYellow(TestHelpers.Sample(surface, 200, 200));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 170));
    }

    [Fact]
    public void Draw_RotationSign_Matters_AsymmetricAnglesDiffer()
    {
        var mapper = TestHelpers.SquareMapper(400);

        using SKSurface plus = TestHelpers.CreateSurface();
        new GraphicEllipse(Props(rot: 30), new Brush(SKColors.Blue), Pen.None)
            .Also(e => e.Precompute(mapper)).Also(e => e.Draw(plus.Canvas));

        using SKSurface minus = TestHelpers.CreateSurface();
        new GraphicEllipse(Props(rot: -30), new Brush(SKColors.Blue), Pen.None)
            .Also(e => e.Precompute(mapper)).Also(e => e.Draw(minus.Canvas));

        TestHelpers.AssertBlue(TestHelpers.Sample(plus, 200, 200));
        TestHelpers.AssertBlue(TestHelpers.Sample(minus, 200, 200));
        Assert.True(TestHelpers.SurfacesDiffer(plus, minus));
    }

    [Fact]
    public void Draw_ZeroWidth_PaintsNothingWithoutThrowing()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var ellipse = new GraphicEllipse(Props(w: 0), new Brush(SKColors.Blue), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        ellipse.Precompute(mapper);

        var ex = Record.Exception(() => ellipse.Draw(surface.Canvas));

        Assert.Null(ex);
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void Draw_ZeroHeight_PaintsNothingWithoutThrowing()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var ellipse = new GraphicEllipse(Props(h: 0), new Brush(SKColors.Blue), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        ellipse.Precompute(mapper);

        var ex = Record.Exception(() => ellipse.Draw(surface.Canvas));

        Assert.Null(ex);
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void Draw_NonSquareMapper_PaintsMappedAxisEndpoints()
    {
        // 800x400 canvas, 200x100 world: ellipse 100x50 centre (100,50) -> canvas x 200..600, y 100..300.
        var mapper = TestHelpers.MapperFor(800, 400, 200, 100);
        var ellipse = new GraphicEllipse(
            new EllipseProperties(100, 50, 0,
                HorizontalAlignment.Centre, VerticalAlignment.Centre, new Point(100, 50)),
            new Brush(SKColors.Blue), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface(800, 400);
        ellipse.Precompute(mapper);
        ellipse.Draw(surface.Canvas);

        surface.Canvas.Flush();
        using SKImage image = surface.Snapshot();
        using SKBitmap bitmap = SKBitmap.FromImage(image);
        TestHelpers.AssertBlue(bitmap.GetPixel(400, 200));
        TestHelpers.AssertBlue(bitmap.GetPixel(595, 200));
        TestHelpers.AssertBlue(bitmap.GetPixel(400, 105));
        TestHelpers.AssertWhite(bitmap.GetPixel(100, 200));
    }

    [Fact]
    public void Draw_WithoutPrecompute_DoesNotThrow_PaintsNothing()
    {
        var ellipse = new GraphicEllipse(Props(), new Brush(SKColors.Blue), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();

        var ex = Record.Exception(() => ellipse.Draw(surface.Canvas));

        Assert.Null(ex);
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void Draw_ResetsCallerTransform()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var ellipse = new GraphicEllipse(Props(), new Brush(SKColors.Blue), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        ellipse.Precompute(mapper);
        surface.Canvas.Translate(-30, 40);
        ellipse.Draw(surface.Canvas);

        SKMatrix m = surface.Canvas.TotalMatrix;
        TestHelpers.Near(1, m.ScaleX, 1e-6);
        TestHelpers.Near(1, m.ScaleY, 1e-6);
        TestHelpers.Near(0, m.TransX, 1e-6);
        TestHelpers.Near(0, m.TransY, 1e-6);
    }
}
