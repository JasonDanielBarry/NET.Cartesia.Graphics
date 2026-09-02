using SkiaSharp;

namespace Cartesia.SkiaSharp.Rendering
{
	public sealed class Brush(SKColor colourIn)
	{
		public SKColor Colour => colourIn;

		public static Brush None => new Brush(SKColors.Transparent);
	}
}
