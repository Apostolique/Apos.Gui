using Microsoft.Xna.Framework;

namespace Apos.Gui {
    /// <summary>
    /// Colors and sizes that the built-in looks read. Change them to restyle every component at once.
    /// Replace a component's Look or DefaultLook to change more than that.
    /// </summary>
    public static class Theme {
        /// <summary>Text, and the default color passed to Put methods that take one.</summary>
        public static Color Text { get; set; }
        /// <summary>Borders of components that don't have focus.</summary>
        public static Color Border { get; set; }
        /// <summary>Borders of the focused component, and the textbox cursor.</summary>
        public static Color Focus { get; set; }
        /// <summary>Drawn over a component while it's hovered or pressed, faded by how much.</summary>
        public static Color Highlight { get; set; }
        /// <summary>Behind windows, tooltips and popups.</summary>
        public static Color Panel { get; set; }
        /// <summary>Inside buttons, boxes, tracks and title bars. Transparent leaves only their borders.</summary>
        public static Color Surface { get; set; }
        /// <summary>What's on or picked: checkmarks, the filled part of bars, slider thumbs, the selected tab, selected text.</summary>
        public static Color Accent { get; set; }
        /// <summary>Over everything under a modal popup.</summary>
        public static Color Backdrop { get; set; }
        /// <summary>In UI units.</summary>
        public static float BorderThickness { get; set; }
        /// <summary>Corner radius of the built-in looks, in UI units.</summary>
        public static float Rounding { get; set; }

        static Theme() {
            UseFilled();
        }

        /// <summary>Thin borders on black, with white for focus and what's picked.</summary>
        public static void UseOutline() {
            Text = new Color(200, 200, 200);
            Border = new Color(76, 76, 76);
            Focus = Color.White;
            Highlight = Color.White;
            Panel = new Color(10, 10, 10);
            Surface = Color.Transparent;
            Accent = new Color(220, 220, 220);
            Backdrop = new Color(0, 0, 0, 160);
            BorderThickness = 2f;
            Rounding = 0f;
        }
        /// <summary>Filled surfaces a bit lighter than their panels, rounded corners, and a blue accent. This is the default.</summary>
        public static void UseFilled() {
            Text = new Color(228, 231, 237);
            Border = new Color(58, 63, 75);
            Focus = new Color(138, 176, 255);
            Highlight = Color.White;
            Panel = new Color(24, 26, 32);
            Surface = new Color(40, 44, 54);
            Accent = new Color(82, 128, 250);
            Backdrop = new Color(0, 0, 0, 170);
            BorderThickness = 2f;
            Rounding = 6f;
        }
    }
}
