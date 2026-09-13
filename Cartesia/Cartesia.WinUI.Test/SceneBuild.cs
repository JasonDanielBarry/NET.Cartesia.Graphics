using Cartesia.Core.Geometry;
using Cartesia.Core.Shapes;
using Cartesia.Render.Entities.Base;
using Cartesia.Render.Entities.Geometry;
using Cartesia.Render.Entities.Shape;
using Cartesia.Render.Rendering;
using SkiaSharp;
using System.Collections.Generic;

namespace Cartesia.WinUI.Test
{
	internal static class SceneBuild
	{
		private static IReadOnlyList<GraphicEntity> ArcEntities()
		{
			ArcProperties
				arcProp1 = new ArcProperties(150, 100, -90, 90, 0, new Point(400, 600)),
				arcProp2 = new ArcProperties(125, 75, -90, 90, -90, new Point(400, 600));

			GraphicArc
				graphicArc1 = new GraphicArc(
					arcProp1,
					new Pen(3, SKColors.Red, [])
				),
				graphicArc2 = new GraphicArc(
					arcProp2,
					new Pen(3, SKColors.Blue, [])
				);

			GraphicLine
				line1 = new GraphicLine(
					new Point(400 - 10, 600),
					new Point(400 + 10, 600),
					new Pen(2, SKColors.White, [])
				),
				line2 = new GraphicLine(
					new Point(400, 600 - 10),
					new Point(400, 600 + 10),
					new Pen(2, SKColors.White, [])
				);

			return [line1, line2, graphicArc1, graphicArc2];
		}

		private static IReadOnlyList<GraphicEntity> EllipseEntities()
		{
			Point handlePoint = new Point(600, 500);

			EllipseProperties
				ellipseProp1 = new EllipseProperties(
					150, 75,
					15,
					HorizontalAlignment.Left,
					VerticalAlignment.Centre,
					handlePoint
				),
				ellipseProp2 = new EllipseProperties(
					150, 75,
					15,
					HorizontalAlignment.Right,
					VerticalAlignment.Centre,
					handlePoint
				),
				ellipseProp3 = new EllipseProperties(
					75, 150,
					15,
					HorizontalAlignment.Centre,
					VerticalAlignment.Top,
					handlePoint
				),
				ellipseProp4 = new EllipseProperties(
					75, 150,
					15,
					HorizontalAlignment.Centre,
					VerticalAlignment.Bottom,
					handlePoint
				);

			GraphicEllipse
				graphicEllipse1 = new GraphicEllipse(
					ellipseProp1,
					new Brush(SKColors.Green),
					new Pen(5, SKColors.LightGray, [5, 5])
				),
				graphicEllipse2 = new GraphicEllipse(
					ellipseProp2,
					new Brush(SKColors.Green),
					new Pen(5, SKColors.LightGray, [5, 5])
				),
				graphicEllipse3 = new GraphicEllipse(
					ellipseProp3,
					new Brush(SKColors.Green),
					new Pen(5, SKColors.LightGray, [5, 5])
				),
				graphicEllipse4 = new GraphicEllipse(
					ellipseProp4,
					new Brush(SKColors.Green),
					new Pen(5, SKColors.LightGray, [5, 5])
				);

			return [graphicEllipse1, graphicEllipse2, graphicEllipse3, graphicEllipse4];
		}

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

		private static IReadOnlyList<GraphicEntity> RectangleEntities()
		{
			Point handlePoint = new Point(600, 200);

			RectangleProperties
				ellipseProp1 = new RectangleProperties(
					150, 75,
					15, 15,
					30,
					HorizontalAlignment.Left,
					VerticalAlignment.Centre,
					handlePoint
				),
				ellipseProp2 = new RectangleProperties(
					150, 75,
					15, 15,
					30,
					HorizontalAlignment.Right,
					VerticalAlignment.Centre,
					handlePoint
				),
				ellipseProp3 = new RectangleProperties(
					75, 150,
					15, 15,
					30,
					HorizontalAlignment.Centre,
					VerticalAlignment.Top,
					handlePoint
				),
				ellipseProp4 = new RectangleProperties(
					75, 150,
					15, 15,
					30,
					HorizontalAlignment.Centre,
					VerticalAlignment.Bottom,
					handlePoint
				);

			GraphicRectangle
				graphicRectangle1 = new GraphicRectangle(
					ellipseProp1,
					new Brush(SKColors.Blue),
					new Pen(5, SKColors.OrangeRed, [])
				),
				graphicRectangle2 = new GraphicRectangle(
					ellipseProp2,
					new Brush(SKColors.Blue),
					new Pen(5, SKColors.OrangeRed, [])
				),
				graphicRectangle3 = new GraphicRectangle(
					ellipseProp3,
					new Brush(SKColors.Blue),
					new Pen(5, SKColors.OrangeRed, [])
				),
				graphicRectangle4 = new GraphicRectangle(
					ellipseProp4,
					new Brush(SKColors.Blue),
					new Pen(5, SKColors.OrangeRed, [])
				);

			return [graphicRectangle1, graphicRectangle2, graphicRectangle3, graphicRectangle4];
		}

		internal static IReadOnlyList<GraphicEntity> BuildScene()
		{
			List<GraphicEntity> entities = new();

			entities.AddRange(
				ArcEntities()
			);

			entities.AddRange(
				EllipseEntities()
			);

			entities.AddRange(
				LineEntities()
			);

			entities.AddRange(
				PolylineEntities()
			);

			entities.AddRange(
				PolygonEntities()
			);

			entities.AddRange(
				RectangleEntities()
			);

			return entities.AsReadOnly();
		}
	}
}
