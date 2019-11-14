using Microsoft.Xna.Framework;
using NewGame.Shared.SceneComponents;
using Nez;
using System;

namespace NewGame.Shared.Components
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


        public override void onAddedToEntity()
        {
            entity.scene.getSceneComponent<SceneEventEmitter>()
                .EntityEventEmitter
                .emit(EntityEventType.MiniMapTrackerAdded, entity);

            PositionGetter = () => entity.position;
        }

        public override void onRemovedFromEntity()
        {
            entity.scene.getSceneComponent<SceneEventEmitter>()
                   .EntityEventEmitter
                   .emit(EntityEventType.MiniMapTrackerRemoved, entity);
        }
    }
}
