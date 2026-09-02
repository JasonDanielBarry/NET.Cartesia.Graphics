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

		public readonly double
			Width,
			Height;

		public readonly Point
			BottomLeft,
			TopRight;

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
	}
}
