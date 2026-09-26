using System.Runtime.CompilerServices;

namespace Apos.Gui {
    /// <summary>
    /// Stacks its children from top to bottom. See <see cref="StackPanel"/>.
    /// </summary>
    public class Vertical(long id) : StackPanel(id, Orientation.Vertical) {
        public static Vertical Push([CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            return IMGUI.Push(id, isAbsoluteId, file, id => new Vertical(id));
        }
        public static void Pop() {
            GuiHelper.CurrentIMGUI.Pop();
        }
    }
}
