using System.Runtime.CompilerServices;

namespace Apos.Gui {
    /// <summary>
    /// A <see cref="Vertical"/> that centers its children from left to right.
    /// </summary>
    public class CenteredVertical : Vertical {
        public CenteredVertical(long id) : base(id) {
            Align = CrossAlign.Center;
        }

        public static new CenteredVertical Push([CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            return IMGUI.Push(id, isAbsoluteId, file, id => new CenteredVertical(id));
        }
        public static new void Pop() {
            GuiHelper.CurrentIMGUI.Pop();
        }
    }
}
