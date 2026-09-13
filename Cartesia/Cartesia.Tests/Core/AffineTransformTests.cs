using Cartesia.Core.Geometry;
using Cartesia.Core.Math;
using Cartesia.Tests.Helpers;

namespace Cartesia.Tests.Core;

public sealed class AffineTransformTests
{
    private static Point Apply(AffineTransform t, double x, double y) => new Point(x, y).Transform(t);

    [Fact]
    public void Identity_HasUnitMatrix()
    {
        AffineTransform t = AffineTransform.Identity();

        Assert.Equal(1, t.M00);
        Assert.Equal(0, t.M01);
        Assert.Equal(0, t.M02);
        Assert.Equal(0, t.M10);
        Assert.Equal(1, t.M11);
        Assert.Equal(0, t.M12);
    }

    [Fact]
    public void Identity_MapsAnyPointToItself()
    {
        TestHelpers.Near(new Point(-8.5, 42), Apply(AffineTransform.Identity(), -8.5, 42));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(5, -7)]
    [InlineData(-3.25, 100.5)]
    public void Translate_MovesPointByOffset(double dx, double dy)
    {
        AffineTransform t = AffineTransform.Identity().Translate(dx, dy);

        TestHelpers.Near(new Point(1 + dx, 2 + dy), Apply(t, 1, 2));
    }

    [Fact]
    public void Translate_SetsTranslationComponents()
    {
        AffineTransform t = AffineTransform.Identity().Translate(11, -4);

        Assert.Equal(11, t.M02);
        Assert.Equal(-4, t.M12);
        Assert.Equal(1, t.M00);
        Assert.Equal(1, t.M11);
    }

    [Theory]
    [InlineData(2, 3)]
    [InlineData(-1, 1)]
    [InlineData(0.5, 0.25)]
    [InlineData(0, 0)]
    public void Scale_MultipliesAxes(double sx, double sy)
    {
        AffineTransform t = AffineTransform.Identity().Scale(sx, sy);

        TestHelpers.Near(new Point(4 * sx, -5 * sy), Apply(t, 4, -5));
    }

    [Fact]
    public void Scale_SetsDiagonalComponents()
    {
        AffineTransform t = AffineTransform.Identity().Scale(2.5, -1.5);

        Assert.Equal(2.5, t.M00);
        Assert.Equal(-1.5, t.M11);
        Assert.Equal(0, t.M02);
        Assert.Equal(0, t.M12);
    }

    [Theory]
    [InlineData(0, 1, 0)]
    [InlineData(90, 0, 1)]
    [InlineData(180, -1, 0)]
    [InlineData(270, 0, -1)]
    [InlineData(360, 1, 0)]
    [InlineData(-90, 0, -1)]
    public void Rotate_TakesDegrees_MapsUnitX(double degrees, double expX, double expY)
    {
        AffineTransform t = AffineTransform.Identity().Rotate(degrees);

        TestHelpers.Near(new Point(expX, expY), Apply(t, 1, 0), 1e-9);
    }

    [Theory]
    [InlineData(90, -1, 0)]
    [InlineData(180, 0, -1)]
    [InlineData(45, -0.7071067811865475, 0.7071067811865475)]
    public void Rotate_MapsUnitY(double degrees, double expX, double expY)
    {
        AffineTransform t = AffineTransform.Identity().Rotate(degrees);

        TestHelpers.Near(new Point(expX, expY), Apply(t, 0, 1), 1e-9);
    }

    [Fact]
    public void Rotate_45_PreservesRadius()
    {
        AffineTransform t = AffineTransform.Identity().Rotate(45);

        Point r = Apply(t, 3, 4);

        TestHelpers.Near(5.0, Math.Sqrt(r.X * r.X + r.Y * r.Y), 1e-9);
    }

    [Fact]
    public void Rotate_360_IsIdentity()
    {
        AffineTransform t = AffineTransform.Identity().Rotate(360);

        TestHelpers.Near(new Point(3.3, -7.7), Apply(t, 3.3, -7.7), 1e-9);
    }

    [Fact]
    public void Rotate_UsesDegreesNotRadians_Rotate1IsTiny()
    {
        // If this were radians, Rotate(1) would move (1,0) to (cos1, sin1) ~ (0.54, 0.84).
        // In degrees it barely moves.
        AffineTransform t = AffineTransform.Identity().Rotate(1);

        Point r = Apply(t, 1, 0);

        TestHelpers.Near(new Point(Math.Cos(Math.PI / 180), Math.Sin(Math.PI / 180)), r, 1e-12);
    }

    [Fact]
    public void Chaining_AppliesInCallOrder_TranslateThenScale()
    {
        // (1,1) -> translate -> (11,1) -> scale -> (22,2)
        AffineTransform t = AffineTransform.Identity().Translate(10, 0).Scale(2, 2);

        TestHelpers.Near(new Point(22, 2), Apply(t, 1, 1));
    }

    [Fact]
    public void Chaining_AppliesInCallOrder_ScaleThenTranslate()
    {
        // (1,1) -> scale -> (2,2) -> translate -> (12,2)
        AffineTransform t = AffineTransform.Identity().Scale(2, 2).Translate(10, 0);

        TestHelpers.Near(new Point(12, 2), Apply(t, 1, 1));
    }

    [Fact]
    public void Chaining_TranslateThenRotate()
    {
        // (1,0) -> translate(1,0) -> (2,0) -> rotate90 -> (0,2)
        AffineTransform t = AffineTransform.Identity().Translate(1, 0).Rotate(90);

        TestHelpers.Near(new Point(0, 2), Apply(t, 1, 0), 1e-9);
    }

