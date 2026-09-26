using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Xunit;

namespace Apos.Gui.Tests {
    public class PerUITests {
        [Fact]
        public void FramesMakeTheirUICurrentAndGiveItBack() {
            var outer = new IMGUI();
            var inner = new IMGUI();
            Label a = null!, b = null!, c = null!;

            outer.UpdateSetup(_gameTime);
            a = Label.Put("a");
            inner.UpdateSetup(_gameTime);
            b = Label.Put("b");
            inner.Cleanup();
            c = Label.Put("c");
            outer.Cleanup();

            Assert.Same(outer, a.Parent);
            Assert.Same(inner, b.Parent);
            Assert.Same(outer, c.Parent);
            Assert.Same(inner, GuiHelper.CurrentIMGUI);
        }

        [Fact]
        public void AreaAndScaleSetTheRootSize() {
            var ui = new IMGUI { Area = new RectangleF(100, 50, 400, 300), Scale = 2f };

            ui.UpdateSetup(_gameTime);
            ui.Cleanup();
            ui.UpdateLayout(_gameTime);

            Assert.Equal(200f, ui.Width);
            Assert.Equal(150f, ui.Height);
        }

        [Fact]
        public void ToUIGoesThroughThePointerTransformThenTheAreaAndScale() {
            var ui = new IMGUI {
                Area = new RectangleF(100, 50, 400, 300),
                Scale = 2f,
                PointerTransform = p => p - new Vector2(1000, 0),
            };

            Assert.Equal(new Vector2(10, 5), ui.ToUI(new Vector2(1120, 60)));
        }

        private static readonly GameTime _gameTime = new();
    }
}
