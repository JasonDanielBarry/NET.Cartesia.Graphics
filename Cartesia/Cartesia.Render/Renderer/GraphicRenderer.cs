using Cartesia.Core.Mapping;
using Cartesia.Render.Entities.Base;
using SkiaSharp;

namespace Cartesia.Render.Renderer
{
	public sealed class GraphicRenderer
	{
		private int _entityCount = 0;

		private GraphicEntity[] _entities = null!;

		private void Precompute(WorldToCanvasMapper mapperIn)
		{
			_ = Parallel.For(0, _entityCount, (i) =>
			{
				_entities[i].Precompute(mapperIn);
			});
		}

		private void Draw(SKCanvas canvasIn)
		{
			for (int i = 0; i < _entityCount; i++)
			{
				_entities[i].Draw(canvasIn);
			}
		}

		public void Render(WorldToCanvasMapper mapperIn, SKCanvas canvasIn)
		{
			Precompute(mapperIn);

			Draw(canvasIn);
		}

		public void SetEntities(IReadOnlyList<GraphicEntity> entitiesIn)
		{
			_entities = entitiesIn.ToArray();
			_entityCount = _entities.Length;
		}
	}
}
