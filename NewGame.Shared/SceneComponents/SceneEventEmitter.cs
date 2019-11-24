using Nez;
using Nez.Systems;
using System.Collections.Generic;

namespace NewGame.Shared.SceneComponents
{
    public enum EntityEventType
    {
        CameraTrackerAdded,
        CameraTrackerRemoved,
        MiniMapTrackerAdded,
        MiniMapTrackerRemoved
    }

    internal class EntityEventTypeComparer : IEqualityComparer<EntityEventType>
    {
        public bool Equals(EntityEventType x, EntityEventType y)
        {
            return x == y;
        }

        public int GetHashCode(EntityEventType obj)
        {
            return obj.GetHashCode();
        }
    }

    public class CameraTrackerEventEmitter : Emitter<EntityEventType, Entity>
    {
        public CameraTrackerEventEmitter() : base(new EntityEventTypeComparer())
        {
        }
    }

    public class SceneEventEmitter : SceneComponent
    {
        public CameraTrackerEventEmitter EntityEventEmitter { get; set; }

        public override void OnEnabled()
        {
            base.OnEnabled();
            EntityEventEmitter = new CameraTrackerEventEmitter();
        }
    }

}
