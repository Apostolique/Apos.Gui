using Apos.Input;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Apos.Gui {
    public class Button : Component, IParent {
        public Button(string name) : base(name) { }

        public bool Clicked {
            get;
            set;
        } = false;
        public IComponent? Child {
            get;
            set;
        }
        public override bool IsFocusable {
            get;
            set;
        } = true;

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
                _pressed = true;
                // TODO: Grab focus.
            }
            if (_pressed && Default.MouseInteraction.HeldOnly()) {
                _hovered = Clip.Contains(GuiHelper.Mouse);
            }
            if (_pressed && Default.MouseInteraction.Released()) {
                if (Clip.Contains(GuiHelper.Mouse))
                    Clicked = true;
                _pressed = false;
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
            } else if (_pressed) {
                if (_hovered) {
                    GuiHelper.SpriteBatch.FillRectangle(Bounds, Color.White * 0.2f);
                } else {
                    GuiHelper.SpriteBatch.FillRectangle(Bounds, Color.White * 0.15f);
                }
            }
            if (IsFocused) {
                GuiHelper.SpriteBatch.DrawRectangle(Bounds, Color.Red, 2f);
            } else {
                GuiHelper.SpriteBatch.DrawRectangle(Bounds, Color.White, 2f);
            }

            if (Child != null) {
                Child.Draw(gameTime);
            }

            GuiHelper.ResetScissor();
        }

        public void Add(IComponent c) {
            c.Parent = this;
            // TODO: Maybe we need to check if there's already a child and call remove on it.
            Child = c;
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

        private bool _pressed = false;
        private bool _hovered = false;

        public static Button Put(string text, int id = 0) {
            Button b = Put(id);
            Label.Put(text, id);

            return b;
        }
        public static Button Put(int id = 0) {
            // 1. Check if button with id already exists.
            //      a. If already exists. Get it.
            //      b  If not, create it.
            // 4. Ping it.
            var fullName = $"button{(id == 0 ? GuiHelper.CurrentIMGUI.NextId() : id)}";

            IParent? parent = GuiHelper.CurrentIMGUI.CurrentParent;
            GuiHelper.CurrentIMGUI.TryGetValue(fullName, out IComponent c);

            Button a;
            if (c is Button) {
                a = (Button)c;
            } else {
                a = new Button(fullName);
                GuiHelper.CurrentIMGUI.Add(fullName, a);
            }

            if (a.LastPing != InputHelper.CurrentFrame) {
                a.LastPing = InputHelper.CurrentFrame;
                if (parent != null) {
                    a.Index = parent.NextIndex();
                }
            }

            GuiHelper.CurrentIMGUI.Push(a, 1);

            return a;
        }
    }
}
