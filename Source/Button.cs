using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace AposGui {
    /// <summary>
    /// Goal: A button component that handles actions.
    /// </summary>
    public class Button : Component {
        public Button() : this(new Component()) { }
        public Button(Component iItem) {
            _item = iItem;
            _buttonActions = new List<ButtonConditionAction>();
            IsFocusable = true;
            OldIsHovered = false;
            IsHovered = false;
            ShowBox = true;
        }
        protected struct ButtonConditionAction {
            public Func<Button, bool> condition;
            public Action<Button> buttonAction;
            public ButtonConditionAction(Func<Button, bool> iCondition, Action<Button> iButtonAction) {
                condition = iCondition;
                buttonAction = iButtonAction;
            }
        }
        protected Component _item;
        public virtual bool OldIsHovered {
            get;
            set;
        }
        public virtual bool IsHovered {
            get;
            set;
        }
        public virtual bool ShowBox {
            get;
            set;
        }
        protected List<ButtonConditionAction> _buttonActions;

        public override Point Position {
            get => base.Position;
            set {
                base.Position = value;
                if (_item != null) {
                    _item.Position = base.Position;
                }
            }
        }
        public override int Width {
            get => base.Width;
            set {
                base.Width = value;
                if (_item != null) {
                    _item.Width = base.Width;
                }
            }
        }
        public override int Height {
            get => base.Height;
            set {
                base.Height = value;
                if (_item != null) {
                    _item.Height = base.Height;
                }
            }
        }
        public override Rectangle ClippingRect {
            get {
                return base.ClippingRect;
            }
            set {
                base.ClippingRect = value;

                if (_item != null) {
                    _item.ClippingRect = base.ClippingRect;
                }
            }
        }
        public void AddAction(Func<Button, bool> condition, Action<Button> bAction) {
            _buttonActions.Add(new ButtonConditionAction(condition, bAction));
        }
        public override bool UpdateInput() {
            OldIsHovered = IsHovered;
            IsHovered = IsInsideClip(new Point(Input.NewMouse.X, Input.NewMouse.Y));

            bool isUsed = false;

            foreach (ButtonConditionAction bca in _buttonActions) {
                if (bca.condition(this)) {
                    bca.buttonAction(this);
                    isUsed = true;
                }
            }

            return isUsed;
        }
        public override void Draw(SpriteBatch s) {
            if (ShowBox) {
                if (HasFocus) {
                    s.FillRectangle(new RectangleF(Left, Top, Width, Height), new Color(20, 20, 20));
                } else {
                    s.FillRectangle(new RectangleF(Left, Top, Width, Height), Color.Black);
                }
            }

            if (ShowBox || HasFocus) {
                _item.DrawActive(s);
            } else {
                _item.Draw(s);
            }

            if (ShowBox && HasFocus) {
                s.DrawLine(Left, Top, Left, Bottom, Color.White, 2);
                s.DrawLine(Right, Top, Right, Bottom, Color.White, 2);
                s.DrawLine(Left, Top, Right, Top, Color.White, 2);
                s.DrawLine(Left, Bottom, Right, Bottom, Color.White, 2);
            }
        }
        public override int PrefWidth => _item.PrefWidth;
        public override int PrefHeight => _item.PrefHeight;
    }
}