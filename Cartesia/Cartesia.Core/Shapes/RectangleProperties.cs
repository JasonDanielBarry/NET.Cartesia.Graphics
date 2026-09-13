using System.Drawing;

namespace Cartesia.Core.Shapes
{
	public readonly struct RectangleProperties(double widthIn, double heightIn,
												double cornerRadiusXIn, double cornerRadiusYIn,
												double rotationIn,
												HorizontalAlignment horizontalAlignmentIn,
												VerticalAlignment verticalAlignmentIn,
												Point handlePointIn)
	{
		public readonly double
			Width = widthIn,
			Height = heightIn,
			CornerRadiusX = cornerRadiusXIn,
			CornerRadiusY = cornerRadiusYIn,
			Rotation = rotationIn;

		public readonly HorizontalAlignment HorizontalAlignmentIn = horizontalAlignmentIn;

		public readonly VerticalAlignment VerticalAlignment = verticalAlignmentIn;

		public readonly Point HandlePoint = handlePointIn;
	}
}
