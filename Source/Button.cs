using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace AposGui {
    /// <summary>
    /// Goal: A button component that handles actions.
    /// </summary>
    public class Button : Border {
        //constructors
        public Button() : this(new Component()) { }
        public Button(Component c) : this(c, 0, 0, 0, 0) { }
        public Button(Component c, int iMarginLeft, int iMarginTop, int iMarginRight, int iMarginBottom) : base(c, iMarginLeft, iMarginTop, iMarginRight, iMarginBottom) {
            IsFocusable = true;
            ShowBox = true;
        }

        //public vars
        public virtual bool ShowBox {
            get;
            set;
        }

        //public functions
        public override Component GetFinal() {
            return this;
        }
        public override Component GetFinalInverse() {
            return this;
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
    }
}