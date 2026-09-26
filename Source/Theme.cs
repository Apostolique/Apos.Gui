using Microsoft.Xna.Framework;

namespace Apos.Gui {
    /// <summary>
    /// Colors and sizes that the built-in looks read. Change them to restyle every component at once.
    /// Replace a component's Look or DefaultLook to change more than that.
    /// </summary>
    public static class Theme {
        /// <summary>Text, and the default color passed to Put methods that take one.</summary>
        public static Color Text { get; set; } = new(200, 200, 200);
        /// <summary>Borders of components that don't have focus.</summary>
        public static Color Border { get; set; } = new(76, 76, 76);
        /// <summary>Borders of the focused component, and the textbox cursor.</summary>
        public static Color Focus { get; set; } = Color.White;
        /// <summary>Drawn over a component while it's hovered or pressed, faded by how much.</summary>
        public static Color Highlight { get; set; } = Color.White;
        /// <summary>Behind windows, tooltips and popups.</summary>
        public static Color Panel { get; set; } = Color.Black;
        /// <summary>Over everything under a modal popup.</summary>
        public static Color Backdrop { get; set; } = new(0, 0, 0, 160);
        /// <summary>In UI units.</summary>
        public static float BorderThickness { get; set; } = 2f;
        /// <summary>Corner radius of the built-in looks, in UI units.</summary>
        public static float Rounding { get; set; } = 0f;
    }
}
