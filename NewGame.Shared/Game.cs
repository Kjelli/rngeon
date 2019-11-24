using Microsoft.Xna.Framework.Input;
using NewGame.Shared.Data;
using NewGame.Shared.Scenes;
using NewGame.Shared.Utilities;
using Nez;
using Nez.Console;

namespace NewGame.Shared
{
    public class Game : Core
    {
        protected override void Initialize()
        {
            base.Initialize();

            var foo = YamlSerializer.Deserialize<TileSheet>(Shared.Content.Data.Tileset_subtiles_test_atlas);

            Window.AllowUserResizing = true;
            Window.Title = "New Game";
            //Core.debugRenderEnabled = true;
            DebugConsole.ConsoleKey = Keys.F2;
            DebugConsole.RenderScale = 3;
            Screen.SetSize(800, 600);

            Scene = new NewScene();
        }
    }
}
