using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NewGame.Shared.SceneComponents;
using Nez;
using System.Collections.Generic;

namespace NewGame.Shared.Components
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
        };

        public MiniMapComponent()
        {
            _trackers = new List<MiniMapTracker>();

            var emitter = Core.scene
                .getSceneComponent<SceneEventEmitter>()
                .EntityEventEmitter;

            // Events emitted by MiniMapTracker
            emitter.addObserver(EntityEventType.MiniMapTrackerAdded,
                    OnTrackerAdded);
            emitter.addObserver(EntityEventType.MiniMapTrackerRemoved,
                    OnTrackerRemoved);
        }

        private void OnTrackerAdded(Entity entity)
        {
            var tracker = entity.getComponent<MiniMapTracker>();
            _trackers.Add(tracker);

        }

        private void OnTrackerRemoved(Entity entity)
        {
            var tracker = entity.getComponent<MiniMapTracker>();
            _trackers.Remove(tracker);
        }

        public void Build(Tile[,] tileMap)
        {
            _width = (int)tileMap.GetLongLength(0);
            _height = (int)tileMap.GetLongLength(1);

            Texture = new Texture2D(Core.graphicsDevice, _width, _height);

            var colorData = new Color[_width * _height];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    colorData[x + y * _width] = Colors[tileMap[x, y].Type];
                }
            }

            Texture.SetData(0, new Rectangle(0, 0, _width, _height), colorData, 0, _width * _height);
            _areBoundsDirty = true;
        }

        public override bool isVisibleFromCamera(Camera camera)
        {
            return true;
        }

        public override void render(Graphics graphics, Camera camera)
        {
            if (Texture == null) return;

            graphics.batcher.draw(Texture, bounds);
            foreach (var tracker in _trackers)
            {
                var position = tracker.PositionGetter();
                graphics.batcher.drawRect(
                    _bounds.location.X + position.X / Tile.Width - 1,
                    _bounds.location.Y + position.Y / Tile.Height - 1,
                    2,
                    2,
                    tracker.DotColor);

            }
        }

        public override RectangleF bounds
        {
            get
            {
                if (_areBoundsDirty)
                {
                    _bounds.size = new Vector2(_width, _height);
                    _bounds.location = new Vector2(entity?.position.X ?? 0, entity?.position.Y ?? 0);
                    _areBoundsDirty = false;
                }

                return _bounds;
            }
        }
    }
}
