using NewGame.Shared.Components;
using Nez;
using Nez.Systems;
using System.Collections.Generic;

namespace NewGame.Shared.SceneComponents
{
    public enum EntityEventType
    {
        CameraTrackerAdded,
        CameraTrackerChanged,
        CameraTrackerRemoved
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

    public class CameraTrackerEventEmitter : Emitter<EntityEventType, CameraTracker>
    {
        public CameraTrackerEventEmitter() : base(new EntityEventTypeComparer())
        {
        }
    }

    public class SceneEventEmitter : SceneComponent
    {
        public CameraTrackerEventEmitter EntityEventEmitter { get; set; }

        public override void onEnabled()
        {
            base.onEnabled();
            EntityEventEmitter = new CameraTrackerEventEmitter();
        }
    }

}
