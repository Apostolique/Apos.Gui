using System;
using Apos.Input;
using FontStashSharp;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Apos.Gui {
    public class Button : Component, IParent {
        public Button() { }
        public Button(IComponent child) {
            Child = child;
        }

        public bool Clicked {
            get;
            set;
        } = false;
        public IComponent? Child {
            get;
            set;
        }

        public override void UpdatePrefSize() {
            if (Child != null) {
                Child.UpdatePrefSize();

                PrefWidth = Child.PrefWidth;
                PrefHeight = Child.PrefHeight;
            }
        }
        public override void UpdateSetup() {
            if (Clicked) {
                Clicked = false;
            }

            if (Child != null) {
                Child.X = X;
                Child.Y = Y;
                Child.Width = Width;
                Child.Height = Height;

                Child.UpdateSetup();
            }
        }
        public override void UpdateInput() {
            if (Clip.Contains(GuiHelper.Mouse) && Default.MouseInteraction.Pressed()) {
                _pressed = true;
            }
            if (_pressed) {
                Default.MouseInteraction.HeldOnly();
                _contained = Clip.Contains(GuiHelper.Mouse);
            }
            if (_pressed && Default.MouseInteraction.Released()) {
                if (Clip.Contains(GuiHelper.Mouse))
                    Clicked = true;
                _pressed = false;
            }

            if (Child != null) {
                Child.UpdateInput();
            }
        }
        public override void Update() {
            if (Child != null) {
                Child.Update();
            }
        }

        public override void Draw() {
            GuiHelper.SetScissor(Clip);

            if (Clicked) {
                GuiHelper.SpriteBatch.FillRectangle(Bounds, Color.White * 0.5f);
            } else if (_pressed) {
                if (_contained) {
                    GuiHelper.SpriteBatch.FillRectangle(Bounds, Color.White * 0.2f);
                } else {
                    GuiHelper.SpriteBatch.FillRectangle(Bounds, Color.White * 0.15f);
                }
            }
            GuiHelper.SpriteBatch.DrawRectangle(Bounds, Color.White, 2f);

            if (Child != null) {
                Child.Draw();
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

        private bool _pressed = false;
        private bool _contained = false;

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
                a = new Button();
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
