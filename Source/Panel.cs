using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Apos.Input;
using Track = Apos.Input.Track;
using Microsoft.Xna.Framework;
using System;
using Apos.Tweens;

namespace Apos.Gui {
    public class Panel : Component, IParent {
        public Panel(int id) : base(id) { }

        public float OffsetX {
            get => _offsetX;
            set {
                SetOffset(_offsetXTween, ClampOffsetX(value));
            }
        }
        public float OffsetY {
            get => _offsetY;
            set {
                SetOffset(_offsetYTween, ClampOffsetY(value));
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
        public float ScrollSpeed { get; set; } = 0.25f;
        public long ScrollMaxDuration { get; set; } = 1000;

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
            if (_offsetYTween.B != ClampOffsetY(_offsetYTween.B)) {
                SetOffset(_offsetYTween, ClampOffsetY(_offsetYTween.B));
            }

            _offsetX = _offsetXTween.Value;
            _offsetY = _offsetYTween.Value;

            float maxWidth = Width;
            float maxHeight = Height;

            float currentY = 0f;
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
                SetOffset(_offsetYTween, ClampOffsetY(_offsetYTween.B + Math.Sign(MouseCondition.ScrollDelta) * ScrollIncrement));
            }

            // TODO: Consume clicks on the panel? Otherwise it's possible to click stuff under it.
        }
        public override void Update(GameTime gameTime) {
            foreach (var c in _children)
                c.Update(gameTime);
        }
        public override void Draw(GameTime gameTime) {
            // TODO: Only draw visible stuff. Use clip.
            foreach (var c in _childrenRenderOrder)
                c.Draw(gameTime);

            // TODO: Draw scrollbars if needed?
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

            if (c.Y < Y) {
                float yDiff = Y - c.Y;
                float oDiff = _offsetYTween.B - _offsetY;
                SetOffset(_offsetYTween, ClampOffsetY(_offsetYTween.B + yDiff - oDiff));
            }
            if (c.Bottom > Bottom) {
                float yDiff = Bottom - c.Bottom;
                float oDiff = _offsetYTween.B - _offsetY;
                SetOffset(_offsetYTween, ClampOffsetY(_offsetYTween.B + yDiff - oDiff));
            }

            Parent?.SendToTop(this);
        }

        protected virtual float ClampOffsetX(float x) {
            return MathHelper.Min(MathHelper.Max(x, Clip.Width - FullWidth), 0f);
        }
        protected virtual float ClampOffsetY(float y) {
            return MathHelper.Min(MathHelper.Max(y, Clip.Height - FullHeight), 0f);
        }

        protected void SetOffset(FloatTween ft, float b) {
            var a = ft.Value;
            ft.StartTime = TweenHelper.TotalMS;
            ft.A = a;
            ft.B = b;
            ft.Duration = GetDuration(a, b, ScrollSpeed, ScrollMaxDuration);
        }

        protected long GetDuration(float a, float b, float speed, long maxDuration) {
            return (long)Math.Min(Math.Abs((b - a) / speed), maxDuration);
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

        protected FloatTween _offsetXTween = new FloatTween(0f, 0f, 0, Easing.ExpoOut);
        protected FloatTween _offsetYTween = new FloatTween(0f, 0f, 0, Easing.ExpoOut);

        protected float _offsetX = 0;
        protected float _offsetY = 0;
    }
}
