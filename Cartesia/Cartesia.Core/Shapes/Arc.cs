namespace Cartesia.Core.Shapes
{
	public readonly struct Arc(double radiusXIn, double radiusYIn, double startAngleIn, double endAngleIn, double rotationIn)
	{
		public readonly double
			RadiusX = radiusXIn,
			RadiusY = radiusYIn,
			StartAngle = startAngleIn,
			EndAngle = endAngleIn,
			Rotation = rotationIn;
	}
}
