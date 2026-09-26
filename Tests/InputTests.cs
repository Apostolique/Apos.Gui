using System;
using System.IO;
using Apos.Input;
using Apos.Shapes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Xunit;

namespace Apos.Gui.Tests {
    /// <summary>Drives whole frames with simulated devices, the way a game loop would.</summary>
    public class InputTests {
        public InputTests() {
            GuiHelper.Font = _font;
            InputHelper.Setup(_input);
            _ui = new IMGUI { Area = new RectangleF(0, 0, 800, 480) };
        }

        [Fact]
        public void MouseClicksAButton() {
            Button b = null!;
            void Build() => b = Button.Put("Play");

            Frame(Build);
            Vector2 center = b.Bounds.Center;
            _input.Mouse = Mouse(center, ButtonState.Pressed);
            Frame(Build);
            Assert.False(b.Clicked);
            _input.Mouse = Mouse(center, ButtonState.Released);
            Frame(Build);

            Assert.True(b.Clicked);
        }

        [Fact]
        public void ArrowsMoveTheFocusAndEnterClicks() {
            Button play = null!, quit = null!;
            void Build() {
                Vertical.Push();
                play = Button.Put("Play");
                quit = Button.Put("Quit");
                Vertical.Pop();
            }

            Frame(Build);
            Tap(Build, Keys.Down);
            Assert.True(play.IsFocused);
            Tap(Build, Keys.Down);
            Assert.True(quit.IsFocused);

            bool clicked = false;
            Tap(() => { Build(); clicked |= quit.Clicked; }, Keys.Enter);
            Assert.True(clicked);
        }

        [Fact]
        public void AFocusedSliderTakesTheArrowBeforeNavigation() {
            float value = 0f;
            Slider s = null!;
            Button right = null!;
            void Build() {
                Horizontal.Push();
                s = Slider.Put(ref value, 0f, 10f, 1f);
                right = Button.Put("Next");
                Horizontal.Pop();
            }

            Frame(Build);
            _ui.GrabFocus(s);
            Tap(Build, Keys.Right);

            Assert.Equal(1f, value);
            Assert.True(s.IsFocused);
            Assert.False(right.IsFocused);
        }

        [Fact]
        public void PressingTheEndsOfASliderReachesItsLimits() {
            Theme.Rounding = 8f;
            try {
                float value = 5f;
                Slider s = null!;
                void Build() => s = Slider.Put(ref value, 0f, 10f);

                Frame(Build);
                Click(Build, new Vector2(s.Right - 1f, s.Top + s.Height / 2f));
                Assert.Equal(10f, value);
                Click(Build, new Vector2(s.Left + 1f, s.Top + s.Height / 2f));
                Assert.Equal(0f, value);
            } finally {
                Theme.Rounding = 0f;
            }
        }

        [Fact]
        public void TextboxTypesAndErases() {
            string text = "";
            Textbox t = null!;
            void Build() => t = Textbox.Put(ref text);

            Frame(Build);
            _ui.GrabFocus(t);
            _input.Type("abc");
            Frame(Build);
            _input.Type('\b', Keys.Back);
            Frame(Build);

            Assert.Equal("ab", text);
        }

        [Fact]
        public void AGamePadUIIgnoresTheMouse() {
            _ui.Bindings = Bindings.ForGamePad(1);
            Button b = null!;
            void Build() => b = Button.Put("Play");

            Frame(Build);
            Vector2 center = b.Bounds.Center;
            _input.Mouse = Mouse(center, ButtonState.Pressed);
            Frame(Build);
            _input.Mouse = Mouse(center, ButtonState.Released);
            Frame(Build);

            Assert.False(b.Clicked);
        }

        [Fact]
        public void TooltipOpensAfterTheDelayAndStaysInTheUI() {
            Tooltip t = null!;
            Button b = null!;
            void Build() {
                // In the bottom right corner, so the tip has to go above and to the left.
                Dock.Put(700f, 430f, 800f, 480f);
                t = Tooltip.Put();
                b = Button.Put("Help");
                Label.Put("A tip that is a lot wider than the button it explains.");
            }

            Frame(Build);
            _ui.GrabFocus(b);
            Frame(Build);
            Assert.False(t.IsOpen);

            for (int i = 0; i < 30; i++) Frame(Build);

            Assert.True(t.IsOpen);
            Assert.True(t.Bubble.Bottom <= b.Top);
            Assert.True(t.Bubble.Right <= 800f);
            Assert.True(t.Bubble.Left >= 0f);
        }

