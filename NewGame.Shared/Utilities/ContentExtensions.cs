using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using System.IO;

namespace NewGame.Shared.Utilities
{
    public static class ContentExtensions
    {
        public static Effect LoadEffect(this ContentManager contentManager, string name)
        {
            using (BinaryReader Reader = new BinaryReader(File.OpenRead($@"Content\{name}.mgfxo")))
            {
                Effect effect = new Effect(Core.GraphicsDevice, Reader.ReadBytes((int)Reader.BaseStream.Length));
                return effect;
            }
        }
    }
}
