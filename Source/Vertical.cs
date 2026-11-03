using System.Runtime.CompilerServices;

namespace Apos.Gui {
    /// <summary>
    /// Stacks its children from top to bottom. See <see cref="StackPanel"/>.
    /// </summary>
    public class Vertical(long id) : StackPanel(id, Orientation.Vertical) {
        /// <summary>Starts a vertical stack. The components put after it go in it until <see cref="Pop"/>.</summary>
        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="file">The caller's file. The same line in two files gives two different ids.</param>
        public static Vertical Push([CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            return IMGUI.Push(id, isAbsoluteId, file, id => new Vertical(id));
        }
        /// <summary>Ends the stack started with <see cref="Push"/>.</summary>
        public static void Pop() {
            GuiHelper.CurrentIMGUI.Pop();
        }
    }
}
