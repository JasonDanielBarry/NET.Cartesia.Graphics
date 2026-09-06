using Cartesia.Core.Geometry;
using SkiaSharp;

namespace Cartesia.Render.Utilities
{
	internal static class PointHelper
	{
		internal static SKPoint ToSKPoint(this Point point)
		{
			return new SKPoint(
				(float)point.X,
				(float)point.Y
			);
		}
	}
}