    [Fact]
    public void Chaining_RotateThenTranslate()
    {
        // (1,0) -> rotate90 -> (0,1) -> translate(1,0) -> (1,1)
        AffineTransform t = AffineTransform.Identity().Rotate(90).Translate(1, 0);

        TestHelpers.Near(new Point(1, 1), Apply(t, 1, 0), 1e-9);
    }

    [Fact]
    public void Multiply_LeftTimesRight_TranslationComposition()
    {
        AffineTransform t = AffineTransform.Identity().Translate(3, 4).Translate(5, 6);

        TestHelpers.Near(new Point(8, 10), Apply(t, 0, 0));
    }

    [Fact]
    public void Scale_ByZero_CollapsesAxis()
    {
        AffineTransform t = AffineTransform.Identity().Scale(0, 1);

        TestHelpers.Near(new Point(0, 7), Apply(t, 123, 7));
    }

    [Fact]
    public void Scale_Negative_Mirrors()
    {
        AffineTransform t = AffineTransform.Identity().Scale(-1, -1);

        TestHelpers.Near(new Point(-2, -3), Apply(t, 2, 3));
    }

    [Fact]
    public void Translate_ThenTranslateBack_IsIdentity()
    {
        AffineTransform t = AffineTransform.Identity().Translate(17, -23).Translate(-17, 23);

        TestHelpers.Near(new Point(4.5, 6.5), Apply(t, 4.5, 6.5));
    }

    [Fact]
    public void Rotate_PlusMinusCancel()
    {
        AffineTransform t = AffineTransform.Identity().Rotate(33).Rotate(-33);

        TestHelpers.Near(new Point(9, -2), Apply(t, 9, -2), 1e-9);
    }

    [Fact]
    public void Scale_ThenInverseScale_Restores()
    {
        AffineTransform t = AffineTransform.Identity().Scale(4, 0.5).Scale(0.25, 2);

        TestHelpers.Near(new Point(-3, 8), Apply(t, -3, 8));
    }

    [Fact]
    public void RotateSmallAngles_30_60_Combine()
    {
        AffineTransform t = AffineTransform.Identity().Rotate(30).Rotate(60);

        TestHelpers.Near(new Point(0, 1), Apply(t, 1, 0), 1e-9);
    }

    [Fact]
    public void Chaining_DoesNotMutateReceiver_Translate()
    {
        AffineTransform b = AffineTransform.Identity();
        AffineTransform t = b.Translate(10, 0);

        Assert.Equal(0, b.M02);
        Assert.Equal(10, t.M02);
    }

    [Fact]
    public void Chaining_DoesNotMutateReceiver_ScaleAndRotate()
    {
        AffineTransform b = AffineTransform.Identity().Translate(1, 2);
        AffineTransform s = b.Scale(2, 3);
        AffineTransform r = b.Rotate(90);

        Assert.Equal(1, b.M00);
        Assert.Equal(1, b.M02);
        Assert.Equal(2, s.M00);
        Assert.Equal(-1, r.M01, 9);
    }

    [Fact]
    public void Chaining_ExposesComposedMatrix()
    {
        // Translate(10,0) then Scale(2,2): M = S*T -> M00=2, M02=20.
        AffineTransform t = AffineTransform.Identity().Translate(10, 0).Scale(2, 2);

        Assert.Equal(2, t.M00);
        Assert.Equal(0, t.M01);
        Assert.Equal(20, t.M02);
        Assert.Equal(0, t.M10);
        Assert.Equal(2, t.M11);
        Assert.Equal(0, t.M12);
    }

    [Fact]
    public void Rotate90_MatrixComponents()
    {
        AffineTransform t = AffineTransform.Identity().Rotate(90);

        Assert.Equal(0, t.M00, 9);
        Assert.Equal(-1, t.M01, 9);
        Assert.Equal(0, t.M02, 9);
        Assert.Equal(1, t.M10, 9);
        Assert.Equal(0, t.M11, 9);
        Assert.Equal(0, t.M12, 9);
    }

    [Fact]
    public void Rotate_FractionalAngle_22Point5()
    {
        AffineTransform t = AffineTransform.Identity().Rotate(22.5);

        double rad = 22.5 * Math.PI / 180;
        TestHelpers.Near(new Point(Math.Cos(rad), Math.Sin(rad)), Apply(t, 1, 0), 1e-9);
    }

    [Fact]
    public void Rotate_720_IsIdentity()
    {
        TestHelpers.Near(new Point(3, -4), Apply(AffineTransform.Identity().Rotate(720), 3, -4), 1e-9);
    }

    [Fact]
    public void Rotate_450_EqualsRotate90()
    {
        Point a = Apply(AffineTransform.Identity().Rotate(450), 2, 5);
        Point b = Apply(AffineTransform.Identity().Rotate(90), 2, 5);

        TestHelpers.Near(b, a, 1e-9);
    }

    [Fact]
    public void Rotate_NaN_PropagatesNaN_DocumentsBehavior()
    {
        Point r = Apply(AffineTransform.Identity().Rotate(double.NaN), 1, 2);

        Assert.True(double.IsNaN(r.X) && double.IsNaN(r.Y));
    }

    [Fact]
    public void Scale_ZeroBoth_CollapsesToOrigin()
    {
        TestHelpers.Near(new Point(0, 0), Apply(AffineTransform.Identity().Scale(0, 0), 5, 7));
    }
}
