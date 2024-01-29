using System.Runtime.CompilerServices;
using FontStashSharp;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    public class Label : Component {
        public Label(int id, string text, int fontSize, Color c) : base(id) {
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
            GuiHelper.PushScissor(Clip);

            var font = GuiHelper.GetFont(_fontSize);
            // TODO: Hover color?
            GuiHelper.SpriteBatch.DrawString(font, _text, XY + new Vector2(Padding), Color, GuiHelper.FontScale);

            GuiHelper.PopScissor();
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

        public static Label Put(string text, int fontSize = 30, Color? color = null, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            id = GuiHelper.CurrentIMGUI.TryCreateId(id, isAbsoluteId, out IComponent c);

            color ??= new Color(200, 200, 200);

            Label a;
            if (c is Label d) {
                a = d;
                a.Text = text;
                a.Color = color.Value;
                a.FontSize = fontSize;
            } else {
                a = new Label(id, text, fontSize, color.Value);
            }

            GuiHelper.CurrentIMGUI.GrabParent(a);

            return a;
        }
    }
}
