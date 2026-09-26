using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Apos.Gui {
    public class Checkbox(long id, bool isChecked) : Component(id) {
        public Interaction Interaction { get; } = new();
        public bool Clicked => Interaction.Clicked;
        public bool IsChecked { get; set; } = isChecked;
        public override bool IsFocusable { get; set; } = true;
        /// <summary>Drawn next to the box. Pressing it works the same as pressing the box. Empty shows only the box.</summary>
        public string Text { get; set; } = "";
        public int FontSize { get; set; } = 30;
        /// <summary>Where the box is, in UI coordinates. The text goes to its right.</summary>
        public RectangleF Box => new(X, Y + (Height - BoxSize) / 2f, BoxSize, BoxSize);
        /// <summary>Where the text starts, in UI coordinates.</summary>
        public Vector2 TextPosition => new(X + BoxSize + TextGap, Y + (Height - GuiHelper.MeasureString(Text, FontSize).Y) / 2f);
        /// <summary>Draws this checkbox. Null uses <see cref="DefaultLook"/>.</summary>
        public Action<Checkbox>? Look { get; set; }
        /// <summary>Draws every checkbox that doesn't have its own <see cref="Look"/>.</summary>
        public static Action<Checkbox> DefaultLook { get; set; } = DrawDefault;

        public override void UpdateInput(GameTime gameTime) {
            Interaction.UpdateInput(this);
            if (Clicked) {
                IsChecked = !IsChecked;
            }
        }

        public override void UpdatePrefSize(GameTime gameTime) {
            if (Text.Length == 0) {
                PrefWidth = BoxSize;
                PrefHeight = BoxSize;
            } else {
                Vector2 text = GuiHelper.MeasureString(Text, FontSize);
                PrefWidth = BoxSize + TextGap + text.X;
                PrefHeight = MathF.Max(BoxSize, text.Y);
            }
        }

        public override void Draw(GameTime gameTime) {
            (Look ?? DefaultLook)(this);
        }

        /// <summary>A box in a box, and the text. The inner box fills when checked.</summary>
        public static void DrawDefault(Checkbox c) {
            RectangleF box = c.Box;
            Vector2 boxSize = new(box.Width, box.Height);
            Vector2 innerXY = box.Position + new Vector2(6f);
            Vector2 innerSize = boxSize - new Vector2(12f);
            Color border = c.IsFocused ? Theme.Focus : Theme.Border;

            if (c.IsChecked) {
                GuiHelper.ShapeBatch.FillRectangle(innerXY, innerSize, Theme.Focus, Theme.Rounding);
            }
            float highlight = c.Clicked ? 0.5f : 0.08f * c.Interaction.Hover + 0.15f * c.Interaction.Press;
            if (highlight > 0f) {
                GuiHelper.ShapeBatch.FillRectangle(box.Position, boxSize, new Color(Theme.Highlight, highlight), Theme.Rounding);
            }
            GuiHelper.ShapeBatch.BorderRectangle(box.Position, boxSize, border, Theme.BorderThickness, Theme.Rounding);
            GuiHelper.ShapeBatch.BorderRectangle(innerXY, innerSize, border, Theme.BorderThickness, Theme.Rounding);
            if (c.Text.Length > 0) {
                GuiHelper.ShapeBatch.DrawString(GuiHelper.Font, c.Text, c.TextPosition, c.FontSize, Theme.Text);
            }
        }

        protected const float BoxSize = 30f;
        protected const float TextGap = 10f;

        public static Checkbox Put(ref bool isChecked, [CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            return Put(ref isChecked, "", 30, id, isAbsoluteId, file);
        }
        /// <summary>A checkbox with text next to it. Pressing the text toggles it too.</summary>
        public static Checkbox Put(ref bool isChecked, string text, int fontSize = 30, [CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            bool initial = isChecked;
            Checkbox a = IMGUI.Put(id, isAbsoluteId, file, id => new Checkbox(id, initial));
            a.Text = text;
            a.FontSize = fontSize;
            if (a.IsFocused) {
                isChecked = a.IsChecked;
            } else {
                a.IsChecked = isChecked;
            }

            return a;
        }
    }
}
