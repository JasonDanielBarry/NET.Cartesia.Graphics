using SkiaSharp;

namespace Cartesia.SkiaSharp.Rendering
{
	public sealed class Pen
	{
		private readonly double _thickness;
		private readonly SKColor _colour;
		private readonly double[] _dashPattern;

		private Pen()
		{
			_thickness = 0;
			_colour = SKColors.Transparent;
			_dashPattern = [];
		}

		private static double ValidateThickness(double thicknessIn)
		{
			if (thicknessIn <= 0)
				throw new ArgumentOutOfRangeException(
					nameof(thicknessIn),
					"Thickness must be greater than zero."
				);

			return thicknessIn;
		}

		private static double[] ValidateDashPattern(double[] dashPatternIn)
		{
			int arrLen = dashPatternIn.Length;

			bool arrLenIsEven = arrLen % 2 == 0;

			if (!arrLenIsEven)
				throw new ArgumentException(
					"Dash pattern must have an even number of elements.",
					nameof(dashPatternIn)
				);

			return dashPatternIn;
		}

		public double Thickness => _thickness;
		public SKColor Colour => _colour;
		public double[] DashPattern => _dashPattern;
		public static Pen None => new Pen();

		public Pen(double thicknessIn, SKColor colourIn, double[] dashPatternIn)
		{
			_thickness = ValidateThickness(thicknessIn);
			_colour = colourIn;
			_dashPattern = ValidateDashPattern(dashPatternIn);
		}

	}
}
