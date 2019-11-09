using Microsoft.Xna.Framework;
using Nez;

namespace NewGame.Shared.Components
{
    public class Velocity : Component
    {
        public Velocity() { }
        public Velocity(Vector2 value) => Value = value;
        public Velocity(float x, float y) => Value = new Vector2(x, y);

        public Vector2 Value { get; set; }
    }
}
