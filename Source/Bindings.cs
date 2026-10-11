using System.Collections.Generic;
using Apos.Input;
using Microsoft.Xna.Framework.Input;
using Track = Apos.Input.Track;

namespace Apos.Gui {
    /// <summary>
    /// The inputs a UI listens to. Each <see cref="IMGUI"/> has its own, so split screen players can each drive their own UI.
    /// Any property can be replaced with your own condition.
    /// </summary>
    public class Bindings {
        /// <summary>Keyboard, mouse, touch and every gamepad.</summary>
        public Bindings() : this(null, true) { }
        private Bindings(int? gamePad, bool usesGamePads) {
            _gamePad = gamePad;
            _usesGamePads = usesGamePads;
            bool desktop = gamePad == null;

            MouseInteraction = new Track.MouseCondition(MouseButton.LeftButton);
            ButtonInteraction = Any(Key(Keys.Space), Key(Keys.Enter), Pad(GamePadButton.A));
            FocusPrev = Any(desktop ? new AllCondition(Shift(), new Track.KeyboardCondition(Keys.Tab)) : null);
            FocusNext = Any(Key(Keys.Tab));
            MoveLeft = Repeat(Any(Key(Keys.Left), Pad(GamePadButton.Left)));
            MoveRight = Repeat(Any(Key(Keys.Right), Pad(GamePadButton.Right)));
            MoveUp = Repeat(Any(Key(Keys.Up), Pad(GamePadButton.Up)));
            MoveDown = Repeat(Any(Key(Keys.Down), Pad(GamePadButton.Down)));
            OpenKeyboard = Any(Pad(GamePadButton.A));
            Back = Any(Key(Keys.Escape), Pad(GamePadButton.B), Pad(GamePadButton.Back), desktop ? new Track.MouseCondition(MouseButton.XButton2) : null);

            ExtendSelection = Shift();
            ByWord = Any(new KeyboardCondition(Keys.LeftControl), new KeyboardCondition(Keys.RightControl));
            LineStart = new Track.KeyboardCondition(Keys.Home);
            LineEnd = new Track.KeyboardCondition(Keys.End);
            SelectAll = Shortcut(Keys.A);
            Copy = Shortcut(Keys.C);
            Cut = Shortcut(Keys.X);
            Paste = Shortcut(Keys.V);
        }

        /// <summary>
        /// Only the given gamepad. The keyboard, mouse and touch don't reach the UI.
        /// For split screen, where each player has a UI.
        /// </summary>
        /// <param name="index">The gamepad's index, from 0.</param>
        public static Bindings ForGamePad(int index) => new(index, true) { UsesPointer = false };
        /// <summary>
        /// Keyboard, mouse and touch, without gamepads.
        /// For a UI that shares the screen with split screen UIs, so it doesn't take their gamepads' presses.
        /// </summary>
        public static Bindings ForKeyboardAndMouse() => new(null, false);

        /// <summary>Whether the mouse and touch can hover and press components in this UI.</summary>
        public bool UsesPointer { get; set; } = true;

        public ICondition MouseInteraction { get; set; }
        public ICondition ButtonInteraction { get; set; }
        /// <summary>Moves the focus to the previous component in the order they were put.</summary>
        public ICondition FocusPrev { get; set; }
        /// <summary>Moves the focus to the next component in the order they were put.</summary>
        public ICondition FocusNext { get; set; }
        /// <summary>
        /// Moves the focus, or whatever the focused component does with it, like a slider or a textbox's cursor.
        /// Repeats while held. Wrap your own condition in a <see cref="RepeatCondition"/> to keep that.
        /// </summary>
        public ICondition MoveLeft { get; set; }
        /// <summary>Repeats while held. Wrap your own condition in a <see cref="RepeatCondition"/> to keep that.</summary>
        public ICondition MoveRight { get; set; }
        /// <summary>Repeats while held. Wrap your own condition in a <see cref="RepeatCondition"/> to keep that.</summary>
        public ICondition MoveUp { get; set; }
        /// <summary>Repeats while held. Wrap your own condition in a <see cref="RepeatCondition"/> to keep that.</summary>
        public ICondition MoveDown { get; set; }
        public ICondition Back { get; set; }
        /// <summary>Opens <see cref="GuiHelper.OnScreenKeyboard"/> on the focused textbox.</summary>
        public ICondition OpenKeyboard { get; set; }

        /// <summary>Held to grow the selection while moving the cursor in a textbox.</summary>
        public ICondition ExtendSelection { get; set; }
        /// <summary>Held to move or delete a word at a time in a textbox.</summary>
        public ICondition ByWord { get; set; }
        public ICondition LineStart { get; set; }
        public ICondition LineEnd { get; set; }
        public ICondition SelectAll { get; set; }
        public ICondition Copy { get; set; }
        public ICondition Cut { get; set; }
        public ICondition Paste { get; set; }

        private ICondition? Key(Keys key) => _gamePad == null ? new Track.KeyboardCondition(key) : null;
        private ICondition? Pad(GamePadButton button) {
            if (!_usesGamePads) {
                return null;
            }
            if (_gamePad is int index) {
                return new Track.GamePadCondition(button, index);
            }
            var pads = new ICondition[GamePad.MaximumGamePadCount];
            for (int i = 0; i < pads.Length; i++) {
                pads[i] = new Track.GamePadCondition(button, i);
            }
            return new AnyCondition(pads);
        }
        private static ICondition Any(params ICondition?[] conditions) {
            var result = new List<ICondition>();
            foreach (ICondition? c in conditions) {
                if (c != null) result.Add(c);
            }
            return new AnyCondition([.. result]);
        }
        private static ICondition Repeat(ICondition c) => new RepeatCondition(c, 400, 50);
        private static ICondition Shift() => new AnyCondition(new KeyboardCondition(Keys.LeftShift), new KeyboardCondition(Keys.RightShift));
        private static ICondition Shortcut(Keys key) =>
            new AllCondition(
                new AnyCondition(
                    new KeyboardCondition(Keys.LeftControl),
                    new KeyboardCondition(Keys.RightControl)
                ),
                new Track.KeyboardCondition(key)
            );

        private readonly int? _gamePad;
        private readonly bool _usesGamePads;
    }
}
