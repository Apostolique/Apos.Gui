namespace Apos.Gui {
    /// <summary>
    /// Goal: The ScreenPanel is always the same size as the Window.
    /// </summary>
    public class ScreenPanel : Panel {

        // Group: Constructors

        public ScreenPanel() { }

        // Group: Public Variables

        public override int Width => GuiHelper­.WindowWidth;
        public override int Height => GuiHelper.WindowHeight;
    }
}