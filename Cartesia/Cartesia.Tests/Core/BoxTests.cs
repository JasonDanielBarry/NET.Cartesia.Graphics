using Cartesia.Core.Geometry;
using Cartesia.Core.Math;
using Cartesia.Core.Shapes;
using Cartesia.Tests.Helpers;

namespace Cartesia.Tests.Core;

public sealed class BoxTests
{
    [Fact]
    public void Ctor_TwoPoints_SetsCornersAndDimensions()
    {
        Box box = new([new Point(1, 2), new Point(4, 6)]);

        TestHelpers.Near(new Point(1, 2), box.BottomLeft);
        TestHelpers.Near(new Point(4, 6), box.TopRight);
        TestHelpers.Near(3, box.Width);
        TestHelpers.Near(4, box.Height);
    }

    [Fact]
    public void Ctor_UnorderedPoints_NormalizesMinMax()
    {
        Box box = new([new Point(4, 6), new Point(1, 2)]);

        TestHelpers.Near(new Point(1, 2), box.BottomLeft);
        TestHelpers.Near(new Point(4, 6), box.TopRight);
    }

    [Fact]
    public void Ctor_ManyPoints_TakesExtremes()
    {
        Box box = new([new Point(0, 0), new Point(10, 1), new Point(3, 9), new Point(-2, 4)]);

        TestHelpers.Near(new Point(-2, 0), box.BottomLeft);
        TestHelpers.Near(new Point(10, 9), box.TopRight);
        TestHelpers.Near(12, box.Width);
        TestHelpers.Near(9, box.Height);
    }

    [Fact]
    public void Ctor_SinglePoint_ZeroSizeBox()
    {
        Box box = new([new Point(5, -5)]);

        TestHelpers.Near(new Point(5, -5), box.BottomLeft);
        TestHelpers.Near(new Point(5, -5), box.TopRight);
        TestHelpers.Near(0, box.Width);
        TestHelpers.Near(0, box.Height);
    }

    [Fact]
    public void Ctor_NegativeCoordinates_Works()
    {
        Box box = new([new Point(-10, -20), new Point(-1, -2)]);

        TestHelpers.Near(new Point(-10, -20), box.BottomLeft);
        TestHelpers.Near(new Point(-1, -2), box.TopRight);
        TestHelpers.Near(9, box.Width);
        TestHelpers.Near(18, box.Height);
    }

    [Fact]
    public void Ctor_DuplicatePoints_IgnoresDuplicates()
    {
        Box box = new([new Point(2, 2), new Point(2, 2), new Point(8, 8), new Point(8, 8)]);

        TestHelpers.Near(6, box.Width);
        TestHelpers.Near(6, box.Height);
    }

    [Fact]
    public void Ctor_EmptyList_ProducesInvertedSentinelBox()
    {
        // No points: minima stay at +1e9, maxima at -1e9. Documents degenerate input.
        Box box = new([]);

        TestHelpers.Near(new Point(1e9, 1e9), box.BottomLeft);
        TestHelpers.Near(new Point(-1e9, -1e9), box.TopRight);
        TestHelpers.Near(-2e9, box.Width);
        TestHelpers.Near(-2e9, box.Height);
    }

    [Fact]
    public void FromBoxes_TwoBoxes_SpansBoth()
    {
        Box a = new([new Point(0, 0), new Point(2, 2)]);
        Box b = new([new Point(5, 1), new Point(7, 9)]);

        Box merged = Box.FromBoxes([a, b]);

        TestHelpers.Near(new Point(0, 0), merged.BottomLeft);
        TestHelpers.Near(new Point(7, 9), merged.TopRight);
    }

    [Fact]
    public void FromBoxes_EmptyList_ProducesInvertedSentinelBox()
    {
        Box merged = Box.FromBoxes([]);

        TestHelpers.Near(new Point(1e9, 1e9), merged.BottomLeft);
        TestHelpers.Near(new Point(-1e9, -1e9), merged.TopRight);
    }

    [Fact]
    public void FromBoxes_OverlappingBoxes_SpansExtremes()
    {
        Box a = new([new Point(0, 0), new Point(10, 10)]);
        Box b = new([new Point(5, 5), new Point(6, 6)]);

        Box merged = Box.FromBoxes([a, b]);

        TestHelpers.Near(new Point(0, 0), merged.BottomLeft);
        TestHelpers.Near(new Point(10, 10), merged.TopRight);
    }

