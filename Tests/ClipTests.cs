using MonoGame.Extended;
using Xunit;

namespace Apos.Gui.Tests {
    public class ClipTests {
        private static readonly RectangleF _active = new(0f, 0f, 100f, 100f);

        [Fact]
        public void AClipInsideKeepsItsRounding() {
            var (rect, rounding) = GuiHelper.IntersectClip(new RectangleF(10f, 10f, 50f, 50f), 4f, _active, 8f);

            Assert.Equal(new RectangleF(10f, 10f, 50f, 50f), rect);
            Assert.Equal(4f, rounding);
        }

        [Fact]
        public void AClipThatCoversTakesTheActiveOne() {
            var (rect, rounding) = GuiHelper.IntersectClip(new RectangleF(-10f, -10f, 200f, 200f), 0f, _active, 8f);

            Assert.Equal(_active, rect);
            Assert.Equal(8f, rounding);
        }

        [Fact]
        public void TheSameClipKeepsTheBiggerRounding() {
            var (rect, rounding) = GuiHelper.IntersectClip(new RectangleF(0f, 0f, 100.001f, 100f), 2f, _active, 8f);

            Assert.Equal(8f, rounding);
            Assert.Equal(100.001f, rect.Width);
        }

        [Fact]
        public void ClipsThatCutEachOtherOverlapWithSquareCorners() {
            var (rect, rounding) = GuiHelper.IntersectClip(new RectangleF(50f, -20f, 100f, 80f), 4f, _active, 8f);

            Assert.Equal(new RectangleF(50f, 0f, 50f, 60f), rect);
            Assert.Equal(0f, rounding);
        }

        [Fact]
        public void ClipsThatDontTouchLeaveNothing() {
            var (rect, _) = GuiHelper.IntersectClip(new RectangleF(200f, 200f, 50f, 50f), 0f, _active, 0f);

            Assert.Equal(0f, rect.Width);
            Assert.Equal(0f, rect.Height);
        }
    }
}
