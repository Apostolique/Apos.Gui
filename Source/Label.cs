using System.Runtime.CompilerServices;
using Apos.Input;
using FontStashSharp;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    public class Label : Component {
        public Label(int id, string text, int fontSize, Color c) : base(id) {
            // TODO: Optimize cache, no need to cache twice. Will need to also figure out updating from outside.
            Text = text;
            Color = c;
            FontSize = fontSize;
        }

        public string Text {
            get => _text;
            set {
                if (value != _text) {
                    _text = value;
                    _cachedSize = GuiHelper.MeasureString(_text, FontSize);
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
                    _cachedSize = GuiHelper.MeasureString(_text, FontSize);
                }
            }
        }

        public override void UpdatePrefSize(GameTime gameTime) {
            PrefWidth = _cachedSize.X + Padding * 2;
            PrefHeight = _cachedSize.Y + Padding * 2;
        }
        public override void Draw(GameTime gameTime) {
            GuiHelper.PushScissor(Clip);

            var font = GuiHelper.GetFont(FontSize);
            GuiHelper.SpriteBatch.DrawString(font, Text, XY + new Vector2(Padding), Color, GuiHelper.FontScale);

            GuiHelper.PopScissor();
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
    }
}
