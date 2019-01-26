using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;

namespace AposGui {
    /// <summary>
    /// Goal: Unorganized helper functions for AposGui.
    /// </summary>
    public static class GuiHelper {
        //public vars
        public static float Scale {
            get => _scale;
            set {
                if (value > 0) {
                    _scale = value;
                }
            }
        }
        public static DynamicSpriteFont Font {
            get;
            set;
        }
        public static GameWindow Window;
        public static int WindowWidth => (int)(Window.ClientBounds.Width * (1 / GuiHelper.Scale));
        public static int WindowHeight => (int)(Window.ClientBounds.Height * (1 / GuiHelper.Scale));
        public static SamplerState GuiSampler = SamplerState.PointClamp;

        //public functions
        public static Matrix GetUIMatrix() {
            return Matrix.CreateScale(Scale, Scale, 1);
        }
        public static int ScrollWheelDelta() {
            return Input.NewMouse.ScrollWheelValue - Input.OldMouse.ScrollWheelValue;
        }
        public static Point MouseToUI() {
            return ScreenToUI(Input.NewMouse.Position);
        }
        public static Point ScreenToUI(Point p) {
            return new Point((int)(p.X * (1 / Scale)), (int)(p.Y * (1 / Scale)));
        }
        public static Point UIToScreen(Point p) {
            return new Point((int)(p.X * Scale), (int)(p.Y * Scale));
        }
        public static void Begin(SpriteBatch s) {
            s.Begin(rasterizerState: _rasterState, transformMatrix: GetUIMatrix(), samplerState: GuiSampler);
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
        public static void ResetScissor(SpriteBatch s, Rectangle r) {
            if (_beginCalled) {
                End(s);
            }

            s.GraphicsDevice.ScissorRectangle = r;
            Begin(s);
        }
        public static void DrawGui(SpriteBatch s, Component c) {
            c.Draw(s);
            if (_beginCalled) {
                End(s);
            }
        }

        //private vars
        private static float _scale = 1f;
        private static RasterizerState _rasterState = new RasterizerState {
            ScissorTestEnable = true
        };
        private static bool _beginCalled = false;
    }
}