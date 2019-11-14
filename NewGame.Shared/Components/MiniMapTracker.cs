using NewGame.Shared.SceneComponents;
using Nez;

namespace NewGame.Shared.Components
{
    public class MiniMapTracker : Component
    {
        public override void onAddedToEntity()
        {
            entity.scene.getSceneComponent<SceneEventEmitter>()
                .EntityEventEmitter
                .emit(EntityEventType.MiniMapTrackerAdded, entity);
        }
    }
}
