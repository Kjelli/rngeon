using Microsoft.Xna.Framework;
using Nez;

namespace NewGame.Shared.Components
{
    public class RandomInputController : InputController
    {
        private float _axisLimit = 1.00f;
        public override void onAddedToEntity()
        {
            entity.updateInterval = 30;
        }
        public override void update()
        {
            MoveInput = new Vector2(_axisLimit * 2 * (Random.nextFloat() - 0.5f), _axisLimit * 2 * (Random.nextFloat() - 0.5f));
        }
    }
}
