using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Xunit;

namespace Apos.Gui.Tests {
    public class NavigationTests {
        [Theory]
        [InlineData(Direction.Left, 1, 0)]
        [InlineData(Direction.Right, 1, 2)]
        [InlineData(Direction.Up, 0, 1)]
        [InlineData(Direction.Down, 2, 1)]
        public void MovesToTheNeighborInAGrid(Direction direction, int row, int column) {
            var ui = new IMGUI();
            Spot[,] grid = new Spot[3, 3];
            Frame(ui, () => {
                for (int r = 0; r < 3; r++) {
                    for (int c = 0; c < 3; c++) {
                        grid[r, c] = Spot.Put(new RectangleF(c * 60, r * 60, 50, 50), id: r * 3 + c);
                    }
                }
            });

            ui.GrabFocus(grid[1, 1]);
            ui.Navigate(direction);

            Assert.True(grid[row, column].IsFocused);
        }

        [Fact]
        public void LeftAndRightStayInTheRow() {
            // A centered checkbox, then a row of three radio buttons. The checkbox is a bit to the right of the middle radio.
            var ui = new IMGUI();
            Spot normal = null!, hard = null!;
            Frame(ui, () => {
                Spot.Put(new RectangleF(430, 0, 30, 30), id: 1);
                Spot.Put(new RectangleF(300, 60, 30, 30), id: 2);
                normal = Spot.Put(new RectangleF(422, 60, 30, 30), id: 3);
                hard = Spot.Put(new RectangleF(580, 60, 30, 30), id: 4);
                Spot.Put(new RectangleF(455, 300, 90, 50), id: 5);
            });

            ui.GrabFocus(normal);
            ui.Navigate(Direction.Right);
            Assert.True(hard.IsFocused);

            ui.Navigate(Direction.Left);
            Assert.True(normal.IsFocused);
        }

        [Fact]
        public void UpAndDownGoDownAMenuWithOffCenterRows() {
            // A centered slider, then a checkbox with its text, a bit off center, then a centered button.
            var ui = new IMGUI();
            Spot slider = null!, checkbox = null!;
            Frame(ui, () => {
                slider = Spot.Put(new RectangleF(450, 0, 100, 40), id: 1);
                checkbox = Spot.Put(new RectangleF(355, 50, 290, 30), id: 2);
                Spot.Put(new RectangleF(455, 90, 90, 50), id: 3);
            });

            ui.GrabFocus(slider);
            ui.Navigate(Direction.Down);

            Assert.True(checkbox.IsFocused);
        }

        [Fact]
        public void DownStopsAtARowThatsOffCenter() {
            // A centered slider, then a small checkbox left of center, then a centered button.
            var ui = new IMGUI();
            Spot slider = null!, checkbox = null!;
            Frame(ui, () => {
                slider = Spot.Put(new RectangleF(450, 0, 100, 40), id: 1);
                checkbox = Spot.Put(new RectangleF(355, 50, 30, 30), id: 2);
                Spot.Put(new RectangleF(455, 90, 90, 50), id: 3);
            });

            ui.GrabFocus(slider);
            ui.Navigate(Direction.Down);

            Assert.True(checkbox.IsFocused);
        }

        [Fact]
        public void UpFromTheSideOfARowGoesToTheRowAbove() {
            // Tabs, then a centered checkbox, then radio buttons that are wider than the checkbox.
            var ui = new IMGUI();
            Spot music = null!, easy = null!;
            Frame(ui, () => {
                Spot.Put(new RectangleF(262, 140, 163, 55), id: 1);
                music = Spot.Put(new RectangleF(435, 200, 130, 30), id: 2);
                easy = Spot.Put(new RectangleF(293, 236, 112, 34), id: 3);
                Spot.Put(new RectangleF(425, 236, 150, 34), id: 4);
            });

            ui.GrabFocus(easy);
            ui.Navigate(Direction.Up);

            Assert.True(music.IsFocused);
        }

