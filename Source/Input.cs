using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace AposGui
{
    //This will get refactored into it's own separate package most likely.
    public static class Input
    {
        private static MouseState _oldMouse;
        private static MouseState _newMouse;
        private static KeyboardState _oldKeyboard;
        private static KeyboardState _newKeyboard;
        private static GamePadState[] _oldGamePad;
        private static GamePadState[] _newGamepad;
        private static GamePadCapabilities _capabilities;

        public static MouseState OldMouse => _oldMouse;
        public static MouseState NewMouse => _newMouse;
        public static KeyboardState OldKeyboard => _oldKeyboard;
        public static KeyboardState NewKeyboard => _newKeyboard;
        public static GamePadState[] OldGamePad => _oldGamePad;
        public static GamePadState[] NewGamePad => _newGamepad;
        public static GamePadCapabilities Capabilities => _capabilities;

        private static bool _initiated = false;

        public static void Setup() {
            _newMouse = Mouse.GetState();
            _newKeyboard = Keyboard.GetState();

            _newGamepad = new GamePadState[GamePad.MaximumGamePadCount];
            for (int i = 0; i < GamePad.MaximumGamePadCount; i++) {
                _newGamepad[i] = GamePad.GetState(i);
            }

            _initiated = true;
        }
        public static void Update() {
            if (!_initiated) {
                Setup();
            }

            _oldMouse = _newMouse;
            _oldKeyboard = _newKeyboard;
            _oldGamePad = _newGamepad;

            _newMouse = Mouse.GetState();
            _newKeyboard = Keyboard.GetState();
            _newGamepad = new GamePadState[GamePad.MaximumGamePadCount];
            for (int i = 0; i < GamePad.MaximumGamePadCount; i++) {
                _newGamepad[i] = GamePad.GetState(i);
            }
            _capabilities = GamePad.GetCapabilities(PlayerIndex.One);
        }
    }
}