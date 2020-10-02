using System;
using System.Collections.Generic;
using Apos.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;

namespace Apos.Gui {
    /// <summary>
    /// Unorganized helper functions for Apos.Gui.
    /// </summary>
    public static class GuiHelper {

        // Group: Public Variables

        /// <value>The scale of the UI. Defaults to 1f.</value>
        public static float Scale {
            get => _scale;
            set {
                if (value > 0) {
                    _scale = value;
                    _virtualScale = (float)Math.Ceiling(_scale);
                    _finalScale = 1f / _virtualScale;
                    SetFontSize();
                }
            }
        }
        /// <value>The font used by components that display text.</value>
        public static DynamicSpriteFont Font {
            get => _font;
            set {
                _font = value;
                _fontSize = _font.Size;
                SetFontSize();
            }
        }
        /// <value>Size for the font.</value>
        public static int FontSize {
            get => _fontSize;
            set {
                _fontSize = value;
                SetFontSize();
            }
        }
        /// <value>Your game's window. Used by responsive components.</value>
        public static GameWindow Window => InputHelper.Window;
        /// <returns>The window's width in UI scale.</returns>
        public static int WindowWidth => (int)(Window.ClientBounds.Width * (1 / Scale));
        /// <returns>The window's height in UI scale.</returns>
        public static int WindowHeight => (int)(Window.ClientBounds.Height * (1 / Scale));
        /// <value>Defaults to LinearClamp.</value>
        public static SamplerState GuiSampler {
            get;
            set;
        } = SamplerState.LinearClamp;
        /// <summary>SpriteBatch used and managed by the Gui.</summary>
        public static SpriteBatch SpriteBatch {
            get;
            set;
        }
        /// <summary>
        /// This is a list of actions that will be executed at the start of a game loop. This is useful for queueing
        /// actions that will be done in the next loop cycle.
        /// This is required when doing something that will mess up layouts before the draw function.
        /// </summary>
        public static List<Action> NextLoopActions {
            get;
            set;
        } = new List<Action>();
        /// <returns>A matrix representing UI coordinate system.</returns>
        public static Matrix UIMatrix => Matrix.CreateScale(Scale, Scale, 1);
        /// <returns>The scroll wheel delta since the last update.</returns>
        public static int ScrollWheelDelta => InputHelper.NewMouse.ScrollWheelValue - InputHelper.OldMouse.ScrollWheelValue;

        // Group: Public Functions

        /// <returns>The mouse in the UI coordinate system.</returns>
        public static Point MouseToUI() {
            return ScreenToUI(InputHelper.NewMouse.Position);
        }
        /// <param name="p">A point in the screen coordinate system.</param>
        /// <returns>The point converted into it's equivalent UI coordinate.</returns>
        public static Point ScreenToUI(Point p) {
            return new Point(ScreenToUI(p.X), ScreenToUI(p.Y));
        }
        /// <param name="n">A number on the screen's X or Y axis.</param>
        /// <returns>The number converted into it's equivalent UI coordinate on the X or Y axis.</returns>
        public static int ScreenToUI(int n) {
            return (int)(n * (1 / Scale));
        }
        /// <param name="p">A point in the UI coordinate system.</param>
        /// <returns>The point converted into it's equivalent screen coordinate.</returns>
        public static Point UIToScreen(Point p) {
            return new Point(UIToScreen(p.X), UIToScreen(p.Y));
        }
        /// <param name="n">A number on the UI's X or Y axis.</param>
        /// <returns>The number converted into it's equivalent screen coordinate on the X or Y axis.</returns>
        public static int UIToScreen(int n) {
            return (int)(n * Scale);
        }
        /// <summary>
        /// Uses a rectangle to limit the area that the spritebatch is allowed to draw to.
        /// The rectangle is converted into screen coordinates.
        /// </summary>
        /// <param name="r">The rectangle to use for the spritebatch scissor in UI coordinates.</param>
        public static void SetScissor(Rectangle r) {
            if (_beginCalled) {
                End();
            }
            int x = (int)(r.X * Scale);
            int y = (int)(r.Y * Scale);
            int w = (int)(r.Width * Scale);
            int h = (int)(r.Height * Scale);

            SpriteBatch.GraphicsDevice.ScissorRectangle = new Rectangle(x, y, w, h);
            Begin();
        }
        /// <summary>
        /// Uses a rectangle to limit the area that the spritebatch is allowed to draw to.
        /// </summary>
        /// <param name="r">The rectangle to use for the spritebatch scissor in screen coordinates.</param>
        public static void ResetScissor(Rectangle r) {
            if (_beginCalled) {
                End();
            }

            SpriteBatch.GraphicsDevice.ScissorRectangle = r;
            Begin();
        }
        /// <summary>
        /// Call Setup in the game's LoadContent.
        /// </summary>
        /// <param name="game">Your game object.</param>
        /// <param name="font">The font that you want to use for the UI.</param>
        public static void Setup(Game game, DynamicSpriteFont font) {
            InputHelper.Setup(game);
            Font = font;

            SpriteBatch = new SpriteBatch(InputHelper.Game.GraphicsDevice);
        }
        /// <summary>
        /// This should be called at the start of the game's update loop.
        /// </summary>
        public static void UpdateSetup() {
            InputHelper.UpdateSetup();
            for (int i = NextLoopActions.Count - 1; i >= 0; i--) {
                NextLoopActions[i]();
                NextLoopActions.RemoveAt(i);
            }
        }
        /// <summary>
        /// Call this at the end of your update loop.
        /// </summary>
        public static void UpdateCleanup() {
            InputHelper.UpdateCleanup();
        }
        /// <summary>
        /// This should be used on a UI root component.
        /// This function tracks the spritebatch to call being and end correctly.
        /// </summary>
        /// <param name="c">A root component to draw.</param>
        public static void DrawGui(Component c) {
            c.Draw();
            if (_beginCalled) {
                End();
            }
        }
        /// <summary>
        /// Draws a string using the Font, FontSize and UI scale.
        /// </summary>
        /// <param name="t">The string to draw.</param>
        /// <param name="p">The position for the string.</param>
        /// <param name="c">The color for the string.</param>
        public static void DrawString(string t, Vector2 p, Color c) {
            SpriteBatch.DrawString(Font, t, p, c, new Vector2(_finalScale));
        }
        /// <param name="t">The string to measure.</param>
        public static Vector2 MeasureString(string t) {
            return Font.MeasureString(t) * _finalScale;
        }

        // Group: Private Functions

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
        private static void SetFontSize() {
            if (_font != null) {
                _font.Size = (int)(_fontSize * _virtualScale);
            }
        }

        // Group: Private Variables

        private static DynamicSpriteFont _font;
        private static int _fontSize = 30;
        private static float _scale = 1f;
        private static float _virtualScale = 1f;
        private static float _finalScale = 1f;
        private static RasterizerState _rasterState = new RasterizerState {
            ScissorTestEnable = true
        };
        /// <summary>
        /// Tracks whether begin has been called on the spritebatch.
        /// </summary>
        private static bool _beginCalled = false;
    }
}
