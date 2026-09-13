using Cartesia.Core.Geometry;

namespace Cartesia.Core.Shapes
{
	public readonly struct EllipseProperties(double widthIn, double heightIn,
											double rotationIn,
											HorizontalAlignment horizontalAlignmentIn,
											VerticalAlignment verticalAlignmentIn,
											Point handlePointIn)
	{
		public readonly double
			Width = widthIn,
			Height = heightIn,
			Rotation = rotationIn;

		public readonly HorizontalAlignment HorizontalAlignment = horizontalAlignmentIn;

		public readonly VerticalAlignment VerticalAlignment = verticalAlignmentIn;

		public readonly Point HandlePoint = handlePointIn;
	}
}
