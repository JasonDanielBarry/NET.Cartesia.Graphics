using Cartesia.Core.Geometry;
using Cartesia.Core.Shapes;
using Cartesia.Render.Entities.Shape;
using Cartesia.Render.Rendering;
using Cartesia.Tests.Helpers;
using SkiaSharp;

namespace Cartesia.Tests.Render;

public sealed class GraphicRectangleTests
{
    private static RectangleProperties Props(
        double w = 100, double h = 60, double rx = 0, double ry = 0, double rot = 0,
        HorizontalAlignment ha = HorizontalAlignment.Centre,
        VerticalAlignment va = VerticalAlignment.Centre,
        double hx = 200, double hy = 200) =>
        new(w, h, rx, ry, rot, ha, va, new Point(hx, hy));

    [Fact]
    public void BoundingBox_UnrotatedCentre_MatchesHandleBox()
    {
        var rect = new GraphicRectangle(Props(), new Brush(SKColors.Blue), Pen.None);

        var box = rect.BoundingBox();

        TestHelpers.Near(new Point(150, 170), box.BottomLeft);
        TestHelpers.Near(new Point(250, 230), box.TopRight);
    }

    [Fact]
    public void BoundingBox_LeftBottom_OriginAtHandle()
    {
        var rect = new GraphicRectangle(
            Props(ha: HorizontalAlignment.Left, va: VerticalAlignment.Bottom, hx: 10, hy: 20),
            new Brush(SKColors.Blue), Pen.None);

        var box = rect.BoundingBox();

        TestHelpers.Near(new Point(10, 20), box.BottomLeft);
        TestHelpers.Near(new Point(110, 80), box.TopRight);
    }

    [Fact]
    public void BoundingBox_Rotated90AboutCentre_SwapsExtents()
    {
        var rect = new GraphicRectangle(Props(rot: 90), new Brush(SKColors.Blue), Pen.None);

        var box = rect.BoundingBox();

        TestHelpers.Near(new Point(170, 150), box.BottomLeft, 1e-9);
        TestHelpers.Near(new Point(230, 250), box.TopRight, 1e-9);
        TestHelpers.Near(60, box.Width, 1e-9);
        TestHelpers.Near(100, box.Height, 1e-9);
    }

    [Fact]
    public void BoundingBox_Rotated180AboutCentre_SameBox()
    {
        var rect = new GraphicRectangle(Props(rot: 180), new Brush(SKColors.Blue), Pen.None);

        var box = rect.BoundingBox();

        TestHelpers.Near(new Point(150, 170), box.BottomLeft, 1e-9);
        TestHelpers.Near(new Point(250, 230), box.TopRight, 1e-9);
    }

    [Fact]
    public void BoundingBox_Rotated45AboutCorner_DocumentsTwoCornerLimitation()
    {
        // Handle at box corner + 45deg: true 4-corner AABB would reach x in
        // [-2.83, 7.07], but the two-corner path pins [0, 4.24]. Flag to owner; do not "fix" here.
        var rect = new GraphicRectangle(
            Props(w: 10, h: 4, rot: 45,
                ha: HorizontalAlignment.Left, va: VerticalAlignment.Bottom, hx: 0, hy: 0),
            new Brush(SKColors.Blue), Pen.None);

        var box = rect.BoundingBox();

        double c = Math.Cos(Math.PI / 4);
        double s = Math.Sin(Math.PI / 4);
        TestHelpers.Near(0, box.BottomLeft.X, 1e-9);
        TestHelpers.Near(0, box.BottomLeft.Y, 1e-9);
        TestHelpers.Near(10 * c - 4 * s, box.TopRight.X, 1e-9);
        TestHelpers.Near(10 * s + 4 * c, box.TopRight.Y, 1e-9);
        // True extremes (corner (0,4) -> x=-2.83) are NOT covered:
        Assert.True(box.BottomLeft.X > -2.83);
    }

    [Fact]
    public void Ctor_NullFill_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new GraphicRectangle(Props(), null!, Pen.None));
    }

    [Fact]
    public void Ctor_NullStroke_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new GraphicRectangle(Props(), new Brush(SKColors.Blue), null!));
    }

    [Fact]
    public void Draw_FillOnly_PaintsCentre()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var rect = new GraphicRectangle(Props(), new Brush(SKColors.Blue), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        rect.Precompute(mapper);
        rect.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void Draw_FillOnly_PaintsCornersSharp()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var rect = new GraphicRectangle(Props(), new Brush(SKColors.Blue), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        rect.Precompute(mapper);
        rect.Draw(surface.Canvas);

        // Canvas box x 150..250, y 170..230. Near-corner pixel is filled when radii are 0.
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 152, 228));
    }

    [Fact]
    public void Draw_RoundedCorners_CutCornerPixel()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var rect = new GraphicRectangle(Props(rx: 20, ry: 20), new Brush(SKColors.Blue), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        rect.Precompute(mapper);
        rect.Draw(surface.Canvas);

        // Centre still filled...
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 200));
        // ...but the extreme corner is cut away by the 20-unit radius.
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 152, 228));
    }

    [Fact]
    public void Draw_StrokeOnly_PaintsBorderNotInterior()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var rect = new GraphicRectangle(Props(), Brush.None, new Pen(5, SKColors.Red, []));

        using SKSurface surface = TestHelpers.CreateSurface();
        rect.Precompute(mapper);
        rect.Draw(surface.Canvas);

        TestHelpers.AssertRed(TestHelpers.Sample(surface, 200, 170));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void Draw_NeitherVisible_PaintsNothing()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var rect = new GraphicRectangle(Props(), Brush.None, Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        rect.Precompute(mapper);
        rect.Draw(surface.Canvas);

        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 170));
    }

    [Fact]
    public void Draw_ResetsCallerTransform()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var rect = new GraphicRectangle(Props(), new Brush(SKColors.Blue), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        rect.Precompute(mapper);
        surface.Canvas.Translate(50, 50);
        rect.Draw(surface.Canvas);

        SKMatrix m = surface.Canvas.TotalMatrix;
        TestHelpers.Near(1, m.ScaleX, 1e-6);
        TestHelpers.Near(1, m.ScaleY, 1e-6);
        TestHelpers.Near(0, m.TransX, 1e-6);
        TestHelpers.Near(0, m.TransY, 1e-6);
        TestHelpers.Near(0, m.SkewX, 1e-6);
        TestHelpers.Near(0, m.SkewY, 1e-6);
    }

    [Fact]
    public void Draw_Rotated90_FillsSwappedFootprint()
    {
        var mapper = TestHelpers.SquareMapper(400);
        // 100x60 at centre rotated 90deg -> canvas footprint x 170..230, y 150..250.
        var rect = new GraphicRectangle(Props(rot: 90), new Brush(SKColors.Blue), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        rect.Precompute(mapper);
        rect.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 200));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 160));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 160, 200));
    }
}
