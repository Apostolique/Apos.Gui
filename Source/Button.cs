using System;
using Apos.Input;
using FontStashSharp;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Apos.Gui {
    public class Button : Component {
        public Button() { }
        public Button(string text) {
            Text = text;
        }

        public bool Clicked {
            get;
            set;
        } = false;
        public string Text {
            get;
            set;
        } = "";

        public override void UpdateSetup() {
            if (Clicked) {
                Clicked = false;
            }
        }
        public override void UpdateInput() {
            if (Clip.Contains(GuiHelper.Mouse) && Default.MouseInteraction.Pressed()) {
                _pressed = true;
            }
            if (_pressed) {
                Default.MouseInteraction.HeldOnly();
            }
            if (_pressed && Default.MouseInteraction.Released()) {
                if (Clip.Contains(GuiHelper.Mouse))
                    Clicked = true;
                _pressed = false;
            }
        }

        public override void Draw() {
            GuiHelper.SetScissor(Clip);

            // TODO: Visually show the press down
            Color c = Clicked ? Color.Red : _pressed ? Color.Green : Color.White;
            GuiHelper.SpriteBatch.FillRectangle(Bounds, c * 0.5f);

            var font = GuiHelper.GetFont(30);
            GuiHelper.SpriteBatch.DrawString(font, Text, XY, Color.White, GuiHelper.FontScale);

            GuiHelper.ResetScissor();
        }

        private bool _pressed = false;

        public static Button Use(IMGUI ui, string text, int id = 0) {
            // 1. Check if button with name already exists.
            //      a. If already exists. Get it.
            //      b  If not, create it.
            // 4. Ping it.
            var fullName = $"button{text}{id}";

            ui.TryGetValue(fullName, out Component c);

            if (!(c is Button)) {
                c = new Button();
                ui.Add(fullName, c);
                Console.WriteLine("Created: " + fullName);
            }
            ((Button)c).Text = text;
            if (c.LastPing != InputHelper.CurrentFrame) {
                c.LastPing = InputHelper.CurrentFrame;
                if (ui.CurrentParent != null) {
                    c.Index = ui.CurrentParent.NextIndex();
                }
            }

            return (Button)c;
        }
    }
}
