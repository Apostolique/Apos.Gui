using System.Runtime.CompilerServices;
using Apos.Input;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Apos.Gui {
    public class FloatingWindow : Panel {
        public FloatingWindow(int id) : base(id) { }

        public override bool IsFocusable { get; set; } = true;
        public override bool IsFloatable { get; set; } = true;

        public override void UpdatePrefSize(GameTime gameTime) {
            base.UpdatePrefSize(gameTime);

            PrefHeight += 20;
        }
        public override void UpdateSetup(GameTime gameTime) {
            if (_moveNext) {
                XY = _nextXY;
                _moveNext = false;
            }

            base.UpdateSetup(gameTime);
        }
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
                        _moveNext = true;
                        _nextXY = GuiHelper.Mouse + _dragDelta;
                    }
                }
            }
        }
        public override void Draw(GameTime gameTime) {
            GuiHelper.SetScissor(Clip);
            GuiHelper.SpriteBatch.FillRectangle(new RectangleF(Left, Bottom - 20, Width, 20), Color.White * 0.5f);
            GuiHelper.ResetScissor();

            base.Draw(gameTime);
        }

        public static FloatingWindow Push([CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            // 1. Check if window with id already exists.
            //      a. If already exists. Get it.
            //      b  If not, create it.
            // 2. Parent it.
            // 3. Push it on the stack.
            // 4. Ping it.
            id = GuiHelper.CurrentIMGUI.CreateId(id, isAbsoluteId);
            GuiHelper.CurrentIMGUI.TryGetValue(id, out IComponent c);

            FloatingWindow a;
            if (c is FloatingWindow) {
                a = (FloatingWindow)c;
            } else {
                a = new FloatingWindow(id);
            }

            IParent parent = GuiHelper.CurrentIMGUI.GrabParent(a);

            if (a.LastPing != InputHelper.CurrentFrame) {
                a.Reset();
                a.LastPing = InputHelper.CurrentFrame;
                a.Index = parent.NextIndex();
            }

            GuiHelper.CurrentIMGUI.Push(a);

            return a;
        }
        public static void Pop() {
            GuiHelper.CurrentIMGUI.Pop();
        }

        protected bool _mousePressed = false;
        protected Vector2 _dragDelta = Vector2.Zero;
        protected bool _moveNext = false;
        public Vector2 _nextXY = Vector2.Zero;
    }
}
