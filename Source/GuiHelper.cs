using System;
using System.Collections.Generic;
using Apos.Input;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Apos.Gui {
    public static class GuiHelper {
        public static void Setup(Game game, FontSystem fontSystem) {
            InputHelper.Setup(game);
            FontSystem = fontSystem;
            SpriteBatch = new SpriteBatch(game.GraphicsDevice);
        }
        public static void UpdateSetup() {
            InputHelper.UpdateSetup();
        }
        public static void UpdateCleanup() {
            InputHelper.UpdateCleanup();
        }
        public static IMGUI CurrentIMGUI { get; set; } = null!;

        /// <value>The scale of the UI. Defaults to 1f.</value>
        public static float Scale {
            get => _scale;
            set {
                if (value > 0) {
                    _scale = value;
                    _virtualScale = (float)Math.Ceiling(_scale);
                    _finalScale = 1f / _virtualScale;
                }
            }
        }
        public static Matrix UIMatrix => Matrix.CreateScale(Scale, Scale, 1);
        public static Vector2 Mouse => Vector2.Transform(InputHelper.NewMouse.Position.ToVector2(), Matrix.Invert(UIMatrix));

        public static SpriteBatch SpriteBatch { get; set; } = null!;
        public static FontSystem FontSystem { get; set; } = null!;
        /// <value>Defaults to LinearClamp.</value>
        public static SamplerState GuiSampler { get; set; } = SamplerState.LinearClamp;

        public static DynamicSpriteFont GetFont(int size) {
            return FontSystem.GetFont((int)(size * _virtualScale));
        }
        public static Vector2 MeasureStringTight(string text, int size) {
            return GetFont(size).MeasureString(text) * _finalScale;
        }
        public static Vector2 MeasureString(string text, int size) {
            var font = GetFont(size);
            return new Vector2(font.MeasureString(text).X, font.FontSize * CountLines(text)) * _finalScale;
        }
        public static Vector2 FontScale => new Vector2(_finalScale);
        /// <summary>
        /// Uses a rectangle to limit the area that the spritebatch is allowed to draw to.
        /// The rectangle is converted into screen coordinates.
        /// </summary>
        /// <param name="r">The rectangle to use for the spritebatch scissor in UI coordinates.</param>
        public static void SetScissor(RectangleF r) {
            // TODO: Optimize begin call somehow. Maybe there is no drawing between scissor swaps?
            if (_beginCalled) {
                End();
            }

            int x = (int)(r.X * Scale);
            int y = (int)(r.Y * Scale);
            int w = (int)(r.Width * Scale);
            int h = (int)(r.Height * Scale);

            _scissorStack.Push(SpriteBatch.GraphicsDevice.ScissorRectangle);
            SpriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle(x, y, w, h);
            Begin();
        }
        /// <summary>
        /// Uses a rectangle to limit the area that the spritebatch is allowed to draw to.
        /// </summary>
        /// <param name="r">The rectangle to use for the spritebatch scissor in screen coordinates.</param>
        public static void ResetScissor() {
            if (_beginCalled) {
                End();
            }

            SpriteBatch.GraphicsDevice.ScissorRectangle = _scissorStack.Pop();
            Begin();
        }

        /// <summary>
        /// Calls begin on the spritebatch with the UI rasterizer state, transform matrix and sampler state.
        /// </summary>
        private static void Begin() {
            SpriteBatch.Begin(rasterizerState: _rasterState, transformMatrix: UIMatrix, samplerState: GuiSampler);
            _beginCalled = true;
        }
        /// <summary>
        /// Calls end on the spritebatch.
        /// </summary>
        private static void End() {
            SpriteBatch.End();
            _beginCalled = false;
        }
        private static int CountLines(string text) {
            // https://stackoverflow.com/a/40928366/1710293
            // Defaults to 1 because in a UI, you want the text to always have some height.
            if (text == null || text == string.Empty)
                return 1;
            int index = -1;
            int count = 0;
            while (-1 != (index = text.IndexOf('\n', index + 1)))
                count++;

            return count + 1;
        }
        public static int CombineHash<T1, T2>(T1 value1, T2 value2) {
            unchecked {
                int hash = 17;
                hash *= 31 + value1!.GetHashCode();
                hash *= 31 + value2!.GetHashCode();

                return hash;
            }
        }

        private static float _scale = 1f;
        private static float _virtualScale = 1f;
        private static float _finalScale = 1f;
        private static RasterizerState _rasterState = new RasterizerState {
            ScissorTestEnable = true
        };
        private static bool _beginCalled = false;
        private static Stack<Rectangle> _scissorStack = new Stack<Rectangle>();
    }
}
