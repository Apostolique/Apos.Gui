using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Apos.Gui {
    public class Button(int id) : Component(id), IParent {
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

        public override void UpdateSetup(GameTime gameTime) {
            if (Clicked) {
                Clicked = false;
            }

            Child?.UpdateSetup(gameTime);
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

            Child?.UpdateInput(gameTime);
        }
        public override void Update(GameTime gameTime) {
            Child?.Update(gameTime);
        }

        public override void UpdatePrefSize(GameTime gameTime) {
            if (Child != null) {
                Child.UpdatePrefSize(gameTime);

                PrefWidth = Child.PrefWidth;
                PrefHeight = Child.PrefHeight;
            }
        }
        public virtual void UpdateLayout(GameTime gameTime) {
            if (Child != null) {
                Child.X = X;
                Child.Y = Y;
                Child.Width = Width;
                Child.Height = Height;
                Child.Clip = Child.Bounds.Intersection(Clip);

                if (Child is IParent p) {
                    p.UpdateLayout(gameTime);
                }
            }
        }

        public override void Draw(GameTime gameTime) {
            GuiHelper.PushScissor(Clip);

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

            Child?.Draw(gameTime);

            GuiHelper.PopScissor();
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
        public virtual void Reset() { }
        public virtual int PeekNextIndex() => 0;
        public virtual int NextIndex() => 0;

        public virtual IComponent GetPrev(IComponent c) {
            return this;
        }
        public virtual IComponent GetNext(IComponent c) {
            return Parent?.GetNext(this) ?? this;
        }

        public virtual void SendToTop(IComponent c) {
            Parent?.SendToTop(this);
        }

        protected bool _mousePressed = false;
        protected bool _buttonPressed = false;
        protected bool _hovered = false;

        public static Button Put(string text, int fontSize = 30, Color? color = null, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            Button b = Put(id, isAbsoluteId);
            Label.Put(text, fontSize: fontSize, color: color, id: id, isAbsoluteId: isAbsoluteId);

            return b;
        }
        public static Button Put([CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            id = GuiHelper.CurrentIMGUI.TryCreateId(id, isAbsoluteId, out IComponent c);

            Button a;
            if (c is Button d) {
                a = d;
            } else {
                a = new Button(id);
            }

            GuiHelper.CurrentIMGUI.GrabParent(a);

            GuiHelper.CurrentIMGUI.Push(a, 1);

            return a;
        }
    }
}
