using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AposGui {
    /// <summary>
    /// Goal: Unorganized helper functions for AposGui.
    /// </summary>
    public static class GuiHelper {
        public static GameWindow Window;
        private static RasterizerState _rasterState = new RasterizerState {
            ScissorTestEnable = true
        };
        public static float Scale = 1f;
        private static bool _beginCalled = false;

        public static Matrix GetUIMatrix() {
            return Matrix.CreateScale(Scale, Scale, 1);
        }

        public static int ScrollWheelDelta() {
            return Input.NewMouse.ScrollWheelValue - Input.OldMouse.ScrollWheelValue;
        }

        public static int WindowWidth => (int)(Window.ClientBounds.Width * (1 / GuiHelper.Scale));
        public static int WindowHeight => (int)(Window.ClientBounds.Height * (1 / GuiHelper.Scale));

        public static Vector2 MouseToUI() {
            return ScreenToUI(new Vector2(Input.NewMouse.X, Input.NewMouse.Y));
        }
        public static Vector2 ScreenToUI(Vector2 v) {
            return new Vector2(v.X * (1 / Scale), v.Y * (1 / Scale));
        }
        public static Vector2 UIToScreen(Vector2 v) {
            return new Vector2(v.X * Scale, v.Y * Scale);
        }

        public static void Begin(SpriteBatch s) {
            s.Begin(rasterizerState: _rasterState, transformMatrix: GetUIMatrix());
            _beginCalled = true;
        }
        public static void End(SpriteBatch s) {
            s.End();
            _beginCalled = false;
        }
        public static void SetScissor(SpriteBatch s, Rectangle r) {
            if (_beginCalled) {
                End(s);
            }
            int x = (int)(r.X * Scale);
            int y = (int)(r.Y * Scale);
            int w = (int)(r.Width * Scale);
            int h = (int)(r.Height * Scale);

            s.GraphicsDevice.ScissorRectangle = new Rectangle(x, y, w, h);
            Begin(s);
        }
        public static void DrawGui(SpriteBatch s, Component c) {
            c.Draw(s);
            if (_beginCalled) {
                End(s);
            }
        }
    }
}