    [Fact]
    public void FromBoxes_NegativeBoxes_SpansCorrectly()
    {
        Box a = new([new Point(-8, -8), new Point(-2, -2)]);
        Box b = new([new Point(-5, -9), new Point(-1, -1)]);

        Box merged = Box.FromBoxes([a, b]);

        TestHelpers.Near(new Point(-8, -9), merged.BottomLeft);
        TestHelpers.Near(new Point(-1, -1), merged.TopRight);
    }

    [Fact]
    public void FromBoxes_SingleBox_RoundTrips()
    {
        Box a = new([new Point(1, 2), new Point(3, 4)]);

        Box merged = Box.FromBoxes([a]);

        TestHelpers.Near(a.BottomLeft, merged.BottomLeft);
        TestHelpers.Near(a.TopRight, merged.TopRight);
    }

    [Fact]
    public void FromBoxes_ThreeBoxesDisjoint_SpansAll()
    {
        Box a = new([new Point(-5, -5), new Point(-1, -1)]);
        Box b = new([new Point(0, 0), new Point(1, 1)]);
        Box c = new([new Point(10, -3), new Point(12, 7)]);

        Box merged = Box.FromBoxes([a, b, c]);

        TestHelpers.Near(new Point(-5, -5), merged.BottomLeft);
        TestHelpers.Near(new Point(12, 7), merged.TopRight);
        TestHelpers.Near(17, merged.Width);
        TestHelpers.Near(12, merged.Height);
    }

    public static TheoryData<HorizontalAlignment, VerticalAlignment, double, double, double, double> HandleCases =>
        new()
        {
            { HorizontalAlignment.Left, VerticalAlignment.Bottom, 0, 0, 10, 8 },
            { HorizontalAlignment.Centre, VerticalAlignment.Centre, 0, 0, 10, 8 },
            { HorizontalAlignment.Right, VerticalAlignment.Top, 0, 0, 10, 8 },
            { HorizontalAlignment.Left, VerticalAlignment.Top, 100, 200, 10, 8 },
            { HorizontalAlignment.Right, VerticalAlignment.Bottom, -50, -60, 10, 8 },
        };

    [Theory]
    [MemberData(nameof(HandleCases))]
    public void FromDimensionsAndHandle_PreservesDimensions(
        HorizontalAlignment h, VerticalAlignment v, double hx, double hy, double w, double hgt)
    {
        Box box = Box.FromDimensionsAndHandle(w, hgt, h, v, new Point(hx, hy));

        TestHelpers.Near(w, box.Width);
        TestHelpers.Near(hgt, box.Height);
    }

    [Fact]
    public void FromDimensionsAndHandle_LeftBottom_OriginAtHandle()
    {
        Box box = Box.FromDimensionsAndHandle(10, 8, HorizontalAlignment.Left, VerticalAlignment.Bottom, new Point(3, 4));

        TestHelpers.Near(new Point(3, 4), box.BottomLeft);
        TestHelpers.Near(new Point(13, 12), box.TopRight);
    }

    [Fact]
    public void FromDimensionsAndHandle_Centre_SymmetricAboutHandle()
    {
        Box box = Box.FromDimensionsAndHandle(10, 8, HorizontalAlignment.Centre, VerticalAlignment.Centre, new Point(0, 0));

        TestHelpers.Near(new Point(-5, -4), box.BottomLeft);
        TestHelpers.Near(new Point(5, 4), box.TopRight);
    }

    [Fact]
    public void FromDimensionsAndHandle_RightTop_ExtendsNegative()
    {
        Box box = Box.FromDimensionsAndHandle(10, 8, HorizontalAlignment.Right, VerticalAlignment.Top, new Point(0, 0));

        TestHelpers.Near(new Point(-10, -8), box.BottomLeft);
        TestHelpers.Near(new Point(0, 0), box.TopRight);
    }

