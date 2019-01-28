using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using SpriteFontPlus;

namespace AposGui {
    /// <summary>
    /// Goal: A text component.
    /// </summary>
    public class Label : Component {
        //constructors
        public Label() : this("Text Missing") { }
        public Label(string iText) {
            _text = iText;
            Width = PrefWidth;
            Height = PrefHeight;
        }

        //public vars
        public Color NormalColor {
            get;
            set;
        } = Color.White;
        public Color ActiveColor {
            get;
            set;
        } = new Color(150, 150, 150);
        public override int PrefWidth => (int) _textSize.Width;
        public override int PrefHeight => (int) _textSize.Height;

        //public functions
        public override void Draw(SpriteBatch s) {
            SetScissor(s);
            GuiHelper.DrawString(s, _text, new Vector2(Left, Top), getColor());
            ResetScissor(s);
        }

        //private vars
        protected string _text;
        protected Size2 _textSize {
            get {
                if (_text != null) {
                    return GuiHelper.Font.MeasureString(_text);
                }
                return Size2.Empty;
            }
        }

        //private functions
        protected virtual Color getColor() {
            if (IsHovered || HasFocus) {
                return ActiveColor;
            }
            return NormalColor;
        }
        protected virtual void drawTextCentered(SpriteBatch s, string text) {
            int halfWidth = Width / 2;
            int halfHeight = Height / 2;

            s.DrawString(GuiHelper.Font, text, new Vector2(Left + halfWidth, Top + halfHeight), getColor());
        }
    }
}