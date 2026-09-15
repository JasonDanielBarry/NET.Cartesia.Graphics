using Cartesia.Core.Geometry;
using Cartesia.Core.Mapping;
using Cartesia.Render.Entities.Base;
using Cartesia.Render.Entities.Utilities;
using Cartesia.Render.Rendering;
using SkiaSharp;

namespace Cartesia.Render.Entities.Geometry
{
	public sealed class GraphicPolyline(IReadOnlyList<Point> verticesIn, Brush fillIn, Pen strokeIn) : GraphicEntity(fillIn, strokeIn)
	{
		private SKPath _path = null!;
		private readonly SKPathBuilder _pathBuilder = new SKPathBuilder();
		private readonly Point[] _vertices = verticesIn.ToArray();

		private protected override void DisposeResources()
		{
			base.DisposeResources();

			_pathBuilder.Dispose();
		}

		internal override Box BoundingBox()
		{
			return new Box(_vertices);
		}

		internal override void Draw(SKCanvas canvasIn)
		{
			canvasIn.DrawPath(_path, _strokePaint);

			_path.Dispose();
		}

		internal override void Precompute(WorldToCanvasMapper mapperIn)
		{
			_path = GeometryShared.BuildSKPath(false, _vertices, _pathBuilder, mapperIn);
		}
	}
}