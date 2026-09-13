using Cartesia.Core.Math;
using Cartesia.Core.Shapes;

namespace Cartesia.Core.Geometry
{
	public readonly struct Box
	{
		private static Point CalculateBottomLeft(IReadOnlyList<Point> points)
		{
			double
				xMin = 1e9,
				yMin = 1e9;

			foreach (Point point in points)
			{
				if (point.X < xMin)
					xMin = point.X;

				if (point.Y < yMin)
					yMin = point.Y;
			}

			return new Point(xMin, yMin);
		}

		private static Point CalculateTopRight(IReadOnlyList<Point> points)
		{
			double
				xMax = -1e9,
				yMax = -1e9;

			foreach (Point point in points)
			{
				if (xMax < point.X)
					xMax = point.X;

				if (yMax < point.Y)
					yMax = point.Y;
			}

			return new Point(xMax, yMax);
		}

		public readonly double Width, Height;

		public readonly Point BottomLeft, TopRight;

		public Box(IReadOnlyList<Point> arrPointsIn)
		{
			BottomLeft = CalculateBottomLeft(arrPointsIn);
			TopRight = CalculateTopRight(arrPointsIn);

			Width = TopRight.X - BottomLeft.X;
			Height = TopRight.Y - BottomLeft.Y;
		}

		public static Box FromBoxes(IReadOnlyList<Box> boxesIn)
		{
			List<Point> points = new List<Point>();

			foreach (Box box in boxesIn)
			{
				points.Add(box.BottomLeft);
				points.Add(box.TopRight);
			}

			return new Box(points);
		}

		public static Box FromDimensionsAndHandle(double widthIn, double heightIn,
												HorizontalAlignment horizontalAlignmentIn,
												VerticalAlignment verticalAlignmentIn,
												Point HandlePointIn)
		{
			(
				double left, double right, double bottom, double top
			) = Alignment.HorizontalAndVerticalBounds(
				widthIn, heightIn,
				horizontalAlignmentIn,
				verticalAlignmentIn
			);

			double
				dX = HandlePointIn.X,
				dY = HandlePointIn.Y;

			Point
				bottomLeft = new Point(left + dX, bottom + dY),
				topRight = new Point(right + dX, top + dY);

			return new Box([bottomLeft, topRight]);
		}

		public Box Transform(AffineTransform transformIn)
		{
			Point
				newBottomLeft = BottomLeft.Transform(transformIn),
				newTopRight = TopRight.Transform(transformIn);

			return new Box([newBottomLeft, newTopRight]);
		}
	}
}
