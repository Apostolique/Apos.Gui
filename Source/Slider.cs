using Apos.Input;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Apos.Gui {
    public class Slider : Component {
        public Slider(string name, float value, float min, float max, float? step) : base(name) {
            Value = value;
            Min = min;
            Max = max;
            Step = step;
        }

        public float Value { get; set; }
        public float Min { get; set; }
        public float Max { get; set; }
        public float? Step {
            get => _step != null ? _step.Value : (Max - Min) / 100;
            set {
                _step = value;
            }
        }
        public override bool IsFocusable { get; set; } = true;

        public override void UpdatePrefSize(GameTime gameTime) {
            PrefWidth = 100;
            PrefHeight = 40;
        }
        public override void UpdateInput(GameTime gameTime) {
            if (Clip.Contains(GuiHelper.Mouse) && Default.MouseInteraction.Pressed()) {
                _isPressed = true;
                float percent = (GuiHelper.Mouse.X - Left) / (Width - _thickness);
                Value = MathHelper.Max(MathHelper.Min(percent * (Max - Min) + Min, Max), Min);
                GrabFocus(this);
            }
            if (IsFocused) {
                if (_isPressed) {
                    float percent = (GuiHelper.Mouse.X - Left) / (Width - _thickness);
                    Value = MathHelper.Max(MathHelper.Min(percent * (Max - Min) + Min, Max), Min);

                    if (Default.MouseInteraction.Released()) {
                        _isPressed = false;
                    } else {
                        Default.MouseInteraction.Consume();
                    }
                }

                SlideValue(Default.MoveLeft, -1);
                SlideValue(Default.MoveRight, 1);
            }
        }
        public override void Update(GameTime gameTime) {
            if (IsFocused) {
                if (_inputDelay > 0) {
                    _inputDelay -= (int)gameTime.ElapsedGameTime.TotalMilliseconds;
                }
            }
        }
        public override void Draw(GameTime gameTime) {
            GuiHelper.SetScissor(Clip);
            Color c;
            if (IsFocused) c = Color.White;
            else c = new Color(76, 76, 76);

            float percent = (Value - Min) / (Max - Min);
            GuiHelper.SpriteBatch.DrawRectangle(Bounds, c, 2f);
            GuiHelper.SpriteBatch.FillRectangle(new RectangleF(Left + (Width - _thickness) * percent, Top, _thickness, Height), c);
            GuiHelper.SpriteBatch.FillRectangle(new RectangleF(Left + (Width - _thickness) * percent - _handleThickness / 2, Top + Height * 0.4f, _thickness + _handleThickness, 2), c);
            GuiHelper.SpriteBatch.FillRectangle(new RectangleF(Left + (Width - _thickness) * percent - _handleThickness / 2, Top + Height * 0.6f, _thickness + _handleThickness, 2), c);
            GuiHelper.ResetScissor();
        }

        public static Slider Put(ref float value, float min, float max, float? step = null, int id = 0) {
            // 1. Check if Textbox with id already exists.
            //      a. If already exists. Get it.
            //      b  If not, create it.
            // 4. Ping it.
            var fullName = $"slider{(id == 0 ? GuiHelper.CurrentIMGUI.NextId() : id)}";

            IParent? parent = GuiHelper.CurrentIMGUI.CurrentParent;
            GuiHelper.CurrentIMGUI.TryGetValue(fullName, out IComponent c);

            Slider a;
            if (c is Slider) {
                a = (Slider)c;
                if (a.IsFocused) {
                    value = a.Value;
                } else {
                    a.Value = value;
                    a.Min = min;
                    a.Max = max;
                    a.Step = step;
                }
            } else {
                a = new Slider(fullName, value, min, max, step);
                GuiHelper.CurrentIMGUI.Add(fullName, a);
            }

            if (a.LastPing != InputHelper.CurrentFrame) {
                a.LastPing = InputHelper.CurrentFrame;
                if (parent != null) {
                    a.Index = parent.NextIndex();
                }
            }

            return a;
        }

        private void SlideValue(ICondition condition, int direction) {
            if (condition.Pressed()) {
                Value = MathHelper.Max(MathHelper.Min(Value + direction * Step.Value, Max), Min);
                _inputDelay = _inputDelayInitialSpeed;
            }
            if (condition.HeldOnly()) {
                if (_inputDelay <= 0) {
                    Value = MathHelper.Max(MathHelper.Min(Value + direction * Step.Value, Max), Min);
                    _inputDelay = _inputDelaySpeed;
                }
            }
        }

        private float _thickness = 4;
        private float _handleThickness = 8;
        private bool _isPressed = false;
        private float? _step;

        private int _inputDelay = 0;
        private int _inputDelaySpeed = 50;
        private int _inputDelayInitialSpeed = 400;
    }
}
