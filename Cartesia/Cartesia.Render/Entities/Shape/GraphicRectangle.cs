using Cartesia.Core.Mapping;
using Cartesia.Core.Shapes;
using Cartesia.Render.Rendering;
using SkiaSharp;

namespace Cartesia.Render.Entities.Shape
{
	public sealed class GraphicRectangle(RectangleProperties rectanglePropertiesIn, Brush fillIn, Pen strokeIn)
		: GraphicShape(
			rectanglePropertiesIn.Width, rectanglePropertiesIn.Height,
			rectanglePropertiesIn.Rotation,
			rectanglePropertiesIn.HorizontalAlignment,
			rectanglePropertiesIn.VerticalAlignment,
			rectanglePropertiesIn.HandlePoint,
			fillIn, strokeIn
		)
	{
		private readonly SKRoundRect _roundRect = new SKRoundRect();

		private protected override void DrawShape(SKCanvas canvasIn)
		{
			if (_fillBrush.IsVisible)
				canvasIn.DrawRoundRect(_roundRect, _fillPaint);

			if (_strokePen.IsVisible)
				canvasIn.DrawRoundRect(_roundRect, _strokePaint);
		}

		internal override void Precompute(WorldToCanvasMapper mapperIn)
		{
			base.Precompute(mapperIn);

			float
				canvasRadiusX = (float)mapperIn.WorldDXToCanvasDL(rectanglePropertiesIn.CornerRadiusX),
				canvasRadiusY = (float)Math.Abs(mapperIn.WorldDYToCanvasDT(rectanglePropertiesIn.CornerRadiusY));

			_roundRect.SetRect(
				_shapeRect,
				canvasRadiusX,
				canvasRadiusY
			);
		}
	}
}
