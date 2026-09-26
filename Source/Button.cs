using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    public class Button(long id) : SingleParent(id) {
        public Interaction Interaction { get; } = new();
        public bool Clicked => Interaction.Clicked;
        public override bool IsFocusable { get; set; } = true;
        /// <summary>Draws this button under its child. Null uses <see cref="DefaultLook"/>.</summary>
        public Action<Button>? Look { get; set; }
        /// <summary>Draws every button that doesn't have its own <see cref="Look"/>.</summary>
        public static Action<Button> DefaultLook { get; set; } = DrawDefault;

        public override void UpdateInput(GameTime gameTime) {
            base.UpdateInput(gameTime);
            Interaction.UpdateInput(this);
        }

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

        public static Button Put(string text, int fontSize = 30, Color? color = null, [CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            Button b = Put(id, isAbsoluteId, file);
            Label.Put(text, fontSize: fontSize, color: color);

            return b;
        }
        public static Button Put([CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            return IMGUI.Push(id, isAbsoluteId, file, id => new Button(id), 1);
        }
    }
}
