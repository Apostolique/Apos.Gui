using System.Runtime.CompilerServices;
using Apos.Input;
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
            get => _nextText;
            set {
                if (value != _nextText) {
                    _nextText = value;
                    _isUpdateText = true;
                }
            }
        }
        public int Padding { get; set; } = 10;
        public Color Color { get; set; }
        public int FontSize {
            get => _nextSize;
            set {
                if (value != _nextSize) {
                    _nextSize = value;
                    _isUpdateText = true;
                }
            }
        }

        public override void UpdatePrefSize(GameTime gameTime) {
            if (_isUpdateText) {
                Cache();
            }

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
            _text = _nextText;
            _fontSize = _nextSize;
            _cachedSize = GuiHelper.MeasureString(_text, _fontSize);
            _isUpdateText = false;
        }

        public static Label Put(string text, int fontSize = 30, Color? color = null, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            // 1. Check if Label with id already exists.
            //      a. If already exists. Get it.
            //      b  If not, create it.
            // 4. Ping it.
            id = GuiHelper.CurrentIMGUI.CreateId(id, isAbsoluteId);
            GuiHelper.CurrentIMGUI.TryGetValue(id, out IComponent c);

            color ??= new Color(200, 200, 200);

            Label a;
            if (c is Label) {
                a = (Label)c;
                a.Text = text;
                a.Color = color.Value;
                a.FontSize = fontSize;
            } else {
                a = new Label(id, text, fontSize, color.Value);
            }

            IParent parent = GuiHelper.CurrentIMGUI.GrabParent(a);

            if (a.LastPing != InputHelper.CurrentFrame) {
                a.LastPing = InputHelper.CurrentFrame;
                a.Index = parent.NextIndex();
            }

            return a;
        }

        protected string _text;
        protected int _fontSize;
        protected Vector2 _cachedSize;

        protected bool _isUpdateText = false;
        protected string _nextText = "";
        protected int _nextSize;
    }
}
