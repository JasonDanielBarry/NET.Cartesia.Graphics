namespace Cartesia.Core.Shapes
{
	public readonly struct EllipseProperties(double widthIn, double heightIn, double rotationIn)
	{
		public readonly double
			Width = widthIn,
			Height = heightIn,
			Rotation = rotationIn;
	}
}
