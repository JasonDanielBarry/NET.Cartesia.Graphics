using Cartesia.Core.Geometry;
using Cartesia.Core.Shapes;

namespace Cartesia.Tests.Core;

public sealed class ShapePropertiesTests
{
    [Fact]
    public void RectangleProperties_StoresAllFields()
    {
        var props = new RectangleProperties(
            150, 75, 15, 12, 30,
            HorizontalAlignment.Left, VerticalAlignment.Centre,
            new Point(600, 200));

        Assert.Equal(150, props.Width);
        Assert.Equal(75, props.Height);
        Assert.Equal(15, props.CornerRadiusX);
        Assert.Equal(12, props.CornerRadiusY);
        Assert.Equal(30, props.Rotation);
        Assert.Equal(HorizontalAlignment.Left, props.HorizontalAlignment);
        Assert.Equal(VerticalAlignment.Centre, props.VerticalAlignment);
        Assert.Equal(600, props.HandlePoint.X);
        Assert.Equal(200, props.HandlePoint.Y);
    }

    [Fact]
    public void RectangleProperties_ZeroRadii_Allowed()
    {
        var props = new RectangleProperties(
            10, 10, 0, 0, 0,
            HorizontalAlignment.Centre, VerticalAlignment.Centre,
            new Point(0, 0));

        Assert.Equal(0, props.CornerRadiusX);
        Assert.Equal(0, props.CornerRadiusY);
    }

    [Fact]
    public void EllipseProperties_StoresAllFields()
    {
        var props = new EllipseProperties(
            150, 75, 15,
            HorizontalAlignment.Right, VerticalAlignment.Top,
            new Point(1, 2));

        Assert.Equal(150, props.Width);
        Assert.Equal(75, props.Height);
        Assert.Equal(15, props.Rotation);
        Assert.Equal(HorizontalAlignment.Right, props.HorizontalAlignment);
        Assert.Equal(VerticalAlignment.Top, props.VerticalAlignment);
        Assert.Equal(1, props.HandlePoint.X);
        Assert.Equal(2, props.HandlePoint.Y);
    }

    [Fact]
    public void ArcProperties_StoresAllFields()
    {
        var props = new ArcProperties(150, 100, -90, 90, -45, new Point(400, 600));

        Assert.Equal(150, props.RadiusX);
        Assert.Equal(100, props.RadiusY);
        Assert.Equal(-90, props.StartAngle);
        Assert.Equal(90, props.EndAngle);
        Assert.Equal(-45, props.Rotation);
        Assert.Equal(400, props.HandlePoint.X);
        Assert.Equal(600, props.HandlePoint.Y);
    }

    [Fact]
    public void ArcProperties_FullCircleSpan_StoredVerbatim()
    {
        var props = new ArcProperties(50, 50, 0, 360, 0, new Point(0, 0));

        Assert.Equal(0, props.StartAngle);
        Assert.Equal(360, props.EndAngle);
    }

    [Fact]
    public void ShapeProperties_AreValueTypes_CopyOnAssign()
    {
        var original = new EllipseProperties(10, 20, 0,
            HorizontalAlignment.Centre, VerticalAlignment.Centre, new Point(0, 0));
        var copy = original;

        Assert.Equal(original.Width, copy.Width);
        Assert.Equal(original.HandlePoint.X, copy.HandlePoint.X);
    }

    [Fact]
    public void RectangleProperties_Negatives_StoredVerbatimWithoutValidation()
    {
        var props = new RectangleProperties(
            -10, -5, -3, -4, 0,
            HorizontalAlignment.Centre, VerticalAlignment.Centre, new Point(0, 0));

        Assert.Equal(-10, props.Width);
        Assert.Equal(-5, props.Height);
        Assert.Equal(-3, props.CornerRadiusX);
        Assert.Equal(-4, props.CornerRadiusY);
    }

    [Fact]
    public void EllipseProperties_Negatives_StoredVerbatimWithoutValidation()
    {
        var props = new EllipseProperties(
            -20, 0, -30,
            HorizontalAlignment.Left, VerticalAlignment.Top, new Point(1, 1));

        Assert.Equal(-20, props.Width);
        Assert.Equal(0, props.Height);
        Assert.Equal(-30, props.Rotation);
    }

    [Fact]
    public void ArcProperties_InvertedAndZeroSpans_StoredVerbatim()
    {
        var inverted = new ArcProperties(50, 50, 90, -90, 0, new Point(0, 0));

        Assert.Equal(90, inverted.StartAngle);
        Assert.Equal(-90, inverted.EndAngle);

        var zero = new ArcProperties(50, 50, 45, 45, 0, new Point(0, 0));

        Assert.Equal(zero.StartAngle, zero.EndAngle);
    }
}
