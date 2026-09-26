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
        /// Picks where the focus goes from <paramref name="from"/> in a direction. A candidate has to start past the edge
        /// <paramref name="from"/> leaves through. Returns null when nothing is in that direction.
        /// Up and down go to the next row, then to the one in that row that's the most in line, so they stop at every row of a
        /// menu even when rows are off center. Left and right stay in the row: the closest one wins, and being above or below
        /// costs a lot more than being far.
        /// A parent that overrides <see cref="IParent.Navigate"/> can call this with its own children.
        /// </summary>
        /// <param name="from">The component that has focus.</param>
        /// <param name="direction">Where to look.</param>
        /// <param name="candidates">The components that can get focus. <paramref name="from"/> is skipped if it's in there.</param>
        public static IComponent? Closest(IComponent from, Direction direction, IEnumerable<IComponent> candidates) {
            var ahead = new List<(IComponent C, Measure M)>();
            foreach (IComponent c in candidates) {
                if (c != from && TryMeasure(from.Bounds, c.Bounds, direction, out Measure m)) {
                    ahead.Add((c, m));
                }
            }
            if (ahead.Count == 0) {
                return null;
            }

            bool vertical = direction == Direction.Up || direction == Direction.Down;
            // The next row is everything that starts before the closest candidate ends.
            float rowEnd = float.MaxValue;
            if (vertical) {
                foreach (var (_, m) in ahead) {
                    rowEnd = MathF.Min(rowEnd, m.End);
                }
            }

            IComponent? best = null;
            (float, float, float) bestKey = (float.MaxValue, float.MaxValue, float.MaxValue);
            foreach (var (c, m) in ahead) {
                (float, float, float) key;
                if (vertical) {
                    if (m.Start >= rowEnd) continue;
                    key = (m.Side, m.Offset, m.Gap);
                } else {
                    key = (MathF.Sqrt(m.Gap * m.Gap + m.Side * m.Side) + m.Gap + m.Side * SideWeight, m.Offset, 0f);
                }
                if (best == null || key.CompareTo(bestKey) < 0) {
                    best = c;
                    bestKey = key;
                }
            }

            return best;
        }

        /// <summary>Where a candidate is, measured as if the focus was moving down.</summary>
        /// <param name="Start">Where it starts along the direction.</param>
        /// <param name="End">Where it ends along the direction.</param>
        /// <param name="Gap">How far it is past the focused component along the direction.</param>
        /// <param name="Side">How far it is to the side of the focused component. 0 when they overlap.</param>
        /// <param name="Offset">How far its center is from the focused component's center, to the side.</param>
        private readonly record struct Measure(float Start, float End, float Gap, float Side, float Offset);

        private static bool TryMeasure(RectangleF from, RectangleF to, Direction direction, out Measure measure) {
            // Main is the axis the focus moves along, cross is the one beside it. Up and left are flipped so that "further" is always bigger.
            (float Start, float End) fromMain, toMain, fromCross, toCross;
            switch (direction) {
                case Direction.Down:
                    (fromMain, toMain) = ((from.Top, from.Bottom), (to.Top, to.Bottom));
                    (fromCross, toCross) = ((from.Left, from.Right), (to.Left, to.Right));
                    break;
                case Direction.Up:
                    (fromMain, toMain) = ((-from.Bottom, -from.Top), (-to.Bottom, -to.Top));
                    (fromCross, toCross) = ((from.Left, from.Right), (to.Left, to.Right));
                    break;
                case Direction.Right:
                    (fromMain, toMain) = ((from.Left, from.Right), (to.Left, to.Right));
                    (fromCross, toCross) = ((from.Top, from.Bottom), (to.Top, to.Bottom));
                    break;
                default:
                    (fromMain, toMain) = ((-from.Right, -from.Left), (-to.Right, -to.Left));
                    (fromCross, toCross) = ((from.Top, from.Bottom), (to.Top, to.Bottom));
                    break;
            }

            measure = default;
            if (to.Width <= 0f || to.Height <= 0f) {
                return false;
            }
            // Neighbors can overlap a little, like buttons that share a border. Something that covers the component doesn't count.
            float allowedOverlap = MathF.Min(fromMain.End - fromMain.Start, toMain.End - toMain.Start) / 4f;
            if (toMain.Start < fromMain.End - allowedOverlap || toMain.End <= fromMain.End) {
                return false;
            }

            float gap = MathF.Max(toMain.Start - fromMain.End, 0f);
            float side = MathF.Max(MathF.Max(toCross.Start - fromCross.End, fromCross.Start - toCross.End), 0f);
            float offset = MathF.Abs((toCross.Start + toCross.End) / 2f - (fromCross.Start + fromCross.End) / 2f);
            measure = new Measure(toMain.Start, toMain.End, gap, side, offset);
            return true;
        }

        /// <summary>How much more being above or below counts than being far, when moving left or right.</summary>
        private const float SideWeight = 30f;
    }
}
