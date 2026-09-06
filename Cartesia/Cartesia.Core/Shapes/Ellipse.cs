namespace Cartesia.Core.Shapes
{
	public readonly struct Ellipse(double widthIn, double heightIn, double rotationIn)
	{
		public readonly double
			Width = widthIn,
			Height = heightIn,
			Rotation = rotationIn;
	}
}
