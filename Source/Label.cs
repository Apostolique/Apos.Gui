using Apos.Input;
using FontStashSharp;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    public class Label : Component {
        public Label() { }
        public Label(string text) {
            Text = text;
        }

        public string Text {
            get;
            set;
        } = "";
        public int Padding {
            get;
            set;
        } = 10;

        public override void UpdatePrefSize() {
            var size = GuiHelper.MeasureString(Text, 30);
            PrefWidth = size.X + Padding * 2;
            PrefHeight = size.Y + Padding * 2;
        }
        public override void Draw() {
            GuiHelper.SetScissor(Clip);

            var font = GuiHelper.GetFont(30);
            GuiHelper.SpriteBatch.DrawString(font, Text, XY + new Vector2(Padding), new Color(200, 200, 200), GuiHelper.FontScale);

            GuiHelper.ResetScissor();
        }

        public static Label Put(string text, int id = 0) {
            // 1. Check if Label with id already exists.
            //      a. If already exists. Get it.
            //      b  If not, create it.
            // 4. Ping it.
            var fullName = $"label{(id == 0 ? GuiHelper.CurrentIMGUI.NextId() : id)}";

            IParent? parent = GuiHelper.CurrentIMGUI.CurrentParent;
            GuiHelper.CurrentIMGUI.TryGetValue(fullName, out IComponent c);

            Label a;
            if (c is Label) {
                a = (Label)c;
            } else {
                a = new Label();
                GuiHelper.CurrentIMGUI.Add(fullName, a);
            }

            a.Text = text;
            if (a.LastPing != InputHelper.CurrentFrame) {
                a.LastPing = InputHelper.CurrentFrame;
                if (parent != null) {
                    a.Index = parent.NextIndex();
                }
            }

            return a;
        }
    }
}
