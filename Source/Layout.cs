using System.Collections.Generic;

namespace AposGui {
    /// <summary>
    /// Goal: Handles how components are positioned inside a panel.
    /// </summary>
    public class Layout {
        //constructors
        public Layout() { }

        //public vars
        public virtual Panel Panel {
            get;
            set;
        }

        //public functions
        public virtual void RecomputeChildren(List<Component> children) {
            //Tell each children their position and size.
        }
    }
}