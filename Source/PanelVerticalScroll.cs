using System;
using Microsoft.Xna.Framework;

namespace AposGui {
    /// <summary>
    /// Goal: Container that can hold Components.
    ///       It can also be scrolled over using the mouse wheel.
    /// </summary>
    public class PanelVerticalScroll : Panel {
        //constructors
        public PanelVerticalScroll() { }

        //public functions
        public override bool UpdateInput() {
            bool isUsed = base.UpdateInput();

            if (!isUsed && IsHovered) {
                int scrollWheelDelta = GuiHelper.ScrollWheelDelta();
                if (scrollWheelDelta != 0) {
                    Offset = new Point(Offset.X, (int) Math.Min(Math.Max(Offset.Y + scrollWheelDelta, ClippingRect.Height - Size.Height), 0));
                    isUsed = true;
                }
            }

            return isUsed;
        }
    }
}