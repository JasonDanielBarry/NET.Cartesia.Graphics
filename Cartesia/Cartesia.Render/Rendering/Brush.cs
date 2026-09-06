using SkiaSharp;

namespace Cartesia.Render.Rendering
{
	public sealed class Brush(SKColor colourIn)
	{
		internal readonly bool IsVisible = colourIn != SKColors.Transparent;

		internal SKPaint ToSKPaint()
		{
			return new SKPaint
			{
				Color = colourIn,
				Style = SKPaintStyle.Fill,
				IsAntialias = true
			};
		}

		public SKColor Colour => colourIn;

		public static Brush None => new Brush(SKColors.Transparent);
	}
}
