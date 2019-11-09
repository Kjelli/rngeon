using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NewGame.Shared.Entities;
using Nez;
using Nez.Textures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NewGame.Shared.Components
{
    public class ExplorableTerrainComponent : RenderableComponent, IUpdatable
    {
        private readonly List<Subtexture> _subtiles;
        private readonly Dictionary<(float X, float Y), Tile> _tiles;
        private readonly List<Tile> _undiscoveredTiles;

        public ExplorableTerrainComponent()
        {
            _tiles = new Dictionary<(float X, float Y), Tile>();
            _undiscoveredTiles = new List<Tile>();

            var atlas = Core.content.Load<Texture2D>(Content.Textures.Tileset_test);
            _subtiles = Subtexture.subtexturesFromAtlas(atlas, 8, 8);

            var tile = new Tile
            {
                Texture = _subtiles[0],
                Position = new Vector2(0, 0)
            };

            _tiles[(0, 0)] = tile;
            _undiscoveredTiles.Add(tile);
        }

        public override bool isVisibleFromCamera(Camera camera)
        {
            return true;
        }

        public override void render(Graphics graphics, Camera camera)
        {
            foreach (var tile in _tiles.Values.Where(t => camera.bounds.intersects(new RectangleF(t.Position, t.Size))))
            {
                graphics.batcher.draw(
                    tile.Texture,
                    entity.position + tile.Position,
                    Color.White,
                    entity.rotation,
                    Vector2.Zero,
                    1.00001f,
                    SpriteEffects.None,
                    0);
            }
        }

        public void update()
        {
            var undiscovered = _undiscoveredTiles
                .Where(t => entity.scene.camera.bounds.contains(t.Position))
                .ToList();
            foreach (var tile in undiscovered)
            {
                if (entity.scene.entitiesOfType<Player>()
                    .Any(p => (p.position - tile.Position).Length() < 100))
                {
                    _undiscoveredTiles.Remove(tile);
                    Explore(tile);
                }
            }
        }

        private void Explore(Tile tile)
        {
            var left = TryGenerateTile(tile.Position.X - Tile.Width, tile.Position.Y);
            var up = TryGenerateTile(tile.Position.X, tile.Position.Y - Tile.Height);
            var right = TryGenerateTile(tile.Position.X + Tile.Width, tile.Position.Y);
            var down = TryGenerateTile(tile.Position.X, tile.Position.Y + Tile.Height);

            if (left != null) _undiscoveredTiles.Add(left);
            if (up != null) _undiscoveredTiles.Add(up);
            if (right != null) _undiscoveredTiles.Add(right);
            if (down != null) _undiscoveredTiles.Add(down);
        }

        private Tile TryGenerateTile(float x, float y)
        {
            if (_tiles.ContainsKey((x, y)))
            {
                return null;
            }
            var subtile = DetermineTypeOfTileAt(x, y);

            var newTile = new Tile
            {
                Position = new Vector2(x, y),
                Texture = subtile
            };

            _tiles[(x, y)] = newTile;

            _areBoundsDirty = true;

            Console.WriteLine($"Adding tile at {x},{y}");
            return newTile;
        }

        private Subtexture DetermineTypeOfTileAt(float x, float y)
        {
            var west = _tiles.ContainsKey((x - Tile.Width, y)) ? _tiles[(x - Tile.Width, y)] : null;
            var north = _tiles.ContainsKey((x, y - Tile.Height)) ? _tiles[(x, y - Tile.Height)] : null;
            var east = _tiles.ContainsKey((x + Tile.Width, y)) ? _tiles[(x + Tile.Width, y)] : null;
            var south = _tiles.ContainsKey((x, y + Tile.Height)) ? _tiles[(x, y + Tile.Height)] : null;

            var northwest = _tiles.ContainsKey((x - Tile.Width, y - Tile.Height)) ? _tiles[(x - Tile.Width, y - Tile.Height)] : null;
            var northeast = _tiles.ContainsKey((x + Tile.Width, y - Tile.Height)) ? _tiles[(x + Tile.Width, y - Tile.Height)] : null;
            var southwest = _tiles.ContainsKey((x - Tile.Width, y + Tile.Height)) ? _tiles[(x - Tile.Width, y + Tile.Height)] : null;
            var southeast = _tiles.ContainsKey((x + Tile.Width, y + Tile.Height)) ? _tiles[(x + Tile.Width, y + Tile.Height)] : null;

            var neighbours = new List<Tile>();

            if (west != null) neighbours.Add(west);
            if (north != null) neighbours.Add(north);
            if (east != null) neighbours.Add(east);
            if (south != null) neighbours.Add(south);
            if (northwest != null) neighbours.Add(northwest);
            if (northeast != null) neighbours.Add(northeast);
            if (southwest != null) neighbours.Add(southwest);
            if (southeast != null) neighbours.Add(southeast);

            var newIndex = Nez.Random.chance(0.001f)
                ? Nez.Random.nextInt(_subtiles.Count - 1)
                : neighbours
                .Select(t => _subtiles.IndexOf(t.Texture))
                .ToList()
                .randomItem();

            return _subtiles[newIndex];

        }

        public override RectangleF bounds
        {
            get
            {
                if (_areBoundsDirty)
                {
                    _bounds.size = RectangleF.rectEncompassingPoints(_tiles.Select(tile => tile.Value.Position).ToArray()).size;
                    _areBoundsDirty = false;
                }

                return _bounds;
            }
        }

        public class Tile
        {
            public const float Width = 8f;
            public const float Height = 8f;


            public Vector2 Size = new Vector2(Width, Height);
            public Vector2 Position { get; set; }
            public Subtexture Texture { get; set; }
        }
    }
}
