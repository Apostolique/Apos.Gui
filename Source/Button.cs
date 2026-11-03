using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    /// <summary>A clickable component that holds one child, usually a label.</summary>
    public class Button(long id) : SingleParent(id) {
        /// <summary>Hover, press and click for this button.</summary>
        public Interaction Interaction { get; } = new();
        /// <summary>True on the frame the button is clicked.</summary>
        public bool Clicked => Interaction.Clicked;
        /// <inheritdoc/>
        public override bool IsFocusable { get; set; } = true;
        /// <summary>Draws this button under its child. Null uses <see cref="DefaultLook"/>.</summary>
        public Action<Button>? Look { get; set; }
        /// <summary>Draws every button that doesn't have its own <see cref="Look"/>.</summary>
        public static Action<Button> DefaultLook { get; set; } = DrawDefault;

        /// <inheritdoc/>
        public override void UpdateInput(GameTime gameTime) {
            base.UpdateInput(gameTime);
            Interaction.UpdateInput(this);
        }

        /// <inheritdoc/>
        public override void Draw(GameTime gameTime) {
            (Look ?? DefaultLook)(this);
            base.Draw(gameTime);
        }

        /// <summary>A border that lights up with focus, and a highlight that follows hover and press.</summary>
        public static void DrawDefault(Button b) {
            DrawPressable(b, b.Interaction);
        }
        /// <summary>The highlight and border every built-in clickable component draws. Useful as a base for your own looks.</summary>
        public static void DrawPressable(IComponent c, Interaction i) {
            if (Theme.Surface.A > 0) {
                GuiHelper.ShapeBatch.FillRectangle(c.XY, c.Size, Theme.Surface, Theme.Rounding);
            }
            float highlight = i.Clicked ? 0.5f : 0.08f * i.Hover + 0.15f * i.Press;
            if (highlight > 0f) {
                GuiHelper.ShapeBatch.FillRectangle(c.XY, c.Size, new Color(Theme.Highlight, highlight), Theme.Rounding);
            }
            GuiHelper.ShapeBatch.BorderRectangle(c.XY, c.Size, c.IsFocused ? Theme.Focus : Theme.Border, Theme.BorderThickness, Theme.Rounding);
        }

        /// <summary>A button with a label in it.</summary>
        /// <param name="text">Drawn in the button.</param>
        /// <param name="fontSize">Size of the text.</param>
        /// <param name="color">Color of the text. Null uses <see cref="Theme.Text"/>.</param>
        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="file">The caller's file. The same line in two files gives two different ids.</param>
        public static Button Put(string text, int fontSize = 30, Color? color = null, [CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            Button b = Put(id, isAbsoluteId, file);
            Label.Put(text, fontSize: fontSize, color: color);

            return b;
        }
        /// <summary>A button without content. The next component put becomes its child.</summary>
        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="file">The caller's file. The same line in two files gives two different ids.</param>
        public static Button Put([CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            return IMGUI.Push(id, isAbsoluteId, file, id => new Button(id), 1);
        }
    }
}
