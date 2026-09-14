using Cartesia.Core.Curve;
using Cartesia.Core.Geometry;
using Cartesia.Core.Mapping;
using Cartesia.Render.Entities.Base;
using Cartesia.Render.Rendering;
using Cartesia.Render.Utilities;
using SkiaSharp;

namespace Cartesia.Render.Entities.Curve
{
	public sealed class GraphicBezierCurve(BezierCurveProperties bezierCurvePropertiesIn, Pen strokeIn)
		: GraphicEntity(Brush.None, strokeIn)
	{
		private SKPoint _canvasP0, _canvasP1, _canvasP2, _canvasP3;
		private SKPath _curvePath = null!;
		private readonly SKPathBuilder _pathBuilder = new SKPathBuilder();

		internal override Box BoundingBox()
		{
			return new Box([
				bezierCurvePropertiesIn.P0,
				bezierCurvePropertiesIn.P1,
				bezierCurvePropertiesIn.P2,
				bezierCurvePropertiesIn.P3,
			]);
		}

		internal override void Draw(SKCanvas canvasIn)
		{
			canvasIn.DrawPath(_curvePath, _strokePaint);
		}

		internal override void Precompute(WorldToCanvasMapper mapperIn)
		{
			_canvasP0 = mapperIn.MapWorldToCanvas(bezierCurvePropertiesIn.P0).ToSKPoint();
			_canvasP1 = mapperIn.MapWorldToCanvas(bezierCurvePropertiesIn.P1).ToSKPoint();
			_canvasP2 = mapperIn.MapWorldToCanvas(bezierCurvePropertiesIn.P2).ToSKPoint();
			_canvasP3 = mapperIn.MapWorldToCanvas(bezierCurvePropertiesIn.P3).ToSKPoint();

			_pathBuilder.Reset();
			_pathBuilder.MoveTo(_canvasP0);
			_pathBuilder.CubicTo(_canvasP1, _canvasP2, _canvasP3);

			_curvePath = _pathBuilder.Detach();
		}
	}
}
