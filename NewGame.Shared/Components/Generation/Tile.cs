using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NewGame.Shared.Components.Generation;
using Nez;

namespace NewGame.Shared.Components
{
    public class Tile
    {
        public const int TileWidth = 16, TileHeight = 16;

        public static Vector2[] SubtileOffsets = new Vector2[]
        {
            Vector2.Zero,
            new Vector2(TileWidth / 2, 0),
            new Vector2(0, TileHeight / 2),
            new Vector2(TileWidth / 2, TileHeight / 2)
        };

        public Point TilePosition { get; set; }
        public RectangleF Bounds { get; set; }
        public TileType Type { get; set; }
        public Subtile[] Subtiles { get; set; }

        public Tile(int x, int y, TileType type, Subtile[] subtiles)
        {
            Type = type;
            Subtiles = subtiles;

            TilePosition = new Point(x, y);
            Bounds = new RectangleF(x * TileWidth, y * TileHeight, TileWidth, TileHeight);
        }

        internal void Draw(Graphics graphics, Entity entity, Camera camera)
        {
            for (var i = 0; i < 4; i++)
            {
                graphics.batcher.draw(
                    Subtiles[i].Texture,
                    entity.position + Bounds.location + SubtileOffsets[i],
                    Color.White,
                    entity.rotation,
                    Vector2.Zero,
                    1.001f,
                    SpriteEffects.None,
                    0);

                if (Subtiles[i].Mask == null) continue;
                graphics.batcher.draw(
                   Subtiles[i].Mask,
                   entity.position + Bounds.location + SubtileOffsets[i],
                   Color.White,
                   entity.rotation,
                   Vector2.Zero,
                   1.001f,
                   SpriteEffects.None,
                   0);
            }

        }
    }
}
