using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    public class Label : Component {
        public Label(long id, string text, int fontSize, Color c) : base(id) {
            Text = text;
            FontSize = fontSize;
            Color = c;
        }

        public string Text {
            get => _text;
            set {
                if (value != _text) {
                    _text = value;
                    _isDirty = true;
                }
            }
        }
        public int Padding { get; set; } = 10;
        /// <summary>Draws this label. Null uses <see cref="DefaultLook"/>.</summary>
        public Action<Label>? Look { get; set; }
        /// <summary>Draws every label that doesn't have its own <see cref="Look"/>.</summary>
        public static Action<Label> DefaultLook { get; set; } = DrawDefault;
        public Color Color { get; set; }
        public int FontSize {
            get => _fontSize;
            set {
                if (value != _fontSize) {
                    _fontSize = value;
                    _isDirty = true;
                }
            }
        }

        public override void UpdatePrefSize(GameTime gameTime) {
            Cache();
            PrefWidth = _cachedSize.X + Padding * 2;
            PrefHeight = _cachedSize.Y + Padding * 2;
        }
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

        protected void Cache() {
            if (_isDirty) {
                _cachedSize = GuiHelper.MeasureString(_text, _fontSize);
                _isDirty = false;
            }
        }

        protected string _text = null!;
        protected int _fontSize;
        protected Vector2 _cachedSize;
        protected bool _isDirty = false;

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
