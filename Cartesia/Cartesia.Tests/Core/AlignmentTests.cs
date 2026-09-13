using Cartesia.Core.Geometry;
using Cartesia.Core.Shapes;
using Cartesia.Tests.Helpers;

namespace Cartesia.Tests.Core;

public sealed class AlignmentTests
{
    private static Box BoxFor(double w, double h, HorizontalAlignment ha, VerticalAlignment va, double hx = 0, double hy = 0) =>
        Box.FromDimensionsAndHandle(w, h, ha, va, new Point(hx, hy));

    [Theory]
    [InlineData(HorizontalAlignment.Left, 0, 10)]
    [InlineData(HorizontalAlignment.Centre, -5, 5)]
    [InlineData(HorizontalAlignment.Right, -10, 0)]
    public void HorizontalBounds_AllCases(HorizontalAlignment ha, double expLeft, double expRight)
    {
        Box box = BoxFor(10, 6, ha, VerticalAlignment.Bottom);

        TestHelpers.Near(expLeft, box.BottomLeft.X);
        TestHelpers.Near(expRight, box.TopRight.X);
    }

    [Theory]
    [InlineData(VerticalAlignment.Bottom, 0, 6)]
    [InlineData(VerticalAlignment.Centre, -3, 3)]
    [InlineData(VerticalAlignment.Top, -6, 0)]
    public void VerticalBounds_AllCases(VerticalAlignment va, double expBottom, double expTop)
    {
        Box box = BoxFor(10, 6, HorizontalAlignment.Left, va);

        TestHelpers.Near(expBottom, box.BottomLeft.Y);
        TestHelpers.Near(expTop, box.TopRight.Y);
    }

    [Fact]
    public void Centre_OddDimensions_HalvesExactly()
    {
        Box box = BoxFor(7, 5, HorizontalAlignment.Centre, VerticalAlignment.Centre);

        TestHelpers.Near(-3.5, box.BottomLeft.X);
        TestHelpers.Near(3.5, box.TopRight.X);
        TestHelpers.Near(-2.5, box.BottomLeft.Y);
        TestHelpers.Near(2.5, box.TopRight.Y);
    }

    [Fact]
    public void ZeroDimensions_AllAlignments_CollapseToHandle()
    {
        foreach (HorizontalAlignment ha in Enum.GetValues<HorizontalAlignment>())
        {
            foreach (VerticalAlignment va in Enum.GetValues<VerticalAlignment>())
            {
                Box box = BoxFor(0, 0, ha, va, 11, 22);

                TestHelpers.Near(new Point(11, 22), box.BottomLeft);
                TestHelpers.Near(new Point(11, 22), box.TopRight);
            }
        }
    }

    [Fact]
    public void HandleOffset_ShiftsAllAlignmentsEqually()
    {
        Box atOrigin = BoxFor(10, 6, HorizontalAlignment.Centre, VerticalAlignment.Centre);
        Box shifted = BoxFor(10, 6, HorizontalAlignment.Centre, VerticalAlignment.Centre, 100, -50);

        TestHelpers.Near(atOrigin.BottomLeft.X + 100, shifted.BottomLeft.X);
        TestHelpers.Near(atOrigin.BottomLeft.Y - 50, shifted.BottomLeft.Y);
        TestHelpers.Near(atOrigin.TopRight.X + 100, shifted.TopRight.X);
        TestHelpers.Near(atOrigin.TopRight.Y - 50, shifted.TopRight.Y);
    }

    [Fact]
    public void InvalidHorizontalAlignment_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Box.FromDimensionsAndHandle(10, 6, (HorizontalAlignment)999, VerticalAlignment.Bottom, new Point(0, 0)));
    }

    [Fact]
    public void InvalidVerticalAlignment_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() =>
            Box.FromDimensionsAndHandle(10, 6, HorizontalAlignment.Left, (VerticalAlignment)999, new Point(0, 0)));
    }

    [Fact]
    public void InvalidHorizontalAlignment_ParamNameIsAlignment()
    {
        ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            Alignment.HorizontalAndVerticalBounds(10, 6, (HorizontalAlignment)(-1), VerticalAlignment.Bottom));

        Assert.Equal("horizontalAlignmentIn", ex.ParamName);
    }

    [Fact]
    public void InvalidVerticalAlignment_ParamNameIsAlignment()
    {
        ArgumentException ex = Assert.Throws<ArgumentException>(() =>
            Alignment.HorizontalAndVerticalBounds(10, 6, HorizontalAlignment.Left, (VerticalAlignment)(-1)));

        Assert.Equal("verticalAlignmentIn", ex.ParamName);
    }

    [Fact]
    public void InvalidAlignments_ViaBox_PreserveParamNameAndMessage()
    {
        ArgumentException hEx = Assert.Throws<ArgumentException>(() =>
            Box.FromDimensionsAndHandle(10, 6, (HorizontalAlignment)999, VerticalAlignment.Bottom, new Point(0, 0)));

        Assert.Equal("horizontalAlignmentIn", hEx.ParamName);
        Assert.Contains("Invalid horizontal alignment", hEx.Message);

        ArgumentException vEx = Assert.Throws<ArgumentException>(() =>
            Box.FromDimensionsAndHandle(10, 6, HorizontalAlignment.Left, (VerticalAlignment)999, new Point(0, 0)));

        Assert.Equal("verticalAlignmentIn", vEx.ParamName);
        Assert.Contains("Invalid vertical alignment", vEx.Message);
    }

    [Fact]
    public void HorizontalAndVerticalBounds_Direct_AllNineCombos()
    {
        (HorizontalAlignment h, VerticalAlignment v, double l, double r, double b, double t)[] cases =
        [
            (HorizontalAlignment.Left, VerticalAlignment.Bottom, 0, 4, 0, 2),
            (HorizontalAlignment.Centre, VerticalAlignment.Bottom, -2, 2, 0, 2),
            (HorizontalAlignment.Right, VerticalAlignment.Bottom, -4, 0, 0, 2),
            (HorizontalAlignment.Left, VerticalAlignment.Centre, 0, 4, -1, 1),
            (HorizontalAlignment.Centre, VerticalAlignment.Centre, -2, 2, -1, 1),
            (HorizontalAlignment.Right, VerticalAlignment.Centre, -4, 0, -1, 1),
            (HorizontalAlignment.Left, VerticalAlignment.Top, 0, 4, -2, 0),
            (HorizontalAlignment.Centre, VerticalAlignment.Top, -2, 2, -2, 0),
            (HorizontalAlignment.Right, VerticalAlignment.Top, -4, 0, -2, 0),
        ];

        foreach (var (h, v, l, r, b, t) in cases)
        {
            (double left, double right, double bottom, double top) =
                Alignment.HorizontalAndVerticalBounds(4, 2, h, v);

            Assert.Equal(l, left);
            Assert.Equal(r, right);
            Assert.Equal(b, bottom);
            Assert.Equal(t, top);
        }
    }
}
