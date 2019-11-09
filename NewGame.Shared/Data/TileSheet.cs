using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NewGame.Shared.Components;
using NewGame.Shared.Components.Generation;
using Nez;
using Nez.Textures;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NewGame.Shared.Data
{
    public class TileSheet
    {
        private const int right = 1, bottomRight = 2, bottom = 4, bottomLeft = 8, left = 16, topLeft = 32, top = 64, topRight = 128;

        public Dictionary<TileType, string> TileTypeMaskKindDictionary { get; set; }
        public Dictionary<TileType, Subtexture[]> SubTextureDictionary { get; set; }
        public Dictionary<string, Subtexture[][]> SubTileMaskDictionary { get; set; }

        public Point TileSize { get; set; }
        public string TileSource { get; set; }
        public string MaskSource { get; set; }
        public TileData[] TileAtlas { get; set; }
        public MaskData[] MaskAtlas { get; set; }


        public void Load()
        {
            SubTextureDictionary = new Dictionary<TileType, Subtexture[]>();
            SubTileMaskDictionary = new Dictionary<string, Subtexture[][]>();
            TileTypeMaskKindDictionary = new Dictionary<TileType, string>();

            var tileAtlas = Core.content.Load<Texture2D>($"textures/{TileSource}");
            var maskAtlas = Core.content.Load<Texture2D>($"textures/{MaskSource}");
            var subTiles = Subtexture.subtexturesFromAtlas(tileAtlas, TileSize.X / 2, TileSize.Y / 2);
            var subMasks = Subtexture.subtexturesFromAtlas(maskAtlas, TileSize.X / 2, TileSize.Y / 2);

            var horizontalTileCount = tileAtlas.Width / TileSize.X;

            foreach (var tileData in TileAtlas)
            {
                var upperTiles = subTiles.Skip(tileData.Index * 2 + (tileData.Index / horizontalTileCount) * horizontalTileCount * 2).Take(2);
                var lowerTiles = subTiles.Skip(tileData.Index * 2 + (tileData.Index / horizontalTileCount + 1) * horizontalTileCount * 2).Take(2);

                var joined = upperTiles.Concat(lowerTiles).ToArray();
                SubTextureDictionary[tileData.Type] = joined;
                TileTypeMaskKindDictionary[tileData.Type] = tileData.MaskKind;
            }

            var horizontalMaskCount = maskAtlas.Width / TileSize.X;
            foreach (var maskData in MaskAtlas)
            {
                SubTileMaskDictionary[maskData.Name] = new Subtexture[maskData.EndIndex - maskData.StartIndex + 1][];
                for (var index = maskData.StartIndex; index <= maskData.EndIndex; index++)
                {
                    var upperMasks = subMasks.Skip(index * 2 + (index / horizontalMaskCount) * horizontalMaskCount * 2).Take(2);
                    var lowerMasks = subMasks.Skip(index * 2 + (index / horizontalMaskCount + 1) * horizontalMaskCount * 2).Take(2);

                    var joined = upperMasks.Concat(lowerMasks).ToArray();
                    SubTileMaskDictionary[maskData.Name][index - maskData.StartIndex] = joined;
                }
            }
        }

        public Subtile[] ForType(TileType type, int x, int y, TileType[,] neighbours)
        {
            var textures = SubTextureDictionary[type];

            var masks = MaskForNeighbours(x, y, type, neighbours);

            var subtiles = new Subtile[4];
            foreach (var i in Enumerable.Range(0, 4))
            {
                subtiles[i] = new Subtile(textures[i], masks.masks?[i], masks.maskedTextures?[i]);
            }

            return subtiles;
        }

        public (Subtexture[] masks, Subtexture[] maskedTextures) MaskForNeighbours(int x, int y, TileType centerType, TileType[,] neighbours)
        {
            var maskKind = TileTypeMaskKindDictionary[centerType];
            if (maskKind == null) return (null, null);

            var topLeftFlags = GetNeighbourFlagsForSubtile(topLeft, neighbours, centerType, x, y);
            var topLeftIndex = GetMaskIndexFromNeighbourFlags(topLeftFlags);
            var topRightFlags = GetNeighbourFlagsForSubtile(topRight, neighbours, centerType, x, y);
            var topRightIndex = GetMaskIndexFromNeighbourFlags(topRightFlags);
            var bottomLeftFlags = GetNeighbourFlagsForSubtile(bottomLeft, neighbours, centerType, x, y);
            var bottomLeftIndex = GetMaskIndexFromNeighbourFlags(bottomLeftFlags);
            var bottomRightFlags = GetNeighbourFlagsForSubtile(bottomRight, neighbours, centerType, x, y);
            var bottomRighIndex = GetMaskIndexFromNeighbourFlags(bottomRightFlags);

            var topLeftMask = topLeftIndex < 0 ? null : SubTileMaskDictionary[maskKind][topLeftIndex][0].clone();
            var topRightMask = topRightIndex < 0 ? null : SubTileMaskDictionary[maskKind][topRightIndex][1].clone();
            var bottomLeftMask = bottomLeftIndex < 0 ? null : SubTileMaskDictionary[maskKind][bottomLeftIndex][2].clone();
            var bottomRightMask = bottomRighIndex < 0 ? null : SubTileMaskDictionary[maskKind][bottomRighIndex][3].clone();

            var masks = new Subtexture[] { topLeftMask, topRightMask, bottomLeftMask, bottomRightMask };
            var maskedTextures = GetNeighbourMaskTextures(neighbours, x, y, topLeftFlags, topRightFlags, bottomLeftFlags, bottomRightFlags);

            return (masks, maskedTextures);
        }

        private Subtexture[] GetNeighbourMaskTextures(TileType[,] neighbours, int x, int y, int topLeftFlags, int topRightFlags, int bottomLeftFlags, int bottomRightFlags)
        {
            var neighbourTypes = new TileType[4];

            if ((topLeftFlags & top) > 0)
                neighbourTypes[0] = y > 0 ? neighbours[x, y - 1] : TileType.Void;
            else if ((topLeftFlags & left) > 0)
                neighbourTypes[0] = x > 0 ? neighbours[x - 1, y] : TileType.Void;
            else if ((topLeftFlags & topLeft) > 0)
                neighbourTypes[0] = x > 0 && y > 0 ? neighbours[x - 1, y - 1] : TileType.Void;

            if ((topRightFlags & top) > 0)
                neighbourTypes[1] = y > 0 ? neighbours[x, y - 1] : TileType.Void;
            else if ((topRightFlags & right) > 0)
                neighbourTypes[1] = x < neighbours.GetLongLength(0) - 1 ? neighbours[x + 1, y] : TileType.Void;
            else if ((topRightFlags & topRight) > 0)
                neighbourTypes[1] = x < neighbours.GetLongLength(0) - 1 && y > 0 ? neighbours[x + 1, y - 1] : TileType.Void;

            if ((bottomLeftFlags & bottom) > 0)
                neighbourTypes[2] = y < neighbours.GetLongLength(1) - 1 ? neighbours[x, y + 1] : TileType.Void;
            else if ((bottomLeftFlags & left) > 0)
                neighbourTypes[2] = x > 0 ? neighbours[x - 1, y] : TileType.Void;
            else if ((bottomLeftFlags & bottomLeft) > 0)
                neighbourTypes[2] = y < neighbours.GetLongLength(1) - 1 && x > 0 ? neighbours[x - 1, y + 1] : TileType.Void;

            if ((bottomRightFlags & bottom) > 0)
                neighbourTypes[3] = y < neighbours.GetLongLength(1) - 1 ? neighbours[x, y + 1] : TileType.Void;
            else if ((bottomRightFlags & right) > 0)
                neighbourTypes[3] = x < neighbours.GetLongLength(0) - 1 ? neighbours[x + 1, y] : TileType.Void;
            else if ((bottomRightFlags & bottomRight) > 0)
                neighbourTypes[3] = x < neighbours.GetLongLength(0) - 1 && y < neighbours.GetLongLength(1) - 1 ? neighbours[x + 1, y + 1] : TileType.Void;

            return new Subtexture[] {
                SubTextureDictionary[neighbourTypes[0]][0],
                SubTextureDictionary[neighbourTypes[1]][1],
                SubTextureDictionary[neighbourTypes[2]][2],
                SubTextureDictionary[neighbourTypes[3]][3],
            };
        }

        private int GetMaskIndexFromNeighbourFlags(int flags)
        {
            if (flags == topRight || flags == topLeft || flags == bottomLeft || flags == bottomRight)
            {
                return 3;
            }

            if (((flags & right) > 0 || (flags & left) > 0) && (flags & top) == 0 && (flags & bottom) == 0)
            {
                return 2;
            }

            if (((flags & top) > 0 || (flags & bottom) > 0) && (flags & right) == 0 && (flags & left) == 0)
            {
                return 1;
            }

            if ((flags & (top + right)) > 0 || (flags & (top + left)) > 0 || (flags & (bottom + left)) > 0 || (flags & (bottom + right)) > 0)
            {
                return 0;
            }

            return -1;
        }

        private int GetNeighbourFlagsForSubtile(int subTileIndex, TileType[,] neighbours, TileType centerType, int x, int y)
        {
            switch (subTileIndex)
            {
                case topLeft:
                    return MaskIndex(neighbours, centerType, x - 1, y, left)
                        + MaskIndex(neighbours, centerType, x, y - 1, top)
                        + MaskIndex(neighbours, centerType, x - 1, y - 1, topLeft);
                case topRight:
                    return MaskIndex(neighbours, centerType, x + 1, y, right)
                        + MaskIndex(neighbours, centerType, x, y - 1, top)
                        + MaskIndex(neighbours, centerType, x + 1, y - 1, topRight);
                case bottomLeft:
                    return MaskIndex(neighbours, centerType, x - 1, y, left)
                        + MaskIndex(neighbours, centerType, x, y + 1, bottom)
                        + MaskIndex(neighbours, centerType, x - 1, y + 1, bottomLeft);
                case bottomRight:
                    return MaskIndex(neighbours, centerType, x + 1, y, right)
                        + MaskIndex(neighbours, centerType, x, y + 1, bottom)
                        + MaskIndex(neighbours, centerType, x + 1, y + 1, bottomRight);
                default:
                    throw new InvalidProgramException("Invalid subtile");
            }
        }

        private int MaskIndex(TileType[,] neighbours, TileType centerType, int x, int y, int value)
        {
            return x < 0
                || y < 0
                || x >= neighbours.GetLongLength(0)
                || y >= neighbours.GetLongLength(1)
                    ? (centerType == TileType.Void ? 0 : value)
                    : DataForType(neighbours[x, y]).CanMaskWith(centerType)
                        ? value
                        : 0;
        }

        private TileData DataForType(TileType tileType)
        {
            return TileAtlas.First(td => td.Type == tileType);
        }
    }

    public class TileData
    {
        public string Name { get; set; }
        public TileType Type { get; set; }
        public int Index { get; set; }
        public int Z { get; set; }
        public string MaskKind { get; set; }
        public TileType[] MasksWith { get; set; }

        public bool CanMaskWith(TileType centerType)
        {
            return MasksWith?.Contains(centerType) == true;
        }
    }

    public class MaskData
    {
        public string Name { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    }
}
