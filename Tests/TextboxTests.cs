using System;
using System.IO;
using System.Threading.Tasks;
using Apos.Shapes;
using Microsoft.Xna.Framework;
using Xunit;

namespace Apos.Gui.Tests {
    public class TextboxTests {
        public TextboxTests() {
            GuiHelper.Font = _font;
        }

        [Fact]
        public void OnScreenKeyboardTextReachesTheCaller() {
            var keyboard = new FakeKeyboard();
            GuiHelper.OnScreenKeyboard = keyboard;
            try {
                var ui = new IMGUI();
                string text = "old";
                Textbox t = null!;

                ui.UpdateSetup(_gameTime);
                t = Textbox.Put(ref text, id: 1);
                ui.Cleanup();

                t.OpenKeyboard();
                Assert.Equal("old", keyboard.ShownWith);
                keyboard.Done.SetResult("typed\non a phone");

                ui.UpdateSetup(_gameTime);
                Textbox.Put(ref text, id: 1);
                ui.Cleanup();

                Assert.Equal("typed on a phone", text);
            } finally {
                GuiHelper.OnScreenKeyboard = null;
            }
        }

        private static readonly GameTime _gameTime = new();

        private class FakeKeyboard : IOnScreenKeyboard {
            public TaskCompletionSource<string?> Done { get; } = new();
            public string? ShownWith { get; private set; }

            public Task<string?> Show(Textbox textbox) {
                ShownWith = textbox.Text;
                return Done.Task;
            }
        }

        [Fact]
        public void InsertReplacesTheSelection() {
            var t = new Textbox(1, "hello world", 10, Color.White);

            t.Select(6, 11);
            t.Insert("there");

            Assert.Equal("hello there", t.Text);
            Assert.Equal(11, t.Cursor);
            Assert.False(t.HasSelection);
        }

        [Fact]
        public void InsertStopsAtMaxLength() {
            var t = new Textbox(1, "abc", 10, Color.White) { MaxLength = 5 };

            t.MoveCursor(3);
            t.Insert("defgh");

            Assert.Equal("abcde", t.Text);
            Assert.Equal(5, t.Cursor);
        }

        [Fact]
        public void ExtendingKeepsTheAnchor() {
            var t = new Textbox(1, "hello", 10, Color.White);

            t.MoveCursor(1);
            t.MoveCursor(4, extend: true);

            Assert.Equal(1, t.Anchor);
            Assert.Equal("ell", t.SelectedText);
        }

        [Fact]
        public void ShorterTextClampsTheCursor() {
            var t = new Textbox(1, "hello", 10, Color.White);

            t.MoveCursor(5);
            t.Text = "hi";

            Assert.Equal(2, t.Cursor);
        }

        [Fact]
        public void IndexAtFindsWhatPositionOfPlaced() {
            var t = new Textbox(1, "one two\nthree", 10, Color.White) { Multiline = true };
            t.XY = new Vector2(50f, 20f);

            for (int i = 0; i <= t.Text.Length; i++) {
                Vector2 p = t.PositionOf(i) + new Vector2(0.1f, t.LineHeight / 2f);
                Assert.Equal(i, t.IndexAt(p));
            }
        }

        private static readonly ShapeFont _font = new(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "source-code-pro-medium.ttf")));
    }
}
