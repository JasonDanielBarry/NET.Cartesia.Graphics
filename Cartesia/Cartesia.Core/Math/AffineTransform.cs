using static System.Math;

namespace Cartesia.Core.Math
{
	public sealed class AffineTransform
	{
		private static AffineTransform Multiply(AffineTransform leftIn, AffineTransform rightIn)
		{
			return new AffineTransform(
				(leftIn.M00 * rightIn.M00) + (leftIn.M01 * rightIn.M10),
				(leftIn.M00 * rightIn.M01) + (leftIn.M01 * rightIn.M11),
				(leftIn.M00 * rightIn.M02) + (leftIn.M01 * rightIn.M12) + leftIn.M02,

				(leftIn.M10 * rightIn.M00) + (leftIn.M11 * rightIn.M10),
				(leftIn.M10 * rightIn.M01) + (leftIn.M11 * rightIn.M11),
				(leftIn.M10 * rightIn.M02) + (leftIn.M11 * rightIn.M12) + leftIn.M12
			);
		}

		internal readonly double
			M00, M01, M02,
			M10, M11, M12;

		internal AffineTransform(double m00In, double m01In, double m02In,
								double m10In, double m11In, double m12In)
		{
			M00 = m00In;
			M01 = m01In;
			M02 = m02In;
			M10 = m10In;
			M11 = m11In;
			M12 = m12In;
		}

		public static AffineTransform Identity()
		{
			return new AffineTransform(
				1, 0, 0,
				0, 1, 0
			);
		}

		public AffineTransform Scale(double scaleXIn, double scaleYIn)
		{
			AffineTransform scaleTransform = new AffineTransform(
				scaleXIn, 0, 0,
				0, scaleYIn, 0
			);

			return Multiply(scaleTransform, this);
		}

		public AffineTransform Rotate(double angleIn)
		{
			AffineTransform rotationTransform = new AffineTransform(
				Cos(angleIn), -Sin(angleIn), 0,
				Sin(angleIn), Cos(angleIn), 0
			);

			return Multiply(rotationTransform, this);
		}

		public AffineTransform Translate(double dXIn, double dYIn)
		{
			AffineTransform translationTransform = new AffineTransform(
				1, 0, dXIn,
				0, 1, dYIn
			);

			return Multiply(translationTransform, this);
		}
	}
}
