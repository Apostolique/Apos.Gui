using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    /// <summary>
    /// Places itself at a fixed rectangle in UI coordinates, then gives its child that space.
    /// </summary>
    public class Dock(long id, float left, float top, float right, float bottom) : SingleParent(id) {
        /// <summary>Left edge of the rectangle, in UI coordinates.</summary>
        public float DockLeft { get; set; } = left;
        /// <summary>Top edge of the rectangle, in UI coordinates.</summary>
        public float DockTop { get; set; } = top;
        /// <summary>Right edge of the rectangle, in UI coordinates.</summary>
        public float DockRight { get; set; } = right;
        /// <summary>Bottom edge of the rectangle, in UI coordinates.</summary>
        public float DockBottom { get; set; } = bottom;

        /// <inheritdoc/>
        public override void UpdatePrefSize(GameTime gameTime) {
            if (Child != null) {
                Child.UpdatePrefSize(gameTime);

                PrefWidth = DockRight - DockLeft;
                PrefHeight = DockBottom - DockTop;
            }
        }
        /// <inheritdoc/>
        public override void UpdateLayout(GameTime gameTime) {
            X = DockLeft;
            Y = DockTop;

            if (Parent != null) {
                Clip = Bounds.Intersect(Parent.Clip);
            }

            if (Child != null) {
                Child.X = X;
                Child.Y = Y;
                Child.Width = MathHelper.Min(Width, Child.PrefWidth);
                Child.Height = MathHelper.Min(Height, Child.PrefHeight);
                LayoutChild(gameTime);
            }
        }

        /// <summary>Starts a dock. The next component put becomes its child and gets the rectangle.</summary>
        /// <param name="left">Left edge in UI coordinates.</param>
        /// <param name="top">Top edge in UI coordinates.</param>
        /// <param name="right">Right edge in UI coordinates.</param>
        /// <param name="bottom">Bottom edge in UI coordinates.</param>
        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="file">The caller's file. The same line in two files gives two different ids.</param>
        public static Dock Put(float left, float top, float right, float bottom, [CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            Dock a = IMGUI.Push(id, isAbsoluteId, file, id => new Dock(id, left, top, right, bottom), 1);
            a.DockLeft = left;
            a.DockTop = top;
            a.DockRight = right;
            a.DockBottom = bottom;

            return a;
        }
    }
}
