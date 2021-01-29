using System.Collections.Generic;
using System.Linq;
using Apos.Input;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    public class Panel : Component, IParent {
        public float OffsetX {
            get;
            set;
        } = 0;
        public float OffsetY {
            get;
            set;
        } = 0;
        public float FullWidth {
            get;
            set;
        } = 100;
        public float FullHeight {
            get;
            set;
        } = 100;

        public Vector2 OffsetXY {
            get => new Vector2(OffsetX, OffsetY);
            set {
                OffsetX = value.X;
                OffsetY = value.Y;
            }
        }
        public Vector2 FullSize {
            get => new Vector2(FullWidth, FullHeight);
            set {
                FullWidth = value.X;
                FullHeight = value.Y;
            }
        }

        public override void UpdatePrefSize() {
            float maxWidth = 0;
            float maxHeight = 0;

            foreach (var c in _children) {
                c.UpdatePrefSize();
                maxWidth = MathHelper.Max(c.PrefWidth, maxWidth);
                maxHeight += c.PrefHeight;
            }

            PrefWidth = maxWidth;
            PrefHeight = maxHeight;
        }
        public override void UpdateSetup() {
            float maxWidth = Width;
            float maxHeight = Height;

            float currentY = 0;
            foreach (var c in _children) {
                c.X = X + OffsetX;
                c.Y = currentY + Y + OffsetY;
                c.Width = c.PrefWidth;
                c.Height = c.PrefHeight;

                maxWidth = MathHelper.Max(c.PrefWidth, maxWidth);
                c.Clip = c.Bounds.Intersection(Clip);

                c.UpdateSetup();

                currentY += c.Height;
            }

            FullWidth = maxWidth;
            FullHeight = MathHelper.Max(currentY, maxHeight);
        }
        public override void UpdateInput() {
            foreach (var c in _children)
                c.UpdateInput();

            // TODO: Scrolling input.
        }
        public override void Update() {
            foreach (var c in _children)
                c.Update();
        }
        public override void Draw() {
            foreach (var c in _children)
                c.Draw();

            // TODO: Draw scrollbars if needed.
        }

        public void Add(IComponent c) {
            c.Parent = this;
            _children.Insert(c.Index, c);
        }
        public void Remove(IComponent c) {
            c.Parent = null;
            _children.Remove(c);
        }
        public void Reset() {
            _nextChildIndex = 0;
        }
        public int NextIndex() {
            return _nextChildIndex++;
        }

        public override IComponent GetPrev(IComponent c) {
            int index = _children.IndexOf(c) - 1;
            if (index >= 0 && _children.Count > 0) {
                return _children[index];
            } else if (Parent != null) {
                return Parent.GetPrev(this);
            }
            return _children.Count > 0 ? _children.Last() : this;
        }
        public override IComponent GetNext(IComponent c) {
            int index = _children.IndexOf(c) + 1;
            if (index < _children.Count) {
                return _children[index];
            } else if (Parent != null) {
                return Parent.GetNext(this);
            }
            return _children.Count > 0 ? _children.First() : this;
        }
        public override IComponent GetFirst() {
            if (_children.Count > 0) {
                return _children.First();
            }
            return this;
        }
        public override IComponent GetLast() {
            if (_children.Count > 0) {
                return _children.Last();
            }
            return this;
        }

        protected int _nextChildIndex = 0;
        protected List<IComponent> _children = new List<IComponent>();

        public static Panel Put(int id = 0) {
            // 1. Check if panel with id already exists.
            //      a. If already exists. Get it.
            //      b  If not, create it.
            // 3. Push it on the stack.
            // 4. Ping it.

            var fullName = $"panel{(id == 0 ? GuiHelper.CurrentIMGUI.NextId() : id)}";

            IParent? parent = GuiHelper.CurrentIMGUI.CurrentParent;
            GuiHelper.CurrentIMGUI.TryGetValue(fullName, out IComponent c);

            Panel a;
            if (c is Panel) {
                a = (Panel)c;
            } else {
                a = new Panel();
                GuiHelper.CurrentIMGUI.Add(fullName, a);
            }

            if (a.LastPing != InputHelper.CurrentFrame) {
                a.Reset();
                a.LastPing = InputHelper.CurrentFrame;
                if (parent != null) {
                    a.Index = parent.NextIndex();
                }
            }

            GuiHelper.CurrentIMGUI.Push(a);

            return a;
        }
        public static void Pop() {
            GuiHelper.CurrentIMGUI.Pop();
        }
    }
}
