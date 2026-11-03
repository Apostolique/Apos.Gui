using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Apos.Gui {
    /// <summary>A box that flips between checked and unchecked when it's clicked.</summary>
    public class Checkbox(long id, bool isChecked) : Component(id) {
        /// <summary>Hover, press and click for this checkbox.</summary>
        public Interaction Interaction { get; } = new();
        /// <summary>True on the frame the checkbox is clicked.</summary>
        public bool Clicked => Interaction.Clicked;
        /// <summary>Whether the box is checked. Clicking flips it.</summary>
        public bool IsChecked { get; set; } = isChecked;
        /// <inheritdoc/>
        public override bool IsFocusable { get; set; } = true;
        /// <summary>Drawn next to the box. Pressing it works the same as pressing the box. Empty shows only the box.</summary>
        public string Text { get; set; } = "";
        /// <summary>Size of <see cref="Text"/>.</summary>
        public int FontSize { get; set; } = 30;
        /// <summary>Where the box is, in UI coordinates. The text goes to its right.</summary>
        public RectangleF Box => new(X, Y + (Height - BoxSize) / 2f, BoxSize, BoxSize);
        /// <summary>Where the text starts, in UI coordinates.</summary>
        public Vector2 TextPosition => new(X + BoxSize + TextGap, Y + (Height - GuiHelper.MeasureString(Text, FontSize).Y) / 2f);
        /// <summary>Draws this checkbox. Null uses <see cref="DefaultLook"/>.</summary>
        public Action<Checkbox>? Look { get; set; }
        /// <summary>Draws every checkbox that doesn't have its own <see cref="Look"/>.</summary>
        public static Action<Checkbox> DefaultLook { get; set; } = DrawDefault;

        /// <inheritdoc/>
        public override void UpdateInput(GameTime gameTime) {
            Interaction.UpdateInput(this);
            if (Clicked) {
                IsChecked = !IsChecked;
            }
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
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

            if (Theme.Surface.A > 0) {
                GuiHelper.ShapeBatch.FillRectangle(box.Position, boxSize, Theme.Surface, Theme.Rounding);
            }
            if (c.IsChecked) {
                GuiHelper.ShapeBatch.FillRectangle(innerXY, innerSize, Theme.Accent, MathF.Max(Theme.Rounding - 4f, 0f));
            }
            float highlight = c.Clicked ? 0.5f : 0.08f * c.Interaction.Hover + 0.15f * c.Interaction.Press;
            if (highlight > 0f) {
                GuiHelper.ShapeBatch.FillRectangle(box.Position, boxSize, new Color(Theme.Highlight, highlight), Theme.Rounding);
            }
            GuiHelper.ShapeBatch.BorderRectangle(box.Position, boxSize, border, Theme.BorderThickness, Theme.Rounding);
            if (Theme.Surface.A == 0) {
                GuiHelper.ShapeBatch.BorderRectangle(innerXY, innerSize, border, Theme.BorderThickness, MathF.Max(Theme.Rounding - 4f, 0f));
            }
            if (c.Text.Length > 0) {
                GuiHelper.ShapeBatch.DrawString(GuiHelper.Font, c.Text, c.TextPosition, c.FontSize, Theme.Text);
            }
        }

        /// <summary>Width and height of the box, in UI units.</summary>
        protected const float BoxSize = 30f;
        /// <summary>Space between the box and the text, in UI units.</summary>
        protected const float TextGap = 10f;

        /// <summary>A checkbox without text.</summary>
        /// <param name="isChecked">Your checked state. Changes when the checkbox is clicked.</param>
        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="file">The caller's file. The same line in two files gives two different ids.</param>
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
