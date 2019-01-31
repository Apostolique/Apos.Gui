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
        //constructors
        public Button() : this(new Component()) { }
        public Button(Component c) {
            Item = c;
        }

        //public vars
        public virtual bool ShowBox {
            get;
            set;
        } = true;
        public override bool OldIsHovered {
            get => base.OldIsHovered;
            set {
                base.OldIsHovered = value;
                Item.OldIsHovered = value;
            }
        }
        public override bool IsHovered {
            get => base.IsHovered;
            set {
                base.IsHovered = value;
                Item.IsHovered = value;
            }
        }
        public override bool IsFocusable {
            get;
            set;
        } = true;
        public override bool HasFocus {
            get => base.HasFocus;
            set {
                base.HasFocus = value;
                Item.HasFocus = value;
            }
        }
        public virtual Component Item {
            get => _item;
            set {
                _item = value;
                _item.Parent = this;
            }
        }
        public override int PrefWidth => Item.PrefWidth;
        public override int PrefHeight => Item.PrefHeight;

        //public functions
        public override void UpdateSetup() {
            base.UpdateSetup();

            Item.Width = Width;
            Item.Height = Height;
            Item.Position = Position;
            Item.ClippingRect = ClippingRect;

            Item.UpdateSetup();
        }
        public override void Draw(SpriteBatch s) {
            SetScissor(s);
            if (ShowBox) {
                if (IsHovered) {
                    s.FillRectangle(BoundingRect, new Color(20, 20, 20));
                } else {
                    s.FillRectangle(BoundingRect, Color.Black);
                }
            }

            Item.Draw(s);

            if (ShowBox && HasFocus) {
                s.DrawRectangle(BoundingRect, Color.White, 2);
            }

            ResetScissor(s);
        }

        //private vars
        protected Component _item;
    }
}