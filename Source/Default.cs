using System;
using Microsoft.Xna.Framework.Input;
using Apos.Input;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    /// <summary>
    /// Goal: Predefined inputs for Mouse, Keyboard, Gamepad, Touchscreen.
    /// </summary>
    public static class Default {
        // Group: Public Variables
        public static Func<Component, bool> ConditionHoverMouse = (Component b) => b.IsInsideClip(GuiHelper.MouseToUI());
        public static Func<Component, bool> ConditionGotHovered = (Component b) => !b.OldIsHovered && b.IsHovered;
        public static Func<Component, bool> ConditionInteraction = (Component b) =>
            b.HasFocus && (buttonReleased(s => s[0].Buttons.A) ||
                           buttonReleased(Keys.Space) || buttonReleased(Keys.Enter)) ||
            b.IsHovered && buttonReleased(s => s.LeftButton);
        public static Func<bool> ConditionPreviousFocus = () =>
            InputHelper.OldGamePad[0].ThumbSticks.Left.Y <= 0 && InputHelper.NewGamePad[0].ThumbSticks.Left.Y > 0 ||
            buttonReleased(Keys.Up);
        public static Func<bool> ConditionNextFocus = () =>
            InputHelper.OldGamePad[0].ThumbSticks.Left.Y >= 0 && InputHelper.NewGamePad[0].ThumbSticks.Left.Y < 0 ||
            buttonReleased(Keys.Down);
        public static Func<bool> ConditionBackFocus = () =>
            buttonReleased(s => s[0].Buttons.B) ||
            buttonReleased(Keys.Escape);
        public static Func<Component, bool> ConsumeCondition = (Component c) => true;

        public static Func<Component, bool> IsScrolled = (Component b) => {
            return b.IsHovered && GuiHelper.ScrollWheelDelta() != 0;
        };
        public static Func<Component, bool> ScrollVertically = (Component b) => {
            Panel p = (Panel)b;
            int scrollWheelDelta = GuiHelper.ScrollWheelDelta();
            p.Offset = new Point(p.Offset.X, (int)Math.Min(Math.Max(p.Offset.Y + scrollWheelDelta, p.ClippingRect.Height - p.Size.Height), 0));

            return true;
        };
        public static Func<Component, bool> ScrollHorizontally = (Component b) => {
            Panel p = (Panel)b;
            int scrollWheelDelta = GuiHelper.ScrollWheelDelta();
            p.Offset = new Point((int)Math.Min(Math.Max(p.Offset.X + scrollWheelDelta, p.ClippingRect.Width - p.Size.Width), 0), p.Offset.Y);

            return true;
        };

        // Group: Public Functions
        public static Component CreateButton(string text, Func<Component, bool> operation, Action<Component> grabFocus) {
            Label l = new Label(text);
            l.ActiveColor = Color.White;
            l.NormalColor = new Color(150, 150, 150);

            return CreateButton(l, operation, grabFocus);
        }
        public static Component CreateButton(Func<string> text, Func<Component, bool> operation, Action<Component> grabFocus) {
            LabelDynamic l = new LabelDynamic(text);
            l.ActiveColor = Color.White;
            l.NormalColor = new Color(150, 150, 150);

            return CreateButton(l, operation, grabFocus);
        }
        public static Component CreateButton(Component c, Func<Component, bool> operation, Action<Component> grabFocus) {
            Border border = new Border(c, 20, 20, 20, 20);
            Button b = new Button(border);
            b.ShowBox = false;
            b.GrabFocus = grabFocus;
            b.AddHoverCondition(ConditionHoverMouse);
            b.AddAction(ConditionInteraction, operation);
            b.AddAction(ConditionGotHovered, ConsumeCondition);

            return b;
        }

        // Group: Private Functions
        private static bool buttonReleased(Keys key) {
            return InputHelper.IsActive && ActionKeyboard.Released(key);
        }
        private static bool buttonReleased(Func<MouseState, ButtonState> button) {
            return InputHelper.IsActive && ActionMouse.Released(button);
        }
        private static bool buttonReleased(Func<GamePadState[], ButtonState> button) {
            return InputHelper.IsActive && ActionGamePad.Released(button);
        }
    }
}