using SkiaSharp;

namespace Cartesia.Render.Rendering
{
	public sealed class Pen
	{
		private readonly double _thickness;
		private readonly SKColor _colour;
		private readonly IReadOnlyList<double> _dashPattern;

		private Pen()
		{
			_thickness = 0;
			_colour = SKColors.Transparent;
			_dashPattern = [];

			IsVisible = false;
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

		private static IReadOnlyList<double> ValidateDashPattern(IReadOnlyList<double> dashPatternIn)
		{
			ArgumentNullException.ThrowIfNull(dashPatternIn, nameof(dashPatternIn));

			int arrLen = dashPatternIn.Count;

			bool arrLenIsEven = arrLen % 2 == 0;

			if (!arrLenIsEven)
				throw new ArgumentException(
					"Dash pattern must have an even number of elements.",
					nameof(dashPatternIn)
				);

			return dashPatternIn.ToArray().AsReadOnly();
		}

		internal readonly bool IsVisible;

		internal SKPaint ToSKPaint()
		{
			SKPathEffect pathEffect = null!;

			int arrLen = _dashPattern.Count;

			if (2 <= arrLen)
			{
				float[] dashPattern = new float[arrLen];

				for (int i = 0; i < arrLen; i++)
					dashPattern[i] = (float)_dashPattern[i];

				pathEffect = SKPathEffect.CreateDash(dashPattern, 0);
			}

			return new SKPaint
			{
				Color = _colour,
				Style = SKPaintStyle.Stroke,
				StrokeWidth = (float)_thickness,
				IsAntialias = true,
				PathEffect = pathEffect
			};
		}

		public double Thickness => _thickness;
		public SKColor Colour => _colour;
		public IReadOnlyList<double> DashPattern => _dashPattern;
		public static Pen None => new Pen();

		public Pen(double thicknessIn, SKColor colourIn, IReadOnlyList<double> dashPatternIn)
		{
			_thickness = ValidateThickness(thicknessIn);
			_colour = colourIn;
			_dashPattern = ValidateDashPattern(dashPatternIn);

			IsVisible = _colour != SKColors.Transparent && 0 < _thickness;
		}
	}
}
