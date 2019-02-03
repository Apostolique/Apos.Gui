using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using SpriteFontPlus;

namespace Apos.Gui {
    public class LabelDynamic : Label {
        // Group: Constructors
        public LabelDynamic() : this(() => "Text Missing") { }
        public LabelDynamic(Func<string> iText) {
            _text = iText;
            Width = PrefWidth;
            Height = PrefHeight;
        }

        // Group: Public Variables
        public override int PrefWidth => (int) _textSize.Width;
        public override int PrefHeight => (int) _textSize.Height;

        // Group: Public Functions
        public override void Draw(SpriteBatch s) {
            SetScissor(s);
            GuiHelper.DrawString(s, _text(), new Vector2(Left, Top), getColor());
            ResetScissor(s);
        }

        // Group: Private Variables
        new Func<string> _text = () => "Text Missing";
        new Size2 _textSize => GuiHelper.MeasureString(_text());
    }
}