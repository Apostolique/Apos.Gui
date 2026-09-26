using System;
using System.Collections.Generic;
using Apos.Input;
using Apos.Shapes;
using Apos.Tweens;
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
        /// <param name="font">The font to use for the UI.</param>
        public static void Setup(Game game, ShapeFont font) {
            InputHelper.Setup(game);
            Font = font;
            ShapeBatch = new ShapeBatch(game.GraphicsDevice);
        }
        /// <summary>
        /// Called at the start of an update loop.
        /// </summary>
        public static void UpdateSetup(GameTime gameTime) {
            InputHelper.UpdateSetup(gameTime);
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

        /// <summary>ShapeBatch used to draw the UI.</summary>
        public static ShapeBatch ShapeBatch { get; set; } = null!;
        /// <summary>Font used in the UI.</summary>
        public static ShapeFont Font { get; set; } = null!;
        /// <summary>Defaults to LinearClamp.</summary>
        public static SamplerState GuiSampler { get; set; } = SamplerState.LinearClamp;

        /// <summary>Measures text drawn at a given size. Empty text is still one line tall so that it keeps some height in a UI.</summary>
        public static Vector2 MeasureString(string text, float size) {
            Vector2 measure = Font.MeasureString(text, size);
            return new Vector2(measure.X, MathF.Max(measure.Y, Font.LineHeight * size));
        }
        /// <summary>
        /// Limits the area that the ShapeBatch is allowed to draw to using the built-in
        /// Apos.Shapes clip rect. Unlike a scissor rect, this is handled in the shader so it
        /// doesn't break the current batch.
        /// Clips don't intersect; pushing a new clip replaces the active one until it's popped.
        /// </summary>
        /// <param name="r">The clip rectangle in UI coordinates.</param>
        /// <param name="rounding">Corner radius of the clip. A value equal to half the rectangle's smallest side makes it a circle.</param>
        /// <param name="rotation">Rotation of the clip around its center, in radians.</param>
        /// <param name="aaSize">Anti-aliasing size of the clip edge in pixels. Set to 0f for hard scissor-like edges.</param>
        public static void PushClip(RectangleF r, float rounding = 0f, float rotation = 0f, float aaSize = 1.5f) {
            _clipStack.Push(_currentClip);
            _currentClip = (r, rounding, rotation, aaSize);
            ShapeBatch.SetClipRect(r, rounding, rotation, aaSize);
        }
        /// <summary>
        /// Restores the clip rectangle that was active before the matching <see cref="PushClip"/>.
        /// </summary>
        public static void PopClip() {
            _currentClip = _clipStack.Pop();
            ApplyClip(_currentClip);
        }

        /// <summary>
        /// Calls begin on the ShapeBatch with the UI transform matrix and clears any active clip.
        /// IMGUI.Draw calls this for you; only call it manually if you draw the UI yourself.
        /// </summary>
        public static void Begin() {
            _currentClip = null;
            _clipStack.Clear();
            ShapeBatch.Begin(UIMatrix, samplerState: GuiSampler);
            ShapeBatch.SetClipRect(null);
        }
        /// <summary>
        /// Calls end on the ShapeBatch.
        /// </summary>
        public static void End() {
            ShapeBatch.End();
        }
        private static void ApplyClip((RectangleF Rect, float Rounding, float Rotation, float AaSize)? clip) {
            if (clip is { } c) {
                ShapeBatch.SetClipRect(c.Rect, c.Rounding, c.Rotation, c.AaSize);
            } else {
                ShapeBatch.SetClipRect(null);
            }
        }

        private static float _scale = 1f;
        private static (RectangleF Rect, float Rounding, float Rotation, float AaSize)? _currentClip = null;
        private static Stack<(RectangleF Rect, float Rounding, float Rotation, float AaSize)?> _clipStack = new Stack<(RectangleF Rect, float Rounding, float Rotation, float AaSize)?>();
    }
}
