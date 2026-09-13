using Cartesia.Core.Geometry;
using Cartesia.Render.Entities.Geometry;
using Cartesia.Render.Rendering;
using Cartesia.Tests.Helpers;
using SkiaSharp;

namespace Cartesia.Tests.Render;

public sealed class BrushTests
{
    [Fact]
    public void None_IsInvisibleAndTransparent()
    {
        Assert.False(Brush.None.IsVisible);

        using SKPaint paint = Brush.None.ToSKPaint();

        Assert.Equal(SKColors.Transparent, paint.Color);
    }

    [Fact]
    public void VisibleColour_IsVisible()
    {
        Brush brush = new(SKColors.Yellow);

        Assert.True(brush.IsVisible);
    }

    [Fact]
    public void TransparentColour_IsInvisible()
    {
        Brush brush = new(SKColors.Transparent);

        Assert.False(brush.IsVisible);
    }

    [Theory]
    [InlineData(255, 0, 0)]
    [InlineData(0, 255, 0)]
    [InlineData(0, 0, 255)]
    [InlineData(1, 2, 3)]
    public void OpaqueColours_AreVisible(byte r, byte g, byte b)
    {
        Brush brush = new(new SKColor(r, g, b));

        Assert.True(brush.IsVisible);
    }

    [Fact]
    public void SemiTransparentColour_IsVisible()
    {
        Brush brush = new(new SKColor(255, 0, 0, 1));

        Assert.True(brush.IsVisible);
    }

    [Fact]
    public void ToSKPaint_SetsFillStyleAntialiasAndColour()
    {
        Brush brush = new(SKColors.Cyan);

        using SKPaint paint = brush.ToSKPaint();

        Assert.Equal(SKPaintStyle.Fill, paint.Style);
        Assert.True(paint.IsAntialias);
        Assert.Equal(SKColors.Cyan, paint.Color);
    }

    [Fact]
    public void ToSKPaint_EachCallReturnsFreshPaint()
    {
        Brush brush = new(SKColors.Magenta);

        using SKPaint a = brush.ToSKPaint();
        using SKPaint b = brush.ToSKPaint();

        Assert.NotSame(a, b);
        Assert.Equal(a.Color, b.Color);
    }

    [Fact]
    public void AlphaZeroNonBlack_IsVisibleYetFullyTransparent()
    {
        // != Transparent(0,0,0,0) by RGB, so IsVisible is true despite alpha 0.
        Brush brush = new(new SKColor(255, 0, 0, 0));

        Assert.True(brush.IsVisible);
    }

    [Fact]
    public void AlphaZeroFill_DrawsNothingVisible()
    {
        var mapper = TestHelpers.SquareMapper(400);
        var polygon = new GraphicPolygon(
            [new Point(100, 100), new Point(300, 100), new Point(200, 300)],
            new Brush(new SKColor(255, 255, 0, 0)), Pen.None);

        using SKSurface surface = TestHelpers.CreateSurface();
        polygon.Precompute(mapper);
        polygon.Draw(surface.Canvas);

        TestHelpers.AssertWhite(
            TestHelpers.SampleWorld(surface, mapper, new Point(200, 167)));
    }

    [Fact]
    public void None_ToSKPaint_IsFillAntialiased()
    {
        using SKPaint paint = Brush.None.ToSKPaint();

        Assert.Equal(SKPaintStyle.Fill, paint.Style);
        Assert.True(paint.IsAntialias);
    }
}
