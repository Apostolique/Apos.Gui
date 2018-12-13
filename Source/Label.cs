using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;

namespace AposGui
{
    /// <summary>
    /// Goal: A text component.
    /// </summary>
    class Label : Component
    {
        public Label(BitmapFont iFont) : this(iFont, "Text Missing") {
        }
        public Label(BitmapFont iFont, string iText) {
            _font = iFont;
            _text = iText;
            _textSize = _font.MeasureString(_text);
            Width = PrefWidth;
            Height = PrefHeight;

            NormalColor = Color.White;
            ActiveColor = new Color(150, 150, 150);
        }
        protected string _text;
        protected Size2 _textSize;
        protected BitmapFont _font;
        public Color NormalColor {
            get; set;
        }
        public Color ActiveColor {
            get; set;
        }

        public override void Draw(SpriteBatch s) {
            Draw(s, NormalColor);
        }
        public override void DrawActive(SpriteBatch s) {
            Draw(s, ActiveColor);
        }
        public virtual void Draw(SpriteBatch s, Color c) {
            int halfWidth = Width / 2;
            int textHalfWidth = PrefWidth / 2;

            int halfHeight = Height / 2;
            int textHalfHeight = PrefHeight / 2;

            s.DrawString(_font, _text, new Vector2(Left + halfWidth - textHalfWidth, Top + halfHeight - textHalfHeight), c, ClippingRect);
        }
        public override int PrefWidth => (int)_textSize.Width;
        public override int PrefHeight => (int)_textSize.Height;
    }
}
