using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NewGame.Shared.Entities.Components.Generation;
using NewGame.Shared.SceneComponents;
using Nez;
using System.Collections.Generic;

namespace NewGame.Shared.Entities.Components
{
    public class MiniMapComponent : RenderableComponent
    {
        private int _width;
        private int _height;
        private List<MiniMapTracker> _trackers { get; set; }
        public Texture2D Texture { get; set; }

        public Dictionary<TileType, Color> Colors = new Dictionary<TileType, Color>
        {
            { TileType.Void, new Color(127,127,127,63) },
            { TileType.Floor, Color.White},
            { TileType.Wall, Color.Gray},
            { TileType.Door, Color.DarkGray},
        };

        public MiniMapComponent()
        {
            _trackers = new List<MiniMapTracker>();

            var emitter = Core.Scene
                .GetSceneComponent<SceneEventEmitter>()
                .EntityEventEmitter;

            // Events emitted by MiniMapTracker
            emitter.AddObserver(EntityEventType.MiniMapTrackerAdded,
                    OnTrackerAdded);
            emitter.AddObserver(EntityEventType.MiniMapTrackerRemoved,
                    OnTrackerRemoved);
        }

        private void OnTrackerAdded(Entity Entity)
        {
            var tracker = Entity.GetComponent<MiniMapTracker>();
            _trackers.Add(tracker);

        }

        private void OnTrackerRemoved(Entity Entity)
        {
            var tracker = Entity.GetComponent<MiniMapTracker>();
            _trackers.Remove(tracker);
        }

        public void Build(List<Tile[,]> layers)
        {
            _width = (int)layers[0].GetLongLength(0);
            _height = (int)layers[0].GetLongLength(1);

            Texture = new Texture2D(Core.GraphicsDevice, _width, _height);
            var colorData = new Color[_width * _height];

            foreach (var layer in layers)
            {
                for (var x = 0; x < Width; x++)
                {
                    for (var y = 0; y < Height; y++)
                    {
                        if (layer[x, y] == null) continue;

                        colorData[x + y * _width] = Colors[layer[x, y].Type];
                    }
                }
            }

            Texture.SetData(0, new Rectangle(0, 0, _width, _height), colorData, 0, _width * _height);
            _areBoundsDirty = true;
        }

        public override bool IsVisibleFromCamera(Camera camera)
        {
            return true;
        }

        public override void Render(Batcher batcher, Camera camera)
        {
            if (Texture == null) return;

            batcher.Draw(Texture, Bounds);
            foreach (var tracker in _trackers)
            {
                var position = tracker.PositionGetter();
                batcher.DrawRect(
                    _bounds.Location.X + position.X / Tile.Width - 1,
                    _bounds.Location.Y + position.Y / Tile.Height - 1,
                    2,
                    2,
                    tracker.DotColor);

            }
        }

        public override RectangleF Bounds
        {
            get
            {
                if (_areBoundsDirty)
                {
                    _bounds.Size = new Vector2(_width, _height);
                    _bounds.Location = new Vector2(Entity?.Position.X ?? 0, Entity?.Position.Y ?? 0);
                    _areBoundsDirty = false;
                }

                return _bounds;
            }
        }
    }
}
