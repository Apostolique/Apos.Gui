using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    /// <summary>Text that doesn't wrap. <see cref="Paragraph"/> is text that wraps.</summary>
    public class Label : Component {
        /// <summary>Creates the label. <see cref="Put"/> calls this on the first frame it's put.</summary>
        public Label(long id, string text, int fontSize, Color c) : base(id) {
            Text = text;
            FontSize = fontSize;
            Color = c;
        }

        /// <summary>The text to draw.</summary>
        public string Text {
            get => _text;
            set {
                if (value != _text) {
                    _text = value;
                    _isDirty = true;
                }
            }
        }
        /// <summary>Space around the text on each side, in UI units.</summary>
        public int Padding { get; set; } = 10;
        /// <summary>Draws this label. Null uses <see cref="DefaultLook"/>.</summary>
        public Action<Label>? Look { get; set; }
        /// <summary>Draws every label that doesn't have its own <see cref="Look"/>.</summary>
        public static Action<Label> DefaultLook { get; set; } = DrawDefault;
        /// <summary>Color of the text.</summary>
        public Color Color { get; set; }
        /// <summary>Size of the text.</summary>
        public int FontSize {
            get => _fontSize;
            set {
                if (value != _fontSize) {
                    _fontSize = value;
                    _isDirty = true;
                }
            }
        }

        /// <inheritdoc/>
        public override void UpdatePrefSize(GameTime gameTime) {
            Cache();
            PrefWidth = _cachedSize.X + Padding * 2;
            PrefHeight = _cachedSize.Y + Padding * 2;
        }
        /// <inheritdoc/>
        public override void Draw(GameTime gameTime) {
            // Only clip when the text doesn't fit. The parent's clip already covers the rest.
            bool overflows = _cachedSize.X + Padding * 2 > Width || _cachedSize.Y + Padding * 2 > Height;
            if (overflows) GuiHelper.PushClip(Clip);
            (Look ?? DefaultLook)(this);
            if (overflows) GuiHelper.PopClip();
        }

        /// <summary>The text in its color.</summary>
        public static void DrawDefault(Label l) {
            GuiHelper.ShapeBatch.DrawString(GuiHelper.Font, l.Text, l.XY + new Vector2(l.Padding), l.FontSize, l.Color);
        }

        /// <summary>Measures the text again if it or the font size changed.</summary>
        protected void Cache() {
            if (_isDirty) {
                _cachedSize = GuiHelper.MeasureString(_text, _fontSize);
                _isDirty = false;
            }
        }

        /// <summary>Backs <see cref="Text"/>.</summary>
        protected string _text = null!;
        /// <summary>Backs <see cref="FontSize"/>.</summary>
        protected int _fontSize;
        /// <summary>The size of the text from the last <see cref="Cache"/>, without the padding.</summary>
        protected Vector2 _cachedSize;
        /// <summary>The text or the font size changed since the last <see cref="Cache"/>.</summary>
        protected bool _isDirty = false;

        /// <summary>A line of text.</summary>
        /// <param name="text">The text to draw.</param>
        /// <param name="fontSize">Size of the text.</param>
        /// <param name="color">Color of the text. Null uses <see cref="Theme.Text"/>.</param>
        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="file">The caller's file. The same line in two files gives two different ids.</param>
        public static Label Put(string text, int fontSize = 30, Color? color = null, [CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            Color c = color ?? Theme.Text;

            Label a = IMGUI.Put(id, isAbsoluteId, file, id => new Label(id, text, fontSize, c));
            a.Text = text;
            a.Color = c;
            a.FontSize = fontSize;

            return a;
        }
    }
}
