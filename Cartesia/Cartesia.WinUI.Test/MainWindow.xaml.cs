using Cartesia.Core.Geometry;
using Cartesia.Core.Mapping;
using Cartesia.Render.Entities.Base;
using Cartesia.Render.Renderer;
using Microsoft.UI.Xaml;
using SkiaSharp.Views.Windows;
using System.Collections.Generic;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Cartesia.WinUI.Test
{
	/// <summary>
	/// An empty window that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class MainWindow : Window
	{
		private readonly GraphicRenderer _renderer = new GraphicRenderer();

		public MainWindow()
		{
			InitializeComponent();

			IReadOnlyList<GraphicEntity> entities = SceneBuild.BuildScene();

			_renderer.SetEntities(
				entities
			);
		}

		private void DrawingCanvas_PaintSurface(object sender, SKPaintSurfaceEventArgs e)
		{
			int
				width = e.Info.Width,
				height = e.Info.Height;

			Box worldViewPort = new Box([
				new Point(0, 0),
				new Point(800, 1200)
			]);

			WorldToCanvasMapper mapper = new WorldToCanvasMapper(
				width,
				height,
				worldViewPort
			);

			_renderer.Render(mapper, e.Surface.Canvas);
		}
	}
}
