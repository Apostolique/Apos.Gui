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
        /// Picks where the focus goes from <paramref name="from"/> in a direction. It looks at the next row first, the
        /// candidates closest in that direction, then picks the one in that row that's the most in line with <paramref name="from"/>.
        /// That way moving down a menu stops at every row, even when a row is off center. Returns null when nothing is in that direction.
        /// A parent that overrides <see cref="IParent.Navigate"/> can call this with its own children.
        /// </summary>
        /// <param name="from">The component that has focus.</param>
        /// <param name="direction">Where to look.</param>
        /// <param name="candidates">The components that can get focus. <paramref name="from"/> is skipped if it's in there.</param>
        public static IComponent? Closest(IComponent from, Direction direction, IEnumerable<IComponent> candidates) {
            var ahead = new List<(IComponent C, Span Main, float CrossDistance)>();
            float rowEnd = float.MaxValue;

            foreach (IComponent c in candidates) {
                if (c == from || !TryMeasure(from.Bounds, c.Bounds, direction, out Span main, out float crossDistance)) {
                    continue;
                }
                ahead.Add((c, main, crossDistance));
                rowEnd = MathF.Min(rowEnd, main.End);
            }

            // The next row is everything that starts before the closest candidate ends.
            IComponent? best = null;
            float bestCross = float.MaxValue;
            float bestStart = float.MaxValue;
            foreach (var (c, main, crossDistance) in ahead) {
                if (main.Start >= rowEnd) {
                    continue;
                }
                if (crossDistance < bestCross || crossDistance == bestCross && main.Start < bestStart) {
                    best = c;
                    bestCross = crossDistance;
                    bestStart = main.Start;
                }
            }

            return best;
        }

        private readonly record struct Span(float Start, float End) {
            public float Center => (Start + End) / 2f;
        }

        /// <summary>
        /// Turns every direction into "down": main is the axis the focus moves along, cross is the one beside it.
        /// </summary>
        /// <param name="main">Where the candidate is along the direction.</param>
        /// <param name="crossDistance">How far the candidate's center is from the component's center across the direction.</param>
        private static bool TryMeasure(RectangleF from, RectangleF to, Direction direction, out Span main, out float crossDistance) {
            Span fromMain, fromCross, toCross;
            switch (direction) {
                case Direction.Down:
                    fromMain = new(from.Top, from.Bottom);
                    main = new(to.Top, to.Bottom);
                    fromCross = new(from.Left, from.Right);
                    toCross = new(to.Left, to.Right);
                    break;
                case Direction.Up:
                    fromMain = new(-from.Bottom, -from.Top);
                    main = new(-to.Bottom, -to.Top);
                    fromCross = new(from.Left, from.Right);
                    toCross = new(to.Left, to.Right);
                    break;
                case Direction.Right:
                    fromMain = new(from.Left, from.Right);
                    main = new(to.Left, to.Right);
                    fromCross = new(from.Top, from.Bottom);
                    toCross = new(to.Top, to.Bottom);
                    break;
                default:
                    fromMain = new(-from.Right, -from.Left);
                    main = new(-to.Right, -to.Left);
                    fromCross = new(from.Top, from.Bottom);
                    toCross = new(to.Top, to.Bottom);
                    break;
            }

            crossDistance = MathF.Abs(toCross.Center - fromCross.Center);
            if (to.Width <= 0f || to.Height <= 0f) {
                return false;
            }
            // The candidate has to start, be centered and end further along than the component.
            // Something that covers it, like a window behind it, doesn't count.
            return main.Start > fromMain.Start && main.Center > fromMain.Center && main.End > fromMain.End;
        }
    }
}
