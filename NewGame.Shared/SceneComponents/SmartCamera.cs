using Microsoft.Xna.Framework;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NewGame.Shared.SceneComponents
{
    public class SmartCamera : SceneComponent
    {
        private List<Entity> _entities;
        private Camera _camera;

        private float _baseZoom = 10f;

        public override void onEnabled()
        {
            _entities = new List<Entity>();
            _camera = scene.camera;

            var emitter = scene
                .getSceneComponent<SceneEventEmitter>()
                .EntityEventEmitter;

            emitter.addObserver(EntityEventType.CameraTrackerAdded,
                    OnTrackerAdded);
            emitter.addObserver(EntityEventType.CameraTrackerChanged,
                    OnTrackerChanged);
            emitter.addObserver(EntityEventType.CameraTrackerRemoved,
                    OnTrackerRemoved);
        }

        public override void onDisabled()
        {
            var emitter = scene
                .getSceneComponent<SceneEventEmitter>()
                .EntityEventEmitter;

            emitter.removeObserver(EntityEventType.CameraTrackerAdded,
                    OnTrackerAdded);
            emitter.removeObserver(EntityEventType.CameraTrackerChanged,
                    OnTrackerChanged);
            emitter.removeObserver(EntityEventType.CameraTrackerRemoved,
                    OnTrackerRemoved);
        }

        private void OnTrackerAdded(Entity entity)
        {
            _entities.Add(entity);
        }

        private void OnTrackerChanged(Entity entity)
        {
            Console.WriteLine($"Tracker changed for entity {entity}");
        }

        private void OnTrackerRemoved(Entity entity)
        {
            _entities.Remove(entity);
        }

        public override void update()
        {
            if (_entities.Count == 0)
            {
                return;
            }
            Vector2 targetPosition;
            float targetZoom = 1.0f;
            if (_entities.Count == 1)
            {
                targetZoom = _baseZoom;
                targetPosition = _entities[0].position;
            }
            else
            {
                BoundingBox.CreateFromPoints(_entities.Select(e => e.position.toVector3()))
                    .Deconstruct(out var min, out var max);

                var maxXDistance = max.X - min.X;
                var maxYDistance = max.Y - min.Y;
                var targetWidth = Math.Max(Screen.width, (maxXDistance + 100) * _baseZoom);
                var targetHeight = Math.Max(Screen.height, (maxYDistance + 100) * _baseZoom);

                targetZoom = _baseZoom * Math.Min(Screen.width / targetWidth, Screen.height / targetHeight);
                targetPosition = min.toVector2() + ((max - min).toVector2() / 2);
            }

            _camera.rawZoom = Mathf.lerp(_camera.rawZoom, targetZoom, 0.95f);
            _camera.position = Vector2.Lerp(_camera.position, targetPosition, 0.95f);
        }

    }
}
