using System.Runtime.CompilerServices;
using Apos.Input;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Apos.Gui {
    public class Button : Component, IParent {
        public Button(int id) : base(id) { }

        public bool Clicked { get; set; } = false;
        public IComponent? Child { get; set; }
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

        public override void UpdatePrefSize(GameTime gameTime) {
            if (Child != null) {
                Child.UpdatePrefSize(gameTime);

                PrefWidth = Child.PrefWidth;
                PrefHeight = Child.PrefHeight;
            }
        }
        public override void UpdateSetup(GameTime gameTime) {
            if (Clicked) {
                Clicked = false;
            }

            if (Child != null) {
                Child.X = X;
                Child.Y = Y;
                Child.Width = Width;
                Child.Height = Height;

                Child.UpdateSetup(gameTime);
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
                        if (Clip.Contains(GuiHelper.Mouse))
                            Clicked = true;
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
                        _buttonPressed = false;
                    } else {
                        Default.ButtonInteraction.Consume();
                    }
                }
            }

            if (Child != null) {
                Child.UpdateInput(gameTime);
            }
        }
        public override void Update(GameTime gameTime) {
            if (Child != null) {
                Child.Update(gameTime);
            }
        }

        public override void Draw(GameTime gameTime) {
            GuiHelper.SetScissor(Clip);

            if (Clicked) {
                GuiHelper.SpriteBatch.FillRectangle(Bounds, Color.White * 0.5f);
            } else if (_mousePressed && _hovered || _buttonPressed) {
                GuiHelper.SpriteBatch.FillRectangle(Bounds, Color.White * 0.2f);
            } else if (_mousePressed) {
                GuiHelper.SpriteBatch.FillRectangle(Bounds, Color.White * 0.15f);
            }
            if (IsFocused) {
                GuiHelper.SpriteBatch.DrawRectangle(Bounds, Color.White, 2f);
            } else {
                GuiHelper.SpriteBatch.DrawRectangle(Bounds, new Color(76, 76, 76), 2f);
            }

            if (Child != null) {
                Child.Draw(gameTime);
            }

            GuiHelper.ResetScissor();
        }

        public void Add(IComponent c) {
            if (c != Child) {
                if (Child != null) {
                    Child.Parent = null;
                }
                Child = c;
                Child.Parent = this;
            }
        }
        public void Remove(IComponent c) {
            if (Child == c) {
                Child.Parent = null;
                Child = null;
            }
        }
        public void Reset() { }
        public int NextIndex() => 0;

        public override IComponent GetPrev() {
            return Parent != null ? Parent.GetPrev(this) : Child != null ? Child : this;
        }
        public override IComponent GetNext() {
            return Child != null ? Child : Parent != null ? Parent.GetNext(this) : this;
        }
        public virtual IComponent GetPrev(IComponent c) {
            return this;
        }
        public virtual IComponent GetNext(IComponent c) {
            return Parent != null ? Parent.GetNext(this) : this;
        }

        public static Button Put(string text, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            Button b = Put(id, isAbsoluteId);
            Label.Put(text, id, isAbsoluteId);

            return b;
        }
        public static Button Put([CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            // 1. Check if button with id already exists.
            //      a. If already exists. Get it.
            //      b  If not, create it.
            // 4. Ping it.
            id = GuiHelper.CurrentIMGUI.CreateId(id, isAbsoluteId);
            GuiHelper.CurrentIMGUI.TryGetValue(id, out IComponent c);

            Button a;
            if (c is Button) {
                a = (Button)c;
            } else {
                a = new Button(id);
            }

            IParent? parent = GuiHelper.CurrentIMGUI.GrabParent(a);

            if (a.LastPing != InputHelper.CurrentFrame) {
                a.LastPing = InputHelper.CurrentFrame;
                if (parent != null) {
                    a.Index = parent.NextIndex();
                }
            }

            GuiHelper.CurrentIMGUI.Push(a, 1);

            return a;
        }

        protected bool _mousePressed = false;
        protected bool _buttonPressed = false;
        protected bool _hovered = false;
    }
}
