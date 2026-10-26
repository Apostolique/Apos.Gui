using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    /// <summary>
    /// Fills the window and centers its children in it. Scrolls when they're taller than the window.
    /// </summary>
    public class MenuPanel : Vertical {
        public MenuPanel(long id) : base(id) {
            Align = CrossAlign.Center;
            Spacing = 8f;
        }

        public override void UpdatePrefSize(GameTime gameTime) {
            base.UpdatePrefSize(gameTime);

            PrefWidth = Root.Width;
            PrefHeight = Root.Height;
        }
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

        protected override float ClampOffsetY(float y) {
            float content = ContentSize.Y;
            return content < Height ? (Height - content) / 2f : MathHelper.Clamp(y, Height - content, 0f);
        }

        protected float _lastHeight = -1f;

        public static new MenuPanel Push([CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            return IMGUI.Push(id, isAbsoluteId, file, id => new MenuPanel(id));
        }
        public static new void Pop() {
            GuiHelper.CurrentIMGUI.Pop();
        }
    }
}
