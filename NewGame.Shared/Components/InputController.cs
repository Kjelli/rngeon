using Microsoft.Xna.Framework;
using Nez;

namespace NewGame.Shared.Components
{
    public abstract class InputController : Component, IUpdatable
    {
        public Vector2 MoveInput { get; protected set; }
        public bool Sprint { get; protected set; }
        public abstract void update();
    }
}
