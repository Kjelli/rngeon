using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NewGame.Shared.Entities;
using NewGame.Shared.Entities.Components;
using NewGame.Shared.Entities.Components.Generation;
using NewGame.Shared.SceneComponents;
using NewGame.Shared.Systems;
using Nez;
using Nez.DeferredLighting;

namespace NewGame.Shared.Scenes
{
    public class NewScene : Scene
    {
        private string[] _sceneAssets;

        public DeferredLightingRenderer DeferredRenderer { get; private set; }

        public override void Initialize()
        {
            _sceneAssets = new string[] {
                Shared.Content.Textures.Test,
                Shared.Content.Textures.Tileset_test,
                Shared.Content.Textures.Tileset_props
            };

            Screen.IsFullscreen = true;
            ClearColor = Color.Black;
            SetDesignResolution(800, 600, SceneResolutionPolicy.ShowAll);
            DeferredRenderer = AddRenderer(new DeferredLightingRenderer(0,
                Constants.RenderLayerLight,
                Constants.RenderLayerMapFloor,
                Constants.RenderLayerMapWall,
                Constants.RenderLayerPlayer,
                Constants.RenderLayerProps));
            DeferredRenderer.SetClearColor(Color.Black);

            AddRenderer(new ScreenSpaceRenderer(1, Constants.RenderLayerScreenSpace));
            AddSceneComponent(new SceneEventEmitter());
            AddSceneComponent(new SmartCamera());
            AddEntityProcessor(new EntityMover());
            AddEntityProcessor(new PlayerSystem());

            //AddPostProcessor(new BloomPostProcessor(2))
            //    .SetBloomSettings(BloomSettings.preSetSettings[5]);

            foreach (var asset in _sceneAssets)
            {
                Content.Load<Texture2D>(asset);
            }
        }

        public override void OnStart()
        {
            PopulateScene();
        }

        public override void Update()
        {
            base.Update();
        }

        public void PopulateScene()
        {
            var mapWidth = 35;
            var mapHeight = 35;

            var settings = DungeonMapGeneratorSettings.Random(mapWidth, mapHeight);
            DeferredRenderer.SetAmbientColor(settings.AmbientColor);

            var map = EntityFactory.Presets
                .DungeonMap()
                .AtPosition(0, 0)
                .Create();
            map.Generate(settings);
            AddEntity(map);

            var miniMap = EntityFactory.Presets
                .Minimap(map.TileLayers)
                .AtPosition(Camera.Bounds.Width - mapWidth - 20, 20)
                .Create();
            AddEntity(miniMap);

            var spawn = map.Spawn;

            var player = EntityFactory.Presets
                .Player()
                .AtPosition(spawn.Position.X, spawn.Position.Y)
                .AddInput<KeyboardController>()
                .Create();
            AddEntity(player);

            // Add extra players with random input
            var n = 0;
            for (var i = 0; i < n; i++)
            {
                float x, y;
                do
                {
                    x = Random.NextFloat() * mapWidth * Tile.Width;
                    y = Random.NextFloat() * mapHeight * Tile.Height;
                } while (map.GetTileAtPosition(x, y).Type != TileType.Floor);

                var bot = EntityFactory.Presets.UntrackedPlayer()
                    .With(new MiniMapTracker(Color.Blue))
                    .AddInput<RandomInputController>()
                    .AtPosition(x, y)
                    .Create();
                AddEntity(bot);
            }
        }
    }
}
