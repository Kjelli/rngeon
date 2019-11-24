using Microsoft.Xna.Framework;
using NewGame.Shared.Entities.Components;
using Nez;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NewGame.Shared.SceneComponents
{
    public class SmartCamera : SceneComponent
    {
        private readonly float _baseZoom = 5f;

        private List<Func<Vector2>> _positionGetters;
        private Camera _camera;

        private float _scrollZoom = 1f;

        public override void OnEnabled()
        {
            _positionGetters = new List<Func<Vector2>>();
            _camera = Scene.Camera;

            var emitter = Scene
                .GetSceneComponent<SceneEventEmitter>()
                .EntityEventEmitter;

            // Events emitted by Camera Tracker
            emitter.AddObserver(EntityEventType.CameraTrackerAdded,
                    OnTrackerAdded);
            emitter.AddObserver(EntityEventType.CameraTrackerRemoved,
                    OnTrackerRemoved);

        }

        public override void OnDisabled()
        {
            var emitter = Scene
                .GetSceneComponent<SceneEventEmitter>()
                .EntityEventEmitter;

            // Events emitted by Camera Tracker
            emitter.RemoveObserver(EntityEventType.CameraTrackerAdded,
                    OnTrackerAdded);
            emitter.RemoveObserver(EntityEventType.CameraTrackerRemoved,
                    OnTrackerRemoved);
        }

        private void OnTrackerAdded(Entity Entity)
        {
            Func<Vector2> positionFunc;

            if (Entity.GetComponent<Velocity>(onlyReturnInitializedComponents: false) is Velocity v && v != null)
            {
                positionFunc = () => Entity.Position + (v.Value * 30f);
            }
            else
            {
                positionFunc = () => Entity.Position;
            }

            _positionGetters.Add(positionFunc);
        }

        private void OnTrackerRemoved(Entity Entity)
        {
            Func<Vector2> positionFunc;
            if (Entity.GetComponent<Velocity>(onlyReturnInitializedComponents: false) is Velocity v && v != null)
            {
                positionFunc = () => Entity.Position + (v.Value * 30);
            }
            else
            {
                positionFunc = () => Entity.Position;
            }

            _positionGetters.Remove(positionFunc);
        }

        public override void Update()
        {
            if (_positionGetters.Count == 0)
            {
                return;
            }

            _scrollZoom = Math.Min(Math.Max(_scrollZoom + Input.MouseWheelDelta * 0.0025f, -2f), 8f);

            Vector2 targetPosition;
            float targetZoom = 1.0f;
            if (_positionGetters.Count == 1)
            {
                targetZoom = _baseZoom + _scrollZoom;
                targetPosition = _positionGetters[0]();
            }
            else
            {
                BoundingBox.CreateFromPoints(_positionGetters.Select(position => position().ToVector3()))
                    .Deconstruct(out var min, out var max);

                var maxXDistance = max.X - min.X;
                var maxYDistance = max.Y - min.Y;
                var targetWidth = Math.Max(Screen.Width, maxXDistance + 100);
                var targetHeight = Math.Max(Screen.Height, maxYDistance + 100);

                targetZoom = _baseZoom * Math.Min(Screen.Width / targetWidth, Screen.Height / targetHeight) * _scrollZoom;
                targetPosition = min.ToVector2() + ((max - min).ToVector2() / 2);
            }

            _camera.RawZoom = Mathf.Lerp(_camera.RawZoom, targetZoom, Time.DeltaTime * 4.0f);
            _camera.Position = Vector2.Lerp(_camera.Position, targetPosition, Time.DeltaTime * 4.0f);
        }

    }
}
