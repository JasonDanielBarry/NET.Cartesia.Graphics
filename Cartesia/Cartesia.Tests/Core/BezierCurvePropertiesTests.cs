using Cartesia.Core.Curve;
using Cartesia.Core.Geometry;

namespace Cartesia.Tests.Core;

public sealed class BezierCurvePropertiesTests
{
    [Fact]
    public void Ctor_StoresAllFourPoints()
    {
        // P1/P2 are deliberately asymmetric: a swap must fail, and the order
        // must match GraphicBezierCurve's CubicTo(P1, P2, P3) argument order.
        var props = new BezierCurveProperties(
            new Point(100, 600),
            new Point(150, 750),
            new Point(200, 500),
            new Point(250, 850));

        Assert.Equal(100, props.P0.X);
        Assert.Equal(600, props.P0.Y);
        Assert.Equal(150, props.P1.X);
        Assert.Equal(750, props.P1.Y);
        Assert.Equal(200, props.P2.X);
        Assert.Equal(500, props.P2.Y);
        Assert.Equal(250, props.P3.X);
        Assert.Equal(850, props.P3.Y);
    }

    [Fact]
    public void Ctor_NegativesAndFractions_StoredVerbatimWithoutValidation()
    {
        var props = new BezierCurveProperties(
            new Point(-10.5, -20.25),
            new Point(0, 0),
            new Point(1.5, -3.75),
            new Point(-100, 200));

        Assert.Equal(-10.5, props.P0.X);
        Assert.Equal(-20.25, props.P0.Y);
        Assert.Equal(0, props.P1.X);
        Assert.Equal(0, props.P1.Y);
        Assert.Equal(1.5, props.P2.X);
        Assert.Equal(-3.75, props.P2.Y);
        Assert.Equal(-100, props.P3.X);
        Assert.Equal(200, props.P3.Y);
    }

    [Fact]
    public void Ctor_NonExactFractions_StoredBitIdentical()
    {
        // No rounding/normalization: every lane survives bit-identical.
        var props = new BezierCurveProperties(
            new Point(0.1, 0.2),
            new Point(1.0 / 3.0, 2.0 / 3.0),
            new Point(-0.1, 1.0 / 7.0),
            new Point(double.Epsilon, -double.Epsilon));

        static long Bits(double v) => BitConverter.DoubleToInt64Bits(v);
        Assert.Equal(Bits(0.1), Bits(props.P0.X));
        Assert.Equal(Bits(0.2), Bits(props.P0.Y));
        Assert.Equal(Bits(1.0 / 3.0), Bits(props.P1.X));
        Assert.Equal(Bits(2.0 / 3.0), Bits(props.P1.Y));
        Assert.Equal(Bits(-0.1), Bits(props.P2.X));
        Assert.Equal(Bits(1.0 / 7.0), Bits(props.P2.Y));
        Assert.Equal(Bits(double.Epsilon), Bits(props.P3.X));
        Assert.Equal(Bits(-double.Epsilon), Bits(props.P3.Y));
    }

    [Fact]
    public void Ctor_SpecialDoubles_StoredVerbatimWithoutValidation()
    {
        var props = new BezierCurveProperties(
            new Point(double.NaN, double.PositiveInfinity),
            new Point(double.NegativeInfinity, double.MaxValue),
            new Point(double.MinValue, 0),
            new Point(1, -1));

        Assert.True(double.IsNaN(props.P0.X));
        Assert.Equal(double.PositiveInfinity, props.P0.Y);
        Assert.Equal(double.NegativeInfinity, props.P1.X);
        Assert.Equal(double.MaxValue, props.P1.Y);
        Assert.Equal(double.MinValue, props.P2.X);
        Assert.Equal(0, props.P2.Y);
        Assert.Equal(1, props.P3.X);
        Assert.Equal(-1, props.P3.Y);
    }

    [Fact]
    public void Ctor_NaNInInteriorSlot_StoredVerbatim()
    {
        var props = new BezierCurveProperties(
            new Point(1, 2), new Point(3, 4),
            new Point(5, double.NaN), new Point(7, 8));

        Assert.True(double.IsNaN(props.P2.Y));
        Assert.Equal(5, props.P2.X);
        Assert.Equal(7, props.P3.X);
    }

    [Fact]
    public void Ctor_NegativeZero_PreservedBitIdentical()
    {
        // -0.0 == 0.0 under ==, but the sign bit must survive storage.
        var props = new BezierCurveProperties(
            new Point(-0.0, 0.0), new Point(0.0, -0.0),
            new Point(1, 2), new Point(3, 4));

        Assert.Equal(
            BitConverter.DoubleToInt64Bits(-0.0),
            BitConverter.DoubleToInt64Bits(props.P0.X));
        Assert.Equal(
            BitConverter.DoubleToInt64Bits(0.0),
            BitConverter.DoubleToInt64Bits(props.P0.Y));
        Assert.Equal(
            BitConverter.DoubleToInt64Bits(-0.0),
            BitConverter.DoubleToInt64Bits(props.P1.Y));
    }

