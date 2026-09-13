using Cartesia.Core.Geometry;
using Cartesia.Core.Mapping;
using Cartesia.Tests.Helpers;

namespace Cartesia.Tests.Core;

public sealed class WorldToCanvasMapperTests
{
    [Fact]
    public void MapWorldToCanvas_OriginViewport_MapsCorners()
    {
        var mapper = TestHelpers.SquareMapper(400);

        TestHelpers.Near(new Point(0, 400), mapper.MapWorldToCanvas(new Point(0, 0)));
        TestHelpers.Near(new Point(400, 0), mapper.MapWorldToCanvas(new Point(400, 400)));
    }

    [Fact]
    public void MapWorldToCanvas_YFlips_WorldUpBecomesCanvasDown()
    {
        var mapper = TestHelpers.SquareMapper(400);

        Point low = mapper.MapWorldToCanvas(new Point(100, 100));
        Point high = mapper.MapWorldToCanvas(new Point(100, 300));

        // Higher world Y -> smaller canvas Y.
        Assert.True(high.Y < low.Y);
        TestHelpers.Near(300, low.Y);
        TestHelpers.Near(100, high.Y);
        TestHelpers.Near(100, low.X);
    }

    [Fact]
    public void MapWorldToCanvas_Centre_MapsToCentre()
    {
        var mapper = TestHelpers.SquareMapper(400);

        TestHelpers.Near(new Point(200, 200), mapper.MapWorldToCanvas(new Point(200, 200)));
    }

    [Fact]
    public void MapWorldToCanvas_OffsetViewport_SubtractsBottomLeft()
    {
        // World (1000..1100, 2000..2100) onto 100x100 canvas.
        var mapper = TestHelpers.MapperFor(100, 100, 100, 100, 1000, 2000);

        TestHelpers.Near(new Point(0, 100), mapper.MapWorldToCanvas(new Point(1000, 2000)));
        TestHelpers.Near(new Point(100, 0), mapper.MapWorldToCanvas(new Point(1100, 2100)));
        TestHelpers.Near(new Point(50, 50), mapper.MapWorldToCanvas(new Point(1050, 2050)));
    }

    [Fact]
    public void MapWorldToCanvas_NonUniformScale_PerAxis()
    {
        // World 200x100 onto 400x400 canvas: X doubles, Y quadruples (then flips).
        var mapper = TestHelpers.MapperFor(400, 400, 200, 100);

        TestHelpers.Near(new Point(200, 0), mapper.MapWorldToCanvas(new Point(100, 100)));
        TestHelpers.Near(new Point(0, 400), mapper.MapWorldToCanvas(new Point(0, 0)));
    }

    [Fact]
    public void MapCanvasToWorld_InvertsWorldToCanvas()
    {
        var mapper = TestHelpers.SquareMapper(400);

        TestHelpers.Near(new Point(0, 0), mapper.MapCanvasToWorld(new Point(0, 400)));
        TestHelpers.Near(new Point(400, 400), mapper.MapCanvasToWorld(new Point(400, 0)));
        TestHelpers.Near(new Point(123, 321), mapper.MapCanvasToWorld(new Point(123, 400 - 321)));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(400, 400)]
    [InlineData(13.5, 277.25)]
    [InlineData(-50, 999)]
    public void WorldToCanvasToWorld_RoundTrips_SquareCanvas(double x, double y)
    {
        var mapper = TestHelpers.SquareMapper(400);

        Point roundTripped = mapper.MapCanvasToWorld(mapper.MapWorldToCanvas(new Point(x, y)));

        TestHelpers.Near(new Point(x, y), roundTripped, 1e-9);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(400, 300)]
    [InlineData(13.5, 277.25)]
    public void CanvasToWorldToCanvas_RoundTrips_SquareCanvas(double x, double y)
    {
        var mapper = TestHelpers.SquareMapper(400);

        Point roundTripped = mapper.MapWorldToCanvas(mapper.MapCanvasToWorld(new Point(x, y)));

        TestHelpers.Near(new Point(x, y), roundTripped, 1e-9);
    }

