using System.Runtime.CompilerServices;

namespace Apos.Gui {
    /// <summary>
    /// Stacks its children from left to right. See <see cref="StackPanel"/>.
    /// </summary>
    public class Horizontal(long id) : StackPanel(id, Orientation.Horizontal) {
        public static Horizontal Push([CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            return IMGUI.Push(id, isAbsoluteId, file, id => new Horizontal(id));
        }
        public static void Pop() {
            GuiHelper.CurrentIMGUI.Pop();
        }
    }
}
