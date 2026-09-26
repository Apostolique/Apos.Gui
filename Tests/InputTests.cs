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
