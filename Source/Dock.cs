using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    /// <summary>
    /// Places itself at a fixed rectangle in UI coordinates, then gives its child that space.
    /// </summary>
    public class Dock(int id, float left, float top, float right, float bottom) : SingleParent(id) {
        public float DockLeft { get; set; } = left;
        public float DockTop { get; set; } = top;
        public float DockRight { get; set; } = right;
        public float DockBottom { get; set; } = bottom;

        public override void UpdatePrefSize(GameTime gameTime) {
            if (Child != null) {
                Child.UpdatePrefSize(gameTime);

                PrefWidth = DockRight - DockLeft;
                PrefHeight = DockBottom - DockTop;
            }
        }
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

        public static Dock Put(float left, float top, float right, float bottom, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            Dock a = IMGUI.Push(id, isAbsoluteId, id => new Dock(id, left, top, right, bottom), 1);
            a.DockLeft = left;
            a.DockTop = top;
            a.DockRight = right;
            a.DockBottom = bottom;

            return a;
        }
    }
}
