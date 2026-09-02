using Cartesia.Core.Math;

namespace Cartesia.Core.Geometry
{
	public readonly struct Point(double xIn, double yIn)
	{
		internal Point Transform(AffineTransform transformIn)
		{
			double
				newX = (transformIn.M00 * X) + (transformIn.M01 * Y) + transformIn.M02,
				newY = (transformIn.M10 * X) + (transformIn.M11 * Y) + transformIn.M12;

			return new Point(newX, newY);
		}

		public readonly double
			X = xIn,
			Y = yIn;
	}
}
