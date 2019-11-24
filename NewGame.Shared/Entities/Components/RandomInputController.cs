using Microsoft.Xna.Framework;
using Nez;

namespace NewGame.Shared.Entities.Components
{
    public class RandomInputController : InputController
    {
        private float _axisLimit = 1.00f;
        public override void OnAddedToEntity()
        {
            Entity.UpdateInterval = 30;
        }
        public override void Update()
        {
            MoveInput = new Vector2(_axisLimit * 2 * (Random.NextFloat() - 0.5f), _axisLimit * 2 * (Random.NextFloat() - 0.5f));
        }
    }
}
