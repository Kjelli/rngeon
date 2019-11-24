using NewGame.Shared.Entities.Components;
using Nez;

namespace NewGame.Shared.Systems
{
    public class EntityMover : EntityProcessingSystem
    {
        public EntityMover() : base(new Matcher().All(typeof(Mover), typeof(Velocity)))
        {
        }

        public override void Process(Entity Entity)
        {
            var mover = Entity.GetComponent<Mover>();
            var velocity = Entity.GetComponent<Velocity>();

            mover.Move(velocity.Value, out _);
        }
    }
}
