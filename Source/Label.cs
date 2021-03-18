using System.Runtime.CompilerServices;
using Apos.Input;
using FontStashSharp;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    public class Label : Component {
        public Label(int id, string text) : base(id) {
            Text = text;
        }

        public string Text {
            get => _text;
            set {
                if (value != _text) {
                    _text = value;
                    _size = GuiHelper.MeasureString(_text, 30);
                }
            }
        }
        public int Padding { get; set; } = 10;

        public override void UpdatePrefSize(GameTime gameTime) {
            PrefWidth = _size.X + Padding * 2;
            PrefHeight = _size.Y + Padding * 2;
        }
        public override void Draw(GameTime gameTime) {
            GuiHelper.SetScissor(Clip);

            var font = GuiHelper.GetFont(30);
            GuiHelper.SpriteBatch.DrawString(font, Text, XY + new Vector2(Padding), new Color(200, 200, 200), GuiHelper.FontScale);

            GuiHelper.ResetScissor();
        }

        public static Label Put(string text, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            // 1. Check if Label with id already exists.
            //      a. If already exists. Get it.
            //      b  If not, create it.
            // 4. Ping it.
            id = GuiHelper.CurrentIMGUI.CreateId(id, isAbsoluteId);
            GuiHelper.CurrentIMGUI.TryGetValue(id, out IComponent c);

            Label a;
            if (c is Label) {
                a = (Label)c;
                a.Text = text;
            } else {
                a = new Label(id, text);
            }

            IParent? parent = GuiHelper.CurrentIMGUI.GrabParent(a);

            if (a.LastPing != InputHelper.CurrentFrame) {
                a.LastPing = InputHelper.CurrentFrame;
                if (parent != null) {
                    a.Index = parent.NextIndex();
                }
            }

            return a;
        }

        protected string _text;
        protected Vector2 _size;
    }
}
