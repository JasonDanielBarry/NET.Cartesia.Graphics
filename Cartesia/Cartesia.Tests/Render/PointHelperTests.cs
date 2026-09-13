using Cartesia.Core.Geometry;
using Cartesia.Render.Utilities;
using Cartesia.Tests.Helpers;

namespace Cartesia.Tests.Render;

public sealed class PointHelperTests
{
    [Fact]
    public void ToSKPoint_ConvertsXY()
    {
        var sk = new Point(3.5, -2.25).ToSKPoint();

        Assert.Equal(3.5f, sk.X);
        Assert.Equal(-2.25f, sk.Y);
    }

    [Fact]
    public void ToSKPoint_Zero_MapsToZero()
    {
        var sk = new Point(0, 0).ToSKPoint();

        Assert.Equal(0, sk.X);
        Assert.Equal(0, sk.Y);
    }

    [Fact]
    public void ToSKPoint_LargeValues_CastToFloat()
    {
        var sk = new Point(123456.789, -987654.321).ToSKPoint();

        TestHelpers.Near(123456.789, sk.X, 0.01);
        TestHelpers.Near(-987654.321, sk.Y, 0.01);
    }

    [Fact]
    public void ToSKPoint_NegativeValues_Preserved()
    {
        var sk = new Point(-1, -1).ToSKPoint();

        Assert.Equal(-1, sk.X);
        Assert.Equal(-1, sk.Y);
    }
}
