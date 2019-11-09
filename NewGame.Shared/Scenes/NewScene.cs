using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NewGame.Shared.Components;
using NewGame.Shared.Entities;
using NewGame.Shared.SceneComponents;
using NewGame.Shared.Systems;
using Nez;

namespace NewGame.Shared.Scenes
{
    public class NewScene : Scene
    {
        private string[] _sceneAssets;

        public override void initialize()
        {
            _sceneAssets = new string[] {
                Content.Textures.Test,
                Content.Textures.Tileset_test,
            };

            Screen.isFullscreen = true;
            clearColor = Color.Black;
            setDesignResolution(800, 600, SceneResolutionPolicy.None);
            addRenderer(new RenderLayerExcludeRenderer(0));
            addSceneComponent(new SceneEventEmitter());
            addSceneComponent(new SmartCamera());
            addEntityProcessor(new EntityMover());
            addEntityProcessor(new PlayerSystem());

            addPostProcessor(new VignettePostProcessor(1));

            foreach (var asset in _sceneAssets)
            {
                content.Load<Texture2D>(asset);
            }

            OnLoaded();
        }

        private void OnLoaded()
        {
            PopulateScene();
        }

        public void PopulateScene()
        {
            int mapWidth = 200;
            int mapHeight = 100;
            var map = EntityFactory.Presets.DungeonMap(mapWidth, mapHeight, 1)
                .AtPosition(0, 0)
                .Create();

            addEntity(map);

            var player = EntityFactory.Presets.Player()
                .AtPosition(100, 100)
                .AddInput<KeyboardController>()
                .Create();
            addEntity(player);

            var n = 0;
            for (var i = 0; i < n; i++)
            {
                float x, y;
                do
                {
                    x = Nez.Random.nextFloat() * mapWidth * Tile.TileWidth;
                    y = Nez.Random.nextFloat() * mapHeight * Tile.TileHeight;
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
