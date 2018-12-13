using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AposGui
{
    /// <summary>
    /// Goal: The core of a user interface.
    ///       Handles everything from how something is drawn,
    ///       to how to handle inputs.
    /// </summary>
    class Component
    {
        public Component() {
            Position = new Point(0, 0);
            Width = 100;
            Height = 100;
            Parent = null;
            IsFocusable = false;
            _clippingRect = Maybe<Rectangle>.Nothing;
        }
        public virtual Point Position {
            get; set;
        }
        public virtual int Width {
            get; set;
        }
        public virtual int Height {
            get; set;
        }
        public virtual int PrefWidth => Width;
        public virtual int PrefHeight => Height;
        public virtual int Left => Position.X;
        public virtual int Top => Position.Y;
        public virtual int Right => Position.X + Width;
        public virtual int Bottom => Position.Y + Height;
        public virtual Rectangle BoundingRect => new Rectangle(Left, Top, Width, Height);
        protected Maybe<Rectangle> _clippingRect;
        public virtual Rectangle ClippingRect {
            get {
                if (_clippingRect.HasValue) {
                    return _clippingRect.Value;
                }
                return BoundingRect;
            }
            set {
                _clippingRect = new Maybe<Rectangle>(ClipRectangle(value));
            }
        }
        public virtual Component Parent {
            get; set;
        }
        public virtual bool IsFocusable {
            get; set;
        }
        public virtual bool HasFocus {
            get; set;
        }
        public virtual Component GetPrevious() {
            if (Parent != null) {
                return Parent.GetPrevious(this);
            }
            return this;
        }
        public virtual Component GetNext() {
            if (Parent != null) {
                return Parent.GetNext(this);
            }
            return this;
        }
        public virtual Component GetPrevious(Component c) {
            if (Parent != null) {
                return Parent.GetPrevious(this);
            }
            return this;
        }
        public virtual Component GetNext(Component c) {
            if (Parent != null) {
                return Parent.GetNext(this);
            }
            return this;
        }
        public virtual Component GetFinal() {
            return this;
        }
        public virtual Component GetFinalInverse() {
            return this;
        }

        public Rectangle ClipRectangle(Rectangle rect1) {
            return ClipRectangle(rect1, BoundingRect);
        }
        public Rectangle ClipRectangle(Rectangle rect1, Rectangle rect2) {
            var left = rect1.Left < rect2.Left ? rect2.Left : rect1.Left;
            var top = rect1.Top < rect2.Top ? rect2.Top : rect1.Top;
            var right = rect1.Right < rect2.Right ? rect1.Right : rect2.Right;
            var bottom = rect1.Bottom < rect2.Bottom ? rect1.Bottom : rect2.Bottom;

            int clipWidth = Math.Max(right - left, 0);
            int clipHeight = Math.Max(bottom - top, 0);

            return new Rectangle(left, top, clipWidth, clipHeight);
        }
        public Rectangle ClipSourceRectangle(Rectangle sourceRectangle, Rectangle destinationRectangle, Rectangle clippingRectangle) {
            float left = (float)(clippingRectangle.Left - destinationRectangle.Left);
            float right = (float)(destinationRectangle.Right - clippingRectangle.Right);
            float top = (float)(clippingRectangle.Top - destinationRectangle.Top);
            float bottom = (float)(destinationRectangle.Bottom - clippingRectangle.Bottom);
            float x =  left > 0 ? left : 0;
            float y = top > 0 ? top : 0;
            float w = (right > 0 ? right : 0) + x;
            float h = (bottom > 0 ? bottom : 0) + y;

            float scaleX = (float)destinationRectangle.Width / sourceRectangle.Width;
            float scaleY = (float)destinationRectangle.Height / sourceRectangle.Height;
            x /= scaleX;
            y /= scaleY;
            w /= scaleX;
            h /= scaleY;

            return new Rectangle((int)(sourceRectangle.X + x), (int)(sourceRectangle.Y + y), (int)(sourceRectangle.Width - w), (int)(sourceRectangle.Height - h));
        }
        public Rectangle ClipDestinationRectangle(Rectangle destinationRectangle, Rectangle clippingRectangle)
        {
            var left = clippingRectangle.Left < destinationRectangle.Left ? destinationRectangle.Left : clippingRectangle.Left;
            var top = clippingRectangle.Top < destinationRectangle.Top ? destinationRectangle.Top : clippingRectangle.Top;
            var bottom = clippingRectangle.Bottom < destinationRectangle.Bottom ? clippingRectangle.Bottom : destinationRectangle.Bottom;
            var right = clippingRectangle.Right < destinationRectangle.Right ? clippingRectangle.Right : destinationRectangle.Right;

            return new Rectangle(left, top, right - left, bottom - top);
        }
        public virtual bool IsInside(Point v) {
            return Left < v.X && Right > v.X && Top < v.Y && Bottom > v.Y;
        }
        public virtual bool IsInsideClip(Point v) {
            Rectangle r = ClippingRect;
            return r.Left < v.X && r.Right > v.X && r.Top < v.Y && r.Bottom > v.Y;
        }
        public virtual void UpdateSetup() {
        }
        public virtual bool UpdateInput() {
            return false;
        }
        public virtual void Update() {
        }
        public virtual void Draw(SpriteBatch s) {
        }
        public virtual void DrawActive(SpriteBatch s) {
            Draw(s);
        }
    }
}
