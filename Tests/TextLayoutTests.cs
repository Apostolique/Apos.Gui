using System;
using System.IO;
using System.Linq;
using Apos.Shapes;
using Xunit;

namespace Apos.Gui.Tests {
    public class TextLayoutTests {
        // Source Code Pro is monospace, so every char has the same advance.
        private static readonly ShapeFont _font = new(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "source-code-pro-medium.ttf")));
        private const float Size = 10f;
        private static float Char => _font.Advance('a') * Size;

        [Fact]
        public void ShortTextIsOneLine() {
            var lines = TextLayout.Wrap(_font, "hello world", Size, 1000f);

            var line = Assert.Single(lines);
            Assert.Equal(new TextLine(0, 11, _font.MeasureString("hello world", Size).X), line);
        }

        [Fact]
        public void EmptyTextIsOneEmptyLine() {
            var line = Assert.Single(TextLayout.Wrap(_font, "", Size, 100f));
            Assert.Equal(0, line.Length);
            Assert.Equal(0f, line.Width);
        }

        [Fact]
        public void BreaksAfterSpaces() {
            string text = "one two three";
            var lines = TextLayout.Wrap(_font, text, Size, Char * 8);

            Assert.Equal(["one two ", "three"], lines.Select(l => text.Substring(l.Start, l.Length)));
            Assert.Equal(Char * 7, lines[0].Width, 3);
        }

        [Fact]
        public void BreaksInsideAWordThatDoesntFit() {
            string text = "abcdefghij";
            var lines = TextLayout.Wrap(_font, text, Size, Char * 4);

            Assert.Equal(["abcd", "efgh", "ij"], lines.Select(l => text.Substring(l.Start, l.Length)));
        }

        [Fact]
        public void NewlinesAlwaysBreak() {
            string text = "a\nb\n";
            var lines = TextLayout.Wrap(_font, text, Size, 1000f);

            Assert.Equal(["a\n", "b\n", ""], lines.Select(l => text.Substring(l.Start, l.Length)));
        }

        [Fact]
        public void LinesCoverTheWholeText() {
            string text = "The quick brown fox jumps over the lazy dog.\nThen it sleeps.";
            var lines = TextLayout.Wrap(_font, text, Size, Char * 12);

            Assert.Equal(text, string.Concat(lines.Select(l => text.Substring(l.Start, l.Length))));
            Assert.All(lines, l => Assert.True(l.Width <= Char * 12 + 0.001f));
        }

        [Fact]
        public void EllipsizeFitsAndMarksTheCut() {
            string cut = TextLayout.Ellipsize(_font, "hello world", Size, Char * 8);

            Assert.Equal("hello...", cut);
            Assert.True(_font.MeasureString(cut, Size).X <= Char * 8 + 0.001f);
            Assert.Equal("hi", TextLayout.Ellipsize(_font, "hi", Size, Char * 8));
        }

        [Fact]
        public void HitTestAndCaretXAgree() {
            string text = "hello";
            var line = TextLayout.Wrap(_font, text, Size, 1000f)[0];

            for (int i = 0; i <= text.Length; i++) {
                float x = TextLayout.CaretX(_font, text, line, i, Size);
                Assert.Equal(i, TextLayout.HitTest(_font, text, line, x + 0.1f, Size));
            }
            Assert.Equal(0, TextLayout.HitTest(_font, text, line, -50f, Size));
            Assert.Equal(5, TextLayout.HitTest(_font, text, line, 1000f, Size));
        }

        [Fact]
        public void LineAtFindsTheLine() {
            string text = "ab\ncd";
            var lines = TextLayout.Wrap(_font, text, Size, 1000f);

            Assert.Equal(0, TextLayout.LineAt(lines, 2));
            Assert.Equal(1, TextLayout.LineAt(lines, 3));
            Assert.Equal(1, TextLayout.LineAt(lines, 5));
        }
    }
}
