using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace AposGui {
    public class LabelDynamic : Label {
        //constructors
        public LabelDynamic() {
            _text = delegate() {
                return "Text Missing";
            };
        }
        public LabelDynamic(Func<string> iText) {
            _text = iText;
        }

        //public vars
        new Func<string> _text;
        new Size2 _textSize {
            get {
                if (_text != null) {
                    return GuiHelper.Font.MeasureString(_text());
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