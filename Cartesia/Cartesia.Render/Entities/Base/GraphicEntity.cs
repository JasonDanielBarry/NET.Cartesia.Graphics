using Cartesia.Core.Geometry;
using Cartesia.Core.Mapping;
using Cartesia.Render.Rendering;
using SkiaSharp;

namespace Cartesia.Render.Entities.Base
{
	public abstract class GraphicEntity
	{
		private protected readonly Brush _fillBrush;
		private protected readonly Pen _strokePen;

		private protected GraphicEntity(Brush fillIn, Pen strokeIn)
		{
			ArgumentNullException.ThrowIfNull(fillIn, nameof(fillIn));
			ArgumentNullException.ThrowIfNull(strokeIn, nameof(strokeIn));

			_fillBrush = fillIn;
			_strokePen = strokeIn;

			_fillPaint = fillIn.ToSKPaint();
			_strokePaint = strokeIn.ToSKPaint();
		}

		protected readonly SKPaint
			_fillPaint,
			_strokePaint;

		internal abstract Box BoundingBox();

		internal abstract void Draw(SKCanvas canvasIn);

		internal abstract void Precompute(WorldToCanvasMapper mapperIn);

		public Brush Fill => _fillBrush;
		public Pen Stroke => _strokePen;
	}
}