    [Fact]
    public void RoundTrip_NonSquareCanvas_PointsStillRoundTrip()
    {
        // Point transforms are mutually inverse for any canvas shape; only the
        // scalar DT helpers are asymmetric (see below).
        var mapper = TestHelpers.MapperFor(800, 400, 200, 100);

        Point roundTripped = mapper.MapCanvasToWorld(mapper.MapWorldToCanvas(new Point(77.5, 33.25)));

        TestHelpers.Near(new Point(77.5, 33.25), roundTripped, 1e-9);
    }

    [Fact]
    public void MapWorldToCanvas_Array_PreservesOrderAndLength()
    {
        var mapper = TestHelpers.SquareMapper(400);
        Point[] input = [new Point(0, 0), new Point(400, 400), new Point(10, 20)];

        Point[] output = mapper.MapWorldToCanvas(input);

        Assert.Equal(3, output.Length);
        TestHelpers.Near(new Point(0, 400), output[0]);
        TestHelpers.Near(new Point(400, 0), output[1]);
        TestHelpers.Near(new Point(10, 380), output[2]);
    }

    [Fact]
    public void MapWorldToCanvas_EmptyArray_ReturnsEmpty()
    {
        var mapper = TestHelpers.SquareMapper(400);

        Assert.Empty(mapper.MapWorldToCanvas([]));
    }

    [Fact]
    public void MapCanvasToWorld_Array_PreservesOrderAndLength()
    {
        var mapper = TestHelpers.SquareMapper(400);
        Point[] input = [new Point(0, 400), new Point(400, 0)];

        Point[] output = mapper.MapCanvasToWorld(input);

        Assert.Equal(2, output.Length);
        TestHelpers.Near(new Point(0, 0), output[0]);
        TestHelpers.Near(new Point(400, 400), output[1]);
    }

    [Fact]
    public void MapCanvasToWorld_EmptyArray_ReturnsEmpty()
    {
        var mapper = TestHelpers.SquareMapper(400);

        Assert.Empty(mapper.MapCanvasToWorld([]));
    }

    [Fact]
    public void MapWorldToCanvas_Array_DoesNotMutateInput()
    {
        var mapper = TestHelpers.SquareMapper(400);
        Point[] input = [new Point(5, 6)];

        _ = mapper.MapWorldToCanvas(input);

        TestHelpers.Near(new Point(5, 6), input[0]);
    }

    [Fact]
    public void WorldDXToCanvasDL_ScalesByWidthRatio()
    {
        var mapper = TestHelpers.MapperFor(800, 400, 200, 100);

        TestHelpers.Near(40, mapper.WorldDXToCanvasDL(10));
        TestHelpers.Near(-8, mapper.WorldDXToCanvasDL(-2));
        TestHelpers.Near(0, mapper.WorldDXToCanvasDL(0));
    }

    [Fact]
    public void CanvasDLToWorldDX_InvertsWorldDXToCanvasDL()
    {
        var mapper = TestHelpers.MapperFor(800, 400, 200, 100);

        TestHelpers.Near(10, mapper.CanvasDLToWorldDX(mapper.WorldDXToCanvasDL(10)));
        TestHelpers.Near(200, mapper.CanvasDLToWorldDX(800));
    }

    [Fact]
    public void KnownIssue_WorldDYToCanvasDT_UsesCanvasWidthNumerator()
    {
        // Implementation: -dY * canvasWidthIn / worldHeight. The width numerator
        // is pinned here as actual behavior; flag to owner before changing.
        var mapper = TestHelpers.MapperFor(canvasW: 800, canvasH: 400, worldW: 200, worldH: 100);

        TestHelpers.Near(-10 * 800 / 100, mapper.WorldDYToCanvasDT(10));
        TestHelpers.Near(0, mapper.WorldDYToCanvasDT(0));
    }

    [Fact]
    public void CanvasDTToWorldDY_UsesHeightNumerator()
    {
        var mapper = TestHelpers.MapperFor(canvasW: 800, canvasH: 400, worldW: 200, worldH: 100);

        TestHelpers.Near(-100.0 * 100 / 400, mapper.CanvasDTToWorldDY(100));
    }

