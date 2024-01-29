using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Apos.Gui {
    public class Checkbox(int id, bool isChecked) : Component(id) {
        public bool Clicked { get; set; } = false;
        public bool IsChecked { get; set; } = isChecked;
        public override bool IsFocusable { get; set; } = true;
        public override bool IsFocused {
            get => base.IsFocused;
            set {
                base.IsFocused = value;
                if (!value) {
                    _mousePressed = false;
                    _hovered = false;
                    _buttonPressed = false;
                }
            }
        }

        public override void UpdateSetup(GameTime gameTime) {
            if (Clicked) {
                Clicked = false;
            }
        }
        public override void UpdateInput(GameTime gameTime) {
            if (Clip.Contains(GuiHelper.Mouse) && Default.MouseInteraction.Pressed()) {
                _mousePressed = true;
                GrabFocus(this);
            }

            if (IsFocused) {
                if (_mousePressed) {
                    if (Default.MouseInteraction.Released()) {
                        if (Clip.Contains(GuiHelper.Mouse)) {
                            Clicked = true;
                            IsChecked = !IsChecked;
                        }
                        _mousePressed = false;
                    } else {
                        Default.MouseInteraction.Consume();
                        _hovered = Clip.Contains(GuiHelper.Mouse);
                    }
                }

                if (Default.ButtonInteraction.Pressed()) {
                    _buttonPressed = true;
                } else if (_buttonPressed) {
                    if (Default.ButtonInteraction.Released()) {
                        Clicked = true;
                        IsChecked = !IsChecked;
                        _buttonPressed = false;
                    } else {
                        Default.ButtonInteraction.Consume();
                    }
                }
            }
        }

        public override void UpdatePrefSize(GameTime gameTime) {
            PrefWidth = 30f;
            PrefHeight = 30f;
        }

        public override void Draw(GameTime gameTime) {
            GuiHelper.PushScissor(Clip);

            if (IsChecked) {
                GuiHelper.SpriteBatch.FillRectangle(new RectangleF(Left + 6, Top + 6, Width - 12, Height - 12), Color.White);
            }

            if (Clicked) {
                GuiHelper.SpriteBatch.FillRectangle(Bounds, Color.White * 0.5f);
            } else if (_mousePressed && _hovered || _buttonPressed) {
                GuiHelper.SpriteBatch.FillRectangle(Bounds, Color.White * 0.2f);
            } else if (_mousePressed) {
                GuiHelper.SpriteBatch.FillRectangle(Bounds, Color.White * 0.15f);
            }
            if (IsFocused) {
                GuiHelper.SpriteBatch.DrawRectangle(Bounds, Color.White, 2f);
                GuiHelper.SpriteBatch.DrawRectangle(new RectangleF(Left + 6, Top + 6, Width - 12, Height - 12), Color.White, 2f);
            } else {
                GuiHelper.SpriteBatch.DrawRectangle(Bounds, new Color(76, 76, 76), 2f);
                GuiHelper.SpriteBatch.DrawRectangle(new RectangleF(Left + 6, Top + 6, Width - 12, Height - 12), new Color(76, 76, 76), 2f);
            }

            GuiHelper.PopScissor();
        }

        protected bool _mousePressed = false;
        protected bool _buttonPressed = false;
        protected bool _hovered = false;

        public static Checkbox Put(ref bool isChecked, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            id = GuiHelper.CurrentIMGUI.TryCreateId(id, isAbsoluteId, out IComponent c);

            Checkbox a;
            if (c is Checkbox d) {
                a = d;
                if (a.IsFocused) {
                    isChecked = a.IsChecked;
                } else {
                    a.IsChecked = isChecked;
                }
            } else {
                a = new Checkbox(id, isChecked);
            }

            GuiHelper.CurrentIMGUI.GrabParent(a);

            return a;
        }
    }
}
