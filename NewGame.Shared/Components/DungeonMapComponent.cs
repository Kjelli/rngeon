using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        private readonly int? _seed;

        public int MapWidth, MapHeight;
        public int TileWidth = 16, TileHeight = 16;

        public Tile[,] Tiles;

        public DungeonMapComponent(int mapWidth = 100, int mapHeight = 100, int? seed = null)
        {
            _seed = seed;
            MapWidth = mapWidth;
            MapHeight = mapHeight;

            Generate();

            material = new Material();
            material.blendState = BlendState.NonPremultiplied;
        }

        private void Generate()
        {
            Tiles = new Tile[MapWidth, MapHeight];
            Tiles.Initialize();

            _generator = new DungeonMapGenerator(MapWidth, MapHeight);
            _generator.Generate(_seed);

            foreach (var generated in _generator.Result.Tiles)
            {
                Tiles[generated.TilePosition.X, generated.TilePosition.Y] = generated;
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
            return Tiles[(int)Math.Floor(x / Tile.TileWidth), (int)Math.Floor(y / Tile.TileHeight)];
        }


        private (IEnumerable<int> x, IEnumerable<int> y) GetTileIndicesInView()
        {
            var camera = entity.scene.camera;

            var xStart = (int)Math.Max(camera.bounds.left / Tile.TileWidth - 1, 0);
            var yStart = (int)Math.Max(camera.bounds.top / Tile.TileHeight - 1, 0);
            var xCount = Math.Min((int)(camera.bounds.width / Tile.TileWidth + 3), MapWidth - xStart);
            var yCount = Math.Min((int)(camera.bounds.height / Tile.TileHeight + 3), MapHeight - yStart);

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
                    _bounds.size = new Vector2(MapWidth * TileWidth, MapHeight * TileHeight);
                    _areBoundsDirty = false;
                }

                return _bounds;
            }
        }

        #endregion
    }
}
