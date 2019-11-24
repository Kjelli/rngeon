using Microsoft.Xna.Framework.Input;
using Nez;
using static Nez.VirtualInput;

namespace NewGame.Shared.Entities.Components
{

    public class KeyboardController : InputController
    {
        private VirtualJoystick _leftStick;

        public override void OnAddedToEntity()
        {
            _leftStick = new VirtualJoystick(false);
            _leftStick.AddKeyboardKeys(OverlapBehavior.CancelOut,
                Keys.A, Keys.D, Keys.W, Keys.S);
        }
        public override void Update()
        {
            MoveInput = _leftStick.Value;
            Sprint = Input.IsKeyDown(Keys.LeftShift);
        }
    }
}