    [Fact]
    public void Ctor_AllNaN_StoredVerbatim()
    {
        var props = new BezierCurveProperties(
            new Point(double.NaN, double.NaN), new Point(double.NaN, double.NaN),
            new Point(double.NaN, double.NaN), new Point(double.NaN, double.NaN));

        Assert.True(double.IsNaN(props.P0.X));
        Assert.True(double.IsNaN(props.P1.Y));
        Assert.True(double.IsNaN(props.P2.X));
        Assert.True(double.IsNaN(props.P3.Y));
    }

    [Fact]
    public void Ctor_LargeFinite_StoredVerbatim()
    {
        // The struct stores 2e9 verbatim even though Box clamps beyond +-1e9;
        // struct-vs-box distinction pinned here (see render sentinel tests).
        var props = new BezierCurveProperties(
            new Point(2e9, -2e9), new Point(3e9, 1e9),
            new Point(-3e9, -1e9), new Point(1e9, 1e9));

        Assert.Equal(2e9, props.P0.X);
        Assert.Equal(-2e9, props.P0.Y);
        Assert.Equal(3e9, props.P1.X);
        Assert.Equal(-3e9, props.P2.X);
        Assert.Equal(-1e9, props.P2.Y);
    }

    [Fact]
    public void Ctor_Degenerate_AllSamePoint_Allowed()
    {
        var props = new BezierCurveProperties(
            new Point(7, 7), new Point(7, 7),
            new Point(7, 7), new Point(7, 7));

        Assert.Equal(7, props.P0.X);
        Assert.Equal(7, props.P0.Y);
        Assert.Equal(7, props.P1.X);
        Assert.Equal(7, props.P1.Y);
        Assert.Equal(7, props.P2.X);
        Assert.Equal(7, props.P2.Y);
        Assert.Equal(7, props.P3.X);
        Assert.Equal(7, props.P3.Y);
    }

    [Fact]
    public void Ctor_DegenerateShapes_StoredVerbatim()
    {
        // Closed loop (P0==P3), colinear, P1==P2, reversed endpoints,
        // cross-coincidence (P0==P2) and single-pair (P0==P1): all legal inputs,
        // every lane asserted so a P1/P2 swap or drop fails.
        var loop = new BezierCurveProperties(
            new Point(200, 100), new Point(100, 300),
            new Point(300, 300), new Point(200, 100));
        Assert.Equal(200, loop.P0.X);
        Assert.Equal(100, loop.P0.Y);
        Assert.Equal(100, loop.P1.X);
        Assert.Equal(300, loop.P1.Y);
        Assert.Equal(300, loop.P2.X);
        Assert.Equal(300, loop.P2.Y);
        Assert.Equal(200, loop.P3.X);
        Assert.Equal(100, loop.P3.Y);

        var colinear = new BezierCurveProperties(
            new Point(0, 0), new Point(1, 1),
            new Point(2, 2), new Point(3, 3));
        Assert.Equal(0, colinear.P0.X);
        Assert.Equal(1, colinear.P1.X);
        Assert.Equal(1, colinear.P1.Y);
        Assert.Equal(2, colinear.P2.X);
        Assert.Equal(2, colinear.P2.Y);
        Assert.Equal(3, colinear.P3.X);

        var pinched = new BezierCurveProperties(
            new Point(100, 100), new Point(200, 200),
            new Point(200, 200), new Point(300, 100));
        Assert.Equal(100, pinched.P0.X);
        Assert.Equal(200, pinched.P1.X);
        Assert.Equal(200, pinched.P1.Y);
        Assert.Equal(200, pinched.P2.X);
        Assert.Equal(200, pinched.P2.Y);
        Assert.Equal(300, pinched.P3.X);
        Assert.Equal(100, pinched.P3.Y);

        var reversed = new BezierCurveProperties(
            new Point(300, 100), new Point(200, 50),
            new Point(100, 150), new Point(0, 0));
        Assert.Equal(300, reversed.P0.X);
        Assert.Equal(100, reversed.P0.Y);
        Assert.Equal(200, reversed.P1.X);
        Assert.Equal(50, reversed.P1.Y);
        Assert.Equal(100, reversed.P2.X);
        Assert.Equal(150, reversed.P2.Y);
        Assert.Equal(0, reversed.P3.X);
        Assert.Equal(0, reversed.P3.Y);

        var cross = new BezierCurveProperties(
            new Point(5, 5), new Point(1, 2),
            new Point(5, 5), new Point(7, 8));
        Assert.Equal(cross.P0.X, cross.P2.X);
        Assert.Equal(cross.P0.Y, cross.P2.Y);
        Assert.Equal(1, cross.P1.X);
        Assert.Equal(7, cross.P3.X);

        var endTangent = new BezierCurveProperties(
            new Point(9, 9), new Point(9, 9),
            new Point(1, 2), new Point(7, 8));
        Assert.Equal(endTangent.P0.X, endTangent.P1.X);
        Assert.Equal(endTangent.P0.Y, endTangent.P1.Y);
        Assert.Equal(1, endTangent.P2.X);
        Assert.Equal(8, endTangent.P3.Y);
    }

