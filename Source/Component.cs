using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Optional;

namespace Apos.Gui {
    /// <summary>
    /// Goal: The core of a user interface.
    ///       Handles everything from how something is drawn,
    ///       to how to handle inputs.
    /// </summary>
    public class Component {
        //constructors
        public Component() { }

        //public vars
        public virtual Point Position {
            get;
            set;
        } = new Point(0, 0);
        public virtual int Width {
            get;
            set;
        } = 100;
        public virtual int Height {
            get;
            set;
        } = 100;
        public virtual int PrefWidth => Width;
        public virtual int PrefHeight => Height;
        public virtual int Left => Position.X;
        public virtual int Top => Position.Y;
        public virtual int Right => Position.X + Width;
        public virtual int Bottom => Position.Y + Height;
        public virtual Rectangle BoundingRect => new Rectangle(Left, Top, Width, Height);
        public virtual Rectangle ClippingRect {
            get {
                return _clippingRect.ValueOr(() => BoundingRect);
            }
            set {
                _clippingRect = Option.Some(ClipRectangle(value));
            }
        }
        public virtual Option<Component> Parent {
            get;
            set;
        } = Option.None<Component>();
        public virtual bool OldIsHovered {
            get;
            set;
        } = false;
        public virtual bool IsHovered {
            get;
            set;
        } = false;
        public virtual bool IsFocusable {
            get;
            set;
        } = false;
        public virtual bool HasFocus {
            get;
            set;
        }
        public virtual Action<Component> GrabFocus {
            get;
            set;
        } = (Component c) => {};

        //public functions
        public void AddHoverCondition(Func<Component, bool> c) {
            _hoverConditions.Add(c);
        }
        public void AddAction(Func<Component, bool> c, Func<Component, bool> o) {
            _conditionOperations.Add(new ConditionOperation(c, o));
        }
        public virtual Component GetPrevious() {
            return Parent.Map(parent => parent.GetPrevious(this)).ValueOr(this);
        }
        public virtual Component GetNext() {
            return Parent.Map(parent => parent.GetNext(this)).ValueOr(this);
        }
        public virtual Component GetPrevious(Component c) {
            return GetPrevious();
        }
        public virtual Component GetNext(Component c) {
            return GetNext();
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
        public virtual bool IsInside(Point v) {
            return IsInside(BoundingRect, v);
        }
        public virtual bool IsInsideClip(Point v) {
            return IsInside(ClippingRect, v);
        }
        public virtual bool IsInside(Rectangle r, Point v) {
            return r.Left < v.X && r.Right > v.X && r.Top < v.Y && r.Bottom > v.Y;
        }
        public virtual void SetScissor(SpriteBatch s) {
            _oldScissor = s.GraphicsDevice.ScissorRectangle;
            GuiHelper.SetScissor(s, ClippingRect);
        }
        public virtual void ResetScissor(SpriteBatch s) {
            GuiHelper.ResetScissor(s, _oldScissor);
        }
        public virtual void UpdateSetup() { }
        public virtual bool UpdateInput() {
            bool isUsed = false;
            OldIsHovered = IsHovered;
            IsHovered = false;
            foreach (Func<Component, bool> c in _hoverConditions) {
                if (c(this)) {
                    IsHovered = true;
                    break;
                }
            }

            foreach (ConditionOperation co in _conditionOperations) {
                if (co.Condition(this)) {
                    isUsed = co.Operation(this) || isUsed;
                }
            }

            if (!HasFocus && IsFocusable && isUsed) {
                GrabFocus(this);
            }

            return isUsed;
        }
        public virtual void Update() { }
        public virtual void Draw(SpriteBatch s) { }

        //private vars
        protected struct ConditionOperation {
            public ConditionOperation(Func<Component, bool> c, Func<Component, bool> o) {
                Condition = c;
                Operation = o;
            }
            public Func<Component, bool> Condition;
            public Func<Component, bool> Operation;
        }
        protected Option<Rectangle> _clippingRect = Option.None<Rectangle>();
        protected Rectangle _oldScissor;
        protected List<Func<Component, bool>> _hoverConditions = new List<Func<Component, bool>>();
        protected List<ConditionOperation> _conditionOperations = new List<ConditionOperation>();
    }
}