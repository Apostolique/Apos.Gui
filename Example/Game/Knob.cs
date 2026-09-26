using System;
using System.Runtime.CompilerServices;
using Apos.Gui;
using Microsoft.Xna.Framework;

namespace GameProject {
    /// <summary>
    /// A dial that turns when you drag it up and down, or with left and right while it has focus.
    /// It's built the same way as the components that come with Apos.Gui, so you can copy it to start your own.
    /// </summary>
    public class Knob(long id) : Component(id) {
        /// <summary>From 0 to 1.</summary>
        public float Value {
            get => _value;
            set => _value = MathHelper.Clamp(value, 0f, 1f);
        }
        /// <summary>Hover, press, drag and focus, from the mouse, touch or a gamepad.</summary>
        public Interaction Interaction { get; } = new();
        public override bool IsFocusable { get; set; } = true;

        public override void UpdateInput(GameTime gameTime) {
            Interaction.UpdateInput(this);

            if (Interaction.Pressed && Interaction.Source != InteractionSource.Button) {
                // Dragging up turns it right. 200 UI units go from one end to the other.
                Value -= Interaction.Delta.Y / 200f;
            }
            // Checked while focused so that left and right turn the knob instead of moving the focus.
            if (IsFocused) {
                if (Root.Bindings.MoveLeft.Pressed()) Value -= 0.05f;
                if (Root.Bindings.MoveRight.Pressed()) Value += 0.05f;
            }
        }

        public override void UpdatePrefSize(GameTime gameTime) {
            PrefWidth = 70f;
            PrefHeight = 70f;
        }

        public override void Draw(GameTime gameTime) {
            Vector2 center = XY + Size / 2f;
            float radius = MathF.Min(Width, Height) / 2f - Theme.BorderThickness;
            // From 135 degrees left of up to 135 degrees right of it.
            float angle = -MathF.PI / 2f + (Value * 2f - 1f) * MathF.PI * 0.75f;
            Vector2 tip = center + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * radius * 0.75f;

            float highlight = 0.08f * Interaction.Hover + 0.15f * Interaction.Press;
            if (Theme.Surface.A > 0) {
                GuiHelper.ShapeBatch.FillCircle(center, radius, Theme.Surface);
            }
            GuiHelper.ShapeBatch.FillCircle(center, radius, new Color(Theme.Highlight, highlight));
            GuiHelper.ShapeBatch.BorderCircle(center, radius, IsFocused ? Theme.Focus : Theme.Border, Theme.BorderThickness);
            GuiHelper.ShapeBatch.FillLine(center, tip, 3f, Theme.Accent);
        }

        private float _value;

        /// <param name="value">From 0 to 1. Changes when the knob turns.</param>
        public static Knob Put(ref float value, [CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            Knob k = IMGUI.Put(id, isAbsoluteId, file, id => new Knob(id));
            // While it has focus the knob owns the value, otherwise the game does.
            if (k.IsFocused) {
                value = k.Value;
            } else {
                k.Value = value;
            }
            return k;
        }
    }
}
