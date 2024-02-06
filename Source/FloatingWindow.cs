using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Apos.Gui {
    public class FloatingWindow(int id) : Vertical(id) {
        public override bool IsFocusable { get; set; } = true;
        public override bool IsFloatable { get; set; } = true;

        public override void UpdateInput(GameTime gameTime) {
            base.UpdateInput(gameTime);

            if (Clip.Contains(GuiHelper.Mouse) && Default.MouseInteraction.Pressed()) {
                _mousePressed = true;
                _dragDelta = XY - GuiHelper.Mouse;
                GrabFocus(this);
            }

            if (IsFocused) {
                if (_mousePressed) {
                    if (Default.MouseInteraction.Released()) {
                        _mousePressed = false;
                    } else {
                        Default.MouseInteraction.Consume();
                        XY = GuiHelper.Mouse + _dragDelta;
                    }
                }
            }
        }
        public override void UpdatePrefSize(GameTime gameTime) {
            base.UpdatePrefSize(gameTime);

            PrefHeight += 20;
        }

        public override void Draw(GameTime gameTime) {
            GuiHelper.PushScissor(Clip);
            GuiHelper.SpriteBatch.FillRectangle(Bounds, Color.Black);
            GuiHelper.SpriteBatch.FillRectangle(new RectangleF(Left, Bottom - 20, Width, 20), Color.White * 0.5f);
            GuiHelper.PopScissor();

            base.Draw(gameTime);
        }

        protected bool _mousePressed = false;
        protected Vector2 _dragDelta = Vector2.Zero;

        public static new FloatingWindow Push([CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            id = GuiHelper.CurrentIMGUI.TryCreateId(id, isAbsoluteId, out IComponent c);

            FloatingWindow a;
            if (c is FloatingWindow d) {
                a = d;
            } else {
                a = new FloatingWindow(id);
            }

            GuiHelper.CurrentIMGUI.GrabParent(a);

            GuiHelper.CurrentIMGUI.Push(a);

            return a;
        }
        public static new void Pop() {
            GuiHelper.CurrentIMGUI.Pop();
        }
    }
}
