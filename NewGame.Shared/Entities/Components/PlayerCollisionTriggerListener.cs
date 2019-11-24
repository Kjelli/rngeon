using Microsoft.Xna.Framework;
using NewGame.Shared.Entities.Props;
using NewGame.Shared.Scenes;
using Nez;

namespace NewGame.Shared.Entities.Components
{
    public class PlayerCollisionTriggerListener : Component, ITriggerListener
    {
        public void OnTriggerEnter(Collider other, Collider local)
        {
            if (other.Entity is Exit exit)
            {
                Core.StartSceneTransition(new FadeTransition(() => new NewScene())
                {
                    FadeToColor = Color.Black,
                    FadeInDuration = 1f,
                    FadeOutDuration = 3f,
                    LoadSceneOnBackgroundThread = true
                });
            }
        }

        public void OnTriggerExit(Collider other, Collider local)
        {

        }
    }
}
