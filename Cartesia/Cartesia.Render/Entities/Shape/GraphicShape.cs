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
	public abstract class GraphicShape : GraphicEntity
	{
		private readonly double _shapeRotation;
		private readonly Box _worldBoxToDraw;
		private SKPoint _canvasHandlePoint;
		private readonly Point _worldHandlePoint;
		private protected SKRect _shapeRect;

		private protected GraphicShape(double widthIn, double heightIn, double rotationIn,
										HorizontalAlignment horizontalAlignmentIn,
										VerticalAlignment verticalAlignmentIn,
										Point worldHandlePointIn,
										Brush fillIn, Pen strokeIn) : base(fillIn, strokeIn)
		{
			_shapeRotation = rotationIn;

			_worldHandlePoint = worldHandlePointIn;

			_worldBoxToDraw = Box.FromDimensionsAndHandle(
				widthIn, heightIn,
				horizontalAlignmentIn,
				verticalAlignmentIn,
				worldHandlePointIn
			);
		}

		private protected abstract void DrawShape(SKCanvas canvasIn);

		internal override Box BoundingBox()
		{
			AffineTransform transform = AffineTransform.Identity()
				.Translate(-_worldHandlePoint.X, -_worldHandlePoint.Y)
				.Rotate(_shapeRotation)
				.Translate(_worldHandlePoint.X, _worldHandlePoint.Y);

			return _worldBoxToDraw.Transform(transform);
		}

		internal override void Draw(SKCanvas canvasIn)
		{
			canvasIn.Translate(_canvasHandlePoint.X, _canvasHandlePoint.Y);

			canvasIn.RotateDegrees(
				-(float)_shapeRotation
			);

			DrawShape(canvasIn);

			canvasIn.ResetMatrix();
		}

		internal override void Precompute(WorldToCanvasMapper mapperIn)
		{
			Point
				canvasBottomLeft = mapperIn.MapWorldToCanvas(_worldBoxToDraw.BottomLeft),
				canvasTopRight = mapperIn.MapWorldToCanvas(_worldBoxToDraw.TopRight);

			_canvasHandlePoint = mapperIn.MapWorldToCanvas(_worldHandlePoint).ToSKPoint();

			float
				left = (float)canvasBottomLeft.X - _canvasHandlePoint.X,
				right = (float)canvasTopRight.X - _canvasHandlePoint.X,
				bottom = (float)canvasBottomLeft.Y - _canvasHandlePoint.Y,
				top = (float)canvasTopRight.Y - _canvasHandlePoint.Y;

			_shapeRect = new SKRect(left, top, right, bottom);
		}
	}
}
