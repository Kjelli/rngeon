using Microsoft.Xna.Framework;
using NewGame.Shared.Entities.Components;
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
                  .All(typeof(Velocity))
                  .One(typeof(KeyboardController), typeof(RandomInputController)))
        { }


        public override void Process(Entity Entity)
        {
            var inputController = Entity.GetComponent<InputController>();
            var velocity = Entity.GetComponent<Velocity>();

            velocity.Value = Vector2.Lerp(velocity.Value, inputController.MoveInput * (inputController.Sprint ? 2.0f : 1.0f), Time.DeltaTime * 8f);
        }
    }
}
