using Microsoft.Xna.Framework;
using NewGame.Shared.SceneComponents;
using Nez;

namespace NewGame.Shared.Entities.Components
{
    public class CameraTracker : Component
    {
        public Velocity Velocity { get; set; }
        public Vector2 Position => Entity.Position + Velocity?.Value * 30f ?? Vector2.Zero;

        public override void OnAddedToEntity()
        {
            var velocityComponent = Entity.GetComponent<Velocity>();
            if (velocityComponent != null)
            {
                Velocity = velocityComponent;
            }

            Entity.Scene.GetSceneComponent<SceneEventEmitter>()
                .EntityEventEmitter
                .Emit(EntityEventType.CameraTrackerAdded, Entity);
        }
    }
}
