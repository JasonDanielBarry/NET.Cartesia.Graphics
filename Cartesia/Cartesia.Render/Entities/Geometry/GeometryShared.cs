using Cartesia.Core.Geometry;
using Cartesia.Core.Mapping;
using Cartesia.Render.Utilities;
using SkiaSharp;

namespace Cartesia.Render.Entities.Geometry
{
	internal static class GeometryShared
	{
		internal static SKPath BuildSKPath(bool closeIn, Point[] verticesIn, SKPathBuilder pathBuilderIn, WorldToCanvasMapper mapperIn)
		{
			pathBuilderIn.Reset();

			SKPoint start = mapperIn.MapWorldToCanvas(verticesIn[0]).ToSKPoint();

			pathBuilderIn.MoveTo(start);

			int arrLen = verticesIn.Length;

			for (int i = 1; i < arrLen; i++)
			{
				SKPoint next = mapperIn.MapWorldToCanvas(verticesIn[i]).ToSKPoint();
				pathBuilderIn.LineTo(next);
			}

			if (closeIn)
			{
				pathBuilderIn.Close();
			}

			return pathBuilderIn.Detach();
		}
	}
}
