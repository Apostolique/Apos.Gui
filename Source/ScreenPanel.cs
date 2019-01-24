using Microsoft.Xna.Framework;

namespace AposGui {
    /// <summary>
    /// Goal: The ScreenPanel is always the same size as the Window.
    /// </summary>
    public class ScreenPanel : Panel {
        public ScreenPanel() {
            Position = new Point(0, 0);
        }
        public override int Width => GuiHelper­.WindowWidth;
        public override int Height => GuiHelper.WindowHeight;
    }
}