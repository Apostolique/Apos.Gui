using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    /// <summary>
    /// Fills the window and centers its children in it. Scrolls when they're taller than the window.
    /// </summary>
    public class MenuPanel : Vertical {
        /// <summary>Creates the panel. <see cref="Push"/> calls this on the first frame it's put.</summary>
        public MenuPanel(long id) : base(id) {
            Align = CrossAlign.Center;
            Spacing = 8f;
        }

        /// <inheritdoc/>
        public override void UpdatePrefSize(GameTime gameTime) {
            base.UpdatePrefSize(gameTime);

            PrefWidth = Root.Width;
            PrefHeight = Root.Height;
        }
        /// <inheritdoc/>
        public override void UpdateLayout(GameTime gameTime) {
            // MenuPanel is a root component so it can set its own position.
            X = 0f;
            Y = 0f;

            // After a resize, jump to the new spot instead of easing there.
            if (Height != _lastHeight) {
                _easedOffsetY.Snap(ClampOffsetY(_easedOffsetY.Target));
                _lastHeight = Height;
            }

            base.UpdateLayout(gameTime);
        }

        /// <summary>Centers the children when they're shorter than the panel. Otherwise keeps the offset inside the scrollable range.</summary>
        protected override float ClampOffsetY(float y) {
            float content = ContentSize.Y;
            return content < Height ? (Height - content) / 2f : MathHelper.Clamp(y, Height - content, 0f);
        }

        /// <summary>The height from the last layout. A new height snaps the scroll instead of easing it.</summary>
        protected float _lastHeight = -1f;

        /// <summary>Starts a panel that fills the UI. The components put after it go in it until <see cref="Pop"/>.</summary>
        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="file">The caller's file. The same line in two files gives two different ids.</param>
        public static new MenuPanel Push([CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            return IMGUI.Push(id, isAbsoluteId, file, id => new MenuPanel(id));
        }
        /// <summary>Ends the panel started with <see cref="Push"/>.</summary>
        public static new void Pop() {
            GuiHelper.CurrentIMGUI.Pop();
        }
    }
}
