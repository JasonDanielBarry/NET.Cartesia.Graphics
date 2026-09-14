

using Cartesia.Core.Geometry;

namespace Cartesia.Core.Curve
{
	public readonly struct BezierCurveProperties(Point P0In, Point P1In, Point P2In, Point P3In)
	{
		public readonly Point
			P0 = P0In,
			P1 = P1In,
			P2 = P2In,
			P3 = P3In;
	}
}