    [Fact]
    public void KnownIssue_ScalarDY_RoundTripBreaksOnNonSquareCanvas()
    {
        // WorldDYToCanvasDT scales with canvas WIDTH while CanvasDTToWorldDY
        // scales with canvas HEIGHT, so dy does not round-trip unless square.
        var mapper = TestHelpers.MapperFor(canvasW: 800, canvasH: 400, worldW: 200, worldH: 100);

        double roundTripped = mapper.CanvasDTToWorldDY(mapper.WorldDYToCanvasDT(10));

        Assert.NotEqual(10, roundTripped);
        TestHelpers.Near(10 * 800 / 400, roundTripped);
    }

    [Fact]
    public void ScalarDY_RoundTrip_HoldsOnSquareCanvas()
    {
        var mapper = TestHelpers.SquareMapper(400);

        TestHelpers.Near(7.5, mapper.CanvasDTToWorldDY(mapper.WorldDYToCanvasDT(7.5)));
        TestHelpers.Near(-3.25, mapper.CanvasDTToWorldDY(mapper.WorldDYToCanvasDT(-3.25)));
    }

    [Fact]
    public void ScalarDX_RoundTrip_HoldsForAnyCanvasShape()
    {
        var mapper = TestHelpers.MapperFor(canvasW: 800, canvasH: 400, worldW: 200, worldH: 100);

        TestHelpers.Near(7.5, mapper.CanvasDLToWorldDX(mapper.WorldDXToCanvasDL(7.5)));
    }

    [Fact]
    public void NegativeWorldDeltas_PreserveSignThroughYFlip()
    {
        var mapper = TestHelpers.SquareMapper(400);

        Assert.True(mapper.WorldDYToCanvasDT(5) < 0);
        Assert.True(mapper.WorldDYToCanvasDT(-5) > 0);
        Assert.True(mapper.CanvasDTToWorldDY(5) < 0);
    }

    [Fact]
    public void LargeViewport_SmallCanvas_Downscales()
    {
        var mapper = TestHelpers.MapperFor(canvasW: 10, canvasH: 10, worldW: 1000, worldH: 1000);

        TestHelpers.Near(new Point(10, 0), mapper.MapWorldToCanvas(new Point(1000, 1000)));
        TestHelpers.Near(0.1, mapper.WorldDXToCanvasDL(10));
    }

    [Fact]
    public void MapWorldToCanvas_AbsoluteValues_NonSquareCanvas()
    {
        // widthRatio=4, heightRatio=4: (100,50) -> (400, -4*50+4*100=200).
        var mapper = TestHelpers.MapperFor(800, 400, 200, 100);

        TestHelpers.Near(new Point(400, 200), mapper.MapWorldToCanvas(new Point(100, 50)));
        TestHelpers.Near(new Point(0, 400), mapper.MapWorldToCanvas(new Point(0, 0)));
        TestHelpers.Near(new Point(800, 0), mapper.MapWorldToCanvas(new Point(200, 100)));
    }

    [Fact]
    public void MapCanvasToWorld_AbsoluteValues_NonSquareOffsetViewport()
    {
        // wx = cx*200/800+1000; wy = 2100-cy*100/400.
        var mapper = TestHelpers.MapperFor(800, 400, 200, 100, 1000, 2000);

        TestHelpers.Near(new Point(1000, 2100), mapper.MapCanvasToWorld(new Point(0, 0)));
        TestHelpers.Near(new Point(1200, 2000), mapper.MapCanvasToWorld(new Point(800, 400)));
        TestHelpers.Near(new Point(1050, 2050), mapper.MapCanvasToWorld(new Point(200, 200)));
    }

    [Fact]
    public void MapCanvasToWorld_Array_OffsetViewport_AbsoluteValues()
    {
        var mapper = TestHelpers.MapperFor(100, 100, 100, 100, 1000, 2000);

        Point[] output = mapper.MapCanvasToWorld([new Point(0, 100), new Point(100, 0)]);

        TestHelpers.Near(new Point(1000, 2000), output[0]);
        TestHelpers.Near(new Point(1100, 2100), output[1]);
    }

