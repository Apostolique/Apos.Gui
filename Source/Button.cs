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
        public int Padding {
            get;
            set;
        } = 10;

        public override void UpdatePrefSize() {
            var size = GuiHelper.MeasureString(Text, 30);
            PrefWidth = size.X + Padding * 2;
            PrefHeight = size.Y + Padding * 2;
        }
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
                _contained = Clip.Contains(GuiHelper.Mouse);
            }
            if (_pressed && Default.MouseInteraction.Released()) {
                if (Clip.Contains(GuiHelper.Mouse))
                    Clicked = true;
                _pressed = false;
            }
        }

        public override void Draw() {
            GuiHelper.SetScissor(Clip);

            if (Clicked) {
                GuiHelper.SpriteBatch.FillRectangle(Bounds, Color.White * 0.5f);
            } else if (_pressed) {
                if (_contained) {
                    GuiHelper.SpriteBatch.FillRectangle(Bounds, Color.White * 0.2f);
                } else {
                    GuiHelper.SpriteBatch.FillRectangle(Bounds, Color.White * 0.15f);
                }
            }
            GuiHelper.SpriteBatch.DrawRectangle(Bounds, Color.White, 2f);

            var font = GuiHelper.GetFont(30);
            GuiHelper.SpriteBatch.DrawString(font, Text, XY + new Vector2(Padding), Color.White, GuiHelper.FontScale);

            GuiHelper.ResetScissor();
        }

        private bool _pressed = false;
        private bool _contained = false;

        public static Button Use(string text, int id = 0) {
            // 1. Check if button with id already exists.
            //      a. If already exists. Get it.
            //      b  If not, create it.
            // 4. Ping it.
            var fullName = $"button{(id == 0 ? GuiHelper.CurrentIMGUI.NextId() : id)}";

            GuiHelper.CurrentIMGUI.TryGetValue(fullName, out IComponent c);

            Button a;
            if (c is Button) {
                a = (Button)c;
            } else {
                a = new Button();
                GuiHelper.CurrentIMGUI.Add(fullName, a);
            }

            a.Text = text;
            if (a.LastPing != InputHelper.CurrentFrame) {
                a.LastPing = InputHelper.CurrentFrame;
                if (GuiHelper.CurrentIMGUI.CurrentParent != null) {
                    a.Index = GuiHelper.CurrentIMGUI.CurrentParent.NextIndex();
                }
            }

            return a;
        }
    }
}
