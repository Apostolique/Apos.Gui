using System;
using System.Collections.Generic;
using Apos.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;

namespace Apos.Gui {
    /// <summary>
    /// Goal: Unorganized helper functions for AposGui.
    /// </summary>
    public static class GuiHelper {
        // Group: Public Variables
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
        public static float FontSize {
            get;
            set;
        }
        public static GameWindow Window {
            get;
            set;
        }
        public static int WindowWidth => (int)(Window.ClientBounds.Width * (1 / GuiHelper.Scale));
        public static int WindowHeight => (int)(Window.ClientBounds.Height * (1 / GuiHelper.Scale));
        public static SamplerState GuiSampler = SamplerState.LinearClamp;
        public static List<Action> NextLoopActions = new List<Action>();

        // Group: Public Functions
        public static Matrix GetUIMatrix() {
            return Matrix.CreateScale(Scale, Scale, 1);
        }
        public static int ScrollWheelDelta() {
            return InputHelper.NewMouse.ScrollWheelValue - InputHelper.OldMouse.ScrollWheelValue;
        }
        public static Point MouseToUI() {
            return ScreenToUI(InputHelper.NewMouse.Position);
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
        public static void UpdateSetup() {
            for (int i = NextLoopActions.Count - 1; i >= 0; i--) {
                NextLoopActions[i]();
                NextLoopActions.RemoveAt(i);
            }
        }
        public static void DrawGui(SpriteBatch s, Component c) {
            c.Draw(s);
            if (_beginCalled) {
                End(s);
            }
        }
        public static void DrawString(SpriteBatch s, string t, Vector2 p, Color c) {
            float virtualScale = (float)Math.Ceiling(Scale);
            float finalScale = 1 / virtualScale;

            Font.Size = FontSize * virtualScale;
            Vector2 scale = new Vector2(finalScale);
            s.DrawString(Font, t, p, c, scale);
        }
        public static Vector2 MeasureString(string text) {
            float virtualScale = (float)Math.Ceiling(Scale);
            float finalScale = 1 / virtualScale;

            Font.Size = FontSize * virtualScale;
            return Font.MeasureString(text) * finalScale;
        }

        // Group: Private Variables
        private static float _scale = 1f;
        private static RasterizerState _rasterState = new RasterizerState {
            ScissorTestEnable = true
        };
        private static bool _beginCalled = false;
    }
}