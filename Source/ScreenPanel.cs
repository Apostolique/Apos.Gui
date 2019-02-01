using Microsoft.Xna.Framework;

namespace Apos.Gui {
    /// <summary>
    /// Goal: The ScreenPanel is always the same size as the Window.
    /// </summary>
    public class ScreenPanel : Panel {
        public ScreenPanel() { }
        public override int Width => GuiHelper­.WindowWidth;
        public override int Height => GuiHelper.WindowHeight;
    }
}