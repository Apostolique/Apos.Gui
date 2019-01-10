using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;

namespace AposGui {
    //This will get refactored into it's own separate package most likely.
    public static class Input {
        private static MouseState _oldMouse;
        private static MouseState _newMouse;
        private static KeyboardState _oldKeyboard;
        private static KeyboardState _newKeyboard;
        private static TouchCollection _oldTouchCollection;
        private static TouchCollection _newTouchCollection;
        private static TouchPanelCapabilities _touchPanelCapabilities;
        private static GamePadState[] _oldGamePad;
        private static GamePadState[] _newGamepad;
        private static GamePadCapabilities[] _gamePadCapabilities;

        public static MouseState OldMouse => _oldMouse;
        public static MouseState NewMouse => _newMouse;
        public static KeyboardState OldKeyboard => _oldKeyboard;
        public static KeyboardState NewKeyboard => _newKeyboard;
        public static TouchCollection OldTouchCollection => _oldTouchCollection;
        public static TouchCollection NewTouchCollection => _newTouchCollection;
        public static TouchPanelCapabilities TouchPanelCapabilities => _touchPanelCapabilities;
        public static GamePadState[] OldGamePad => _oldGamePad;
        public static GamePadState[] NewGamePad => _newGamepad;
        public static GamePadCapabilities[] GamePadCapabilities => _gamePadCapabilities;

        private static bool _initiated = false;

        public static void Setup() {
            _newMouse = Mouse.GetState();
            _newKeyboard = Keyboard.GetState();
            TouchPanel.GetCapabilities();

            _newGamepad = new GamePadState[GamePad.MaximumGamePadCount];
            for (int i = 0; i < GamePad.MaximumGamePadCount; i++) {
                _newGamepad[i] = GamePad.GetState(i);
            }

            _newTouchCollection = TouchPanel.GetState();

            _initiated = true;
        }
        public static void Update() {
            if (!_initiated) {
                Setup();
            }

            _oldMouse = _newMouse;
            _oldKeyboard = _newKeyboard;
            _oldGamePad = _newGamepad;
            _oldTouchCollection = _newTouchCollection;

            _newMouse = Mouse.GetState();
            _newKeyboard = Keyboard.GetState();
            _newGamepad = new GamePadState[GamePad.MaximumGamePadCount];
            _newTouchCollection = TouchPanel.GetState();
            _touchPanelCapabilities = TouchPanel.GetCapabilities();
            _gamePadCapabilities = new GamePadCapabilities[GamePad.MaximumGamePadCount];
            for (int i = 0; i < GamePad.MaximumGamePadCount; i++) {
                _newGamepad[i] = GamePad.GetState(i);
            }
            for (int i = 0; i < GamePad.MaximumGamePadCount; i++) {
                _gamePadCapabilities[i] = GamePad.GetCapabilities(i);
            }
        }
    }
}