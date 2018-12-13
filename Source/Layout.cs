using System.Collections.Generic;

namespace AposGui
{
    /// <summary>
    /// Goal: Handles how components are positioned inside a panel.
    /// </summary>
    class Layout
    {
        public Layout() {
        }
        public virtual Panel Panel {
            get; set;
        }
        public virtual void RecomputeChildren(List<Component> children) {
            //Tell each children their position and size.
        }
    }
}
