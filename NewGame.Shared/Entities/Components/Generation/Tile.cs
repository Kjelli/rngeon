using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Textures;

namespace NewGame.Shared.Entities.Components.Generation
{
    public class Tile
    {
        public const int Width = 16, Height = 16;

        public Point TilePosition { get; set; }
        public RectangleF Bounds { get; set; }
        public TileType Type { get; set; }
        public Sprite Sprite { get; set; }

        public Tile(int x, int y, TileType type, Sprite sprite)
        {
            Type = type;
            Sprite = sprite;

            TilePosition = new Point(x, y);
            Bounds = new RectangleF(x * Width, y * Height, Width, Height);
        }

        internal void Draw(Batcher batcher, Entity Entity)
        {
            batcher.Draw(
                Sprite,
                Entity.Position + Bounds.Location,
                Color.White,
                Entity.Rotation,
                Vector2.Zero,
                1.01f,
                SpriteEffects.None,
                0);
        }
    }
}
