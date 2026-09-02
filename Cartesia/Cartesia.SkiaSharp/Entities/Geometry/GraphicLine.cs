using Cartesia.Core.Geometry;
using Cartesia.SkiaSharp.Entities.Base;
using Cartesia.SkiaSharp.Rendering;

namespace Cartesia.SkiaSharp.Entities.Geometry
{
	public sealed class GraphicLine(Point startPointIn, Point endPointIn, Pen strokeIn) : GraphicEntity(Brush.None, strokeIn)
	{
		public Point StartPoint => startPointIn;
		public Point EndPoint => endPointIn;
	}
}
