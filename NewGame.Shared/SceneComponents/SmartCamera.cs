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
        private List<Func<Vector2>> _positionGetters;
        private Camera _camera;

        private float _baseZoom = 5f;
        private float _scrollZoom = 1f;

        public override void onEnabled()
        {
            _positionGetters = new List<Func<Vector2>>();
            _camera = scene.camera;

            var emitter = scene
                .getSceneComponent<SceneEventEmitter>()
                .EntityEventEmitter;

            emitter.addObserver(EntityEventType.CameraTrackerAdded,
                    OnTrackerAdded);
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
            emitter.removeObserver(EntityEventType.CameraTrackerRemoved,
                    OnTrackerRemoved);
        }

        private void OnTrackerAdded(Entity entity)
        {
            Func<Vector2> positionFunc;

            if (entity.getComponent<Velocity>(onlyReturnInitializedComponents: false) is Velocity v && v != null)
            {
                positionFunc = () => entity.position + (v.Value * 20);
            }
            else
            {
                positionFunc = () => entity.position;
            }

            _positionGetters.Add(positionFunc);
        }

        private void OnTrackerRemoved(Entity entity)
        {
            Func<Vector2> positionFunc;
            if (entity.getComponent<Velocity>(onlyReturnInitializedComponents: false) is Velocity v && v != null)
            {
                positionFunc = () => entity.position + (v.Value * 20);
            }
            else
            {
                positionFunc = () => entity.position;
            }

            _positionGetters.Remove(positionFunc);
        }

        public override void update()
        {
            if (_positionGetters.Count == 0)
            {
                return;
            }

            _scrollZoom = Math.Min(Math.Max(_scrollZoom + Input.mouseWheelDelta * 0.0025f, -2f), 8f);
            Console.WriteLine(_scrollZoom);

            Vector2 targetPosition;
            float targetZoom = 1.0f;
            if (_positionGetters.Count == 1)
            {
                targetZoom = _baseZoom + _scrollZoom;
                targetPosition = _positionGetters[0]();
            }
            else
            {
                BoundingBox.CreateFromPoints(_positionGetters.Select(position => position().toVector3()))
                    .Deconstruct(out var min, out var max);

                var maxXDistance = max.X - min.X;
                var maxYDistance = max.Y - min.Y;
                var targetWidth = Math.Max(Screen.width, (maxXDistance + 100));
                var targetHeight = Math.Max(Screen.height, (maxYDistance + 100));

                targetZoom = _baseZoom * Math.Min(Screen.width / targetWidth, Screen.height / targetHeight) * _scrollZoom;
                targetPosition = min.toVector2() + ((max - min).toVector2() / 2);
            }

            _camera.rawZoom = Mathf.lerp(_camera.rawZoom, targetZoom, Time.deltaTime * 4.0f);
            _camera.position = Vector2.Lerp(_camera.position, targetPosition, Time.deltaTime * 4.0f);
        }

    }
}
