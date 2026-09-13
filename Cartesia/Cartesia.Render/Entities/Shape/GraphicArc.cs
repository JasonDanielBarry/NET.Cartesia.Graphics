using Cartesia.Core.Shapes;
using Cartesia.Render.Rendering;
using SkiaSharp;

namespace Cartesia.Render.Entities.Shape
{
	public sealed class GraphicArc(ArcProperties arcPropertiesIn, Pen strokeIn)
		: GraphicShape(
			2 * arcPropertiesIn.RadiusX, 2 * arcPropertiesIn.RadiusY,
			arcPropertiesIn.Rotation,
			HorizontalAlignment.Centre, VerticalAlignment.Centre,
			arcPropertiesIn.HandlePoint,
			Brush.None, strokeIn
		)
	{
		private readonly float _sweepAngle = -(float)(arcPropertiesIn.EndAngle - arcPropertiesIn.StartAngle);

		private protected override void DrawShape(SKCanvas canvasIn)
		{
			canvasIn.DrawArc(
				_shapeRect,
				-(float)arcPropertiesIn.StartAngle,
				_sweepAngle,
				false,
				_strokePaint
			);
		}
	}
}
