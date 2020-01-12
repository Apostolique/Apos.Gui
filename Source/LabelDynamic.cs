using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

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

        public override void Draw() {
            SetScissor();
            DrawString(_text(), new Vector2(Left, Top), getColor());
            ResetScissor();
        }

        // Group: Private Variables

        new Func<string> _text = () => "Text Missing";
        new Size2 _textSize => MeasureString(_text());
    }
}