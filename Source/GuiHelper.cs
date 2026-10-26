using System;
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
        /// The IMGUI that components get added to. A UI makes itself current from UpdateSetup to Cleanup, and while it lays out and draws.
        /// </summary>
        public static IMGUI CurrentIMGUI { get; set; } = null!;

        /// <summary>Same as <see cref="IMGUI.ToUI"/> on the current UI.</summary>
        public static Vector2 ToUI(Vector2 screen) => CurrentIMGUI.ToUI(screen);
        /// <summary>Same as <see cref="IMGUI.Pointer"/> on the current UI.</summary>
        public static Vector2 Pointer => CurrentIMGUI.Pointer;
        /// <summary>Same as <see cref="IMGUI.OldMouse"/> on the current UI.</summary>
        public static Vector2 OldMouse => CurrentIMGUI.OldMouse;
        /// <summary>Same as <see cref="IMGUI.Mouse"/> on the current UI.</summary>
        public static Vector2 Mouse => CurrentIMGUI.Mouse;

        /// <summary>ShapeBatch used to draw the UI.</summary>
        public static ShapeBatch ShapeBatch { get; set; } = null!;
        /// <summary>Font used in the UI.</summary>
        public static ShapeFont Font { get; set; } = null!;
        /// <summary>Where textboxes copy to and paste from. Defaults to a clipboard that stays inside the game.</summary>
        public static IClipboard Clipboard { get; set; } = new GameClipboard();
        /// <summary>Types into textboxes when there's no physical keyboard. Null turns it off, which is the default.</summary>
        public static IOnScreenKeyboard? OnScreenKeyboard { get; set; }
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
        /// The new clip stays inside the active ones, so draws only land where all of them overlap.
        /// Each clip keeps its own rounding and rotation. Up to four clips that cut across each other
        /// apply at once, while a clip that sits whole inside another doesn't count toward that.
        /// </summary>
        /// <param name="r">The clip rectangle in UI coordinates.</param>
        /// <param name="rounding">Corner radius of the clip. A value equal to half the rectangle's smallest side makes it a circle.</param>
        /// <param name="rotation">Rotation of the clip around its center, in radians.</param>
        /// <param name="aaSize">Anti-aliasing size of the clip edge in pixels. Set to 0f for hard scissor-like edges.</param>
        public static void PushClip(RectangleF r, float rounding = 0f, float rotation = 0f, float aaSize = 1.5f) {
            ShapeBatch.PushClip(r, rounding, rotation, aaSize);
        }
        /// <summary>
        /// Takes off the clip the matching <see cref="PushClip"/> added.
        /// </summary>
        public static void PopClip() {
            ShapeBatch.PopClip();
        }

        /// <summary>
        /// Calls begin on the ShapeBatch with the current UI's matrix and clears any active clip.
        /// IMGUI.Draw calls this for you. Only call it if you draw the UI yourself.
        /// </summary>
        public static void Begin() {
            ShapeBatch.Begin(CurrentIMGUI.Matrix, samplerState: GuiSampler);
            ShapeBatch.SetClipRect(null);
        }
        /// <summary>
        /// Calls end on the ShapeBatch.
        /// </summary>
        public static void End() {
            ShapeBatch.End();
        }
    }
}
