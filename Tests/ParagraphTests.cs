using System;
using System.IO;
using System.Linq;
using Apos.Shapes;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Xunit;

namespace Apos.Gui.Tests {
    public class ParagraphTests {
        public ParagraphTests() {
            GuiHelper.Font = _font;
        }

        [Theory]
        [InlineData(null)]
        [InlineData(500f)]
        public void RewrapsInTheSameFrameWhenTheWindowGetsNarrower(float? maxWidth) {
            var ui = new IMGUI { Area = new RectangleF(0, 0, 800, 480) };
            Paragraph p = null!;
            void Build() {
                MenuPanel.Push();
                p = Paragraph.Put(Text, 20);
                p.MaxWidth = maxWidth;
                MenuPanel.Pop();
            }

            Frame(ui, Build);
            int wideLines = p.Lines.Count;

            ui.Area = new RectangleF(0, 0, 300, 480);
            Frame(ui, Build);

            Assert.True(p.Width <= 300f);
            Assert.True(p.Lines.Count > wideLines);
            Assert.All(p.Lines, l => Assert.True(l.Width <= p.Width - p.Padding * 2));
            Assert.Equal(p.PrefHeight, p.Height);
        }

        [Fact]
        public void WidensAgainWhenThereIsRoom() {
            var ui = new IMGUI { Area = new RectangleF(0, 0, 300, 480) };
            Paragraph p = null!;
            void Build() {
                MenuPanel.Push();
                p = Paragraph.Put(Text, 20);
                MenuPanel.Pop();
            }

            Frame(ui, Build);
            int narrowLines = p.Lines.Count;

            ui.Area = new RectangleF(0, 0, 800, 480);
            Frame(ui, Build);

            Assert.True(p.Lines.Count < narrowLines);
            Assert.Equal(p.PrefHeight, p.Height);
        }

        private static void Frame(IMGUI ui, Action build) {
            ui.UpdateSetup(_gameTime);
            build();
            ui.Cleanup();
            ui.UpdateLayout(_gameTime);
        }

        private const string Text = "A paragraph wraps its text to the space it's given. Resize the window to see it flow.";
        private static readonly GameTime _gameTime = new();
        private static readonly ShapeFont _font = new(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "source-code-pro-medium.ttf")));
    }
}