        [Fact]
        public void InTheNextRowTheOneInLineWins() {
            var ui = new IMGUI();
            Spot from = null!, inLine = null!;
            Frame(ui, () => {
                from = Spot.Put(new RectangleF(100, 0, 50, 50), id: 1);
                Spot.Put(new RectangleF(0, 60, 50, 50), id: 2);
                inLine = Spot.Put(new RectangleF(100, 65, 50, 50), id: 3);
                Spot.Put(new RectangleF(200, 60, 50, 50), id: 4);
            });

            ui.GrabFocus(from);
            ui.Navigate(Direction.Down);

            Assert.True(inLine.IsFocused);
        }

        [Fact]
        public void SkipsSomethingThatCoversIt() {
            var ui = new IMGUI();
            Spot from = null!, below = null!;
            Frame(ui, () => {
                Spot.Put(new RectangleF(10, 0, 100, 300), id: 1);
                from = Spot.Put(new RectangleF(10, 50, 100, 30), id: 2);
                below = Spot.Put(new RectangleF(10, 90, 100, 30), id: 3);
            });

            ui.GrabFocus(from);
            ui.Navigate(Direction.Down);

            Assert.True(below.IsFocused);
        }

        [Fact]
        public void StaysWhenNothingIsInThatDirection() {
            var ui = new IMGUI();
            Spot a = null!;
            Frame(ui, () => {
                a = Spot.Put(new RectangleF(0, 0, 50, 50), id: 1);
                Spot.Put(new RectangleF(0, 60, 50, 50), id: 2);
            });

            ui.GrabFocus(a);
            ui.Navigate(Direction.Up);

            Assert.True(a.IsFocused);
        }

        [Fact]
        public void NothingFocusedFocusesTheFirstOne() {
            var ui = new IMGUI();
            Spot a = null!;
            Frame(ui, () => {
                a = Spot.Put(new RectangleF(0, 60, 50, 50), id: 1);
                Spot.Put(new RectangleF(0, 0, 50, 50), id: 2);
            });

            ui.Navigate(Direction.Down);

            Assert.True(a.IsFocused);
        }

        [Fact]
        public void FocusScopesKeepTheFocusInside() {
            var ui = new IMGUI();
            Spot inside = null!, outside = null!;
            Frame(ui, () => {
                outside = Spot.Put(new RectangleF(0, 0, 50, 50), id: 1);
                IMGUI.Push(2, false, "", id => new Group(id) { IsFocusScope = true });
                inside = Spot.Put(new RectangleF(100, 0, 50, 50), id: 1);
                ui.Pop();
            });

            ui.GrabFocus(inside);
            ui.Navigate(Direction.Left);
            Assert.True(inside.IsFocused);

            ui.GrabFocus(outside);
            ui.Navigate(Direction.Right);
            Assert.True(outside.IsFocused);
        }

        [Fact]
        public void ParentsCanPickTheTarget() {
            var ui = new IMGUI();
            Spot first = null!, last = null!;
            Frame(ui, () => {
                IMGUI.Push(1, false, "", id => new Group(id) { Wraps = true });
                first = Spot.Put(new RectangleF(0, 0, 50, 50), id: 1);
                Spot.Put(new RectangleF(60, 0, 50, 50), id: 2);
                last = Spot.Put(new RectangleF(120, 0, 50, 50), id: 3);
                ui.Pop();
            });

            ui.GrabFocus(last);
            ui.Navigate(Direction.Right);

            Assert.True(first.IsFocused);
        }

        private class Spot(long id) : Component(id) {
            public override bool IsFocusable { get; set; } = true;

            public static Spot Put(RectangleF bounds, int id) {
                Spot s = IMGUI.Put(id, false, "", id => new Spot(id));
                s.Bounds = bounds;
                return s;
            }
        }

        /// <summary>A row that can wrap around to its first child.</summary>
        private class Group(long id) : ListParent(id) {
            public bool Wraps { get; set; }

            public override IComponent? Navigate(IComponent from, Direction direction) {
                if (Wraps && direction == Direction.Right && from == _children[^1]) {
                    return _children[0];
                }
                return null;
            }
        }

        private static void Frame(IMGUI ui, Action build) {
            ui.UpdateSetup(_gameTime);
            build();
            ui.Cleanup();
        }

        private static readonly GameTime _gameTime = new();
    }
}
