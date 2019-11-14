using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NewGame.Shared.Components;
using NewGame.Shared.Components.Generation;
using NewGame.Shared.Entities;
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

        public override void initialize()
        {
            _sceneAssets = new string[] {
                Content.Textures.Test,
                Content.Textures.Tileset_test,
                Content.Textures.Tileset_props
            };

            Screen.isFullscreen = true;
            clearColor = Color.Black;
            setDesignResolution(800, 600, SceneResolutionPolicy.ShowAll);
            DeferredRenderer = addRenderer(new DeferredLightingRenderer(0, Constants.RenderLayerLight,
                Constants.RenderLayerMap,
                Constants.RenderLayerPlayer,
                Constants.RenderLayerProps));
            DeferredRenderer.setAmbientColor(Color.Black);
            DeferredRenderer.setClearColor(Color.Black);
            addRenderer(new ScreenSpaceRenderer(1, Constants.RenderLayerScreenSpace));
            addSceneComponent(new SceneEventEmitter());
            addSceneComponent(new SmartCamera());
            addEntityProcessor(new EntityMover());
            addEntityProcessor(new PlayerSystem());

            //addPostProcessor(new BloomPostProcessor(2))
            //    .setBloomSettings(BloomSettings.presetSettings[5]);

            foreach (var asset in _sceneAssets)
            {
                content.Load<Texture2D>(asset);
            }
        }

        public override void onStart()
        {
            PopulateScene();
        }

        public void PopulateScene()
        {
            var mapWidth = 50;
            var mapHeight = 50;
            var minRoomSize = new Point(15, 10);
            var maxRoomSize = new Point(30, 20);

            var settings = new DungeonMapSettings
            {
                Width = mapWidth,
                Height = mapHeight,
                MinRoomSize = minRoomSize,
                MaxRoomSize = maxRoomSize,
                TileSheet = Content.Data.Tileset_subtiles_test_atlas
            };

            var map = EntityFactory.Presets
                .DungeonMap(settings)
                .AtPosition(0, 0)
                .Create();
            addEntity(map);

            var miniMap = EntityFactory.Presets
                .Minimap(map.getComponent<DungeonMapComponent>().Tiles)
                .AtPosition(camera.bounds.width - mapWidth - 20, 20)
                .Create();
            addEntity(miniMap);

            var player = EntityFactory.Presets
                .Player()
                .AtPosition(mapWidth / 2 * 16, mapHeight / 2 * 16)
                .AddInput<KeyboardController>()
                .Create();
            addEntity(player);

            // Add extra players with random input
            var n = 0;
            for (var i = 0; i < n; i++)
            {
                float x, y;
                do
                {
                    x = Random.nextFloat() * mapWidth * Tile.Width;
                    y = Random.nextFloat() * mapHeight * Tile.Height;
                } while (map.getComponent<DungeonMapComponent>().GetTileAtPosition(x, y).Type != TileType.Floor);
                var bot = EntityFactory.Presets.Player()
                    .AddInput<RandomInputController>()
                    .AtPosition(x, y)
                    .Create();
                addEntity(bot);
            }
        }
    }
}
