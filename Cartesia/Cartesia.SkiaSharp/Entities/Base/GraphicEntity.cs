using Cartesia.SkiaSharp.Rendering;

namespace Cartesia.SkiaSharp.Entities.Base
{
	public abstract class GraphicEntity
	{
		private protected readonly Brush _fill;
		private protected readonly Pen _stroke;

		private protected GraphicEntity(Brush fillIn, Pen strokeIn)
		{
			_fill = fillIn;
			_stroke = strokeIn;
		}

		internal abstract void Draw();

		public Brush Fill => _fill;
		public Pen Stroke => _stroke;
	}
}
