using Cartesia.Core.Geometry;
using Cartesia.Core.Shapes;
using Cartesia.Render.Entities.Shape;
using Cartesia.Render.Rendering;
using Cartesia.Tests.Helpers;
using SkiaSharp;

namespace Cartesia.Tests.Render;

public sealed class GraphicArcTests
{
    private static ArcProperties Props(
        double rx = 100, double ry = 100, double start = -90, double end = 90,
        double rot = 0, double hx = 200, double hy = 200) =>
        new(rx, ry, start, end, rot, new Point(hx, hy));

    [Fact]
    public void BoundingBox_Unrotated_SpansFullOval()
    {
        var arc = new GraphicArc(Props(), new Pen(3, SKColors.Red, []));

        var box = arc.BoundingBox();

        TestHelpers.Near(new Point(100, 100), box.BottomLeft);
        TestHelpers.Near(new Point(300, 300), box.TopRight);
    }

    [Fact]
    public void BoundingBox_EllipticalRadii_UsesDoubleRadiusExtents()
    {
        var arc = new GraphicArc(Props(rx: 150, ry: 75), new Pen(3, SKColors.Red, []));

        var box = arc.BoundingBox();

        TestHelpers.Near(new Point(50, 125), box.BottomLeft);
        TestHelpers.Near(new Point(350, 275), box.TopRight);
    }

    [Fact]
    public void BoundingBox_RotationAboutCentre_PreservesSymmetricBox()
    {
        var arc = new GraphicArc(Props(rot: -90), new Pen(3, SKColors.Red, []));

        var box = arc.BoundingBox();

        TestHelpers.Near(new Point(100, 100), box.BottomLeft, 1e-9);
        TestHelpers.Near(new Point(300, 300), box.TopRight, 1e-9);
    }

    [Fact]
    public void Ctor_NullStroke_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new GraphicArc(Props(), null!));
    }

    [Fact]
    public void Draw_RightHalfArc_PaintsRightSideOnly()
    {
        // Angles -90..90 drawn with start=90/sweep=-180: passes through canvas right.
        var mapper = TestHelpers.SquareMapper(400);
        var arc = new GraphicArc(Props(), new Pen(5, SKColors.Red, []));

        using SKSurface surface = TestHelpers.CreateSurface();
        arc.Precompute(mapper);
        arc.Draw(surface.Canvas);

        TestHelpers.AssertRed(TestHelpers.Sample(surface, 299, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 101, 200));
    }

    [Fact]
    public void Draw_RightHalfArc_PaintsStartPointAtBottom()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var arc = new GraphicArc(Props(), new Pen(5, SKColors.Red, []));

        using SKSurface surface = TestHelpers.CreateSurface();
        arc.Precompute(mapper);
        arc.Draw(surface.Canvas);

        TestHelpers.AssertRed(TestHelpers.Sample(surface, 200, 298));
    }

    [Fact]
    public void Draw_OpenArc_DoesNotFillCentre()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var arc = new GraphicArc(Props(), new Pen(5, SKColors.Red, []));

        using SKSurface surface = TestHelpers.CreateSurface();
        arc.Precompute(mapper);
        arc.Draw(surface.Canvas);

        // useCenter:false -> no pie fill; centre stays background.
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void Draw_RotatedArc_StillDrawsWithoutThrowing()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var arc = new GraphicArc(Props(rot: 45), new Pen(5, SKColors.Red, []));

        using SKSurface surface = TestHelpers.CreateSurface();
        arc.Precompute(mapper);

        var ex = Record.Exception(() => arc.Draw(surface.Canvas));

        Assert.Null(ex);
    }

    [Fact]
    public void Draw_ResetsCallerTransform()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var arc = new GraphicArc(Props(), new Pen(5, SKColors.Red, []));

        using SKSurface surface = TestHelpers.CreateSurface();
        arc.Precompute(mapper);
        surface.Canvas.Translate(12, 34);
        arc.Draw(surface.Canvas);

        SKMatrix m = surface.Canvas.TotalMatrix;
        TestHelpers.Near(0, m.TransX, 1e-6);
        TestHelpers.Near(0, m.TransY, 1e-6);
    }
}
