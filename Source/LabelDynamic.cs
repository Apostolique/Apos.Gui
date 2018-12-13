using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;

namespace AposGui {
    class LabelDynamic : Label {
        public LabelDynamic(BitmapFont iFont) : base(iFont) {
            _text = delegate() {
                return "Text Missing";
            };
        }
        public LabelDynamic(BitmapFont iFont, Func<string> iText) : base(iFont) {
            _text = iText;
        }
        new Func<string> _text;
        new Size2 _textSize {
            get {
                if (_text != null) {
                    return _font.MeasureString(_text());
                }
                return Size2.Empty;
            }
        }
        public override void Draw(SpriteBatch s, Color c) {
            int halfWidth = Width / 2;
            int textHalfWidth = PrefWidth / 2;

            int halfHeight = Height / 2;
            int textHalfHeight = PrefHeight / 2;

            s.DrawString(_font, _text(), new Vector2(Left + halfWidth - textHalfWidth, Top + halfHeight - textHalfHeight), c, ClippingRect);
        }
        public override int PrefWidth => (int) _textSize.Width;
        public override int PrefHeight => (int) _textSize.Height;
    }
}