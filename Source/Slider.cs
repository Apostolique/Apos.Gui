using System.Runtime.CompilerServices;
using Apos.Input;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    public class Slider : Component {
        public Slider(int id, float value, float min, float max, float? step) : base(id) {
            Value = value;
            Min = min;
            Max = max;
            Step = step;
        }

        public float Value { get; set; }
        public float Min { get; set; }
        public float Max { get; set; }
        public float? Step {
            get => _step ?? (Max - Min) / 100f;
            set {
                _step = value;
            }
        }
        public override bool IsFocusable { get; set; } = true;
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
            GuiHelper.PushClip(Clip);
            Color c;
            if (IsFocused) c = Color.White;
            else c = new Color(76, 76, 76);

            float percent = (Value - Min) / (Max - Min);
            GuiHelper.ShapeBatch.BorderRectangle(Bounds.Position, Bounds.Size, c, 2f);
            GuiHelper.ShapeBatch.FillRectangle(new Vector2(Left + (Width - _thickness) * percent, Top), new Vector2(_thickness, Height), c);
            GuiHelper.ShapeBatch.FillRectangle(new Vector2(Left + (Width - _thickness) * percent - _handleThickness / 2, Top + Height * 0.4f), new Vector2(_thickness + _handleThickness, 2), c);
            GuiHelper.ShapeBatch.FillRectangle(new Vector2(Left + (Width - _thickness) * percent - _handleThickness / 2, Top + Height * 0.6f), new Vector2(_thickness + _handleThickness, 2), c);
            GuiHelper.PopClip();
        }

        protected void Slide(int direction) {
            Value = MathHelper.Clamp(Value + direction * Step!.Value, Min, Max);
        }

        protected float _thickness = 4;
        protected float _handleThickness = 8;
        protected float? _step;

        protected RepeatCondition _left = new(Default.MoveLeft, 400, 50);
        protected RepeatCondition _right = new(Default.MoveRight, 400, 50);

        public static Slider Put(ref float value, float min, float max, float? step = null, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            float initial = value;
            Slider a = IMGUI.Put(id, isAbsoluteId, id => new Slider(id, initial, min, max, step));
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
