using System.Runtime.CompilerServices;

namespace Apos.Gui {
    /// <summary>
    /// Stacks its children from left to right. See <see cref="StackPanel"/>.
    /// </summary>
    public class Horizontal(long id) : StackPanel(id, Orientation.Horizontal) {
        /// <summary>Starts a horizontal stack. The components put after it go in it until <see cref="Pop"/>.</summary>
        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="file">The caller's file. The same line in two files gives two different ids.</param>
        public static Horizontal Push([CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            return IMGUI.Push(id, isAbsoluteId, file, id => new Horizontal(id));
        }
        /// <summary>Ends the stack started with <see cref="Push"/>.</summary>
        public static void Pop() {
            GuiHelper.CurrentIMGUI.Pop();
        }
    }
}
