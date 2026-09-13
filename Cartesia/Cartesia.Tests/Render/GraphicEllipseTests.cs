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
