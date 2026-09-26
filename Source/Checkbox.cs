using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    public class Checkbox(int id, bool isChecked) : Component(id) {
        public Interaction Interaction { get; } = new();
        public bool Clicked => Interaction.Clicked;
        public bool IsChecked { get; set; } = isChecked;
        public override bool IsFocusable { get; set; } = true;
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
            PrefWidth = 30f;
            PrefHeight = 30f;
        }

        public override void Draw(GameTime gameTime) {
            (Look ?? DefaultLook)(this);
        }

        /// <summary>A box in a box. The inner one fills when checked.</summary>
        public static void DrawDefault(Checkbox c) {
            Vector2 innerXY = c.XY + new Vector2(6f);
            Vector2 innerSize = c.Size - new Vector2(12f);
            Color border = c.IsFocused ? Theme.Focus : Theme.Border;

            if (c.IsChecked) {
                GuiHelper.ShapeBatch.FillRectangle(innerXY, innerSize, Theme.Focus, Theme.Rounding);
            }
            float highlight = c.Clicked ? 0.5f : 0.08f * c.Interaction.Hover + 0.15f * c.Interaction.Press;
            if (highlight > 0f) {
                GuiHelper.ShapeBatch.FillRectangle(c.XY, c.Size, new Color(Theme.Highlight, highlight), Theme.Rounding);
            }
            GuiHelper.ShapeBatch.BorderRectangle(c.XY, c.Size, border, Theme.BorderThickness, Theme.Rounding);
            GuiHelper.ShapeBatch.BorderRectangle(innerXY, innerSize, border, Theme.BorderThickness, Theme.Rounding);
        }


        public static Checkbox Put(ref bool isChecked, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            bool initial = isChecked;
            Checkbox a = IMGUI.Put(id, isAbsoluteId, id => new Checkbox(id, initial));
            if (a.IsFocused) {
                isChecked = a.IsChecked;
            } else {
                a.IsChecked = isChecked;
            }

            return a;
        }
    }
}
