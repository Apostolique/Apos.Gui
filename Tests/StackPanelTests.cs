using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Xunit;

namespace Apos.Gui.Tests {
    public class StackPanelTests {
        [Fact]
        public void SpacingAndPaddingAddUp() {
            var (v, a, b) = Stack(Orientation.Vertical, CrossAlign.Start);

            Assert.Equal(new Vector2(110f, 100f), v.PrefSize);
            Layout(v, v.PrefSize);
            Assert.Equal(new Vector2(5f, 5f), a.XY);
            Assert.Equal(new Vector2(100f, 50f), a.Size);
            Assert.Equal(new Vector2(5f, 65f), b.XY);
            Assert.Equal(new Vector2(60f, 30f), b.Size);
        }

        [Theory]
        [InlineData(CrossAlign.Start, 5f, 60f)]
        [InlineData(CrossAlign.Center, 25f, 60f)]
        [InlineData(CrossAlign.End, 45f, 60f)]
        [InlineData(CrossAlign.Stretch, 5f, 100f)]
        public void AlignPlacesChildrenAcross(CrossAlign align, float x, float width) {
            var (v, _, b) = Stack(Orientation.Vertical, align);

            Layout(v, v.PrefSize);

            Assert.Equal(x, b.X);
            Assert.Equal(width, b.Width);
        }

        [Fact]
        public void HorizontalStacksLeftToRight() {
            var (h, a, b) = Stack(Orientation.Horizontal, CrossAlign.Start);

            Assert.Equal(new Vector2(180f, 60f), h.PrefSize);
            Layout(h, h.PrefSize);
            Assert.Equal(new Vector2(5f, 5f), a.XY);
            Assert.Equal(new Vector2(115f, 5f), b.XY);
        }

        [Fact]
        public void ScrollsOnlyWhenChildrenDontFit() {
            var (v, _, _) = Stack(Orientation.Vertical, CrossAlign.Start);

            Layout(v, v.PrefSize);
            Assert.False(v.CanScroll);
            Assert.Equal(0f, v.ScrollbarThumb.Height);

            Layout(v, new Vector2(110f, 50f));
            Assert.True(v.CanScroll);
            Assert.True(v.ScrollbarThumb.Height > 0f);

            v.OffsetY = -1000f;
            Apos.Tweens.TweenHelper.TotalMS += 10000;
            Layout(v, new Vector2(110f, 50f));
            Assert.Equal(-50f, v.OffsetY, 3);
        }

        [Fact]
        public void TheScrollbarDoesntCoverTheChildren() {
            var ui = new IMGUI { Area = new RectangleF(0, 0, 800, 480) };
            StackPanel v = null!;
            var rows = new Block[5];
            void Build() {
                Block.Put(200f, 100f, id: 1);
                v = Vertical.Push(id: 2);
                for (int i = 0; i < rows.Length; i++) {
                    rows[i] = Block.Put(200f, 40f, id: 3 + i);
                    Block.Pop();
                }
                Vertical.Pop();
            }

            ui.UpdateSetup(_gameTime);
            Build();
            ui.Cleanup();
            ui.UpdateLayout(_gameTime);

            Assert.True(v.CanScroll);
            Assert.All(rows, r => Assert.True(r.Right <= v.ScrollbarThumb.Left));
        }

        private static (StackPanel stack, Block a, Block b) Stack(Orientation orientation, CrossAlign align) {
            var ui = new IMGUI();
            StackPanel stack = null!;
            Block a = null!, b = null!;

            ui.UpdateSetup(_gameTime);
            stack = orientation == Orientation.Vertical ? Vertical.Push(id: 1) : Horizontal.Push(id: 1);
            stack.Spacing = 10f;
            stack.Padding = 5f;
            stack.Align = align;
            a = Block.Put(100f, 50f, id: 2);
            Block.Pop();
            b = Block.Put(60f, 30f, id: 3);
            Block.Pop();
            Vertical.Pop();
            ui.Cleanup();

            stack.UpdatePrefSize(_gameTime);
            return (stack, a, b);
        }

        private static void Layout(StackPanel stack, Vector2 size) {
            stack.XY = Vector2.Zero;
            stack.Size = size;
            stack.Clip = stack.Bounds;
            stack.UpdateLayout(_gameTime);
        }

        private static readonly GameTime _gameTime = new();
    }
}
