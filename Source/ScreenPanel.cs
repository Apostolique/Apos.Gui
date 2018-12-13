using Microsoft.Xna.Framework;

namespace AposGui {
    /// <summary>
    /// Goal: The ScreenPanel is always the same size as the Window.
    /// </summary>
    public class ScreenPanel : Panel {
        public ScreenPanel(GameWindow iWindow) {
            _window = iWindow;
            Position = new Point(0, 0);
        }
        private GameWindow _window;
        public override int Width => _window.ClientBounds.Width;
        public override int Height => _window.ClientBounds.Height;
    }
}