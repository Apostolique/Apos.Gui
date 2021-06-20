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
        /// <summary>
        /// Get or Add a label based on if the id's component already exists as an label &
        /// add it to the parent list.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="id"></param>
        /// <param name="isAbsoluteId"></param>
        /// <returns></returns>
        public static Label Put(string text, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            id = GuiHelper.CurrentIMGUI.CreateId(id, isAbsoluteId);
            GuiHelper.CurrentIMGUI.TryGetValue(id, out IComponent c);

            Label a;
            if (c is Label label) {
                a = label;
                a.Text = text;
            } else {
                a = new Label(id, text);
            }

            IParent? parent = GuiHelper.CurrentIMGUI.GrabParent(a);

            if (a.LastPing != InputHelper.CurrentFrame) {
                a.LastPing = InputHelper.CurrentFrame;
                if (parent != null) {
                    a.Index = parent.NextIndex(); 
                    // I am pretty sure we always get from Panel. So this just sets _nextChildIndex++
                }
            }

            return a;
        }

        protected string _text;
        protected Vector2 _size;
    }
}
