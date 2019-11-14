using Microsoft.Xna.Framework;
using NewGame.Shared.Components;
using Nez;
using System;

namespace NewGame.Shared.Systems
{
    public class PlayerSystem : EntityProcessingSystem
    {
        private static Type[] PlayerComponents => new Type[]
        {
            typeof(Velocity),
            typeof(InputController)
        };

        public PlayerSystem()
            : base(new Matcher()
                  .all(typeof(Velocity))
                  .one(typeof(KeyboardController), typeof(RandomInputController)))
        { }


        public override void process(Entity entity)
        {
            var inputController = entity.getComponent<InputController>();
            var velocity = entity.getComponent<Velocity>();

            velocity.Value = Vector2.Lerp(velocity.Value, inputController.MoveInput * (inputController.Sprint ? 2.0f : 1.0f), Time.deltaTime * 8f);
        }
    }
}
