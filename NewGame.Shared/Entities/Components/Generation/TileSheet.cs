using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NewGame.Shared.Entities.Components.Generation;
using Nez;
using Nez.Textures;
using System.Collections.Generic;

namespace NewGame.Shared.Data
{
    public class TileSheet
    {
        public Dictionary<TileType, Sprite> TileDictionary { get; set; }

        public Point TileSize { get; set; }
        public string TileSource { get; set; }
        public TileData[] TileAtlas { get; set; }


        public void Load()
        {
            TileDictionary = new Dictionary<TileType, Sprite>();

            var tileAtlas = Core.Content.Load<Texture2D>($"textures/{TileSource}");
            var tiles = Sprite.SpritesFromAtlas(tileAtlas, Tile.Width, Tile.Height);

            foreach (var tileData in TileAtlas)
            {
                var tile = tiles[tileData.Index];

                TileDictionary[tileData.Type] = tile;
            }
        }
    }

    public class TileData
    {
        public string Name { get; set; }
        public TileType Type { get; set; }
        public int Index { get; set; }
        public int Z { get; set; }
    }
}
