using Microsoft.Xna.Framework.Input;
using Nez;
using static Nez.VirtualInput;

namespace NewGame.Shared.Components
{

    public class KeyboardController : InputController
    {
        private VirtualJoystick _leftStick;

        public override void onAddedToEntity()
        {
            _leftStick = new VirtualJoystick(false);
            _leftStick.addKeyboardKeys(OverlapBehavior.CancelOut,
                Keys.A, Keys.D, Keys.W, Keys.S);
        }
        public override void update()
        {
            LeftStickInput = _leftStick.value;
        }
    }
}
