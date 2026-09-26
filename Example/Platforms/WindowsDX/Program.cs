using System;
using Apos.Gui;

namespace GameProject {
    public static class Program {
        [STAThread]
        static void Main() {
            GuiHelper.Clipboard = new WinFormsClipboard();
            using var game = new GameRoot();
            game.Run();
        }
    }
}