        [Fact]
        public void TooltipOpensWhenTheMouseStaysOverItsComponent() {
            Tooltip t = null!;
            Button b = null!;
            void Build() {
                t = Tooltip.Put();
                b = Button.Put("Help");
                Label.Put("Tip");
            }

            Frame(Build);
            _input.Mouse = Mouse(b.Bounds.Center, ButtonState.Released);
            for (int i = 0; i < 30; i++) Frame(Build);
            Assert.True(t.IsOpen);
            Assert.True(t.Bubble.Top >= b.Bottom);

            _input.Mouse = Mouse(new Vector2(700f, 400f), ButtonState.Released);
            Frame(Build);
            Assert.False(t.IsOpen);
        }

        [Fact]
        public void TooltipWithoutATipDoesntTrapTheFocusWalk() {
            Button before = null!, after = null!;
            void Build() {
                Vertical.Push();
                before = Button.Put("Before");
                Tooltip.Put();
                Button.Put("Main");
                Tooltip.Pop();
                after = Button.Put("After");
                Vertical.Pop();
            }

            Frame(Build);
            _ui.GrabFocus(before);
            Tap(Build, Keys.Tab);
            Tap(Build, Keys.Tab);

            Assert.True(after.IsFocused);
        }

        [Fact]
        public void WindowOpensCenteredAndCantBeDraggedOut() {
            FloatingWindow w = null!;
            void Build() {
                w = FloatingWindow.Push("Inventory");
                Label.Put("Sword");
                FloatingWindow.Pop();
            }

            Frame(Build);
            Frame(Build);
            Assert.Equal((800f - w.Width) / 2f, w.X, 1);
            Assert.Equal(w.Top + w.TitleHeight, w.Content!.Top, 1);

            Vector2 grab = w.TitleBar.Center;
            _input.Mouse = Mouse(grab, ButtonState.Pressed);
            Frame(Build);
            _input.Mouse = Mouse(grab + new Vector2(2000f, 0f), ButtonState.Pressed);
            Frame(Build);
            _input.Mouse = Mouse(grab + new Vector2(2000f, 0f), ButtonState.Released);
            Frame(Build);

            Assert.Equal(800f, w.Right, 1);
        }

        [Fact]
        public void WindowCloseButtonClosesIt() {
            bool open = true;
            FloatingWindow w = null!;
            void Build() {
                if (!open) return;
                w = FloatingWindow.Push(ref open, "Inventory");
                Label.Put("Sword");
                FloatingWindow.Pop();
            }

            Frame(Build);
            Frame(Build);
            Click(Build, w.CloseButton.Center);

            Assert.False(open);
        }

        /// <summary>Presses the left mouse button at a spot for one frame, then lets it go.</summary>
        private void Click(Action build, Vector2 at) {
            _input.Mouse = Mouse(at, ButtonState.Pressed);
            Frame(build);
            _input.Mouse = Mouse(at, ButtonState.Released);
            Frame(build);
        }

        /// <summary>Presses a key for one frame, then lets it go for one frame.</summary>
        private void Tap(Action build, Keys key) {
            _input.Keyboard = new KeyboardState(key);
            Frame(build);
            _input.Keyboard = new KeyboardState();
            Frame(build);
        }

        private void Frame(Action build) {
            _time += TimeSpan.FromMilliseconds(16);
            var gameTime = new GameTime(_time, TimeSpan.FromMilliseconds(16));
            GuiHelper.UpdateSetup(gameTime);
            _ui.UpdateStart(gameTime);
            build();
            _ui.UpdateEnd(gameTime);
            GuiHelper.UpdateCleanup();
        }

        private static MouseState Mouse(Vector2 p, ButtonState left) =>
            new((int)p.X, (int)p.Y, 0, left, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);

        private readonly SimulatedInput _input = new();
        private readonly IMGUI _ui;
        private TimeSpan _time = TimeSpan.Zero;

        private static readonly ShapeFont _font = new(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "source-code-pro-medium.ttf")));
    }
}
