using System;
using Microsoft.Xna.Framework;

namespace AposGui {
    /// <summary>
    /// Goal: Container that can hold Components.
    ///       It can also be scrolled over using the mouse wheel.
    /// </summary>
    public class PanelVerticalScroll : Panel {
        public PanelVerticalScroll() { }
        public override bool UpdateInput() {
            bool used = base.UpdateInput();
            bool isHovered = IsInsideClip(GuiHelper.MouseToUI());

            if (!used && isHovered) {
                int scrollWheelDelta = GuiHelper.ScrollWheelDelta();
                if (scrollWheelDelta != 0) {
                    Offset = new Point(Offset.X, (int) Math.Min(Math.Max(Offset.Y + scrollWheelDelta, Height - Size.Height), 0));
                    used = true;
                }
            }

            return used;
        }
    }
}