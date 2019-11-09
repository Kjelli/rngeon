using Microsoft.Xna.Framework;
using Nez.Textures;

namespace NewGame.Shared.Components.Generation
{
    public class Subtile
    {
        public Subtexture Texture { get; set; }
        public Subtexture Mask { get; set; }

        public Subtile(Subtexture texture, Subtexture mask, Subtexture maskTexture)
        {
            Texture = texture;
            Mask = mask;
            if (mask != null)
            {
                CopyTextureData(mask, maskTexture);
            }
        }

        private void CopyTextureData(Subtexture mask, Subtexture texture)
        {
            var count = Tile.TileWidth * Tile.TileHeight / 4;
            var maskData = new Color[count];
            mask.texture2D.GetData(0, mask.sourceRect, maskData, 0, count);

            var texData = new Color[count];
            texture.texture2D.GetData(0, texture.sourceRect, texData, 0, count);

            var maskedTexture = new Color[count];
            for (var i = 0; i < count; i++)
            {
                maskedTexture[i] = maskData[i].A == 0
                    ? Color.Transparent
                    : maskData[i].A == 255 && maskData[i].R == 255 && maskData[i].G == 255 && maskData[i].B == 255
                        ? texData[i]
                        : maskData[i];
            }

            mask.texture2D.SetData(0, mask.sourceRect, maskedTexture, 0, count);
        }
    }
}
