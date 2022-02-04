using System.Runtime.CompilerServices;
using Apos.Input;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Apos.Gui {
    public class Dock : Component, IParent {
        public Dock(int id, float left, float top, float right, float bottom) : base(id) {
            DockLeft = left;
            DockTop = top;
            DockRight = right;
            DockBottom = bottom;
        }

        public float DockLeft { get; set; }
        public float DockTop { get; set; }
        public float DockRight { get; set; }
        public float DockBottom { get; set; }

        public IComponent? Child { get; set; }

        public override void UpdatePrefSize(GameTime gameTime) {
            if (Child != null) {
                Child.UpdatePrefSize(gameTime);

                PrefWidth = DockRight - DockLeft;
                PrefHeight = DockBottom - DockTop;
            }
        }
        public override void UpdateSetup(GameTime gameTime) {
            X = DockLeft;
            Y = DockTop;

            if (Child != null) {
                Child.X = X;
                Child.Y = Y;
                Child.Width = MathHelper.Min(Width, Child.PrefWidth);
                Child.Height = MathHelper.Min(Height, Child.PrefHeight);
                Child.Clip = Child.Bounds.Intersection(Clip);

                Child.UpdateSetup(gameTime);
            }
        }
        public override void UpdateInput(GameTime gameTime) {
            if (Child != null) {
                Child.UpdateInput(gameTime);
            }
        }
        public override void Draw(GameTime gameTime) {
            GuiHelper.PushScissor(Clip);
            GuiHelper.SpriteBatch.FillRectangle(Bounds, Color.Red * 0.2f);
            GuiHelper.SpriteBatch.DrawRectangle(Bounds, Color.Blue, 2f);
            GuiHelper.PopScissor();

            if (Child != null) {
                Child.Draw(gameTime);
            }
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

        public virtual void SendToTop(IComponent c) {
            Parent?.SendToTop(this);
        }

        public static new Dock Put(float left, float top, float right, float bottom, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            // 1. Check if dock with id already exists.
            //      a. If already exists. Get it.
            //      b  If not, create it.
            // 3. Push it on the stack.
            // 4. Ping it.
            id = GuiHelper.CurrentIMGUI.CreateId(id, isAbsoluteId);
            GuiHelper.CurrentIMGUI.TryGetValue(id, out IComponent c);

            Dock a;
            if (c is Dock) {
                a = (Dock)c;
                a.DockLeft = left;
                a.DockTop = top;
                a.DockRight = right;
                a.DockBottom = bottom;
            } else {
                a = new Dock(id, left, top, right, bottom);
            }

            IParent parent = GuiHelper.CurrentIMGUI.GrabParent(a);

            if (a.LastPing != InputHelper.CurrentFrame) {
                a.LastPing = InputHelper.CurrentFrame;
                a.Index = parent.NextIndex();
            }

            GuiHelper.CurrentIMGUI.Push(a, 1);

            return a;
        }
    }
}
