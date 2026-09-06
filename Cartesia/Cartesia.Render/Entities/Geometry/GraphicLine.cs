using Cartesia.Core.Geometry;
using Cartesia.Core.Mapping;
using Cartesia.Render.Entities.Base;
using Cartesia.Render.Rendering;
using Cartesia.Render.Utilities;
using SkiaSharp;

namespace Cartesia.Render.Entities.Geometry
{
	public sealed class GraphicLine(Point startPointIn, Point endPointIn, Pen strokeIn) : GraphicEntity(Brush.None, strokeIn)
	{
		private SKPoint _start, _end;

		internal override Box BoundingBox()
		{
			return new Box([
				StartPoint,
				EndPoint
			]);
		}

		internal override void Draw(SKCanvas canvasIn)
		{
			canvasIn.DrawLine(_start, _end, _strokePaint);
		}

		internal override void Precompute(WorldToCanvasMapper mapperIn)
		{
			Point
				startPointCanvas = mapperIn.MapWorldToCanvas(startPointIn),
				endPointCanvas = mapperIn.MapWorldToCanvas(endPointIn);

			_start = startPointCanvas.ToSKPoint();
			_end = endPointCanvas.ToSKPoint();
		}

		public Point StartPoint => startPointIn;
		public Point EndPoint => endPointIn;
	}
}
