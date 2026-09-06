using Cartesia.Core.Geometry;
using Cartesia.Core.Math;

namespace Cartesia.Core.Mapping
{
	public sealed class WorldToCanvasMapper(int canvasWidthIn, int canvasHeightIn, Box worldViewPortIn)
	{
		private readonly AffineTransform
			_canvasToWorldTransform = BuildCanvasToWorldTransform(canvasWidthIn, canvasHeightIn, worldViewPortIn),
			_worldToCanvasTransform = BuildWorldToCanvasTransform(canvasWidthIn, canvasHeightIn, worldViewPortIn);

		private static AffineTransform BuildCanvasToWorldTransform(int canvasWidthIn, int canvasHeightIn, Box worldViewPortIn)
		{
			return AffineTransform.Create(
				worldViewPortIn.Width / canvasWidthIn, 0, worldViewPortIn.BottomLeft.X,
				0, -worldViewPortIn.Height / canvasHeightIn, worldViewPortIn.TopRight.Y
			);
		}

		private static AffineTransform BuildWorldToCanvasTransform(int canvasWidthIn, int canvasHeightIn, Box worldViewPortIn)
		{
			double
				widthRatio = canvasWidthIn / worldViewPortIn.Width,
				heightRatio = canvasHeightIn / worldViewPortIn.Height;

			return AffineTransform.Create(
				widthRatio, 0, -widthRatio * worldViewPortIn.BottomLeft.X,
				0, -heightRatio, heightRatio * worldViewPortIn.TopRight.Y
			);
		}

		public Point MapWorldToCanvas(Point worldPointIn)
		{
			return worldPointIn.Transform(_worldToCanvasTransform);
		}

		public Point[] MapWorldToCanvas(Point[] arrWorldPointsIn)
		{
			int arrLen = arrWorldPointsIn.Length;

			Point[] arrCanvasPoints = new Point[arrLen];

			for (int i = 0; i < arrLen; i++)
			{
				arrCanvasPoints[i] = arrWorldPointsIn[i].Transform(_worldToCanvasTransform);
			}

			return arrCanvasPoints;
		}

		public Point MapCanvasToWorld(Point canvasPointIn)
		{
			return canvasPointIn.Transform(_canvasToWorldTransform);
		}

		public Point[] MapCanvasToWorld(Point[] arrCanvasPointsIn)
		{
			int arrLen = arrCanvasPointsIn.Length;

			Point[] arrWorldPoints = new Point[arrLen];

			for (int i = 0; i < arrLen; i++)
			{
				arrWorldPoints[i] = arrCanvasPointsIn[i].Transform(_canvasToWorldTransform);
			}

			return arrWorldPoints;
		}
	}
}
