using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    /// <summary>
    /// One choice out of a group. Every radio button in a group shares the same selected value, and the one holding its own value is checked.
    /// </summary>
    public class RadioButton(long id) : Component(id) {
        public Interaction Interaction { get; } = new();
        public bool Clicked => Interaction.Clicked;
        public bool IsChecked { get; set; }
        public override bool IsFocusable { get; set; } = true;
        /// <summary>Draws this radio button. Null uses <see cref="DefaultLook"/>.</summary>
        public Action<RadioButton>? Look { get; set; }
        /// <summary>Draws every radio button that doesn't have its own <see cref="Look"/>.</summary>
        public static Action<RadioButton> DefaultLook { get; set; } = DrawDefault;

        public override void UpdateInput(GameTime gameTime) {
            Interaction.UpdateInput(this);
        }
        public override void UpdatePrefSize(GameTime gameTime) {
            PrefWidth = 30f;
            PrefHeight = 30f;
        }
        public override void Draw(GameTime gameTime) {
            (Look ?? DefaultLook)(this);
        }

        /// <summary>A ring with a dot in it when checked.</summary>
        public static void DrawDefault(RadioButton r) {
            Vector2 center = r.Bounds.Center;
            float radius = MathF.Min(r.Width, r.Height) / 2f;

            float highlight = r.Clicked ? 0.5f : 0.08f * r.Interaction.Hover + 0.15f * r.Interaction.Press;
            if (highlight > 0f) {
                GuiHelper.ShapeBatch.FillCircle(center, radius, new Color(Theme.Highlight, highlight));
            }
            if (r.IsChecked) {
                GuiHelper.ShapeBatch.FillCircle(center, radius - 7f, Theme.Focus);
            }
            GuiHelper.ShapeBatch.BorderCircle(center, radius, r.IsFocused ? Theme.Focus : Theme.Border, Theme.BorderThickness);
        }

        /// <param name="selected">The group's selected value. Becomes value when this radio button is clicked.</param>
        /// <param name="value">The value this radio button stands for.</param>
        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="file">The caller's file. The same line in two files gives two different ids.</param>
        public static RadioButton Put(ref int selected, int value, [CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            RadioButton a = IMGUI.Put(id, isAbsoluteId, file, id => new RadioButton(id));
            if (a.Clicked) {
                selected = value;
            }
            a.IsChecked = selected == value;

            return a;
        }
    }
}
