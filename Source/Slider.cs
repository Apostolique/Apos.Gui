using System.Runtime.CompilerServices;
using Apos.Input;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

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

        public override void UpdatePrefSize(GameTime gameTime) {
            PrefWidth = 100;
            PrefHeight = 40;
        }

        public override void Draw(GameTime gameTime) {
            GuiHelper.PushScissor(Clip);
            Color c;
            if (IsFocused) c = Color.White;
            else c = new Color(76, 76, 76);

            float percent = (Value - Min) / (Max - Min);
            GuiHelper.SpriteBatch.DrawRectangle(Bounds, c, 2f);
            GuiHelper.SpriteBatch.FillRectangle(new RectangleF(Left + (Width - _thickness) * percent, Top, _thickness, Height), c);
            GuiHelper.SpriteBatch.FillRectangle(new RectangleF(Left + (Width - _thickness) * percent - _handleThickness / 2, Top + Height * 0.4f, _thickness + _handleThickness, 2), c);
            GuiHelper.SpriteBatch.FillRectangle(new RectangleF(Left + (Width - _thickness) * percent - _handleThickness / 2, Top + Height * 0.6f, _thickness + _handleThickness, 2), c);
            GuiHelper.PopScissor();
        }

        protected void SlideValue(ICondition condition, int direction) {
            if (condition.Pressed()) {
                Value = MathHelper.Max(MathHelper.Min(Value + direction * Step!.Value, Max), Min);
                _inputDelay = _inputDelayInitialSpeed;
            }
            if (condition.HeldOnly()) {
                if (_inputDelay <= 0) {
                    Value = MathHelper.Max(MathHelper.Min(Value + direction * Step!.Value, Max), Min);
                    _inputDelay = _inputDelaySpeed;
                }
            }
        }

        protected float _thickness = 4;
        protected float _handleThickness = 8;
        protected bool _isPressed = false;
        protected float? _step;

        protected int _inputDelay = 0;
        protected int _inputDelaySpeed = 50;
        protected int _inputDelayInitialSpeed = 400;

        public static Slider Put(ref float value, float min, float max, float? step = null, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            id = GuiHelper.CurrentIMGUI.TryCreateId(id, isAbsoluteId, out IComponent c);

            Slider a;
            if (c is Slider d) {
                a = d;
                if (a.IsFocused) {
                    value = a.Value;
                } else {
                    a.Value = value;
                    a.Min = min;
                    a.Max = max;
                    a.Step = step;
                }
            } else {
                a = new Slider(id, value, min, max, step);
            }

            GuiHelper.CurrentIMGUI.GrabParent(a);

            return a;
        }
    }
}
