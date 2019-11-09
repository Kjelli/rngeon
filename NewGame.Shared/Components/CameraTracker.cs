using NewGame.Shared.SceneComponents;
using Nez;

namespace NewGame.Shared.Components
{
    public class CameraTracker : Component
    {
        public override void onAddedToEntity()
        {
            entity.scene.getSceneComponent<SceneEventEmitter>()
                .EntityEventEmitter
                .emit(EntityEventType.CameraTrackerAdded, entity);
        }
    }
}