    [Fact]
    public void MapCanvasToWorld_Array_DoesNotMutateInput()
    {
        var mapper = TestHelpers.MapperFor(100, 100, 100, 100, 1000, 2000);
        Point[] input = [new Point(0, 100)];

        _ = mapper.MapCanvasToWorld(input);

        TestHelpers.Near(new Point(0, 100), input[0]);
    }

    [Fact]
    public void MapSingle_EqualsMapArray_FirstElement_NonSquare()
    {
        var mapper = TestHelpers.MapperFor(800, 400, 200, 100, 1000, 2000);
        Point p = new(1042.5, 2033.25);

        TestHelpers.Near(mapper.MapWorldToCanvas(p), mapper.MapWorldToCanvas([p])[0]);
        TestHelpers.Near(mapper.MapCanvasToWorld(p), mapper.MapCanvasToWorld([p])[0]);
    }

    [Fact]
    public void MapWorldToCanvas_NullArray_ThrowsNullReferenceException()
    {
        var mapper = TestHelpers.SquareMapper(400);

        Assert.Throws<NullReferenceException>(() => mapper.MapWorldToCanvas(null!));
    }

    [Fact]
    public void MapCanvasToWorld_NullArray_ThrowsNullReferenceException()
    {
        var mapper = TestHelpers.SquareMapper(400);

        Assert.Throws<NullReferenceException>(() => mapper.MapCanvasToWorld(null!));
    }

    [Fact]
    public void ScalarMagnitudes_NegativeAndZeroDeltas()
    {
        var mapper = TestHelpers.MapperFor(canvasW: 800, canvasH: 400, worldW: 200, worldH: 100);

        TestHelpers.Near(80, mapper.WorldDYToCanvasDT(-10));
        TestHelpers.Near(25, mapper.CanvasDTToWorldDY(-100));
        TestHelpers.Near(0, mapper.CanvasDTToWorldDY(0));
        TestHelpers.Near(0, mapper.WorldDXToCanvasDL(0));
        TestHelpers.Near(-2, mapper.CanvasDLToWorldDX(-8));
    }

    [Fact]
    public void KnownIssue_ZeroWorldWidth_MapsXToNaN()
    {
        // widthRatio = 800/0 = +Inf; M02 = -Inf*BL.x = NaN when BL.x == 0. No throw; flag to owner.
        var mapper = TestHelpers.MapperFor(400, 400, 0, 100);

        Point mapped = mapper.MapWorldToCanvas(new Point(0, 50));

        Assert.True(double.IsNaN(mapped.X) || double.IsInfinity(mapped.X));
        Assert.True(double.IsNaN(mapper.WorldDXToCanvasDL(0)));
    }

    [Fact]
    public void KnownIssue_ZeroCanvasWidth_CollapsesX()
    {
        var mapper = TestHelpers.MapperFor(0, 400, 200, 100);

        TestHelpers.Near(new Point(0, 0), mapper.MapWorldToCanvas(new Point(100, 100)));
        TestHelpers.Near(0, mapper.WorldDXToCanvasDL(10));
    }

    [Fact]
    public void ShouldBe_WorldDYToCanvasDT_UsesCanvasHeightNumerator()
    {
        // RED: WorldDYToCanvasDT must mirror CanvasDTToWorldDY and scale with canvas HEIGHT.
        var mapper = TestHelpers.MapperFor(canvasW: 800, canvasH: 400, worldW: 200, worldH: 100);

        TestHelpers.Near(-10 * 400 / 100, mapper.WorldDYToCanvasDT(10));
    }

    [Fact]
    public void ShouldBe_ScalarDY_RoundTripsOnNonSquareCanvas()
    {
        // RED: dy -> canvas -> dy must round-trip on any canvas shape.
        var mapper = TestHelpers.MapperFor(canvasW: 800, canvasH: 400, worldW: 200, worldH: 100);

        TestHelpers.Near(10, mapper.CanvasDTToWorldDY(mapper.WorldDYToCanvasDT(10)));
    }
}
