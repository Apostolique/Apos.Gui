using System.Runtime.CompilerServices;
using Apos.Tweens;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    /// <summary>
    /// Fills the window and centers its children in it. Scrolls when they're taller than the window.
    /// </summary>
    public class MenuPanel : Vertical {
        public MenuPanel(int id) : base(id) {
            Align = CrossAlign.Center;
        }

        public override void UpdatePrefSize(GameTime gameTime) {
            base.UpdatePrefSize(gameTime);

            PrefWidth = GuiHelper.WindowWidth;
            PrefHeight = GuiHelper.WindowHeight;
        }
        public override void UpdateLayout(GameTime gameTime) {
            // MenuPanel is a root component so it can set its own position.
            X = 0f;
            Y = 0f;

            // After a resize, jump to the new spot instead of easing there.
            if (Height != _lastHeight) {
                float offset = ClampOffsetY(_offsetYTween.B);
                _offsetYTween.A = offset;
                _offsetYTween.B = offset;
                _offsetYTween.StartTime = TweenHelper.TotalMS;
                _offsetYTween.Duration = 0;
                _lastHeight = Height;
            }

            base.UpdateLayout(gameTime);
        }

        protected override float ClampOffsetY(float y) {
            float content = ContentSize.Y;
            return content < Height ? (Height - content) / 2f : MathHelper.Clamp(y, Height - content, 0f);
        }

        protected float _lastHeight = -1f;

        public static new MenuPanel Push([CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            return IMGUI.Push(id, isAbsoluteId, id => new MenuPanel(id));
        }
        public static new void Pop() {
            GuiHelper.CurrentIMGUI.Pop();
        }
    }
}
