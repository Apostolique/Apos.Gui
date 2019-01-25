using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;

namespace AposGui {
    public class LabelDynamic : Label {
        //constructors
        public LabelDynamic(BitmapFont iFont) : base(iFont) {
            _text = delegate() {
                return "Text Missing";
            };
        }
        public LabelDynamic(BitmapFont iFont, Func<string> iText) : base(iFont) {
            _text = iText;
        }

        //public vars
        new Func<string> _text;
        new Size2 _textSize {
            get {
                if (_text != null) {
                    return _font.MeasureString(_text());
                }
                return Size2.Empty;
            }
        }
        public override int PrefWidth => (int) _textSize.Width;
        public override int PrefHeight => (int) _textSize.Height;

        //public functions
        public override void Draw(SpriteBatch s) {
            SetScissor(s);
            drawTextCentered(s, _text());
            ResetScissor(s);
        }
    }
}