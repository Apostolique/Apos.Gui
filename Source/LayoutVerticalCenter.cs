using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Apos.Gui {
    /// <summary>
    /// Goal: Stacks components on top of each others and centers them inside the panel.
    /// </summary>
    public class LayoutVerticalCenter : Layout {
        // Group: Constructors
        public LayoutVerticalCenter() { }

        // Group: Public Variables
        public override Panel Panel {
            get => base.Panel;
            set {
                base.Panel = value;
                _newWidth = base.Panel.Width;
                _newHeight = base.Panel.Height;
            }
        }

        // Group: Public Functions
        public override void RecomputeChildren(List<Component> children) {
            //Tell each children their position and size.
            _oldWidth = _newWidth;
            _oldHeigth = _newHeight;
            _newWidth = Panel.ClippingRect.Width;
            _newHeight = Panel.ClippingRect.Height;

            if (_oldWidth != _newWidth || _oldHeigth != _newHeight) {
                Panel.Offset = new Point(Panel.Offset.X, (int)Math.Min(Math.Max(Panel.Offset.Y, Panel.ClippingRect.Height - Panel.Size.Height), 0));
            }

            Point position = Panel.Position;
            int halfWidth = _newWidth / 2;
            int halfHeight = _newHeight / 2;

            int canvasWidth = 0;
            int canvasHeight = 0;
            foreach (Component c in children) {
                canvasWidth = Math.Max(canvasWidth, c.PrefWidth);
                canvasHeight += c.PrefHeight;
            }

            int canvasOffsetY = halfHeight - canvasHeight / 2;
            int offsetY = position.Y;
            foreach (Component c in children) {
                int cWidth = c.PrefWidth;
                int cHeight = c.PrefHeight;
                c.Width = cWidth;
                c.Height = cHeight;
                int componentHalfWidth = cWidth / 2;
                if (canvasHeight < _newHeight) {
                    c.Position = new Point(position.X + halfWidth - componentHalfWidth, offsetY + canvasOffsetY) + Panel.Offset;
                } else {
                    c.Position = new Point(position.X + halfWidth - componentHalfWidth, offsetY) + Panel.Offset;
                }
                offsetY += cHeight;
                c.ClippingRect = Panel.ClipRectangle(c.BoundingRect);
            }
            Panel.Size = new Size2(canvasWidth, canvasHeight);
        }

        // Group: Private Variables
        protected int _oldWidth;
        protected int _oldHeigth;
        protected int _newWidth;
        protected int _newHeight;
    }
}