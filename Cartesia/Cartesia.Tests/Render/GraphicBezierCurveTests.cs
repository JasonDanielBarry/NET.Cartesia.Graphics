using Cartesia.Core.Curve;
using Cartesia.Core.Geometry;
using Cartesia.Core.Shapes;
using Cartesia.Render.Entities.Base;
using Cartesia.Render.Entities.Curve;
using Cartesia.Render.Entities.Shape;
using Cartesia.Render.Renderer;
using Cartesia.Render.Rendering;
using Cartesia.Tests.Helpers;
using SkiaSharp;

namespace Cartesia.Tests.Render;

public sealed class GraphicBezierCurveTests
{
    // World (x,y) -> canvas (x,400-y) on the 400 square mapper (Y-up to Y-down).
    private static Pen Blue(double thickness = 5) => new(thickness, SKColors.Blue, []);
    private static Pen Red(double thickness = 5) => new(thickness, SKColors.Red, []);

    private static BezierCurveProperties Straight(
        double y = 200, double x0 = 100, double x3 = 300) => new(
            new Point(x0, y),
            new Point(x0 + (x3 - x0) / 3, y),
            new Point(x0 + 2 * (x3 - x0) / 3, y),
            new Point(x3, y));

    // Symmetric arch: endpoints world (100,100)/(300,100), controls above.
    // B(0.5) = world (200,250) -> canvas (200,150);
    // B(0.25) = world (131.25,212.5) -> canvas (131,187.5).
    private static BezierCurveProperties Arch() => new(
        new Point(100, 100), new Point(100, 300),
        new Point(300, 300), new Point(300, 100));

    private static BezierCurveProperties Diagonal() => new(
        new Point(50, 350), new Point(150, 250),
        new Point(250, 150), new Point(350, 50));

    [Fact]
    public void BoundingBox_SpansAllFourControlPoints()
    {
        var curve = new GraphicBezierCurve(Arch(), Blue());

        var box = curve.BoundingBox();

        TestHelpers.Near(new Point(100, 100), box.BottomLeft);
        TestHelpers.Near(new Point(300, 300), box.TopRight);
        TestHelpers.Near(200, box.Width);
        TestHelpers.Near(200, box.Height);
    }

    [Fact]
    public void BoundingBox_StraightHorizontal_ZeroHeight()
    {
        var curve = new GraphicBezierCurve(Straight(), Blue());

        var box = curve.BoundingBox();

        TestHelpers.Near(new Point(100, 200), box.BottomLeft);
        TestHelpers.Near(new Point(300, 200), box.TopRight);
        TestHelpers.Near(200, box.Width);
        TestHelpers.Near(0, box.Height);
    }

