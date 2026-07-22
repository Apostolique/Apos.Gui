using System;
using System.Collections.Generic;
using Apos.Input;
using Apos.Shapes;
using Apos.Tweens;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace Apos.Gui {
    /// <summary>
    /// Unorganized helper functions for Apos.Gui.
    /// </summary>
    public static class GuiHelper {
        /// <summary>
        /// Usually called once in a game's LoadContent.
        /// </summary>
        /// <param name="game">Game instance.</param>
        /// <param name="fontSystem">The font system to use for the UI.</param>
        public static void Setup(Game game, FontSystem fontSystem) {
            InputHelper.Setup(game);
            FontSystem = fontSystem;
            ShapeBatch = new ShapeBatch(game.GraphicsDevice, game.Content);
        }
        /// <summary>
        /// Called at the start of an update loop.
        /// </summary>
        public static void UpdateSetup(GameTime gameTime) {
            InputHelper.UpdateSetup();
            TweenHelper.UpdateSetup(gameTime);
        }
        /// <summary>
        /// Called at the end of an update loop.
        /// </summary>
        public static void UpdateCleanup() {
            InputHelper.UpdateCleanup();
        }
        /// <summary>
        /// Currently selected IMGUI. This is the IMGUI to use when adding components.
        /// </summary>
        public static IMGUI CurrentIMGUI { get; set; } = null!;

        /// <value>The scale of the UI. Defaults to 1f.</value>
        public static float Scale {
            get => _scale;
            set {
                // TODO: Delay scale change until next frame? It's not likely to be desirable to update before a layout refresh.
                if (value > 0f) {
                    _scale = value;
                    _virtualScale = (float)Math.Ceiling(_scale);
                    _finalScale = 1f / _virtualScale;
                }
            }
        }
        public static float WindowWidth => InputHelper.WindowWidth / Scale;
        public static float WindowHeight => InputHelper.WindowHeight / Scale;

        /// <summary>
        /// Used to convert between the screen and UI coordinate system.
        /// </summary>
        public static Matrix UIMatrix => Matrix.CreateScale(Scale, Scale, 1f);
        /// <summary>
        /// Position of OldMouse in the UI coordinate system.
        /// </summary>
        public static Vector2 OldMouse => Vector2.Transform(InputHelper.OldMouse.Position.ToVector2(), Matrix.Invert(UIMatrix));
        /// <summary>
        /// Position of NewMouse in the UI coordinate system.
        /// </summary>
        public static Vector2 Mouse => Vector2.Transform(InputHelper.NewMouse.Position.ToVector2(), Matrix.Invert(UIMatrix));

        /// <summary>SpriteBatch used to draw the UI.</summary>
        public static ShapeBatch ShapeBatch { get; set; } = null!;
        /// <summary>FontSystem used in the UI.</summary>
        public static FontSystem FontSystem { get; set; } = null!;
        /// <summary>Defaults to LinearClamp.</summary>
        public static SamplerState GuiSampler { get; set; } = SamplerState.LinearClamp;

        /// <summary>Returns a font with a given size.</summary>
        public static DynamicSpriteFont GetFont(int size) {
            return FontSystem.GetFont((int)(size * _virtualScale));
        }
        /// <summary>Measures text using a font with a given size. The size is as tight as possible to the text.</summary>
        public static Vector2 MeasureStringTight(string text, int size) {
            return GetFont(size).MeasureString(text) * _finalScale;
        }
        /// <summary>Measures text using a font with a given size. The line height is the same no matter the text content.</summary>
        public static Vector2 MeasureString(string text, int size) {
            var font = GetFont(size);
            return new Vector2(font.MeasureString(text).X, font.FontSize * CountLines(text)) * _finalScale;
        }
        /// <summary>Used when drawing text to the screen. The text has to be scaled by this value.</summary>
        public static Vector2 FontScale => new Vector2(_finalScale);
        /// <summary>
        /// Limits the area that the ShapeBatch is allowed to draw to using the built-in
        /// Apos.Shapes clip rect. Unlike a scissor rect, this is handled in the shader so it
        /// doesn't break the current batch.
        /// Clips don't intersect; pushing a new clip replaces the active one until it's popped.
        /// </summary>
        /// <param name="r">The clip rectangle in UI coordinates.</param>
        public static void PushClip(RectangleF r) {
            _clipStack.Push(_currentClip);
            _currentClip = r;
            ShapeBatch.SetClipRect(r);
        }
        /// <summary>
        /// Restores the clip rectangle that was active before the matching <see cref="PushClip"/>.
        /// </summary>
        public static void PopClip() {
            _currentClip = _clipStack.Pop();
            ShapeBatch.SetClipRect(_currentClip);
        }

        /// <summary>
        /// Calls begin on the ShapeBatch with the UI transform matrix and clears any active clip.
        /// IMGUI.Draw calls this for you; only call it manually if you draw the UI yourself.
        /// </summary>
        public static void Begin() {
            _currentClip = null;
            _clipStack.Clear();
            ShapeBatch.Begin(UIMatrix);
            ShapeBatch.SetClipRect(null);
        }
        /// <summary>
        /// Calls end on the ShapeBatch.
        /// </summary>
        public static void End() {
            ShapeBatch.End();
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

        private static float _scale = 1f;
        private static float _virtualScale = 1f;
        private static float _finalScale = 1f;
        private static RectangleF? _currentClip = null;
        private static Stack<RectangleF?> _clipStack = new Stack<RectangleF?>();
    }
}
