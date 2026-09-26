using System;
using System.Runtime.CompilerServices;
using Apos.Input;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    public class Slider : Component {
        public Slider(long id, float value, float min, float max, float? step) : base(id) {
            Value = value;
            Min = min;
            Max = max;
            Step = step;
        }

        public float Value { get; set; }
        /// <summary>Where the value sits between Min and Max, from 0 to 1.</summary>
        public float Percent => Max == Min ? 0f : (Value - Min) / (Max - Min);
        public float Min { get; set; }
        public float Max { get; set; }
        public float? Step {
            get => _step ?? (Max - Min) / 100f;
            set {
                _step = value;
            }
        }
        public override bool IsFocusable { get; set; } = true;
        /// <summary>Draws this slider. Null uses <see cref="DefaultLook"/>.</summary>
        public Action<Slider>? Look { get; set; }
        /// <summary>Draws every slider that doesn't have its own <see cref="Look"/>.</summary>
        public static Action<Slider> DefaultLook { get; set; } = DrawDefault;
        public Interaction Interaction { get; } = new() { UsesFocusButton = false };

        public override void UpdateInput(GameTime gameTime) {
            Interaction.UpdateInput(this);

            if (Interaction.Pressed && Interaction.Source != InteractionSource.Button) {
                float percent = (Interaction.Position.X - Left) / (Width - _thickness);
                Value = MathHelper.Clamp(percent * (Max - Min) + Min, Min, Max);
            }
            if (IsFocused) {
                if (_left.Pressed()) Slide(-1);
                if (_right.Pressed()) Slide(1);
            }
        }

        public override void UpdatePrefSize(GameTime gameTime) {
            PrefWidth = 100;
            PrefHeight = 40;
        }

        public override void Draw(GameTime gameTime) {
            (Look ?? DefaultLook)(this);
        }

        /// <summary>A frame with a vertical handle at the value.</summary>
        public static void DrawDefault(Slider s) {
            float thickness = s._thickness;
            float handle = s._handleThickness;
            Color c = s.IsFocused ? Theme.Focus : Theme.Border;

            float x = s.Left + (s.Width - thickness) * s.Percent;
            GuiHelper.ShapeBatch.BorderRectangle(s.XY, s.Size, c, Theme.BorderThickness, Theme.Rounding);
            GuiHelper.ShapeBatch.FillRectangle(new Vector2(x, s.Top), new Vector2(thickness, s.Height), c);
            GuiHelper.ShapeBatch.FillRectangle(new Vector2(x - handle / 2f, s.Top + s.Height * 0.4f), new Vector2(thickness + handle, 2f), c);
            GuiHelper.ShapeBatch.FillRectangle(new Vector2(x - handle / 2f, s.Top + s.Height * 0.6f), new Vector2(thickness + handle, 2f), c);
        }

        protected void Slide(int direction) {
            Value = MathHelper.Clamp(Value + direction * Step!.Value, Min, Max);
        }

        protected float _thickness = 4;
        protected float _handleThickness = 8;
        protected float? _step;

        protected RepeatCondition _left = new(Default.MoveLeft, 400, 50);
        protected RepeatCondition _right = new(Default.MoveRight, 400, 50);

        public static Slider Put(ref float value, float min, float max, float? step = null, [CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            float initial = value;
            Slider a = IMGUI.Put(id, isAbsoluteId, file, id => new Slider(id, initial, min, max, step));
            if (a.IsFocused) {
                value = a.Value;
            } else {
                a.Value = value;
                a.Min = min;
                a.Max = max;
                a.Step = step;
            }

            return a;
        }
    }
}