    [Fact]
    public void FromDimensionsAndHandle_AllNineCombos_CorrectCorners()
    {
        (HorizontalAlignment h, VerticalAlignment v, double l, double r, double b, double t)[] cases =
        [
            (HorizontalAlignment.Left, VerticalAlignment.Bottom, 0, 10, 0, 6),
            (HorizontalAlignment.Centre, VerticalAlignment.Bottom, -5, 5, 0, 6),
            (HorizontalAlignment.Right, VerticalAlignment.Bottom, -10, 0, 0, 6),
            (HorizontalAlignment.Left, VerticalAlignment.Centre, 0, 10, -3, 3),
            (HorizontalAlignment.Centre, VerticalAlignment.Centre, -5, 5, -3, 3),
            (HorizontalAlignment.Right, VerticalAlignment.Centre, -10, 0, -3, 3),
            (HorizontalAlignment.Left, VerticalAlignment.Top, 0, 10, -6, 0),
            (HorizontalAlignment.Centre, VerticalAlignment.Top, -5, 5, -6, 0),
            (HorizontalAlignment.Right, VerticalAlignment.Top, -10, 0, -6, 0),
        ];

        foreach (var (h, v, l, r, b, t) in cases)
        {
            Box box = Box.FromDimensionsAndHandle(10, 6, h, v, new Point(7, 9));

            TestHelpers.Near(new Point(l + 7, b + 9), box.BottomLeft);
            TestHelpers.Near(new Point(r + 7, t + 9), box.TopRight);
        }
    }

    [Fact]
    public void Transform_Identity_RoundTrips()
    {
        Box box = new([new Point(1, 2), new Point(5, 9)]);

        Box result = box.Transform(AffineTransform.Identity());

        TestHelpers.Near(box.BottomLeft, result.BottomLeft);
        TestHelpers.Near(box.TopRight, result.TopRight);
    }

    [Fact]
    public void Transform_Translation_ShiftsBothCorners()
    {
        Box box = new([new Point(1, 2), new Point(5, 9)]);

        Box result = box.Transform(AffineTransform.Identity().Translate(10, -3));

        TestHelpers.Near(new Point(11, -1), result.BottomLeft);
        TestHelpers.Near(new Point(15, 6), result.TopRight);
        TestHelpers.Near(4, result.Width);
        TestHelpers.Near(7, result.Height);
    }

    [Fact]
    public void Transform_PositiveScaleAroundOrigin_ScalesExtents()
    {
        Box box = new([new Point(1, 2), new Point(3, 4)]);

        Box result = box.Transform(AffineTransform.Identity().Scale(2, 3));

        TestHelpers.Near(new Point(2, 6), result.BottomLeft);
        TestHelpers.Near(new Point(6, 12), result.TopRight);
    }

    [Fact]
    public void KnownIssue_Transform_MapsOnlyTwoCorners()
    {
        // Box.Transform maps BottomLeft/TopRight only (no 4-corner rotation handling).
        // Rotation-aware consumers must use GraphicShape.BoundingBox() instead.
        Box box = new([new Point(0, 0), new Point(10, 4)]);

        Box result = box.Transform(AffineTransform.Identity().Rotate(90));

        // BL(0,0)->(0,0); TR(10,4)->(-4,10); Box of those two alone:
        TestHelpers.Near(new Point(-4, 0), result.BottomLeft, 1e-9);
        TestHelpers.Near(new Point(0, 10), result.TopRight, 1e-9);
    }

    [Fact]
    public void Transform_YFlip_SwapsVerticalOrder()
    {
        Box box = new([new Point(1, 2), new Point(5, 9)]);

        Box result = box.Transform(AffineTransform.Identity().Scale(1, -1));

        // BL(1,2)->(1,-2); TR(5,9)->(5,-9); re-normalized:
        TestHelpers.Near(new Point(1, -9), result.BottomLeft);
        TestHelpers.Near(new Point(5, -2), result.TopRight);
        TestHelpers.Near(4, result.Width);
        TestHelpers.Near(7, result.Height);
    }

    [Fact]
    public void Transform_ZeroSizeBox_StaysZeroSizeUnderTranslation()
    {
        Box box = new([new Point(5, 5)]);

        Box result = box.Transform(AffineTransform.Identity().Translate(3, 3));

        TestHelpers.Near(new Point(8, 8), result.BottomLeft);
        TestHelpers.Near(new Point(8, 8), result.TopRight);
        TestHelpers.Near(0, result.Width);
        TestHelpers.Near(0, result.Height);
    }

    [Fact]
    public void KnownIssue_CoordinatesBeyondSentinel_Clamp()
    {
        // Min/max seeds are +/-1e9, so the X minimum never registers (true min 2e9 pins at 1e9).
        // Pinned as actual; flag to owner.
        Box box = new([new Point(2e9, 0), new Point(3e9, 1)]);

        TestHelpers.Near(new Point(1e9, 0), box.BottomLeft);
        TestHelpers.Near(new Point(3e9, 1), box.TopRight);
    }

