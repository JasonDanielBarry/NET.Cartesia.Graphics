using Cartesia.Core.Geometry;
using Cartesia.Render.Entities.Base;
using Cartesia.Render.Entities.Geometry;
using Cartesia.Render.Rendering;
using SkiaSharp;
using System.Collections.Generic;

namespace Cartesia.WinUI.Test
{
	internal static class SceneBuild
	{
		private static IReadOnlyList<GraphicEntity> LineEntities()
		{
			GraphicLine
				line1 = new GraphicLine(
					new Point(50, 100),
					new Point(200, 250),
					new Pen(3, SKColors.Blue, [])
				),
				line2 = new GraphicLine(
					new Point(100, 100),
					new Point(250, 250),
					new Pen(5, SKColors.Red, [10, 10, 20, 20])
				);

			return [line1, line2];
		}

		private static IReadOnlyList<GraphicEntity> PolylineEntities()
		{
			GraphicPolyline
				polyline1 = new GraphicPolyline(
					[
						new Point(50, 200),
						new Point(200, 350),
						new Point(300, 250)
					],
					new Brush(SKColors.Transparent),
					new Pen(3, SKColors.Green, [])
				),
				polyline2 = new GraphicPolyline(
					[
						new Point(100, 200),
						new Point(250, 350),
						new Point(350, 250)
					],
					new Brush(SKColors.Transparent),
					new Pen(5, SKColors.Orange, [10, 10, 20, 20])
				);

			return [polyline1, polyline2];
		}

		private static IReadOnlyList<GraphicEntity> PolygonEntities()
		{
			GraphicPolygon
				polygon1 = new GraphicPolygon(
					[
						new Point(50, 300),
						new Point(200, 450),
						new Point(300, 350)
					],
					new Brush(SKColors.Yellow),
					new Pen(3, SKColors.Purple, [])
				),
				polygon2 = new GraphicPolygon(
					[
						new Point(100, 300),
						new Point(250, 450),
						new Point(350, 350)
					],
					new Brush(SKColors.Cyan),
					new Pen(5, SKColors.Magenta, [10, 10, 20, 20])
				);
			return [polygon1, polygon2];
		}

		internal static IReadOnlyList<GraphicEntity> BuildScene()
		{
			List<GraphicEntity> entities = new();

			entities.AddRange(
				LineEntities()
			);

			entities.AddRange(
				PolylineEntities()
			);

			entities.AddRange(
				PolygonEntities()
			);

			return entities.AsReadOnly();
		}
	}
}
