using Cartesia.Core.Geometry;
using Cartesia.Core.Mapping;
using Cartesia.Render.Rendering;
using SkiaSharp;

namespace Cartesia.Render.Entities.Base
{
	public abstract class GraphicEntity : IDisposable
	{
		private bool _isDisposed = false;
		private protected readonly Brush _fillBrush;
		private protected readonly Pen _strokePen;
		private protected readonly SKPaint _fillPaint, _strokePaint;

		private protected GraphicEntity(Brush fillIn, Pen strokeIn)
		{
			ArgumentNullException.ThrowIfNull(fillIn, nameof(fillIn));
			ArgumentNullException.ThrowIfNull(strokeIn, nameof(strokeIn));

			_fillBrush = fillIn;
			_strokePen = strokeIn;

			_fillPaint = fillIn.ToSKPaint();
			_strokePaint = strokeIn.ToSKPaint();
		}

		private protected virtual void DisposeResources()
		{
			_fillPaint.Dispose();
			_fillPaint.Dispose();
		}

		public void Dispose()
		{
			if (_isDisposed)
				return;

			DisposeResources();

			GC.SuppressFinalize(this);

			_isDisposed = true;
		}

		internal abstract Box BoundingBox();

		internal abstract void Draw(SKCanvas canvasIn);

		internal abstract void Precompute(WorldToCanvasMapper mapperIn);
	}
}
