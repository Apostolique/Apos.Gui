using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    public class Dock(int id, float left, float top, float right, float bottom) : Component(id), IParent {
        public float DockLeft { get; set; } = left;
        public float DockTop { get; set; } = top;
        public float DockRight { get; set; } = right;
        public float DockBottom { get; set; } = bottom;

        public IComponent? Child { get; set; }

        public override void UpdateSetup(GameTime gameTime) {
            Child?.UpdateSetup(gameTime);
        }
        public override void UpdateInput(GameTime gameTime) {
            Child?.UpdateInput(gameTime);
        }
        public override void Update(GameTime gameTime) {
            Child?.Update(gameTime);
        }

        public override void UpdatePrefSize(GameTime gameTime) {
            if (Child != null) {
                Child.UpdatePrefSize(gameTime);

                PrefWidth = DockRight - DockLeft;
                PrefHeight = DockBottom - DockTop;
            }
        }
        public virtual void UpdateLayout(GameTime gameTime) {
            X = DockLeft;
            Y = DockTop;

            if (Parent != null) {
                Clip = Bounds.Intersection(Parent.Clip);
            }

            if (Child != null) {
                Child.X = X;
                Child.Y = Y;
                Child.Width = MathHelper.Min(Width, Child.PrefWidth);
                Child.Height = MathHelper.Min(Height, Child.PrefHeight);
                Child.Clip = Child.Bounds.Intersection(Clip);

                if (Child is IParent p) {
                    p.UpdateLayout(gameTime);
                }
            }
        }

        public override void Draw(GameTime gameTime) {
            Child?.Draw(gameTime);
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

        public override IComponent GetPrev() {
            return Parent?.GetPrev(this) ?? Child?.GetLast() ?? this;
        }
        public override IComponent GetNext() {
            return Child ?? Parent?.GetNext(this) ?? this;
        }
        public virtual IComponent GetPrev(IComponent c) {
            return this;
        }
        public virtual IComponent GetNext(IComponent c) {
            return Parent?.GetNext(this) ?? this;
        }
        public override IComponent GetLast() {
            return Child?.GetLast() ?? this;
        }

        public virtual void SendToTop(IComponent c) {
            Parent?.SendToTop(this);
        }

        public static Dock Put(float left, float top, float right, float bottom, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            id = GuiHelper.CurrentIMGUI.TryCreateId(id, isAbsoluteId, out IComponent c);

            Dock a;
            if (c is Dock d) {
                a = d;
                a.DockLeft = left;
                a.DockTop = top;
                a.DockRight = right;
                a.DockBottom = bottom;
            } else {
                a = new Dock(id, left, top, right, bottom);
            }

            GuiHelper.CurrentIMGUI.GrabParent(a);

            GuiHelper.CurrentIMGUI.Push(a, 1);

            return a;
        }
    }
}
