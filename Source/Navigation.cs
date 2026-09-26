using System;
using System.Collections.Generic;
using MonoGame.Extended;

namespace Apos.Gui {
    /// <summary>A direction to move the focus in, with the arrow keys or the d-pad.</summary>
    public enum Direction {
        Left,
        Right,
        Up,
        Down,
    }

    /// <summary>
    /// Finds where the focus goes when it moves in a direction.
    /// </summary>
    public static class Navigation {
        /// <summary>
        /// Picks the candidate closest to <paramref name="from"/> in a direction. Candidates that line up with it, like the
        /// next item in a row or a column, win over the ones that are off to the side. Returns null when nothing is in that direction.
        /// A parent that overrides <see cref="IParent.Navigate"/> can call this with its own children.
        /// </summary>
        /// <param name="from">The component that has focus.</param>
        /// <param name="direction">Where to look.</param>
        /// <param name="candidates">The components that can get focus. <paramref name="from"/> is skipped if it's in there.</param>
        public static IComponent? Closest(IComponent from, Direction direction, IEnumerable<IComponent> candidates) {
            IComponent? best = null;
            bool bestLinedUp = false;
            float bestScore = float.MaxValue;

            foreach (IComponent c in candidates) {
                if (c == from || !TryScore(from.Bounds, c.Bounds, direction, out bool linedUp, out float score)) {
                    continue;
                }
                if (best == null || linedUp && !bestLinedUp || linedUp == bestLinedUp && score < bestScore) {
                    best = c;
                    bestLinedUp = linedUp;
                    bestScore = score;
                }
            }

            return best;
        }

        private static bool TryScore(RectangleF from, RectangleF to, Direction direction, out bool linedUp, out float score) {
            // Turns every direction into "down": main is the axis we move along, cross is the one beside it.
            (float fromStart, float fromEnd, float toStart, float toEnd) main;
            (float fromStart, float fromEnd, float toStart, float toEnd) cross;
            switch (direction) {
                case Direction.Down:
                    main = (from.Top, from.Bottom, to.Top, to.Bottom);
                    cross = (from.Left, from.Right, to.Left, to.Right);
                    break;
                case Direction.Up:
                    main = (-from.Bottom, -from.Top, -to.Bottom, -to.Top);
                    cross = (from.Left, from.Right, to.Left, to.Right);
                    break;
                case Direction.Right:
                    main = (from.Left, from.Right, to.Left, to.Right);
                    cross = (from.Top, from.Bottom, to.Top, to.Bottom);
                    break;
                default:
                    main = (-from.Right, -from.Left, -to.Right, -to.Left);
                    cross = (from.Top, from.Bottom, to.Top, to.Bottom);
                    break;
            }

            linedUp = false;
            score = 0f;
            if (to.Width <= 0f || to.Height <= 0f) {
                return false;
            }
            // The candidate has to start, be centered and end further along than the component.
            // Something that covers it, like a window behind it, doesn't count.
            float fromCenter = (main.fromStart + main.fromEnd) / 2f;
            float toCenter = (main.toStart + main.toEnd) / 2f;
            if (main.toStart <= main.fromStart || toCenter <= fromCenter || main.toEnd <= main.fromEnd) {
                return false;
            }

            float gap = MathF.Max(main.toStart - main.fromEnd, 0f);
            float overlap = MathF.Min(cross.fromEnd, cross.toEnd) - MathF.Max(cross.fromStart, cross.toStart);
            float centerOffset = MathF.Abs((cross.toStart + cross.toEnd) / 2f - (cross.fromStart + cross.fromEnd) / 2f);

            linedUp = overlap > 0f;
            score = linedUp ? gap + centerOffset * 0.25f : gap + -overlap * 2f;
            return true;
        }
    }
}
