using Microsoft.Xna.Framework;
using NewGame.Shared.SceneComponents;
using Nez;
using System;

namespace NewGame.Shared.Entities.Components
{
    public class MiniMapTracker : Component
    {
        private readonly Color DefaultColor = new Color(1.0f, 0.0f, 0.0f);

        public Func<Vector2> PositionGetter { get; set; }
        public Color DotColor { get; set; }
        public MiniMapTracker()
        {
            DotColor = DefaultColor;
        }

        public MiniMapTracker(Color color)
        {
            DotColor = color;
        }


        public override void OnAddedToEntity()
        {
            Core.Scene.GetSceneComponent<SceneEventEmitter>()
                .EntityEventEmitter
                .Emit(EntityEventType.MiniMapTrackerAdded, Entity);

            PositionGetter = () => Entity.Position;
        }

        public override void OnRemovedFromEntity()
        {
            Entity.Scene.GetSceneComponent<SceneEventEmitter>()
                   .EntityEventEmitter
                   .Emit(EntityEventType.MiniMapTrackerRemoved, Entity);
        }
    }
}
