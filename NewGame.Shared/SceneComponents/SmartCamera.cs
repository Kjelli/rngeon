using Microsoft.Xna.Framework;
using NewGame.Shared.Components;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NewGame.Shared.SceneComponents
{
    public class SmartCamera : SceneComponent
    {
        private List<CameraTracker> _entities;
        private Camera _camera;

        private float _baseZoom = 5f;
        private float _scrollZoom = 1f;

        public override void onEnabled()
        {
            _entities = new List<CameraTracker>();
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

        private void OnTrackerAdded(CameraTracker entity)
        {
            _entities.Add(entity);
        }

        private void OnTrackerChanged(CameraTracker entity)
        {
            Console.WriteLine($"Tracker changed for entity {entity}");
        }

        private void OnTrackerRemoved(CameraTracker entity)
        {
            _entities.Remove(entity);
        }

        public override void update()
        {
            if (_entities.Count == 0)
            {
                return;
            }

            _scrollZoom = Math.Min(Math.Max(_scrollZoom + Input.mouseWheelDelta * 0.0025f, -2f), 8f);
            Console.WriteLine(_scrollZoom);

            Vector2 targetPosition;
            float targetZoom = 1.0f;
            if (_entities.Count == 1)
            {
                targetZoom = _baseZoom + _scrollZoom;
                targetPosition = _entities[0].Position;
            }
            else
            {
                BoundingBox.CreateFromPoints(_entities.Select(e => e.Position.toVector3()))
                    .Deconstruct(out var min, out var max);

                var maxXDistance = max.X - min.X;
                var maxYDistance = max.Y - min.Y;
                var targetWidth = Math.Max(Screen.width, (maxXDistance + 100));
                var targetHeight = Math.Max(Screen.height, (maxYDistance + 100));

                targetZoom = _baseZoom * Math.Min(Screen.width / targetWidth, Screen.height / targetHeight) * _scrollZoom;
                targetPosition = min.toVector2() + ((max - min).toVector2() / 2);
            }

            _camera.rawZoom = Mathf.lerp(_camera.rawZoom, targetZoom, Time.deltaTime * 5.0f);
            _camera.position = Vector2.Lerp(_camera.position, targetPosition, Time.deltaTime * 5.0f);
        }

    }
}
