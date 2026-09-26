using System.Collections.Generic;

namespace Apos.Gui {
    /// <summary>
    /// Bookkeeping shared by parents that keep their children in a list.
    /// Every child's Index stays equal to its position in the list, which is what lets IMGUI.GrabParent
    /// leave a child in place when it shows up in the same slot as last frame.
    /// </summary>
    internal static class ChildList {
        /// <summary>
        /// Inserts a child at its Index and renumbers the children it pushed down.
        /// </summary>
        public static void Insert(List<IComponent> children, List<IComponent> renderOrder, IComponent c) {
            children.Insert(c.Index, c);
            Renumber(children, c.Index + 1);

            // Render order: non-floatables sorted by Index, then floatables in the order SendToTop promoted them.
            if (c.IsFloatable) {
                renderOrder.Add(c);
            } else {
                int i = 0;
                while (i < renderOrder.Count && !renderOrder[i].IsFloatable && renderOrder[i].Index < c.Index) i++;
                renderOrder.Insert(i, c);
            }
        }
        /// <summary>
        /// Removes a child and renumbers the children that moved up to fill its place.
        /// </summary>
        public static void Remove(List<IComponent> children, List<IComponent> renderOrder, IComponent c) {
            int index = children.IndexOf(c);
            if (index == -1) return;

            children.RemoveAt(index);
            Renumber(children, index);
            renderOrder.Remove(c);
        }

        private static void Renumber(List<IComponent> children, int start) {
            for (int i = start; i < children.Count; i++) {
                children[i].Index = i;
            }
        }
    }
}
