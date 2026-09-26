using System;
using System.Collections.Generic;
using System.Text;
using Apos.Shapes;

namespace Apos.Gui {
    /// <summary>How lines of text line up in the width they're given.</summary>
    public enum TextAlign {
        Left,
        Center,
        Right
    }

    /// <summary>A line of laid out text. It's a slice of the original string and how wide that slice draws.</summary>
    /// <param name="Start">Index of the line's first char in the original string.</param>
    /// <param name="Length">How many chars the line covers, including the spaces or newline it broke at.</param>
    /// <param name="Width">How wide the line draws, without the spaces it broke at.</param>
    public readonly record struct TextLine(int Start, int Length, float Width) {
        /// <summary>Index right after the line's last char.</summary>
        public int End => Start + Length;
    }

    /// <summary>
    /// Breaks text into lines that fit a width, and maps between char indices and positions in those lines.
    /// ShapeBatch.DrawString draws what it's given, so wrapping, alignment and ellipsis happen here.
    /// </summary>
    public static class TextLayout {
        /// <summary>
        /// Breaks text into lines no wider than maxWidth. A line breaks after a space when it can, and inside a word when the word alone is too wide.
        /// A newline always breaks. Empty text is one empty line.
        /// The lines list is cleared, then filled with the lines in order. Together they cover the whole text.
        /// </summary>
        public static void Wrap(ShapeFont font, string text, float size, float maxWidth, List<TextLine> lines) {
            lines.Clear();

            int lineStart = 0;
            float x = 0f;
            int prev = -1;
            int breakAt = -1;
            float breakWidth = 0f;

            int i = 0;
            while (i < text.Length) {
                int cp = CodePointAt(text, i, out int step);

                if (cp == '\n') {
                    lines.Add(new TextLine(lineStart, i + step - lineStart, x));
                    i += step;
                    lineStart = i;
                    x = 0f;
                    prev = -1;
                    breakAt = -1;
                    continue;
                }
                if (cp == '\r') {
                    i += step;
                    continue;
                }

                float advance = Advance(font, prev, cp, size);
                if (cp == ' ') {
                    // Spaces hang past the edge so that the next word starts the new line.
                    breakWidth = x;
                    x += advance;
                    prev = cp;
                    i += step;
                    breakAt = i;
                    continue;
                }

                if (x + advance > maxWidth && i > lineStart) {
                    if (breakAt > lineStart) {
                        lines.Add(new TextLine(lineStart, breakAt - lineStart, breakWidth));
                        lineStart = breakAt;
                        x = font.MeasureString(text.AsSpan(lineStart, i - lineStart), size).X;
                        prev = i > lineStart ? LastCodePoint(text, i) : -1;
                    } else {
                        lines.Add(new TextLine(lineStart, i - lineStart, x));
                        lineStart = i;
                        x = 0f;
                        prev = -1;
                    }
                    breakAt = -1;
                    advance = Advance(font, prev, cp, size);
                }

                x += advance;
                prev = cp;
                i += step;
            }

            lines.Add(new TextLine(lineStart, text.Length - lineStart, x));
        }
        /// <summary>Same as the other Wrap but returns a new list.</summary>
        public static List<TextLine> Wrap(ShapeFont font, string text, float size, float maxWidth) {
            var lines = new List<TextLine>();
            Wrap(font, text, size, maxWidth, lines);
            return lines;
        }

        /// <summary>
        /// Cuts text to fit maxWidth on one line, ending it with the ellipsis when something was cut.
        /// Returns the text unchanged when it already fits.
        /// </summary>
        public static string Ellipsize(ShapeFont font, string text, float size, float maxWidth, string ellipsis = "...") {
            if (font.MeasureString(text, size).X <= maxWidth) {
                return text;
            }

            float available = maxWidth - font.MeasureString(ellipsis, size).X;
            float x = 0f;
            int prev = -1;
            int i = 0;
            while (i < text.Length) {
                int cp = CodePointAt(text, i, out int step);
                float advance = Advance(font, prev, cp, size);
                if (x + advance > available || cp == '\n') break;
                x += advance;
                prev = cp;
                i += step;
            }

            return text[..i].TrimEnd() + ellipsis;
        }

        /// <summary>How far from the start of its line the caret sits when it's before the char at index.</summary>
        public static float CaretX(ShapeFont font, string text, TextLine line, int index, float size) {
            int end = Math.Clamp(index, line.Start, line.End);
            return font.MeasureString(text.AsSpan(line.Start, end - line.Start), size).X;
        }

        /// <summary>The char index in a line that's closest to x, measured from the start of the line.</summary>
        public static int HitTest(ShapeFont font, string text, TextLine line, float x, float size) {
            float current = 0f;
            int prev = -1;
            int end = LineContentEnd(text, line);
            int i = line.Start;
            while (i < end) {
                int cp = CodePointAt(text, i, out int step);
                float advance = Advance(font, prev, cp, size);
                if (x < current + advance / 2f) {
                    return i;
                }
                current += advance;
                prev = cp;
                i += step;
            }
            return end;
        }

        /// <summary>How far to move a line from the left so that it lines up in the given width.</summary>
        public static float AlignOffset(TextLine line, float width, TextAlign align) {
            return align switch {
                TextAlign.Center => (width - line.Width) / 2f,
                TextAlign.Right => width - line.Width,
                _ => 0f,
            };
        }

        /// <summary>Index of the line that contains the char index. An index at the very end belongs to the last line.</summary>
        public static int LineAt(List<TextLine> lines, int index) {
            for (int i = 0; i < lines.Count - 1; i++) {
                if (index < lines[i].End) {
                    return i;
                }
            }
            return lines.Count - 1;
        }

        /// <summary>Where a line's drawable chars end, before the newline it broke at.</summary>
        public static int LineContentEnd(string text, TextLine line) {
            int end = line.End;
            if (end > line.Start && text[end - 1] == '\n') end--;
            if (end > line.Start && text[end - 1] == '\r') end--;
            return end;
        }

        private static float Advance(ShapeFont font, int prev, int cp, float size) {
            float advance = font.Advance(cp) * size;
            if (prev >= 0) {
                advance += font.Kerning(prev, cp) * size;
            }
            return advance;
        }
        private static int CodePointAt(string text, int i, out int step) {
            Rune.DecodeFromUtf16(text.AsSpan(i), out Rune r, out step);
            return r.Value;
        }
        private static int LastCodePoint(string text, int end) {
            Rune.DecodeLastFromUtf16(text.AsSpan(0, end), out Rune r, out _);
            return r.Value;
        }
    }
}
