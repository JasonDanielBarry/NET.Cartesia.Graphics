using Cartesia.Core.Geometry;
using Cartesia.Core.Math;
using Cartesia.Tests.Helpers;

namespace Cartesia.Tests.Core;

public sealed class PointTests
{
    [Fact]
    public void Ctor_StoresXY()
    {
        Point p = new(3.5, -2.25);

        Assert.Equal(3.5, p.X);
        Assert.Equal(-2.25, p.Y);
    }

    [Fact]
    public void Ctor_ZeroAndNegatives_StoredExactly()
    {
        Point p = new(0, -0.0);

        Assert.Equal(0, p.X);
        Assert.Equal(0, p.Y);
    }

    [Fact]
    public void Transform_Identity_ReturnsSameCoordinates()
    {
        Point p = new(7.5, -3.25);

        Point result = p.Transform(AffineTransform.Identity());

        TestHelpers.Near(p, result);
    }

    [Fact]
    public void Transform_Translation_AddsOffsets()
    {
        Point p = new(1, 2);

        Point result = p.Transform(new AffineTransform(1, 0, 10, 0, 1, -5));

        TestHelpers.Near(new Point(11, -3), result);
    }

    [Fact]
    public void Transform_Scale_MultipliesComponents()
    {
        Point p = new(3, -4);

        Point result = p.Transform(new AffineTransform(2, 0, 0, 0, 3, 0));

        TestHelpers.Near(new Point(6, -12), result);
    }

    [Fact]
    public void Transform_FullMatrix_AppliesRowMajorFormula()
    {
        // newX = M00*X + M01*Y + M02 ; newY = M10*X + M11*Y + M12
        Point p = new(2, 5);

        Point result = p.Transform(new AffineTransform(1, 2, 3, 4, 5, 6));

        TestHelpers.Near(new Point(1 * 2 + 2 * 5 + 3, 4 * 2 + 5 * 5 + 6), result);
    }

    [Fact]
    public void Transform_NegativeScale_FlipsSign()
    {
        Point p = new(4, 9);

        Point result = p.Transform(new AffineTransform(1, 0, 0, 0, -1, 0));

        TestHelpers.Near(new Point(4, -9), result);
    }

    [Fact]
    public void Transform_Origin_StaysAtTranslation()
    {
        Point origin = new(0, 0);

        Point result = origin.Transform(new AffineTransform(5, 6, 7, 8, 9, 10));

        TestHelpers.Near(new Point(7, 10), result);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1e9, -1e9)]
    [InlineData(-123.456, 789.012)]
    public void Transform_Identity_RoundTripsArbitraryPoints(double x, double y)
    {
        Point p = new(x, y);

        TestHelpers.Near(p, p.Transform(AffineTransform.Identity()));
    }

    [Fact]
    public void Transform_Shear_MixesAxes()
    {
        Point p = new(2, 3);

        Point result = p.Transform(new AffineTransform(1, 4, 0, 5, 1, 0));

        TestHelpers.Near(new Point(2 + 12, 10 + 3), result);
    }

    [Fact]
    public void IsReadonlyStruct_CopySemantics_Preserved()
    {
        Point a = new(1, 1);
        Point b = a;

        Assert.Equal(a.X, b.X);
        Assert.Equal(a.Y, b.Y);
    }
}
