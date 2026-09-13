

using Cartesia.Core.Geometry;

namespace Cartesia.Core.Shapes
{
	public readonly struct ArcProperties(double radiusXIn, double radiusYIn,
										double startAngleIn, double endAngleIn,
										double rotationIn,
										Point handlePointIn)
	{
		public readonly double
			RadiusX = radiusXIn,
			RadiusY = radiusYIn,
			StartAngle = startAngleIn,
			EndAngle = endAngleIn,
			Rotation = rotationIn;

		public readonly Point HandlePoint = handlePointIn;
	}
}