    [Fact]
    public void BoundingBox_StraightVertical_ZeroWidth()
    {
        var curve = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(100, 100), new Point(100, 100 + 200.0 / 3),
            new Point(100, 100 + 400.0 / 3), new Point(100, 300)), Blue());

        var box = curve.BoundingBox();

        TestHelpers.Near(new Point(100, 100), box.BottomLeft);
        TestHelpers.Near(new Point(100, 300), box.TopRight);
        TestHelpers.Near(0, box.Width);
        TestHelpers.Near(200, box.Height);
    }

    [Fact]
    public void BoundingBox_DegeneratePoint_ZeroSize()
    {
        var curve = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(7, 7), new Point(7, 7),
            new Point(7, 7), new Point(7, 7)), Blue());

        var box = curve.BoundingBox();

        TestHelpers.Near(new Point(7, 7), box.BottomLeft);
        TestHelpers.Near(new Point(7, 7), box.TopRight);
        TestHelpers.Near(0, box.Width);
        TestHelpers.Near(0, box.Height);
    }

    [Fact]
    public void BoundingBox_ReversedControlOrder_SameBox()
    {
        var forward = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(10, 20), new Point(50, 60),
            new Point(90, 10), new Point(200, 250)), Blue());
        var reversed = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(200, 250), new Point(90, 10),
            new Point(50, 60), new Point(10, 20)), Blue());

        TestHelpers.Near(forward.BoundingBox().BottomLeft, reversed.BoundingBox().BottomLeft);
        TestHelpers.Near(forward.BoundingBox().TopRight, reversed.BoundingBox().TopRight);
        // Absolute values, not just order-invariance: min(10,50,90,200)=10,
        // min(20,60,10,250)=10, max X=200, max Y=250. Box normalizes like Line.
        TestHelpers.Near(new Point(10, 10), forward.BoundingBox().BottomLeft);
        TestHelpers.Near(new Point(200, 250), forward.BoundingBox().TopRight);
    }

    [Fact]
    public void BoundingBox_NegativeCoordinates_SpansMinMax()
    {
        var curve = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(-50, -20), new Point(10, 30),
            new Point(-10, -40), new Point(40, 25)), Blue());

        var box = curve.BoundingBox();

        TestHelpers.Near(new Point(-50, -40), box.BottomLeft);
        TestHelpers.Near(new Point(40, 30), box.TopRight);
        TestHelpers.Near(90, box.Width);
        TestHelpers.Near(70, box.Height);
    }

    [Fact]
    public void BoundingBox_IncludesControlPointsBeyondCurve()
    {
        // Control-hull box, not tight curve bounds: controls reach world y=300
        // but the arch apex only reaches world y=250 (B(0.5) below). The hull is
        // the full (100,100)-(300,300), 200x200 — over-approximation is intended,
        // matching Line/Polyline endpoint semantics.
        var curve = new GraphicBezierCurve(Arch(), Blue());

        var box = curve.BoundingBox();

        TestHelpers.Near(new Point(100, 100), box.BottomLeft);
        TestHelpers.Near(new Point(300, 300), box.TopRight);
        TestHelpers.Near(200, box.Width);
        TestHelpers.Near(200, box.Height);
        Assert.True(box.TopRight.Y > 250);
    }

    [Fact]
    public void BoundingBox_OutlierControl_ExpandsBox()
    {
        var tight = new GraphicBezierCurve(Straight(), Blue());
        var outlier = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(100, 200), new Point(100, 390),
            new Point(100 + 400.0 / 3, 200), new Point(300, 200)), Blue());

        Assert.True(outlier.BoundingBox().TopRight.Y > tight.BoundingBox().TopRight.Y);
        TestHelpers.Near(new Point(100, 200), outlier.BoundingBox().BottomLeft);
        TestHelpers.Near(new Point(300, 390), outlier.BoundingBox().TopRight);
        TestHelpers.Near(200, outlier.BoundingBox().Width);
        TestHelpers.Near(190, outlier.BoundingBox().Height);
    }

    [Fact]
    public void BoundingBox_ColinearDiagonal_SpansEndpoints()
    {
        // Diagonal() points all satisfy x+y==400. No rotation path exists for
        // beziers (unlike GraphicShape): the box is the axis-aligned min/max.
        var curve = new GraphicBezierCurve(Diagonal(), Blue());

        var box = curve.BoundingBox();

        TestHelpers.Near(new Point(50, 50), box.BottomLeft);
        TestHelpers.Near(new Point(350, 350), box.TopRight);
        TestHelpers.Near(300, box.Width);
        TestHelpers.Near(300, box.Height);
    }

    [Fact]
    public void BoundingBox_ClosedLoopP0EqualsP3_SpansControls()
    {
        var loop = new BezierCurveProperties(
            new Point(200, 100), new Point(100, 300),
            new Point(300, 300), new Point(200, 100));
        var curve = new GraphicBezierCurve(loop, Blue());

        var box = curve.BoundingBox();

        TestHelpers.Near(new Point(100, 100), box.BottomLeft);
        TestHelpers.Near(new Point(300, 300), box.TopRight);
    }

    [Fact]
    public void BoundingBox_InteriorControlsDefineBothExtrema()
    {
        // Both extrema come solely from P1/P2: an endpoint-only impl would
        // return (0,0)-(10,10) instead of (-50,-50)-(100,100).
        var curve = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(0, 0), new Point(-50, 100),
            new Point(100, -50), new Point(10, 10)), Blue());

        var box = curve.BoundingBox();

        TestHelpers.Near(new Point(-50, -50), box.BottomLeft);
        TestHelpers.Near(new Point(100, 100), box.TopRight);
    }

    [Fact]
    public void BoundingBox_PartialCoincidence_SpansDistinctPoints()
    {
        // P0==P1 and P2==P3 (zero end tangents): still a 200-long hull.
        var curve = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(100, 200), new Point(100, 200),
            new Point(300, 200), new Point(300, 200)), Blue());

        var box = curve.BoundingBox();

        TestHelpers.Near(new Point(100, 200), box.BottomLeft);
        TestHelpers.Near(new Point(300, 200), box.TopRight);
        TestHelpers.Near(200, box.Width);
        TestHelpers.Near(0, box.Height);
    }

    [Fact]
    public void BoundingBox_DefaultProps_ZeroSizeAtOrigin()
    {
        // The struct always carries four points, so unlike an empty polyline
        // (Width -2e9 sentinel) the default is a zero-size box at the origin.
        var curve = new GraphicBezierCurve(default, Blue());

        var box = curve.BoundingBox();

        TestHelpers.Near(new Point(0, 0), box.BottomLeft);
        TestHelpers.Near(new Point(0, 0), box.TopRight);
        TestHelpers.Near(0, box.Width);
        TestHelpers.Near(0, box.Height);
    }

    [Fact]
    public void BoundingBox_IgnoresStrokeThickness()
    {
        var thin = new GraphicBezierCurve(Arch(), Pen.None);
        var thick = new GraphicBezierCurve(Arch(), Blue(20));

        TestHelpers.Near(thin.BoundingBox().BottomLeft, thick.BoundingBox().BottomLeft);
        TestHelpers.Near(thin.BoundingBox().TopRight, thick.BoundingBox().TopRight);
    }

    [Fact]
    public void BoundingBox_IgnoresDashAndTransparency()
    {
        var plain = new GraphicBezierCurve(Arch(), Blue());
        var dashed = new GraphicBezierCurve(Arch(), new Pen(5, SKColors.Red, [10, 10, 20, 20]));
        var transparent = new GraphicBezierCurve(Arch(), new Pen(5, SKColors.Transparent, []));

        foreach (var other in new[] { dashed, transparent })
        {
            TestHelpers.Near(plain.BoundingBox().BottomLeft, other.BoundingBox().BottomLeft);
            TestHelpers.Near(plain.BoundingBox().TopRight, other.BoundingBox().TopRight);
        }
    }

    [Fact]
    public void BoundingBox_UnchangedByPrecompute()
    {
        // BoundingBox is world-space: Precompute (even with an offset,
        // non-square mapper) must not move it — for arch and degenerate alike.
        foreach (var props in new[]
        {
            Arch(),
            new BezierCurveProperties(
                new Point(7, 7), new Point(7, 7),
                new Point(7, 7), new Point(7, 7)),
        })
        {
            var curve = new GraphicBezierCurve(props, Blue());
            var before = curve.BoundingBox();

            curve.Precompute(TestHelpers.SquareMapper(400));
            TestHelpers.Near(before.BottomLeft, curve.BoundingBox().BottomLeft);
            TestHelpers.Near(before.TopRight, curve.BoundingBox().TopRight);

            curve.Precompute(TestHelpers.MapperFor(800, 400, 200, 100, 1000, 2000));
            TestHelpers.Near(before.BottomLeft, curve.BoundingBox().BottomLeft);
            TestHelpers.Near(before.TopRight, curve.BoundingBox().TopRight);
        }
    }

    [Fact]
    public void KnownIssue_BoundingBox_BeyondSentinel_ClampsToSeed()
    {
        // Box seeds min/max at +-1e9 (see BoxTests KnownIssue_CoordinatesBeyondSentinel_Clamp):
        // true min X 2e9 is lost, BL.X sticks at 1e9. Pinned as actual behavior.
        var curve = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(2e9, 0), new Point(3e9, 1),
            new Point(2.5e9, 0.5), new Point(2e9, 0)), Blue());

        var box = curve.BoundingBox();

        TestHelpers.Near(new Point(1e9, 0), box.BottomLeft);
        TestHelpers.Near(new Point(3e9, 1), box.TopRight);
        TestHelpers.Near(2e9, box.Width);
        TestHelpers.Near(1, box.Height);
    }

    [Fact]
    public void KnownIssue_BoundingBox_NegativeBeyondSentinel_ClampsToSeed()
    {
        // Mirror image: true max X -2e9 is lost, TR.X sticks at -1e9.
        var curve = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(-2e9, 0), new Point(-3e9, 1),
            new Point(-2.5e9, 0.5), new Point(-2e9, 0)), Blue());

        var box = curve.BoundingBox();

        TestHelpers.Near(new Point(-3e9, 0), box.BottomLeft);
        TestHelpers.Near(new Point(-1e9, 1), box.TopRight);
        TestHelpers.Near(2e9, box.Width);
        TestHelpers.Near(1, box.Height);
    }

    [Fact]
    public void KnownIssue_BoundingBox_AllBeyondSentinel_ClampsToSeed()
    {
        // All four points beyond +1e9: BL sticks at the (1e9,1e9) seed instead
        // of the true (2e9,2e9), so Width reports 1e9 instead of 0.
        var curve = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(2e9, 2e9), new Point(2e9, 2e9),
            new Point(2e9, 2e9), new Point(2e9, 2e9)), Blue());

        var box = curve.BoundingBox();

        TestHelpers.Near(new Point(1e9, 1e9), box.BottomLeft);
        TestHelpers.Near(new Point(2e9, 2e9), box.TopRight);
        TestHelpers.Near(1e9, box.Width);
        TestHelpers.Near(1e9, box.Height);
    }

    [Fact]
    public void ShouldBe_BoundingBox_BeyondSentinel_FullRange()
    {
        // RED: the box must span the true coordinates on both sides of the
        // +-1e9 seeds, like BoxTests.ShouldBe_CoordinatesBeyondSentinel_Work.
        var curve = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(-2e9, 0), new Point(-3e9, 1),
            new Point(-2.5e9, 0.5), new Point(-2e9, 0)), Blue());

        var box = curve.BoundingBox();

        TestHelpers.Near(new Point(-3e9, 0), box.BottomLeft);
        TestHelpers.Near(new Point(-2e9, 1), box.TopRight);
        TestHelpers.Near(1e9, box.Width);
        TestHelpers.Near(1, box.Height);
    }

    [Fact]
    public void BoundingBox_AtSentinelBoundary_ZeroSize()
    {
        // Exactly +-1e9 still compares (1e9<1e9 is false), so the box is exact.
        var curve = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(1e9, 1e9), new Point(1e9, 1e9),
            new Point(1e9, 1e9), new Point(1e9, 1e9)), Blue());

        var box = curve.BoundingBox();

        TestHelpers.Near(0, box.Width);
        TestHelpers.Near(0, box.Height);
    }

    [Fact]
    public void BoundingBox_AtNegativeSentinelBoundary_ZeroSize()
    {
        var curve = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(-1e9, -1e9), new Point(-1e9, -1e9),
            new Point(-1e9, -1e9), new Point(-1e9, -1e9)), Blue());

        var box = curve.BoundingBox();

        TestHelpers.Near(new Point(-1e9, -1e9), box.BottomLeft);
        TestHelpers.Near(new Point(-1e9, -1e9), box.TopRight);
        TestHelpers.Near(0, box.Width);
        TestHelpers.Near(0, box.Height);
    }

    [Fact]
    public void BoundingBox_MixedSentinelBoundaries_ExactSpan()
    {
        var curve = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(-1e9, -1e9), new Point(1e9, 1e9),
            new Point(-1e9, 1e9), new Point(1e9, -1e9)), Blue());

        var box = curve.BoundingBox();

        TestHelpers.Near(new Point(-1e9, -1e9), box.BottomLeft);
        TestHelpers.Near(new Point(1e9, 1e9), box.TopRight);
        TestHelpers.Near(2e9, box.Width);
        TestHelpers.Near(2e9, box.Height);
    }

    [Fact]
    public void KnownIssue_BoundingBox_NaNControl_IsIgnored()
    {
        // Box uses < comparisons, so NaN never wins: the NaN X is skipped and
        // the box spans the remaining points (0,0)-(20,20).
        var curve = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(double.NaN, 0), new Point(0, 0),
            new Point(10, 10), new Point(20, 20)), Blue());

        var box = curve.BoundingBox();

        TestHelpers.Near(new Point(0, 0), box.BottomLeft);
        TestHelpers.Near(new Point(20, 20), box.TopRight);
    }

    [Fact]
    public void KnownIssue_BoundingBox_NaNYControl_IsIgnored()
    {
        var curve = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(0, double.NaN), new Point(0, 0),
            new Point(10, 10), new Point(20, 20)), Blue());

        var box = curve.BoundingBox();

        TestHelpers.Near(new Point(0, 0), box.BottomLeft);
        TestHelpers.Near(new Point(20, 20), box.TopRight);
    }

    [Fact]
    public void KnownIssue_BoundingBox_AllNaN_InvertedSentinelBox()
    {
        // Every comparison is false, so both seeds survive: the box is inverted
        // (min 1e9 > max -1e9), Width/Height -2e9, like an empty point list.
        var curve = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(double.NaN, double.NaN), new Point(double.NaN, double.NaN),
            new Point(double.NaN, double.NaN), new Point(double.NaN, double.NaN)), Blue());

        var box = curve.BoundingBox();

        TestHelpers.Near(new Point(1e9, 1e9), box.BottomLeft);
        TestHelpers.Near(new Point(-1e9, -1e9), box.TopRight);
        TestHelpers.Near(-2e9, box.Width);
        TestHelpers.Near(-2e9, box.Height);
    }

    [Fact]
    public void BoundingBox_InfinityControl_Propagates()
    {
        var curve = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(double.PositiveInfinity, 0), new Point(0, 0),
            new Point(10, 10), new Point(20, 20)), Blue());

        var box = curve.BoundingBox();

        TestHelpers.Near(new Point(0, 0), box.BottomLeft);
        Assert.Equal(double.PositiveInfinity, box.TopRight.X);
        TestHelpers.Near(20, box.TopRight.Y);
    }

    [Fact]
    public void BoundingBox_NegativeInfinityControl_Propagates()
    {
        var curve = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(double.NegativeInfinity, 0), new Point(0, 0),
            new Point(10, 10), new Point(20, 20)), Blue());

        var box = curve.BoundingBox();

        Assert.Equal(double.NegativeInfinity, box.BottomLeft.X);
        TestHelpers.Near(new Point(20, 20), box.TopRight);
    }

    [Fact]
    public void BoundingBox_InfinityYControl_Propagates()
    {
        var curve = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(0, double.PositiveInfinity), new Point(0, 0),
            new Point(10, 10), new Point(20, 20)), Blue());

        var box = curve.BoundingBox();

        TestHelpers.Near(new Point(0, 0), box.BottomLeft);
        TestHelpers.Near(20, box.TopRight.X);
        Assert.Equal(double.PositiveInfinity, box.TopRight.Y);
        Assert.Equal(double.PositiveInfinity, box.Height);
    }

    [Fact]
    public void Ctor_CapturesPropsByValue_ReassigningSourceHasNoEffect()
    {
        // BezierCurveProperties is a value type: the entity snapshots it, unlike
        // Polyline's array which needs an explicit ToArray() copy.
        BezierCurveProperties props = Arch();
        var curve = new GraphicBezierCurve(props, Blue());

        props = Straight();

        var box = curve.BoundingBox();
        TestHelpers.Near(new Point(100, 100), box.BottomLeft);
        TestHelpers.Near(new Point(300, 300), box.TopRight);
    }

    [Fact]
    public void Ctor_NullStroke_ThrowsArgumentNullException()
    {
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(
            () => new GraphicBezierCurve(Arch(), null!));

        Assert.Equal("strokeIn", ex.ParamName);
    }

    [Fact]
    public void Draw_StraightHorizontal_PaintsMidRow()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Straight(), Blue());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 200));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 150, 200));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 250, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 210));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 190));
        // Near-field whites 4px off the centreline pin the 5px width:
        // a 10px stroke would still paint (200,204)...
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 204));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 196));
        // ...while 1px off-centre (pixel 201 spans y 201..202, fully inside the
        // 197.5..202.5 stroke) stays solid: a 1px stroke would leave it white.
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 201));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 199));
    }

    [Fact]
    public void Draw_ThickStroke_PaintsWiderRow()
    {
        // Same geometry, thickness 10 (half-width 5): 4px off-centre still paints.
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Straight(), Blue(10));

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 200));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 204));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 196));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 210));
        // Upper bound: 7px off-centre exceeds the 5px half-width + AA.
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 207));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 193));
    }

    [Fact]
    public void Draw_StraightHorizontal_PaintsNearEndpoints()
    {
        // Butt caps make the exact end pixel unreliable; 2px inside is solid.
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Straight(), Blue());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 102, 200));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 298, 200));
    }

    [Fact]
    public void Draw_ButtCaps_FlushAtEndpoints_PixelSquareDecides()
    {
        // Butt caps clip flush at the endpoint: the cap pixel paints iff its
        // [x,x+1)x[y,y+1) square falls inside the path — not a "half-open
        // contour" rule. For +X travel the P0 pixel (x 100..101 inside) paints
        // and the P3 pixel (x 300..301 outside) does not; -Y travel mirrors it
        // (see Draw_VerticalButtCaps_Mirrored). Proves the open path + butt caps.
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Straight(), Blue());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 100, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 300, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 99, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 301, 200));
    }

    [Fact]
    public void Draw_VerticalButtCaps_Mirrored()
    {
        // Same flush-cap rule, -Y travel (canvas up): the P0 pixel at canvas
        // (200,300) spans y 300..301, outside the path -> white; the P3 pixel
        // at (200,100) spans y 100..101, inside -> blue. Mirror of horizontal.
        var mapper = TestHelpers.SquareMapper(400);
        var vertical = new BezierCurveProperties(
            new Point(200, 100), new Point(200, 100 + 200.0 / 3),
            new Point(200, 100 + 400.0 / 3), new Point(200, 300));
        var curve = new GraphicBezierCurve(vertical, Blue());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 300));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 100));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 200));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 150));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 204, 200));
    }

    [Fact]
    public void Draw_StraightVertical_PaintsColumn()
    {
        // Vertical degenerate cubic: solid column, 4px lateral background.
        var mapper = TestHelpers.SquareMapper(400);
        var vertical = new BezierCurveProperties(
            new Point(200, 100), new Point(200, 100 + 200.0 / 3),
            new Point(200, 100 + 400.0 / 3), new Point(200, 300));
        var curve = new GraphicBezierCurve(vertical, Blue());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 200));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 250));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 204, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 196, 200));
    }

    [Fact]
    public void Draw_PartiallyOffViewport_PaintsVisiblePartWithoutThrowing()
    {
        // Canvas x -100..100 at y=200: only x 0..100 is on the 400-surface.
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Straight(x0: -100, x3: 100), Blue());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);

        var ex = Record.Exception(() => curve.Draw(surface.Canvas));

        Assert.Null(ex);
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 50, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 350, 200));
    }

    [Fact]
    public void Draw_FullyOffViewport_PaintsNothingWithoutThrowing()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Straight(x0: 500, x3: 600), Blue());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);

        var ex = Record.Exception(() => curve.Draw(surface.Canvas));

        Assert.Null(ex);
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 399, 200));
    }

    [Fact]
    public void Draw_VerticallyOffViewport_PaintsNothingWithoutThrowing()
    {
        // World y=500 -> canvas y=-100: fully above the surface.
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Straight(y: 500), Blue());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);

        var ex = Record.Exception(() => curve.Draw(surface.Canvas));

        Assert.Null(ex);
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 0));
    }

    [Fact]
    public void Draw_JustOffEdgeWithThickStroke_BleedsIn()
    {
        // World y=401 -> canvas y=-1 with half-width 5: rows 0..3 paint even
        // though every control point is off-surface.
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Straight(y: 401), Blue(10));

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 2));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 50));
    }

    [Fact]
    public void Draw_Diagonal_PaintsMidpoint()
    {
        // Colinear controls: cubic degenerates to the (50,350)-(350,50) diagonal.
        // (52,52) proves the Y flip: without it the line would run (50,350)-(350,50)
        // on-canvas and (52,52) would be ~300px away, white.
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Diagonal(), Blue());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 200));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 52, 52));
        // (52,348) is where the line would run WITHOUT the Y flip: white.
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 52, 348));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 210));
        // ~4.2px lateral: near-field white pins the 5px width on obliques too...
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 206, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 206));
        // ...while ~1.4px lateral stays solid (lower bound).
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 202, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 210, 200));
    }

    [Fact]
    public void Draw_Arch_PaintsApex()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Arch(), Red());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        // B(0.5) world (200,250) -> canvas (200,150).
        TestHelpers.AssertRed(TestHelpers.Sample(surface, 200, 150));
        // Tangent at the apex is horizontal (B'(0.5)=(300,0)): 5px along the row
        // stays red (extent, not a dot), 5px above/below is background.
        TestHelpers.AssertRed(TestHelpers.Sample(surface, 195, 150));
        TestHelpers.AssertRed(TestHelpers.Sample(surface, 205, 150));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 155));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 145));
    }

    [Fact]
    public void Draw_Arch_ChordMidpoint_StaysBackground()
    {
        // The straight chord (100,100)-(300,100) midpoint canvas (200,300) is far
        // from the bulging curve: proves the cubic is not a straight segment.
        // (200,250) is the no-Y-flip apex location: white proves the flip too,
        // and that the open arch never fills its interior (stroke-only).
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Arch(), Red());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 300));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 250));
    }

    [Fact]
    public void Draw_Arch_ControlPoints_NotOnCurve()
    {
        // Endpoints lie on the curve; interior controls do not.
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Arch(), Red());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        // Controls world (100,300)/(300,300) -> canvas (100,100)/(300,100).
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 100, 100));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 300, 100));
    }

    [Fact]
    public void Draw_Arch_NearEndpoints_Painted()
    {
        // Tangent at P0/P3 is vertical, so step 2px along the curve (canvas up).
        // The exact cap pixels straddle the flush cap line (y 300..301 outside
        // the path for both ends: -Y travel in, +Y travel out) and stay white.
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Arch(), Red());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertRed(TestHelpers.Sample(surface, 100, 298));
        TestHelpers.AssertRed(TestHelpers.Sample(surface, 300, 298));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 100, 300));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 300, 300));
    }

    [Fact]
    public void Draw_Arch_QuarterPoints_Painted()
    {
        // B(0.25) world (131.25,212.5) -> canvas (131,187.5) and mirror B(0.75)
        // world (268.75,212.5) -> canvas (269,187.5): proves cubic blending with
        // the true y=187.5 straddling rows 187/188, not just endpoints + apex.
        // (A P1/P2 swap would move B(0.25) to x=187.5, ~57px away.)
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Arch(), Red());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertRed(TestHelpers.Sample(surface, 131, 188));
        TestHelpers.AssertRed(TestHelpers.Sample(surface, 131, 187));
        TestHelpers.AssertRed(TestHelpers.Sample(surface, 269, 188));
        TestHelpers.AssertRed(TestHelpers.Sample(surface, 269, 187));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 131, 195));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 269, 195));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 131, 180));
    }

    [Fact]
    public void Draw_Arch_NearEndCurvature_Painted()
    {
        // B(0.1) world (105.6,154.2) -> canvas (106,246): near-P0 curvature,
        // far from both the endpoint tangent and any straight interpolation.
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Arch(), Red());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertRed(TestHelpers.Sample(surface, 106, 246));
    }

    [Fact]
    public void Draw_SCurve_InflectionPaints()
    {
        // S with controls on opposite sides: B(0.5) world (200,200) (on the
        // P0-P3 chord), B(0.25) world (131.25,187.5) -> canvas (131,212), while
        // the chord at x=131 runs through canvas (131,269). A quadratic through
        // the endpoints cannot match both the midpoint and the quarter point.
        var mapper = TestHelpers.SquareMapper(400);
        var s = new BezierCurveProperties(
            new Point(100, 100), new Point(100, 300),
            new Point(300, 100), new Point(300, 300));
        var curve = new GraphicBezierCurve(s, Blue());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 200));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 131, 212));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 131, 269));
    }

    [Fact]
    public void Draw_HorizontalAtWorldTop_MapsToCanvasTop_YFlipProof()
    {
        // World y=300 -> canvas y=100: the Y flip, not the identity row, paints.
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Straight(y: 300), Blue());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 100));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 300));
    }

    [Fact]
    public void Draw_OffCurvePixels_StayBackground()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Arch(), Red());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 20, 20));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 380, 380));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 20, 380));
    }

    [Fact]
    public void Draw_ClosedLoop_InteriorStaysBackground_ProvesStrokeOnly()
    {
        // Teardrop with P0==P3: the rim strokes but nothing fills (fill is Brush.None).
        // Loop apex B(0.5) world (200,250) -> canvas (200,150).
        var mapper = TestHelpers.SquareMapper(400);
        var loop = new BezierCurveProperties(
            new Point(200, 100), new Point(100, 300),
            new Point(300, 300), new Point(200, 100));
        var curve = new GraphicBezierCurve(loop, Blue());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 150));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 250));
    }

    [Fact]
    public void Draw_ClosedLoop_CuspIsHalfCovered_ProvesOpenPath()
    {
        // P0==P3 meets as two butt ends at an angle (no Close()): the cusp pixel
        // is partially covered — neither solid blue nor background white.
        var mapper = TestHelpers.SquareMapper(400);
        var loop = new BezierCurveProperties(
            new Point(200, 100), new Point(100, 300),
            new Point(300, 300), new Point(200, 100));
        var curve = new GraphicBezierCurve(loop, Blue());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        SKColor cusp = TestHelpers.Sample(surface, 200, 300);
        Assert.NotEqual(SKColors.White, cusp);
        Assert.True(cusp.Red > 55 || cusp.Green > 55 || cusp.Blue < 200,
            $"Expected a half-covered cusp pixel, not solid blue, but was {cusp}.");
        // ...while 5px inside along the axis the stroke is already solid...
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 295));
        // ...each branch paints individually (axis x=200 sits between them)...
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 197, 295));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 203, 295));
        // ...and 10px inside the bowl the fill stays absent.
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 160));
    }

    [Fact]
    public void Draw_PenNone_PaintsNothing()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Straight(), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 150, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 204));
    }

    [Fact]
    public void Draw_TransparentPen_PaintsNothing()
    {
        // Nonzero thickness but transparent colour: IsVisible=false, and the
        // transparent paint changes no pixel either way (no early-out pinned).
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Straight(), new Pen(5, SKColors.Transparent, []));

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 202));
    }

    [Fact]
    public void Draw_TransparentDashedPen_PaintsNothing()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(
            Straight(), new Pen(5, SKColors.Transparent, [10, 10]));

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 105, 200));
    }

    [Fact]
    public void Draw_DashedStroke_PaintsDashesAndGaps()
    {
        // Straight 200-long curve, dash [10,10] phase 0: x 100..110 on, 110..120 off.
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Straight(), new Pen(5, SKColors.Red, [10, 10]));

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertRed(TestHelpers.Sample(surface, 105, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 115, 200));
        TestHelpers.AssertRed(TestHelpers.Sample(surface, 125, 200));
        // 200 units is exactly 10 periods, so the last on-dash is [280,290)
        // and the path ends inside the trailing [290,300) gap.
        TestHelpers.AssertRed(TestHelpers.Sample(surface, 285, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 295, 200));
    }

    [Fact]
    public void Draw_MultiPatternDash_PaintsRunsAndGaps()
    {
        // [10,10,20,20] phase 0 from x=100: on 100..110, off 110..120,
        // on 120..140, off 140..160. Samples sit >=5px from every transition.
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Straight(), new Pen(5, SKColors.Red, [10, 10, 20, 20]));

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertRed(TestHelpers.Sample(surface, 105, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 115, 200));
        TestHelpers.AssertRed(TestHelpers.Sample(surface, 130, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 150, 200));
    }

    [Fact]
    public void Draw_DashedArch_DiffersFromSolidArch()
    {
        // Dashing applies by arc length on curves too: the dashed render must
        // differ somewhere from the solid one (same geometry, same mapper).
        var mapper = TestHelpers.SquareMapper(400);

        using SKSurface solid = TestHelpers.CreateSurface();
        new GraphicBezierCurve(Arch(), Red())
            .Also(c => c.Precompute(mapper)).Also(c => c.Draw(solid.Canvas));

        using SKSurface dashed = TestHelpers.CreateSurface();
        new GraphicBezierCurve(Arch(), new Pen(5, SKColors.Red, [10, 10]))
            .Also(c => c.Precompute(mapper)).Also(c => c.Draw(dashed.Canvas));

        Assert.True(TestHelpers.SurfacesDiffer(solid, dashed));
        Assert.True(TestHelpers.AnyPixelMatches(dashed, TestHelpers.IsReddish));
    }

    [Fact]
    public void Draw_DashedArch_ApexPaintedQuarterGapped()
    {
        // Exact arc-length phase pin: with [10,10] the apex (200,150) lands on
        // a dash while the quarter point (131,188) lands in a gap — dashing
        // follows the flattened arc length, deterministically per Skia version.
        var mapper = TestHelpers.SquareMapper(400);

        using SKSurface surface = TestHelpers.CreateSurface();
        new GraphicBezierCurve(Arch(), new Pen(5, SKColors.Red, [10, 10]))
            .Also(c => c.Precompute(mapper)).Also(c => c.Draw(surface.Canvas));

        TestHelpers.AssertRed(TestHelpers.Sample(surface, 200, 150));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 131, 188));
    }

    [Fact]
    public void Draw_DashedPartialCoincidence_DiffersFromUniformDash()
    {
        // Same pixels, different parametrization (zero end tangents compress
        // the ends): dashing follows the curve parametrization, so the two
        // dashed renders differ even though the solid ones would coincide.
        var mapper = TestHelpers.SquareMapper(400);

        using SKSurface uniform = TestHelpers.CreateSurface();
        new GraphicBezierCurve(Straight(), new Pen(5, SKColors.Red, [10, 10]))
            .Also(c => c.Precompute(mapper)).Also(c => c.Draw(uniform.Canvas));

        using SKSurface pinched = TestHelpers.CreateSurface();
        new GraphicBezierCurve(new BezierCurveProperties(
            new Point(100, 200), new Point(100, 200),
            new Point(300, 200), new Point(300, 200)), new Pen(5, SKColors.Red, [10, 10]))
            .Also(c => c.Precompute(mapper)).Also(c => c.Draw(pinched.Canvas));

        Assert.True(TestHelpers.SurfacesDiffer(uniform, pinched));
    }

    [Fact]
    public void Draw_NonSquareMapper_PaintsMappedMidpoint()
    {
        // 800x400 canvas, 200x100 world: world (100,50) -> canvas (400,200).
        var mapper = TestHelpers.MapperFor(800, 400, 200, 100);
        double third = 100.0 / 3;
        var props = new BezierCurveProperties(
            new Point(50, 50), new Point(50 + third, 50),
            new Point(50 + 2 * third, 50), new Point(150, 50));
        var curve = new GraphicBezierCurve(props, Blue());

        using SKSurface surface = TestHelpers.CreateSurface(800, 400);
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 400, 200));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 300, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 400, 210));
    }

    [Fact]
    public void Draw_AnisotropicMapper_ScalesAxesIndependently()
    {
        // 800x400 canvas, 400x400 world: sx=2, sy=1 (not uniform 4x/4x, so an
        // X/Y scale swap cannot hide). World (100,50) -> canvas (200,350).
        var mapper = TestHelpers.MapperFor(800, 400, 400, 400);
        var props = new BezierCurveProperties(
            new Point(50, 50), new Point(50 + 100.0 / 3, 50),
            new Point(50 + 200.0 / 3, 50), new Point(150, 50));
        var curve = new GraphicBezierCurve(props, Blue());

        using SKSurface surface = TestHelpers.CreateSurface(800, 400);
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 350));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 150, 350));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 100));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 360));
        // Near-field whites pin the row width under anisotropic scale too.
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 354));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 346));
    }

    [Fact]
    public void Draw_AnisotropicVertical_PaintsScaledColumn()
    {
        // Same mapper (sx=2, sy=1): vertical world x=250, y 0..100 ->
        // canvas x=500, y 400..300. Proves sx applies to verticals as well.
        var mapper = TestHelpers.MapperFor(800, 400, 400, 400);
        var vertical = new BezierCurveProperties(
            new Point(250, 0), new Point(250, 100.0 / 3),
            new Point(250, 200.0 / 3), new Point(250, 100));
        var curve = new GraphicBezierCurve(vertical, Blue());

        using SKSurface surface = TestHelpers.CreateSurface(800, 400);
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 500, 350));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 504, 350));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 496, 350));
    }

    [Fact]
    public void Draw_OffsetViewport_PaintsShifted()
    {
        // MapperFor(400,400,400,400,200,100): canvas=(x-200,500-y), so Straight()
        // lands at x -100..100, y=300. Both axes of the offset are exercised.
        var mapper = TestHelpers.MapperFor(400, 400, 400, 400, 200, 100);
        var curve = new GraphicBezierCurve(Straight(), Blue());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 50, 300));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 50, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 250, 300));
    }

    [Fact]
    public void Draw_DoesNotTouchMatrix()
    {
        // Like Line/Polyline (unlike GraphicShape): caller transform survives...
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Straight(), Blue());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        surface.Canvas.Translate(10, 0);
        curve.Draw(surface.Canvas);

        TestHelpers.Near(10, surface.Canvas.TotalMatrix.TransX, 1e-6);
        TestHelpers.Near(0, surface.Canvas.TotalMatrix.TransY, 1e-6);
    }

    [Fact]
    public void Draw_RespectsCallerTransform_PixelsShift()
    {
        // ...and is honoured: shifted 10px right, the row runs 110..310, so
        // 112 paints (2px inside the shifted start) while 102 stays background.
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Straight(), Blue());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        surface.Canvas.Translate(10, 0);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 112, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 102, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 102, 210));
    }

    [Fact]
    public void ShouldBe_Draw_TwiceWithoutRePrecompute_PaintsIdentically()
    {
        // DEFUSED (was Draw_TwiceWithoutRePrecompute_PaintsIdentically): second Draw
        // with the disposed SKPath AVs native (0xC0000005 at sk_canvas_draw_path)
        // instead of throwing managed. Prove single-use via handle so the suite
        // stays red/green instead of crashing the host.
        // See GraphicDisposalTests for the ShouldBe/KnownIssue pair.
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Arch(), Red());

        using SKSurface first = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(first.Canvas);

        var path = (SKPath)typeof(GraphicBezierCurve)
            .GetField("_curvePath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(curve)!;
        Assert.NotEqual(IntPtr.Zero, path.Handle);
    }

    [Fact]
    public void Draw_PartialCoincidence_PaintsRow()
    {
        // P0==P1, P2==P3: zero end tangents, still the straight row y=200.
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(100, 200), new Point(100, 200),
            new Point(300, 200), new Point(300, 200)), Blue());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);

        var ex = Record.Exception(() => curve.Draw(surface.Canvas));

        Assert.Null(ex);
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 210));
    }

    [Fact]
    public void Draw_PinchedControls_QuadraticLikeArch()
    {
        // P1==P2=(200,200): B(0.5) world (200,175) -> canvas (200,225).
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(100, 100), new Point(200, 200),
            new Point(200, 200), new Point(300, 100)), Red());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertRed(TestHelpers.Sample(surface, 200, 225));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 300));
        // Apex tangent is horizontal (B'(0.5)=(150,0)): 5px above/below white.
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 230));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 220));
    }

    [Fact]
    public void KnownIssue_DrawWithoutPrecompute_ThrowsArgumentNull()
    {
        // _curvePath is null! until Precompute, so the Skia call receives a null
        // path and Skia throws. Fail-fast is reasonable, but the Skia-sourced
        // ArgumentNullException (exact ParamName unpinned) leaks internals;
        // Line/Arc instead paint nothing without throwing.
        var curve = new GraphicBezierCurve(Straight(), Blue());

        using SKSurface surface = TestHelpers.CreateSurface();

        Assert.Throws<ArgumentNullException>(() => curve.Draw(surface.Canvas));
    }

    [Fact]
    public void Draw_DegeneratePoint_PaintsNothingWithoutThrowing()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(200, 200), new Point(200, 200),
            new Point(200, 200), new Point(200, 200)), Blue());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);

        var ex = Record.Exception(() => curve.Draw(surface.Canvas));

        Assert.Null(ex);
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 202));
    }

    [Fact]
    public void Draw_DegeneratePointWithPenNone_PaintsNothingWithoutThrowing()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(200, 200), new Point(200, 200),
            new Point(200, 200), new Point(200, 200)), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);

        var ex = Record.Exception(() => curve.Draw(surface.Canvas));

        Assert.Null(ex);
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void Draw_NaNControl_DoesNotThrow()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(double.NaN, 0), new Point(0, 0),
            new Point(10, 10), new Point(20, 20)), Blue());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);

        var ex = Record.Exception(() => curve.Draw(surface.Canvas));

        Assert.Null(ex);
    }

    [Fact]
    public void Draw_InfinityControl_DoesNotThrow()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(double.PositiveInfinity, 0), new Point(0, 0),
            new Point(10, 10), new Point(20, 20)), Blue());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);

        var ex = Record.Exception(() => curve.Draw(surface.Canvas));

        Assert.Null(ex);
    }

    [Fact]
    public void Draw_ZeroWorldWidthViewport_PaintsNothingWithoutThrowing()
    {
        // widthRatio is +Inf: control X maps to NaN/Inf, Skia draws nothing.
        var mapper = TestHelpers.MapperFor(400, 400, 0, 100);
        var curve = new GraphicBezierCurve(Straight(), Blue());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);

        var ex = Record.Exception(() => curve.Draw(surface.Canvas));

        Assert.Null(ex);
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void Draw_HugeWorldCoordinates_PaintsNothingWithoutThrowing()
    {
        // 1e30 overflows the (float) cast in ToSKPoint to Infinity: Skia clips
        // everything, no throw, background untouched.
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(1e30, 1e30), new Point(0, 0),
            new Point(10, 10), new Point(20, 20)), Blue());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);

        var ex = Record.Exception(() => curve.Draw(surface.Canvas));

        Assert.Null(ex);
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void KnownIssue_PrecomputeNullMapper_ThrowsNullReference()
    {
        // Bare deref of mapperIn (no ThrowIfNull, unlike GraphicEntity's ctor):
        // pinned as actual behavior per repo convention, like Line/Polyline.
        var curve = new GraphicBezierCurve(Straight(), Blue());

        Assert.Throws<NullReferenceException>(() => curve.Precompute(null!));
    }

    [Fact]
    public void KnownIssue_DrawNullCanvas_ThrowsNullReference()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Straight(), Blue());
        curve.Precompute(mapper);

        using SKSurface surface = TestHelpers.CreateSurface();

        Assert.Throws<NullReferenceException>(() => curve.Draw(null!));
    }

    [Fact]
    public void Draw_NullCanvasWithoutPrecompute_ThrowsNullReference()
    {
        // Null canvas receiver throws before the null _curvePath is even read.
        var curve = new GraphicBezierCurve(Straight(), Blue());

        Assert.Throws<NullReferenceException>(() => curve.Draw(null!));
    }

    [Fact]
    public void ShouldBe_PrecomputeNullMapper_ThrowsArgumentNull()
    {
        // RED: public API must throw ArgumentNullException (like the
        // GraphicEntity ctor's ThrowIfNull), not NullReferenceException.
        var curve = new GraphicBezierCurve(Straight(), Blue());

        Assert.Throws<ArgumentNullException>(() => curve.Precompute(null!));
    }

    [Fact]
    public void ShouldBe_DrawNullCanvas_ThrowsArgumentNull()
    {
        // RED: public API must throw ArgumentNullException, not
        // NullReferenceException from the bare canvasIn deref.
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Straight(), Blue());
        curve.Precompute(mapper);

        Assert.Throws<ArgumentNullException>(() => curve.Draw(null!));
    }

    [Fact]
    public void GraphicEntity_ImplementsIDisposable()
    {
        // Fixed: GraphicEntity now implements IDisposable. Kept to prove the
        // design demand; see GraphicDisposalTests for exhaustive cover.
        Assert.Contains(typeof(IDisposable), typeof(GraphicEntity).GetInterfaces());
    }

    [Fact]
    public void Precompute_Twice_SameMapper_StillPaints()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var curve = new GraphicBezierCurve(Straight(), Blue());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(mapper);
        curve.Precompute(mapper);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void Precompute_Twice_SecondMapperWins()
    {
        // Second mapper shifts world +200 in X: curve canvas x -100..100 at y=200.
        var first = TestHelpers.SquareMapper(400);
        var second = TestHelpers.MapperFor(400, 400, 400, 400, 200, 0);
        var curve = new GraphicBezierCurve(Straight(), Blue());

        using SKSurface surface = TestHelpers.CreateSurface();
        curve.Precompute(first);
        curve.Precompute(second);
        curve.Draw(surface.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 50, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 200));
    }

    [Fact]
    public void Draw_Precompute_Draw_SecondMapperWinsOnFreshSurface()
    {
        // Interleaved with a Draw: re-Precompute fully replaces the path, the
        // stale curve leaves no residue on the new surface.
        var first = TestHelpers.SquareMapper(400);
        var second = TestHelpers.MapperFor(400, 400, 400, 400, 200, 0);
        var curve = new GraphicBezierCurve(Straight(), Blue());

        using SKSurface stale = TestHelpers.CreateSurface();
        curve.Precompute(first);
        curve.Draw(stale.Canvas);
        TestHelpers.AssertBlue(TestHelpers.Sample(stale, 200, 200));

        using SKSurface moved = TestHelpers.CreateSurface();
        curve.Precompute(second);
        curve.Draw(moved.Canvas);

        TestHelpers.AssertBlue(TestHelpers.Sample(moved, 50, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(moved, 200, 200));
    }

    [Fact]
    public void Renderer_RendersBezier_NoThrowAndPaintsApex()
    {
        var renderer = new GraphicRenderer();
        renderer.SetEntities([new GraphicBezierCurve(Arch(), Red())]);

        using SKSurface surface = TestHelpers.CreateSurface();

        var ex = Record.Exception(() => renderer.Render(TestHelpers.SquareMapper(400), surface.Canvas));

        Assert.Null(ex);
        TestHelpers.AssertRed(TestHelpers.Sample(surface, 200, 150));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 200, 300));
    }

    [Fact]
    public void Renderer_RenderTwice_RepaintsApex()
    {
        var renderer = new GraphicRenderer();
        renderer.SetEntities([new GraphicBezierCurve(Arch(), Red())]);

        using SKSurface first = TestHelpers.CreateSurface();
        renderer.Render(TestHelpers.SquareMapper(400), first.Canvas);
        using SKSurface second = TestHelpers.CreateSurface();
        renderer.Render(TestHelpers.SquareMapper(400), second.Canvas);

        TestHelpers.AssertRed(TestHelpers.Sample(first, 200, 150));
        TestHelpers.AssertRed(TestHelpers.Sample(second, 200, 150));
    }

    [Fact]
    public void Renderer_DrawOrder_LaterEntityOverdraws()
    {
        // Straight world y=250 -> canvas y=150 crosses the arch apex (200,150).
        var mapper = TestHelpers.SquareMapper(400);

        using SKSurface archUnder = TestHelpers.CreateSurface();
        new GraphicRenderer().Also(r => r.SetEntities(
            [new GraphicBezierCurve(Arch(), Red()), new GraphicBezierCurve(Straight(y: 250), Blue())]))
            .Also(r => r.Render(mapper, archUnder.Canvas));

        using SKSurface archOver = TestHelpers.CreateSurface();
        new GraphicRenderer().Also(r => r.SetEntities(
            [new GraphicBezierCurve(Straight(y: 250), Blue()), new GraphicBezierCurve(Arch(), Red())]))
            .Also(r => r.Render(mapper, archOver.Canvas));

        TestHelpers.AssertBlue(TestHelpers.Sample(archUnder, 200, 150));
        TestHelpers.AssertRed(TestHelpers.Sample(archOver, 200, 150));
    }

    [Fact]
    public void Renderer_ShapeThenBezier_BezierDrawsUnshifted()
    {
        // GraphicShape.Draw calls ResetMatrix: like the trailing line in
        // GraphicRendererTests, a trailing bezier draws UNSHIFTED afterwards.
        var mapper = TestHelpers.SquareMapper(400);
        var rect = new GraphicRectangle(
            new RectangleProperties(100, 60, 0, 0, 0,
                HorizontalAlignment.Centre, VerticalAlignment.Centre, new Point(200, 200)),
            new Brush(SKColors.Blue), Pen.None);
        var renderer = new GraphicRenderer();
        renderer.SetEntities([rect, new GraphicBezierCurve(Straight(), Blue())]);

        using SKSurface surface = TestHelpers.CreateSurface();
        surface.Canvas.Translate(50, 0);
        renderer.Render(mapper, surface.Canvas);

        // Unshifted row runs 100..300 (rect fill only covers x 150..250, so
        // (102,200) is bezier-only); shifted it would run 150..350, painting
        // (340,200) instead — which stays background.
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 102, 200));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 340, 200));
        SKMatrix m = surface.Canvas.TotalMatrix;
        TestHelpers.Near(0, m.TransX, 1e-6);
    }

    [Fact]
    public void Renderer_ManyBeziers_ParallelPrecomputeCompletes()
    {
        // Mirrors GraphicRendererTests.Render_ManyEntities with vertical beziers:
        // one entity per x (wrapping, so each column is drawn several times).
        // Several distinct columns must paint: a single column could not prove
        // that every entity precomputed.
        var entities = new List<GraphicEntity>();
        for (int i = 0; i < 1000; i++)
        {
            double x = i % 400;
            entities.Add(new GraphicBezierCurve(new BezierCurveProperties(
                new Point(x, 0), new Point(x, 133),
                new Point(x, 266), new Point(x, 400)), Blue(1)));
        }

        var renderer = new GraphicRenderer();
        renderer.SetEntities(entities);

        using SKSurface surface = TestHelpers.CreateSurface();

        var ex = Record.Exception(() => renderer.Render(TestHelpers.SquareMapper(400), surface.Canvas));

        Assert.Null(ex);
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 50, 200));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 150, 200));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 200, 200));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 350, 200));
    }

    [Fact]
    public void Renderer_ManyBeziers_EveryInteriorColumnPaints()
    {
        // Full sweep: each interior column x=1..398 must contain a stroke pixel,
        // proving no Parallel.For iteration was dropped (edge columns are
        // half-clipped by the surface, so presence uses a lenient band).
        var entities = new List<GraphicEntity>();
        for (int i = 0; i < 1000; i++)
        {
            double x = i % 400;
            entities.Add(new GraphicBezierCurve(new BezierCurveProperties(
                new Point(x, 0), new Point(x, 133),
                new Point(x, 266), new Point(x, 400)), Blue(1)));
        }

        var renderer = new GraphicRenderer();
        renderer.SetEntities(entities);

        using SKSurface surface = TestHelpers.CreateSurface();
        renderer.Render(TestHelpers.SquareMapper(400), surface.Canvas);

        surface.Canvas.Flush();
        using SKImage image = surface.Snapshot();
        using SKBitmap bitmap = SKBitmap.FromImage(image);
        for (int x = 1; x <= 398; x++)
        {
            bool painted = false;
            for (int y = 0; y < 400; y += 2)
            {
                SKColor c = bitmap.GetPixel(x, y);
                if (c.Blue >= 150 && c.Red <= 100 && c.Green <= 100)
                {
                    painted = true;
                    break;
                }
            }
            Assert.True(painted, $"Column {x} has no stroke pixel: an entity was dropped.");
        }
    }

    [Fact]
    public void Draw_SceneBuildCurves_PaintWithoutThrowing()
    {
        // The two curves from Cartesia.WinUI.Test SceneBuild, each pinned at its
        // own B(0.5) apex. Viewport MapperFor(400,400,400,1000): sx=1, sy=0.4,
        // canvas=(x,400-0.4y). Bez1 apex world (175,650)->canvas (175,140);
        // Bez2 apex world (350,812.5)->canvas (350,75).
        var mapper = TestHelpers.MapperFor(400, 400, 400, 1000);
        var green = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(100, 600), new Point(150, 750),
            new Point(200, 500), new Point(250, 850)), new Pen(3, SKColors.ForestGreen, []));
        var blue = new GraphicBezierCurve(new BezierCurveProperties(
            new Point(300, 700), new Point(200, 850),
            new Point(500, 850), new Point(400, 700)), new Pen(3, SKColors.Blue, []));

        using SKSurface surface = TestHelpers.CreateSurface();
        green.Precompute(mapper);
        blue.Precompute(mapper);

        Assert.Null(Record.Exception(() => green.Draw(surface.Canvas)));
        Assert.Null(Record.Exception(() => blue.Draw(surface.Canvas)));

        TestHelpers.AssertGreenISH(TestHelpers.Sample(surface, 175, 140));
        TestHelpers.AssertBlue(TestHelpers.Sample(surface, 350, 75));
        TestHelpers.AssertWhite(TestHelpers.Sample(surface, 20, 200));
        // Bez1 B(0.25) world (137.6,653.1) -> canvas (138,139): a P1/P2 swap
        // would move it ~14px in x, so this detects swapped controls.
        TestHelpers.AssertGreenISH(TestHelpers.Sample(surface, 138, 139));
    }
}
