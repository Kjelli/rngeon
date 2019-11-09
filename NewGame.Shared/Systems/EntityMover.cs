using NewGame.Shared.Components;
using Nez;

namespace NewGame.Shared.Systems
{
    public class EntityMover : EntityProcessingSystem
    {
        public EntityMover() : base(new Matcher().all(typeof(Mover), typeof(Velocity)))
        {
        }

        public override void process(Entity entity)
        {
            var mover = entity.getComponent<Mover>();
            var velocity = entity.getComponent<Velocity>();

            mover.move(velocity.Value, out _);
        }
    }
}
