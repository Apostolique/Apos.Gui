using System.Runtime.CompilerServices;
using Apos.Input;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    public class Block(int id, float? width, float? height, float padding) : Component(id), IParent {
        public float? ForcedWidth { get; set; } = width;
        public float? ForcedHeight { get; set; } = height;
        public float Padding { get; set; } = padding;
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

                PrefWidth = ForcedWidth ?? (Child.PrefWidth + Padding * 2f);
                PrefHeight = ForcedHeight ?? (Child.PrefHeight + Padding * 2f);
            } else {
                PrefWidth = ForcedWidth ?? 100f;
                PrefHeight = ForcedHeight ?? 100f;
            }
        }

        public virtual void UpdateLayout(GameTime gameTime) {
            if (Child != null) {
                Child.Width = Child.PrefWidth;
                Child.Height = Child.PrefHeight;
                Child.X = X + Width / 2f - Child.Width / 2f;
                Child.Y = Y + Height / 2f - Child.Height / 2f;

                Child.Clip = Child.Bounds.Intersect(Clip);

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

        public void Reset() { }

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
            LastFocus = InputHelper.CurrentFrame;
            Parent?.SendToTop(this);
        }

        public static Block Put(float? width = null, float? height = null, float padding = 0f, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            id = IMGUI.TryCreateId(id, isAbsoluteId, out IComponent? c);

            Block a;
            if (c is Block d) {
                a = d;
                a.ForcedWidth = width;
                a.ForcedHeight = height;
                a.Padding = padding;
            } else {
                a = new Block(id, width, height, padding);
            }

            GuiHelper.CurrentIMGUI.GrabParent(a);

            GuiHelper.CurrentIMGUI.Push(a, 1);

            return a;
        }
        /// <summary>
        /// Useful for doing an empty block.
        /// </summary>
        public static void Pop() {
            GuiHelper.CurrentIMGUI.Pop();
        }
    }
}