    [Fact]
    public void BezierCurveProperties_IsValueType_CopyOnAssign()
    {
        Assert.True(typeof(BezierCurveProperties).IsValueType);
        System.Reflection.FieldInfo[] fields = typeof(BezierCurveProperties).GetFields(
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        Assert.Equal(4, fields.Length);
        Assert.Equal(["P0", "P1", "P2", "P3"], fields.Select(f => f.Name));
        Assert.All(fields, f =>
        {
            Assert.True(f.IsInitOnly, $"Field {f.Name} must stay readonly.");
            Assert.Equal(typeof(Point), f.FieldType);
        });
        Assert.True(
            Attribute.IsDefined(
                typeof(BezierCurveProperties),
                typeof(System.Runtime.CompilerServices.IsReadOnlyAttribute)),
            "Struct must stay readonly.");

        var original = new BezierCurveProperties(
            new Point(1, 2), new Point(3, 4),
            new Point(5, 6), new Point(7, 8));
        var copy = original;

        Assert.Equal(original.P0.X, copy.P0.X);
        Assert.Equal(original.P0.Y, copy.P0.Y);
        Assert.Equal(original.P1.X, copy.P1.X);
        Assert.Equal(original.P1.Y, copy.P1.Y);
        Assert.Equal(original.P2.X, copy.P2.X);
        Assert.Equal(original.P2.Y, copy.P2.Y);
        Assert.Equal(original.P3.X, copy.P3.X);
        Assert.Equal(original.P3.Y, copy.P3.Y);
    }

    [Fact]
    public void ValueCopy_ReassigningCopy_LeavesOriginalIntact()
    {
        var original = new BezierCurveProperties(
            new Point(1, 2), new Point(3, 4),
            new Point(5, 6), new Point(7, 8));
        var copy = original;

        copy = new BezierCurveProperties(
            new Point(0, 0), new Point(0, 0),
            new Point(0, 0), new Point(0, 0));

        Assert.Equal(1, original.P0.X);
        Assert.Equal(4, original.P1.Y);
        Assert.Equal(0, copy.P0.X);
    }

    [Fact]
    public void ValueEquality_SamePoints_Equal_AnyLaneDiffers_NotEqual()
    {
        var a = new BezierCurveProperties(
            new Point(1, 2), new Point(3, 4),
            new Point(5, 6), new Point(7, 8));
        var same = new BezierCurveProperties(
            new Point(1, 2), new Point(3, 4),
            new Point(5, 6), new Point(7, 8));
        var p0Differs = new BezierCurveProperties(
            new Point(99, 2), new Point(3, 4),
            new Point(5, 6), new Point(7, 8));
        var p1Differs = new BezierCurveProperties(
            new Point(1, 2), new Point(30, 4),
            new Point(5, 6), new Point(7, 8));
        var p2Differs = new BezierCurveProperties(
            new Point(1, 2), new Point(3, 4),
            new Point(5, 60), new Point(7, 8));
        var p3Differs = new BezierCurveProperties(
            new Point(1, 2), new Point(3, 4),
            new Point(5, 6), new Point(7, 99));

        Assert.Equal(a, same);
        Assert.Equal(a.GetHashCode(), same.GetHashCode());
        Assert.NotEqual(a, p0Differs);
        Assert.NotEqual(a, p1Differs);
        Assert.NotEqual(a, p2Differs);
        Assert.NotEqual(a, p3Differs);
        // No hash-inequality assert: ValueType.GetHashCode uses a first-field
        // fast path, so differing instances legally collide (verified: p0/p3
        // variants share a's hash). Only equal=>equal-hash is contractual.
    }

    [Fact]
    public void ValueEquality_NaNBits_EqualByDoubleEqualsSemantics()
    {
        // ValueType.Equals compares lanes with double.Equals, where NaN==NaN;
        // (note NaN==NaN via == is false). Pinned as actual struct semantics.
        var nanA = new BezierCurveProperties(
            new Point(double.NaN, 0), new Point(0, 0),
            new Point(0, 0), new Point(0, 0));
        var nanB = new BezierCurveProperties(
            new Point(double.NaN, 0), new Point(0, 0),
            new Point(0, 0), new Point(0, 0));

        Assert.Equal(nanA, nanB);
    }

    [Fact]
    public void Default_AllControlPointsAreOrigin()
    {
        BezierCurveProperties props = default;

        Assert.Equal(0, props.P0.X);
        Assert.Equal(0, props.P0.Y);
        Assert.Equal(0, props.P1.X);
        Assert.Equal(0, props.P1.Y);
        Assert.Equal(0, props.P2.X);
        Assert.Equal(0, props.P2.Y);
        Assert.Equal(0, props.P3.X);
        Assert.Equal(0, props.P3.Y);
        Assert.Equal(props, new BezierCurveProperties(
            new Point(0, 0), new Point(0, 0),
            new Point(0, 0), new Point(0, 0)));
    }
}
