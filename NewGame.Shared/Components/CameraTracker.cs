using Microsoft.Xna.Framework;
using NewGame.Shared.SceneComponents;
using Nez;

namespace NewGame.Shared.Components
{
    public class CameraTracker : Component
    {
        public Velocity Velocity { get; set; }
        public Vector2 Position => entity.position + Velocity?.Value * 30f ?? Vector2.Zero;

        public override void onAddedToEntity()
        {
            var velocityComponent = entity.getComponent<Velocity>();
            if (velocityComponent != null)
            {
                Velocity = velocityComponent;
            }

            entity.scene.getSceneComponent<SceneEventEmitter>()
                .EntityEventEmitter
                .emit(EntityEventType.CameraTrackerAdded, entity);
        }
    }
}
