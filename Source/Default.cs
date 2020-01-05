using System;
using Apos.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Apos.Gui {
    /// <summary>
    /// Predefined inputs for Mouse, Keyboard, Gamepad, Touchscreen.
    /// </summary>
    public static class Default {

        // Group: Public Variables

        /// <returns>Returns true when the mouse is inside the clip area of a component.</returns>
        public static Func<Component, bool> ConditionHoverMouse = c => c.IsInsideClip(GuiHelper.MouseToUI());
        /// <returns>Returns true when a component just got hovered.</returns>
        public static Func<Component, bool> ConditionGotHovered = c => !c.OldIsHovered && c.IsHovered;
        /// <returns>
        /// Returns true when gamepad 0's A button or space or enter or left mouse button are released.
        /// The left mouse button requires that the component is hovered.
        /// </returns>
        public static Func<Component, bool> ConditionInteraction = c =>
            c.HasFocus && _buttonInteraction.Released() ||
            c.IsHovered && _mouseInteraction.Released();
        /// <returns>
        /// Returns true when gamepad 0's left thumbstick has just been made positive or the up arrow key is released.
        /// </returns>
        public static Func<bool> ConditionPreviousFocus = () =>
            InputHelper.OldGamePad[0].ThumbSticks.Left.Y <= 0 && InputHelper.NewGamePad[0].ThumbSticks.Left.Y > 0 ||
            _buttonPreviousFocus.Released();
        /// <returns>
        /// Returns true when gamepad 0's left thumbstick has just been made negative or the down arrow key is released.
        /// </returns>
        public static Func<bool> ConditionNextFocus = () =>
            InputHelper.OldGamePad[0].ThumbSticks.Left.Y >= 0 && InputHelper.NewGamePad[0].ThumbSticks.Left.Y < 0 ||
            _buttonNextFocus.Released();
        /// <returns>
        /// Returns true when gamepad 0's B button is released or the escape key is released.
        /// </returns>
        public static Func<bool> ConditionBackFocus = () =>
            _buttonBackFocus.Released();
        /// <returns>
        /// Always returns true. This is useful when you want a condition to mark an input as used.
        /// When an input is marked as used, the component will request to be put in focus.
        /// </returns>
        public static Func<Component, bool> ConsumeCondition = c => true;
        /// <returns>Returns true when a component is hovered and the mouse wheel is being scrolled.</returns>
        public static Func<Component, bool> IsScrolled = c => {
            return c.IsHovered && GuiHelper.ScrollWheelDelta != 0;
        };
        /// <returns>This should be strictly used on a panel so that it can be scrolled vertically.</returns>
        /// <seealso cref="IsScrolled"/>
        public static Func<Component, bool> ScrollVertically = c => {
            Panel p = (Panel)c;
            int scrollWheelDelta = GuiHelper.ScrollWheelDelta;
            p.Offset = new Point(p.Offset.X, (int)Math.Min(Math.Max(p.Offset.Y + scrollWheelDelta, p.ClippingRect.Height - p.Size.Height), 0));

            return true;
        };
        /// <returns>This should be strictly used on a panel so that it can be scrolled horizontally.</returns>
        /// <seealso cref="IsScrolled"/>
        public static Func<Component, bool> ScrollHorizontally = c => {
            Panel p = (Panel)c;
            int scrollWheelDelta = GuiHelper.ScrollWheelDelta;
            p.Offset = new Point((int)Math.Min(Math.Max(p.Offset.X + scrollWheelDelta, p.ClippingRect.Width - p.Size.Width), 0), p.Offset.Y);

            return true;
        };

        // Group: Public Functions

        /// <summary>
        /// Creates a button with a label that becomes white on hover.
        /// The button can be interacted with using gamepad 0, keyboard and mouse.
        /// Adds a border of size 20 around the label.
        /// </summary>
        /// <param name="t">The string to use for the label.</param>
        /// <param name="operation">The action that the button does when interacted with.</param>
        /// <param name="grabFocus">A way for the component to request focus.</param>
        /// <returns>Returns the button that was created.</returns>
        public static Component CreateButton(string t, Func<Component, bool> operation, Action<Component> grabFocus) {
            Label l = new Label(t);
            l.ActiveColor = Color.White;
            l.NormalColor = new Color(150, 150, 150);

            return CreateButton(l, operation, grabFocus);
        }
        /// <summary>
        /// Creates a button with a dynamic label that becomes white on hover.
        /// The button can be interacted with using gamepad 0, keyboard and mouse.
        /// Adds a border of size 20 around the label.
        /// </summary>
        /// <param name="ld">A function that returns a string.</param>
        /// <param name="operation">The action that the button does when interacted with.</param>
        /// <param name="grabFocus">A way for the component to request focus.</param>
        /// <returns>Returns the button that was created.</returns>
        public static Component CreateButton(Func<string> ld, Func<Component, bool> operation, Action<Component> grabFocus) {
            LabelDynamic l = new LabelDynamic(ld);
            l.ActiveColor = Color.White;
            l.NormalColor = new Color(150, 150, 150);

            return CreateButton(l, operation, grabFocus);
        }
        /// <summary>
        /// Creates a button with a custom component.
        /// The button can be interacted with using gamepad 0, keyboard and mouse.
        /// Adds a border of size 20 around the component.
        /// </summary>
        /// <param name="c">The component to give to the button.</param>
        /// <param name="operation">The action that the button does when interacted with.</param>
        /// <param name="grabFocus">A way for the component to request focus.</param>
        /// <returns></returns>
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

        // Group: Private Variables

        private static ConditionComposite _buttonInteraction =
            new ConditionComposite(
                new ConditionSet(
                    new ConditionKeyboard(Keys.Space),
                    new ConditionKeyboard(Keys.Enter),
                    new ConditionGamePad(GamePadButton.A, 0)
                )
            );
        private static ConditionComposite _mouseInteraction =
            new ConditionComposite(
                new ConditionSet(
                    new ConditionMouse(MouseButton.LeftButton)
                )
            );
        private static ConditionComposite _buttonPreviousFocus =
            new ConditionComposite(
                new ConditionSet(
                    new ConditionKeyboard(Keys.Up)
                )
            );
        private static ConditionComposite _buttonNextFocus =
            new ConditionComposite(
                new ConditionSet(
                    new ConditionKeyboard(Keys.Down)
                )
            );
        private static ConditionComposite _buttonBackFocus =
            new ConditionComposite(
                new ConditionSet(
                    new ConditionKeyboard(Keys.Escape),
                    new ConditionGamePad(GamePadButton.B, 0)
                )
            );
    }
}