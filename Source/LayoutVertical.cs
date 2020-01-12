using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Apos.Gui {
    /// <summary>
    /// Goal: Stacks components on top of each others.
    /// </summary>
    public class LayoutVertical : Layout {

        // Group: Constructors

        public LayoutVertical() { }

        // Group: Public Functions

        public override void RecomputeChildren(List<Component> childs) {
            //Tell each children their position and size.
            Point position = Panel.Position;
            int width = Panel.Width;
            int height = Panel.Height;

            int offsetY = 0;
            foreach (Component c in childs) {
                int cHeight = c.PrefHeight;
                c.Width = width;
                c.Height = cHeight;
                c.Position = new Point(position.X, offsetY + position.Y) + Panel.Offset;
                offsetY += cHeight;
                c.ClippingRect = Panel.ClipRectangle(c.BoundingRect);
            }
            Panel.Size = new Size2(width, offsetY);
        }
    }
}