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

        public void Add(Component c) {
            c.Parent = this;
            _children.Insert(c.Index, c);
        }
        public void Remove(Component c) {
            c.Parent = null;
            _children.Remove(c);
        }
        public void Reset() {
            _nextChildIndex = 0;
        }
        public int NextIndex() {
            return _nextChildIndex++;
        }

        public override Component GetPrev(Component c) {
            int index = _children.IndexOf(c) - 1;
            if (index >= 0 && _children.Count > 0) {
                return _children[index];
            } else if (Parent != null) {
                return Parent.GetPrev(this);
            }
            return _children.Count > 0 ? _children.Last() : this;
        }
        public override Component GetNext(Component c) {
            int index = _children.IndexOf(c) + 1;
            if (index < _children.Count) {
                return _children[index];
            } else if (Parent != null) {
                return Parent.GetNext(this);
            }
            return _children.Count > 0 ? _children.First() : this;
        }
        public override Component GetFirst() {
            if (_children.Count > 0) {
                return _children.First();
            }
            return this;
        }
        public override Component GetLast() {
            if (_children.Count > 0) {
                return _children.Last();
            }
            return this;
        }

        protected int _nextChildIndex = 0;
        protected List<Component> _children = new List<Component>();

        public static Panel Use(IMGUI ui, string name, int id = 0) {
            // 1. Check if panel with name already exists.
            //      a. If already exists. Get it.
            //      b  If not, create it.
            // 3. Push it on the stack.
            // 4. Ping it.

            var fullName = $"panel{name}{id}";

            ui.TryGetValue(fullName, out Component? c);

            if (!(c is Panel)) {
                c = new Panel();
                ui.Add(fullName, c);
            }
            ui.Push((Panel)c);
            if (c.LastPing != InputHelper.CurrentFrame) {
                ((Panel)c).Reset();
                c.LastPing = InputHelper.CurrentFrame;
            }

            return (Panel)c;
        }
    }
}
