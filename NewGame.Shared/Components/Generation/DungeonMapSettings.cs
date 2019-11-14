using Microsoft.Xna.Framework;

namespace NewGame.Shared.Components.Generation
{
    public class DungeonMapSettings
    {
        public int? Seed { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Point MinRoomSize { get; set; }
        public Point MaxRoomSize { get; set; }
        public string TileSheet { get; set; }
    }
}