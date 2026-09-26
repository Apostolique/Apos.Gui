using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    public class Button(int id) : SingleParent(id) {
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
            float highlight = b.Clicked ? 0.5f : 0.08f * b.Interaction.Hover + 0.15f * b.Interaction.Press;
            if (highlight > 0f) {
                GuiHelper.ShapeBatch.FillRectangle(b.XY, b.Size, new Color(Theme.Highlight, highlight), Theme.Rounding);
            }
            GuiHelper.ShapeBatch.BorderRectangle(b.XY, b.Size, b.IsFocused ? Theme.Focus : Theme.Border, Theme.BorderThickness, Theme.Rounding);
        }

        public static Button Put(string text, int fontSize = 30, Color? color = null, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            Button b = Put(id, isAbsoluteId);
            Label.Put(text, fontSize: fontSize, color: color, id: id, isAbsoluteId: isAbsoluteId);

            return b;
        }
        public static Button Put([CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            return IMGUI.Push(id, isAbsoluteId, id => new Button(id), 1);
        }
    }
}
