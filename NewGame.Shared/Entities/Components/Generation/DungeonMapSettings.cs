using Microsoft.Xna.Framework;
using RNG = Nez.Random;
namespace NewGame.Shared.Entities.Components.Generation
{
    public class DungeonMapGeneratorSettings
    {
        public int? Seed { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Point MinRoomSize { get; set; }
        public Point MaxRoomSize { get; set; }
        public string TileSheet { get; set; }
        public Color AmbientColor { get; set; }
        public Color TorchColor { get; set; }

        public static DungeonMapGeneratorSettings Random(int width, int height, int? seed = null)
        {
            if (seed.HasValue)
            {
                RNG.SetSeed(seed.Value);
            }

            var min = new Point(7 + RNG.NextInt(10), 7 + RNG.NextInt(10));
            var max = new Point(min.X + RNG.NextInt(10), min.Y + RNG.NextInt(10));
            return new DungeonMapGeneratorSettings
            {
                Seed = seed,
                Width = width,
                Height = height,
                MinRoomSize = min,
                MaxRoomSize = max,
                AmbientColor = new Color(RNG.NextFloat() / 16, RNG.NextFloat() / 16, RNG.NextFloat() / 16),
                TorchColor = new Color(RNG.NextFloat() / 4, RNG.NextFloat() / 4, RNG.NextFloat() / 4),
                TileSheet = Content.Data.Tileset_subtiles_test_atlas
            };
        }
    }
}