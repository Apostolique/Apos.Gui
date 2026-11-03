using System.Runtime.CompilerServices;

namespace Apos.Gui {
    /// <summary>
    /// A <see cref="Vertical"/> that centers its children from left to right.
    /// </summary>
    public class CenteredVertical : Vertical {
        /// <summary>Creates the stack. <see cref="Push"/> calls this on the first frame it's put.</summary>
        public CenteredVertical(long id) : base(id) {
            Align = CrossAlign.Center;
        }

        /// <summary>Starts a stack that centers its children. The components put after it go in it until <see cref="Pop"/>.</summary>
        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="file">The caller's file. The same line in two files gives two different ids.</param>
        public static new CenteredVertical Push([CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            return IMGUI.Push(id, isAbsoluteId, file, id => new CenteredVertical(id));
        }
        /// <summary>Ends the stack started with <see cref="Push"/>.</summary>
        public static new void Pop() {
            GuiHelper.CurrentIMGUI.Pop();
        }
    }
}
