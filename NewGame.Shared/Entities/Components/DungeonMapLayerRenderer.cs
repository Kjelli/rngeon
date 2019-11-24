using Microsoft.Xna.Framework;
using NewGame.Shared.Entities.Components.Generation;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NewGame.Shared.Entities.Components
{

    public class DungeonMapLayerRenderer : RenderableComponent
    {
        public Tile[,] Tiles { get; private set; }

        private long _mapWidth;
        private long _mapHeight;

        public void SetTiles(Tile[,] tiles)
        {
            Tiles = tiles;
            _mapWidth = Tiles.GetLongLength(0);
            _mapHeight = Tiles.GetLongLength(1);
        }

        private (IEnumerable<int> x, IEnumerable<int> y) GetTileIndicesInView(Camera camera)
        {
            var xStart = (int)Math.Max(camera.Bounds.Left / Tile.Width - 1, 0);
            var yStart = (int)Math.Max(camera.Bounds.Top / Tile.Height - 1, 0);
            var xCount = (int)Math.Min(camera.Bounds.Width / Tile.Width + 3, _mapWidth - xStart);
            var yCount = (int)Math.Min(camera.Bounds.Height / Tile.Height + 3, _mapHeight - yStart);

            var xBounds = Enumerable.Range(xStart, xCount);
            var yBounds = Enumerable.Range(yStart, yCount);

            return (xBounds, yBounds);
        }

        #region Rendering overrides

        public override void Render(Batcher batcher, Camera camera)
        {
            if (Tiles is null) return;

            var (xBounds, yBounds) = GetTileIndicesInView(camera);

            foreach (var x in xBounds)
            {
                foreach (var y in yBounds)
                {
                    var tile = Tiles[x, y];
                    if (tile == null) continue;

                    tile.Draw(batcher, Entity);
                }
            }
        }

        public override RectangleF Bounds
        {
            get
            {
                if (_areBoundsDirty)
                {
                    _bounds.Size = new Vector2(_mapWidth * Tile.Width, _mapHeight * Tile.Height);
                    _areBoundsDirty = false;
                }

                return _bounds;
            }
        }


        #endregion
    }
}
