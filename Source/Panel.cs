using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Apos.Input;
using Track = Apos.Input.Track;
using Microsoft.Xna.Framework;
using System;

namespace Apos.Gui {
    public class Panel : Component, IParent {
        public Panel(int id) : base(id) { }

        public float OffsetX {
            get => _offsetX;
            set {
                _targetOffsetX = MathHelper.Min(MathHelper.Max(value, Clip.Width - FullWidth), 0);
            }
        }
        public float OffsetY {
            get => _offsetY;
            set {
                _targetOffsetY = MathHelper.Min(MathHelper.Max(value, Clip.Height - FullHeight), 0);
            }
        }
        public float FullWidth { get; set; } = 100;
        public float FullHeight { get; set; } = 100;

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

        public float ScrollIncrement { get; set; } = 50f;
        public float ScrollSpeed { get; set; } = 0.008f;

        public override void UpdatePrefSize(GameTime gameTime) {
            float maxWidth = 0;
            float maxHeight = 0;

            foreach (var c in _children) {
                c.UpdatePrefSize(gameTime);
                maxWidth = MathHelper.Max(c.PrefWidth, maxWidth);
                maxHeight += c.PrefHeight;
            }

            PrefWidth = maxWidth;
            PrefHeight = maxHeight;
        }
        public override void UpdateSetup(GameTime gameTime) {
            _offsetX = Interpolate(_offsetX, _targetOffsetX, ScrollSpeed * (float)gameTime.ElapsedGameTime.TotalMilliseconds, 0.1f);
            _offsetY = Interpolate(_offsetY, _targetOffsetY, ScrollSpeed * (float)gameTime.ElapsedGameTime.TotalMilliseconds, 0.1f);

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

                c.UpdateSetup(gameTime);

                currentY += c.Height;
            }

            FullWidth = maxWidth;
            FullHeight = MathHelper.Max(currentY, maxHeight);
        }
        public override void UpdateInput(GameTime gameTime) {
            for (int i = _childrenRenderOrder.Count - 1; i >= 0; i--) {
                _childrenRenderOrder[i].UpdateInput(gameTime);
            }

            if (Clip.Contains(GuiHelper.Mouse) && Track.MouseCondition.Scrolled()) {
                _targetOffsetY = MathHelper.Min(MathHelper.Max(_targetOffsetY + Math.Sign(MouseCondition.ScrollDelta) * ScrollIncrement, Clip.Height - FullHeight), 0);
            }
        }
        public override void Update(GameTime gameTime) {
            foreach (var c in _children)
                c.Update(gameTime);
        }
        public override void Draw(GameTime gameTime) {
            foreach (var c in _childrenRenderOrder)
                c.Draw(gameTime);

            // TODO: Draw scrollbars if needed.
        }

        public virtual void Add(IComponent c) {
            c.Parent = this;
            _children.Insert(c.Index, c);

            // TODO: Optimize this?
            _childrenRenderOrder.Add(c);
            _childrenRenderOrder.Sort((a, b) => {
                if (a.IsFloatable && b.IsFloatable) {
                    return 0;
                } else if (!a.IsFloatable && !b.IsFloatable) {
                    return a.Index.CompareTo(b.Index);
                } else if (a.IsFloatable) {
                    return 1;
                } else {
                    return -1;
                }
            });
        }
        public virtual void Remove(IComponent c) {
            c.Parent = null;
            _children.Remove(c);
            _childrenRenderOrder.Remove(c);
        }
        public virtual void Reset() {
            _nextChildIndex = 0;
        }
        public virtual int NextIndex() {
            return _nextChildIndex++;
        }

        /// <summary>
        /// If this component has a parent, it will ask the parent to return this component's previous neighbor.
        /// If it has children, it will return the last one.
        /// Otherwise it will return itself.
        /// </summary>
        public override IComponent GetPrev() {
            return Parent != null ? Parent.GetPrev(this) : _children.Count > 0 ? _children.Last().GetLast() : this;
        }
        /// <summary>
        /// If this component has children, it will return the first one.
        /// If it has a parent it will ask the parent to return this component's next neighbor.
        /// Otherwise, it will return itself.
        /// </summary>
        public override IComponent GetNext() {
            return _children.Count > 0 ? _children.First() : Parent != null ? Parent.GetNext(this) : this;
        }
        /// <summary>
        /// If the child isn't the first one, it will return the child before it.
        /// Otherwise it will return itself.
        /// </summary>
        public virtual IComponent GetPrev(IComponent c) {
            int index = c.Index - 1;
            return index >= 0 ? _children[index].GetLast() : this;
        }
        /// <summary>
        /// If the child isn't the last one, it will return the child after it.
        /// If it has a parent, it will ask the parent to return this component's next neighbor.
        /// Otherwise it will return itself.
        /// </summary>
        public virtual IComponent GetNext(IComponent c) {
            int index = c.Index + 1;
            return index < _children.Count ? _children[index] : Parent != null ? Parent.GetNext(this) : this;
        }
        /// <summary>
        /// Returns the last child in this component tree.
        /// </summary>
        public virtual IComponent GetLast() {
            return _children.Count > 0 ? _children.Last().GetLast() : this;
        }

        public virtual void SendToTop(IComponent c) {
            if (c.IsFloatable) {
                _childrenRenderOrder.Remove(c);
                _childrenRenderOrder.Add(c);
            }

            Parent?.SendToTop(this);
        }

        private float Interpolate(float start, float target, float speed, float snapNear) {
            float result = MathHelper.Lerp(start, target, speed);

            if (start < target) {
                result = MathHelper.Clamp(result, start, target);
            } else {
                result = MathHelper.Clamp(result, target, start);
            }

            if (Math.Abs(target - result) < snapNear) {
                return target;
            } else {
                return result;
            }
        }

        public static Panel Push([CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            // 1. Check if panel with id already exists.
            //      a. If already exists. Get it.
            //      b  If not, create it.
            // 3. Push it on the stack.
            // 4. Ping it.
            id = GuiHelper.CurrentIMGUI.CreateId(id, isAbsoluteId);
            GuiHelper.CurrentIMGUI.TryGetValue(id, out IComponent c);

            Panel a;
            if (c is Panel) {
                a = (Panel)c;
            } else {
                a = new Panel(id);
            }

            IParent parent = GuiHelper.CurrentIMGUI.GrabParent(a);

            if (a.LastPing != InputHelper.CurrentFrame) {
                a.Reset();
                a.LastPing = InputHelper.CurrentFrame;
                a.Index = parent.NextIndex();
            }

            GuiHelper.CurrentIMGUI.Push(a);

            return a;
        }
        public static void Pop() {
            GuiHelper.CurrentIMGUI.Pop();
        }

        protected int _nextChildIndex = 0;
        protected List<IComponent> _children = new List<IComponent>();
        protected List<IComponent> _childrenRenderOrder = new List<IComponent>();

        protected float _offsetX = 0;
        protected float _offsetY = 0;
        protected float _targetOffsetX = 0;
        protected float _targetOffsetY = 0;
    }
}
