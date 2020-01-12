using System.Collections.Generic;

namespace Apos.Gui {
    /// <summary>
    /// Goal: Handles how components are positioned inside a panel.
    /// </summary>
    public class Layout {

        // Group: Constructors

        public Layout() { }

        // Group: Public Variables

        public virtual Panel Panel {
            get;
            set;
        }

        // Group: Public Functions

        public virtual void RecomputeChildren(List<Component> children) {
            //Tell each children their position and size.
        }
    }
}