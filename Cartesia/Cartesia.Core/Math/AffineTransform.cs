namespace Cartesia.Core.Math
{
	internal sealed class AffineTransform
	{
		private AffineTransform(double m00In, double m01In, double m02In,
								double m10In, double m11In, double m12In)
		{
			M00 = m00In;
			M01 = m01In;
			M02 = m02In;
			M10 = m10In;
			M11 = m11In;
			M12 = m12In;
		}

		internal readonly double
			M00, M01, M02,
			M10, M11, M12;

		internal static AffineTransform Create(double m00In, double m01In, double m02In,
												double m10In, double m11In, double m12In)
		{
			return new AffineTransform(
				m00In, m01In, m02In,
				m10In, m11In, m12In
			);
		}

		internal static AffineTransform Create(double[] transformMatrixValuesIn)
		{
			if (transformMatrixValuesIn.Length != 6)
			{
				throw new ArgumentException("The transform matrix values array must contain exactly 6 elements.");
			}

			return new AffineTransform(
				transformMatrixValuesIn[0], transformMatrixValuesIn[1], transformMatrixValuesIn[2],
				transformMatrixValuesIn[3], transformMatrixValuesIn[4], transformMatrixValuesIn[5]
			);
		}
	}
}
