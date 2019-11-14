using Microsoft.Xna.Framework;
using NewGame.Shared.Components.Generation;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NewGame.Shared.Components
{

    public class DungeonMapComponent : RenderableComponent, IUpdatable
    {
        private DungeonMapGenerator _generator;

        private int _mapWidth;
        private int _mapHeight;

        public Tile[,] Tiles;

        public void Generate(DungeonMapSettings settings)
        {
            _mapWidth = settings.Width;
            _mapHeight = settings.Height;

            Tiles = new Tile[_mapWidth, _mapHeight];
            Tiles.Initialize();

            _generator = new DungeonMapGenerator(settings);
            _generator.Generate();

            foreach (var generated in _generator.Result.Tiles)
            {
                Tiles[generated.TilePosition.X, generated.TilePosition.Y] = generated;
            }
            foreach (var entity in _generator.Result.Entities)
            {
                Core.scene.addEntity(entity);
            }
        }

        public override void onAddedToEntity()
        {
            foreach (var colliderBounds in _generator.Result.ColliderBounds)
            {
                entity.addComponent(new BoxCollider(colliderBounds));
            }
        }

        public void update()
        {
        }

        public Tile GetTileAtPosition(float x, float y)
        {
            return Tiles[(int)Math.Floor(x / Tile.Width), (int)Math.Floor(y / Tile.Height)];
        }


        private (IEnumerable<int> x, IEnumerable<int> y) GetTileIndicesInView()
        {
            var camera = entity.scene.camera;

            var xStart = (int)Math.Max(camera.bounds.left / Tile.Width - 1, 0);
            var yStart = (int)Math.Max(camera.bounds.top / Tile.Height - 1, 0);
            var xCount = Math.Min((int)(camera.bounds.width / Tile.Width + 3), _mapWidth - xStart);
            var yCount = Math.Min((int)(camera.bounds.height / Tile.Height + 3), _mapHeight - yStart);

            var xBounds = Enumerable.Range(xStart, xCount);
            var yBounds = Enumerable.Range(yStart, yCount);

            return (xBounds, yBounds);
        }

        #region Rendering overrides

        public override void render(Graphics graphics, Camera camera)
        {
            var (xBounds, yBounds) = GetTileIndicesInView();

            foreach (var x in xBounds)
            {
                foreach (var y in yBounds)
                {
                    var tile = Tiles[x, y];
                    tile.Draw(graphics, entity, camera);
                }
            }
        }

        public override RectangleF bounds
        {
            get
            {
                if (_areBoundsDirty)
                {
                    _bounds.size = new Vector2(_mapWidth * Tile.Width, _mapHeight * Tile.Height);
                    _areBoundsDirty = false;
                }

                return _bounds;
            }
        }

        #endregion
    }
}
