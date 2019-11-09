using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace NewGame.Shared.Components.Generation
{
    internal class Connector
    {
        public Rectangle A { get; internal set; }
        public Rectangle Mid { get; internal set; }
        public Rectangle B { get; internal set; }

        public IEnumerable<Rectangle> GetRectangles()
        {
            return new Rectangle[] { A, Mid, B };
        }
    }
}