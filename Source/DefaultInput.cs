using System;
using Microsoft.Xna.Framework.Input;

namespace AposGui {
    /// <summary>
    /// Goal: Predefined inputs for Mouse, Keyboard, Gamepad, Touchscreen.
    /// </summary>
    public static class DefaultInput {
        //public vars
        public static Func<Component, bool> ConditionHoverMouse = (Component b) => b.IsInsideClip(GuiHelper.MouseToUI());
        public static Func<Component, bool> ConditionGotHovered = (Component b) => !b.OldIsHovered && b.IsHovered;
        public static Func<Component, bool> ConditionInteraction = (Component b) =>
                b.HasFocus && (ButtonReleased(Input.OldGamePad[0].Buttons.A, Input.NewGamePad[0].Buttons.A) ||
                               KeyReleased(Keys.Space) || KeyReleased(Keys.Enter)) ||
                b.IsHovered && ButtonReleased(Input.OldMouse.LeftButton, Input.NewMouse.LeftButton);

        public static Func<Keys, bool> KeyReleased = (key) =>
            Input.OldKeyboard.IsKeyDown(key) && Input.NewKeyboard.IsKeyUp(key);
        public static Func<ButtonState, ButtonState, bool> ButtonReleased = (oldButtonState, newButtonState) =>
            oldButtonState == ButtonState.Pressed && newButtonState == ButtonState.Released;

        //public functions

        //private vars

        //private functions
    }
}