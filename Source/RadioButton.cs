using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Apos.Gui {
    /// <summary>
    /// One choice out of a group. Every radio button in a group shares the same selected value, and the one holding its own value is checked.
    /// </summary>
    public class RadioButton(long id) : Component(id) {
        public Interaction Interaction { get; } = new();
        public bool Clicked => Interaction.Clicked;
        public bool IsChecked { get; set; }
        public override bool IsFocusable { get; set; } = true;
        /// <summary>Drawn next to the box. Pressing it works the same as pressing the box. Empty shows only the box.</summary>
        public string Text { get; set; } = "";
        public int FontSize { get; set; } = 30;
        /// <summary>Where the box is, in UI coordinates. The text goes to its right.</summary>
        public RectangleF Box => new(X, Y + (Height - BoxSize) / 2f, BoxSize, BoxSize);
        /// <summary>Where the text starts, in UI coordinates.</summary>
        public Vector2 TextPosition => new(X + BoxSize + TextGap, Y + (Height - GuiHelper.MeasureString(Text, FontSize).Y) / 2f);
        /// <summary>Draws this radio button. Null uses <see cref="DefaultLook"/>.</summary>
        public Action<RadioButton>? Look { get; set; }
        /// <summary>Draws every radio button that doesn't have its own <see cref="Look"/>.</summary>
        public static Action<RadioButton> DefaultLook { get; set; } = DrawDefault;

        public override void UpdateInput(GameTime gameTime) {
            Interaction.UpdateInput(this);
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

        /// <summary>A ring with a dot in it when checked, and the text.</summary>
        public static void DrawDefault(RadioButton r) {
            RectangleF box = r.Box;
            Vector2 center = box.Center;
            float radius = box.Width / 2f;

            float highlight = r.Clicked ? 0.5f : 0.08f * r.Interaction.Hover + 0.15f * r.Interaction.Press;
            if (highlight > 0f) {
                GuiHelper.ShapeBatch.FillCircle(center, radius, new Color(Theme.Highlight, highlight));
            }
            if (Theme.Surface.A > 0) {
                GuiHelper.ShapeBatch.FillCircle(center, radius, Theme.Surface);
            }
            if (r.IsChecked) {
                GuiHelper.ShapeBatch.FillCircle(center, radius - 7f, Theme.Accent);
            }
            GuiHelper.ShapeBatch.BorderCircle(center, radius, r.IsFocused ? Theme.Focus : Theme.Border, Theme.BorderThickness);
            if (r.Text.Length > 0) {
                GuiHelper.ShapeBatch.DrawString(GuiHelper.Font, r.Text, r.TextPosition, r.FontSize, Theme.Text);
            }
        }

        protected const float BoxSize = 30f;
        protected const float TextGap = 10f;

        /// <param name="selected">The group's selected value. Becomes value when this radio button is clicked.</param>
        /// <param name="value">The value this radio button stands for.</param>
        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="file">The caller's file. The same line in two files gives two different ids.</param>
        public static RadioButton Put(ref int selected, int value, [CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            return Put(ref selected, value, "", 30, id, isAbsoluteId, file);
        }
        /// <summary>A radio button with text next to it. Pressing the text picks it too.</summary>
        /// <param name="selected">The group's selected value. Becomes value when this radio button is clicked.</param>
        /// <param name="value">The value this radio button stands for.</param>
        /// <param name="text">Drawn next to the ring.</param>
        /// <param name="fontSize">Size of the text.</param>
        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="file">The caller's file. The same line in two files gives two different ids.</param>
        public static RadioButton Put(ref int selected, int value, string text, int fontSize = 30, [CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            RadioButton a = IMGUI.Put(id, isAbsoluteId, file, id => new RadioButton(id));
            a.Text = text;
            a.FontSize = fontSize;
            if (a.Clicked) {
                selected = value;
            }
            a.IsChecked = selected == value;

            return a;
        }
    }
}
