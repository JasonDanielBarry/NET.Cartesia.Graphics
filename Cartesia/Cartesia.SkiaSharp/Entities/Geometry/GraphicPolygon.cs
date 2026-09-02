using Cartesia.Core.Geometry;
using Cartesia.SkiaSharp.Entities.Base;
using Cartesia.SkiaSharp.Rendering;

namespace Cartesia.SkiaSharp.Entities.Geometry
{
	public sealed class GraphicPolygon(IReadOnlyList<Point> verticesIn, Brush fillIn, Pen strokeIn) : GraphicEntity(fillIn, strokeIn)
	{
		private readonly Point[] _vertices = verticesIn.ToArray();

		public Point[] Vertices => _vertices;
	}
}
