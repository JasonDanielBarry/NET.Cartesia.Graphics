using Cartesia.Core.Geometry;
using Cartesia.Core.Mapping;
using Cartesia.Core.Math;
using Cartesia.Core.Shapes;
using Cartesia.Render.Entities.Base;
using Cartesia.Render.Rendering;
using Cartesia.Render.Utilities;
using SkiaSharp;

namespace Cartesia.Render.Entities.Shape
{
	public sealed class GraphicArc(ArcProperties arcPropertiesIn, Point handlePointIn, Pen strokeIn) : GraphicEntity(Brush.None, strokeIn)
	{
		readonly float _sweepAngle = -(float)(arcPropertiesIn.EndAngle - arcPropertiesIn.StartAngle);
		private SKRect _arcRect;
		private SKPoint _canvasHandlePoint;
		private readonly Box _worldBoxToDraw = Box.FromDimensionsAndHandle(
			2 * arcPropertiesIn.RadiusX,
			2 * arcPropertiesIn.RadiusY,
			HorizontalAlignment.Centre,
			VerticalAlignment.Centre,
			handlePointIn
		);

		internal override Box BoundingBox()
		{
			AffineTransform transform = AffineTransform.Identity()
				.Translate(-handlePointIn.X, -handlePointIn.Y)
				.Rotate(arcPropertiesIn.Rotation * Math.PI / 180)
				.Translate(handlePointIn.X, handlePointIn.Y);

			return _worldBoxToDraw.Transform(transform);
		}

		internal override void Draw(SKCanvas canvasIn)
		{
			canvasIn.Translate(_canvasHandlePoint.X, _canvasHandlePoint.Y);

			canvasIn.RotateDegrees(
				-(float)arcPropertiesIn.Rotation
			);

			canvasIn.DrawArc(
				_arcRect,
				-(float)arcPropertiesIn.StartAngle,
				_sweepAngle,
				false,
				_strokePaint
			);

			canvasIn.ResetMatrix();
		}

		internal override void Precompute(WorldToCanvasMapper mapperIn)
		{
			Point
				canvasBottomLeft = mapperIn.MapWorldToCanvas(_worldBoxToDraw.BottomLeft),
				canvasTopRight = mapperIn.MapWorldToCanvas(_worldBoxToDraw.TopRight);

			_canvasHandlePoint = mapperIn.MapWorldToCanvas(handlePointIn).ToSKPoint();

			float
				left = (float)canvasBottomLeft.X - _canvasHandlePoint.X,
				right = (float)canvasTopRight.X - _canvasHandlePoint.X,
				bottom = (float)canvasBottomLeft.Y - _canvasHandlePoint.Y,
				top = (float)canvasTopRight.Y - _canvasHandlePoint.Y;

			_arcRect = new SKRect(left, top, right, bottom);
		}
	}
}
