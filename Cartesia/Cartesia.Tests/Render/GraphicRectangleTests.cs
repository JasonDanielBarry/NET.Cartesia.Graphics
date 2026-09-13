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

    [Fact]
    public void Draw_FillAndStroke_PaintsBoth()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var rect = new GraphicRectangle(Props(), new Brush(SKColors.Yellow), new Pen(5, SKColors.Blue, []));

        using SKSurface surface = TestHelpers.CreateSurface();
        rect.Precompute(mapper);
        rect.Draw(surface.Canvas);

        TestHelpers.AssertYellow(TestHelpers.Sample(surface, 200, 200));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 170));
    }

    [Fact]
    public void Draw_RotationSign_Matters_AsymmetricAnglesDiffer()
    {
        var mapper = TestHelpers.SquareMapper(400);

        using SKSurface plus = TestHelpers.CreateSurface();
        new GraphicRectangle(Props(rot: 30), new Brush(SKColors.Blue), Pen.None)
            .Also(r => r.Precompute(mapper)).Also(r => r.Draw(plus.Canvas));

        using SKSurface minus = TestHelpers.CreateSurface();
        new GraphicRectangle(Props(rot: -30), new Brush(SKColors.Blue), Pen.None)
            .Also(r => r.Precompute(mapper)).Also(r => r.Draw(minus.Canvas));

        using SKSurface zero = TestHelpers.CreateSurface();
        new GraphicRectangle(Props(rot: 0), new Brush(SKColors.Blue), Pen.None)
            .Also(r => r.Precompute(mapper)).Also(r => r.Draw(zero.Canvas));

        // Centre painted in all; +30 and -30 footprints differ (sign would flip them).
        TestHelpers.AssertBlue(TestHelpers.Sample(plus, 200, 200));
        TestHelpers.AssertBlue(TestHelpers.Sample(minus, 200, 200));
        Assert.True(TestHelpers.SurfacesDiffer(plus, minus));
        Assert.True(TestHelpers.SurfacesDiffer(plus, zero));
    }

    [Fact]
    public void Draw_NonCentreHandle_PaintsHandleRelativeBox()
    {
        var mapper = TestHelpers.SquareMapper(400);
        // 100x60 Left/Bottom at (10,20): world (10,20)-(110,80); canvas y 320..380.
        var rect = new GraphicRectangle(
            Props(ha: HorizontalAlignment.Left, va: VerticalAlignment.Bottom, hx: 10, hy: 20),
            new Brush(SKColors.Blue), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        rect.Precompute(mapper);
        rect.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.SampleWorld(surface, mapper, new Point(60, 50)));
        TestHelpers.AssertWhite(TestHelpers.SampleWorld(surface, mapper, new Point(5, 5)));
    }

    [Fact]
    public void Draw_NonSquareMapper_PaintsMappedCentre()
    {
        // 800x400 canvas, 200x100 world: rect 100x50 centre (100,50) -> canvas x 200..600, y 100..300.
        var mapper = TestHelpers.MapperFor(800, 400, 200, 100);
        var rect = new GraphicRectangle(
            new RectangleProperties(100, 50, 0, 0, 0,
                HorizontalAlignment.Centre, VerticalAlignment.Centre, new Point(100, 50)),
            new Brush(SKColors.Blue), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface(800, 400);
        rect.Precompute(mapper);
        rect.Draw(surface.Canvas);

        surface.Canvas.Flush();
        using SKImage image = surface.Snapshot();
        using SKBitmap bitmap = SKBitmap.FromImage(image);
        SKColor centre = bitmap.GetPixel(400, 200);
        TestHelpers.AssertBlue(centre);
        TestHelpers.AssertWhite(bitmap.GetPixel(100, 200));
    }

    [Fact]
    public void Draw_SingleZeroRadius_StaysSharp()
    {
        var mapper = TestHelpers.SquareMapper(400);

        using SKSurface rxOnly = TestHelpers.CreateSurface();
        new GraphicRectangle(Props(rx: 20, ry: 0), new Brush(SKColors.Blue), Pen.None)
            .Also(r => r.Precompute(mapper)).Also(r => r.Draw(rxOnly.Canvas));

        using SKSurface ryOnly = TestHelpers.CreateSurface();
        new GraphicRectangle(Props(rx: 0, ry: 20), new Brush(SKColors.Blue), Pen.None)
            .Also(r => r.Precompute(mapper)).Also(r => r.Draw(ryOnly.Canvas));

        // Either radius zero -> square corner -> near-corner pixel stays filled.
        TestHelpers.AssertBlue(TestHelpers.Sample(rxOnly, 152, 228));
        TestHelpers.AssertBlue(TestHelpers.Sample(ryOnly, 152, 228));
    }

    [Fact]
    public void Draw_NegativeRadii_DoNotThrow_PaintCentre()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var rect = new GraphicRectangle(Props(rx: -5, ry: -5), new Brush(SKColors.Blue), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        rect.Precompute(mapper);

        var ex = Record.Exception(() => rect.Draw(surface.Canvas));

        Assert.Null(ex);
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void Draw_OversizedRadii_DoNotThrow_PaintCentre()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var rect = new GraphicRectangle(Props(rx: 1000, ry: 1000), new Brush(SKColors.Blue), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        rect.Precompute(mapper);

        var ex = Record.Exception(() => rect.Draw(surface.Canvas));

        Assert.Null(ex);
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void Draw_WithoutPrecompute_DoesNotThrow_PaintsNothing()
    {
        var rect = new GraphicRectangle(Props(), new Brush(SKColors.Blue), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();

        var ex = Record.Exception(() => rect.Draw(surface.Canvas));

        Assert.Null(ex);
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 0, 0));
    }

    [Fact]
    public void Draw_DashedStroke_PaintsRimDashes()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var rect = new GraphicRectangle(Props(), Brush.None, new Pen(3, SKColors.Red, [20, 20]));

        using SKSurface surface = TestHelpers.CreateSurface();
        rect.Precompute(mapper);
        rect.Draw(surface.Canvas);

        // Step-1 scan: the step-4 grid can straddle the 3px rim between antialiased rows.
        Assert.True(TestHelpers.AnyPixelMatches(surface, TestHelpers.IsReddish, 400, 400, 1));
        TestHelpers.AssertRed(TestHelpers.Sample(surface, 160, 170));
    }
}
