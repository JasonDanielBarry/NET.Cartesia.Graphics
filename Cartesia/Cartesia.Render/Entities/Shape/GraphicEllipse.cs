using Cartesia.Core.Shapes;
using Cartesia.Render.Rendering;
using SkiaSharp;

namespace Cartesia.Render.Entities.Shape
{
	public sealed class GraphicEllipse(EllipseProperties ellipsePropertiesIn, Brush fillIn, Pen strokeIn)
		: GraphicShape(
			ellipsePropertiesIn.Width, ellipsePropertiesIn.Height,
			ellipsePropertiesIn.Rotation,
			ellipsePropertiesIn.HorizontalAlignment,
			ellipsePropertiesIn.VerticalAlignment,
			ellipsePropertiesIn.HandlePoint,
			fillIn, strokeIn
		)
	{
		private protected override void DrawShape(SKCanvas canvasIn)
		{
			if (_fillBrush.IsVisible)
				canvasIn.DrawOval(
					_shapeRect,
					_fillPaint
				);

			if (_strokePen.IsVisible)
				canvasIn.DrawOval(
					_shapeRect,
					_strokePaint
				);
		}
	}
}