    [Fact]
    public void KnownIssue_NullList_ThrowsNullReference()
    {
        Assert.Throws<NullReferenceException>(() => new Box(null!));
    }

    [Fact]
    public void KnownIssue_FromBoxesNull_ThrowsNullReference()
    {
        Assert.Throws<NullReferenceException>(() => Box.FromBoxes(null!));
    }

    [Fact]
    public void ShouldBe_Transform_RotatesAllFourCorners()
    {
        // RED: a bounding box must contain the whole transformed shape. True 45deg AABB of
        // (0,0)-(10,4) about the origin spans x -2.83..7.07, not the two-corner 0..4.24.
        Box box = new([new Point(0, 0), new Point(10, 4)]);

        Box result = box.Transform(AffineTransform.Identity().Rotate(45));

        TestHelpers.Near(new Point(-2.82842712474619, 0), result.BottomLeft, 1e-9);
        TestHelpers.Near(new Point(7.0710678118654755, 9.899494936611665), result.TopRight, 1e-9);
    }

    [Fact]
    public void ShouldBe_CoordinatesBeyondSentinel_Work()
    {
        // RED: min/max must work for any finite coordinate, not just |v| <= 1e9.
        Box box = new([new Point(2e9, 0), new Point(3e9, 1)]);

        TestHelpers.Near(new Point(2e9, 0), box.BottomLeft);
        TestHelpers.Near(new Point(3e9, 1), box.TopRight);
    }

    [Fact]
    public void ShouldBe_NullList_ThrowsArgumentNull()
    {
        // RED: public API must throw ArgumentNullException, not NullReferenceException.
        Assert.Throws<ArgumentNullException>(() => new Box(null!));
    }

    [Fact]
    public void ShouldBe_FromBoxesNull_ThrowsArgumentNull()
    {
        // RED: public API must throw ArgumentNullException, not NullReferenceException.
        Assert.Throws<ArgumentNullException>(() => Box.FromBoxes(null!));
    }

    [Fact]
    public void FromDimensionsAndHandle_NegativeDimensions_NormalizeToAbsolute()
    {
        // (0,-10) re-normalizes: width becomes +10, handle flips to the right edge.
        Box box = Box.FromDimensionsAndHandle(-10, -6,
            HorizontalAlignment.Left, VerticalAlignment.Bottom, new Point(0, 0));

        TestHelpers.Near(new Point(-10, -6), box.BottomLeft);
        TestHelpers.Near(new Point(0, 0), box.TopRight);
        TestHelpers.Near(10, box.Width);
        TestHelpers.Near(6, box.Height);
    }

    [Fact]
    public void Transform_Rotate180_NegatesBothCorners()
    {
        Box box = new([new Point(1, 2), new Point(5, 9)]);

        Box result = box.Transform(AffineTransform.Identity().Rotate(180));

        TestHelpers.Near(new Point(-5, -9), result.BottomLeft, 1e-9);
        TestHelpers.Near(new Point(-1, -2), result.TopRight, 1e-9);
    }

    [Fact]
    public void Transform_Shear_MapsBothCorners()
    {
        // (x,y) -> (x+y, y): BL(0,0)->(0,0); TR(2,4)->(6,4).
        Box box = new([new Point(0, 0), new Point(2, 4)]);

        Box result = box.Transform(new AffineTransform(1, 1, 0, 0, 1, 0));

        TestHelpers.Near(new Point(0, 0), result.BottomLeft);
        TestHelpers.Near(new Point(6, 4), result.TopRight);
    }

    [Fact]
    public void Transform_ComposedTranslateRotate_MapsBothCorners()
    {
        // (1,0) -> translate(1,0) -> (2,0) -> rotate90 -> (0,2); origin -> (1,0) -> (0,1).
        Box box = new([new Point(0, 0), new Point(1, 0)]);

        Box result = box.Transform(AffineTransform.Identity().Translate(1, 0).Rotate(90));

        TestHelpers.Near(new Point(0, 1), result.BottomLeft, 1e-9);
        TestHelpers.Near(new Point(0, 2), result.TopRight, 1e-9);
    }
}
