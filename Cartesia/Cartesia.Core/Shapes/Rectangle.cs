namespace Cartesia.Core.Shapes
{
	public readonly struct Rectangle(double widthIn, double heightIn, double cornerRadiusXIn, double cornerRadiusYIn, double rotationIn)
	{
		public readonly double
			Width = widthIn,
			Height = heightIn,
			CornerRadiusX = cornerRadiusXIn,
			CornerRadiusY = cornerRadiusYIn,
			Rotation = rotationIn;
	}
}
