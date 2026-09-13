namespace Cartesia.Core.Shapes
{
	public enum HorizontalAlignment
	{
		Left, Centre, Right
	}

	public enum VerticalAlignment
	{
		Bottom, Centre, Top
	}

	internal static class Alignment
	{
		private static (double left, double right) HorizontalBounds(double widthIn, HorizontalAlignment horizontalAlignmentIn)
		{
			switch (horizontalAlignmentIn)
			{
				case HorizontalAlignment.Left:
					return (0, widthIn);

				case HorizontalAlignment.Centre:
				{
					double halfWidth = widthIn / 2.0;

					return (-halfWidth, halfWidth);
				}

				case HorizontalAlignment.Right:
					return (-widthIn, 0);

				default:
					throw new ArgumentException("Invalid horizontal alignment", nameof(horizontalAlignmentIn));
			}
		}

		private static (double bottom, double top) VerticalBounds(double heightIn, VerticalAlignment verticalAlignmentIn)
		{
			switch (verticalAlignmentIn)
			{
				case VerticalAlignment.Bottom:
					return (0, heightIn);

				case VerticalAlignment.Centre:
				{
					double halfHeight = heightIn / 2.0;

					return (-halfHeight, halfHeight);
				}

				case VerticalAlignment.Top:
					return (-heightIn, 0);

				default:
					throw new ArgumentException("Invalid vertical alignment", nameof(verticalAlignmentIn));
			}
		}

		internal static (double left, double right, double bottom, double top) HorizontalAndVerticalBounds(double widthIn, double heightIn,
																										HorizontalAlignment horizontalAlignmentIn,
																										VerticalAlignment verticalAlignmentIn)
		{
			(double left, double right) = HorizontalBounds(widthIn, horizontalAlignmentIn);
			(double bottom, double top) = VerticalBounds(heightIn, verticalAlignmentIn);

			return (left, right, bottom, top);
		}
	}
}
