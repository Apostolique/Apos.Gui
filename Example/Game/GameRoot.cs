using System;
using Apos.Gui;
using Apos.Input;
using Apos.Shapes;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Graphics;

namespace GameProject {
    public class GameRoot : Game {
        public GameRoot() {
            _graphics = new GraphicsDeviceManager(this) {
#if KNI
                GraphicsProfile = GraphicsProfile.FL10_0
#else
                GraphicsProfile = GraphicsProfile.HiDef
#endif
            };
            IsMouseVisible = true;
            Content.RootDirectory = "Content";

            _graphics.PreferredBackBufferWidth = 1000;
            _graphics.PreferredBackBufferHeight = 700;
        }

        protected override void Initialize() {
            Window.AllowUserResizing = true;

            base.Initialize();
        }

        protected override void LoadContent() {
            _sb = new ShapeBatch(GraphicsDevice, Content);

            FontSystem fontSystem = new();
            fontSystem.AddFont(TitleContainer.OpenStream($"{Content.RootDirectory}/source-code-pro-medium.ttf"));

            GuiHelper.Setup(this, fontSystem);
            _ui = new IMGUI();
            GuiHelper.CurrentIMGUI = _ui;

            var texture = Content.Load<Texture2D>("apos");

            _apos = new Texture2DRegion(texture, 0, 0, texture.Width, texture.Height);
        }

        private string _name = null;
        private float _slider = 1f;

        protected override void Update(GameTime gameTime) {
            GuiHelper.UpdateSetup(gameTime);

            if (_quit.Pressed())
                Exit();

            _ui.UpdateStart(gameTime);

            Dock.Put(0, 0, InputHelper.WindowWidth, InputHelper.WindowHeight);
            Vertical.Push();
            if (_menu == Menu.Main) {
                var t = Label.Put("Main Menu", id: 1);
                Label.Put($"{t.Id}");
                Label.Put($"Your name is '{_name}'");
                if (Button.Put("Settings").Clicked) _menu = Menu.Settings;
                if (Button.Put("Quit").Clicked) _menu = Menu.Quit;
            } else if (_menu == Menu.Settings) {
                Label.Put("What is your name?");
                Textbox.Put(ref _name);
                Slider.Put(ref _slider, 1f, 3f, 0.1f);
                Label.Put($"{Math.Round(_slider, 3)}");
                Icon.Put(_apos);
                GuiHelper.Scale = _slider;
                if (Button.Put("Back").Clicked) _menu = Menu.Main;
            } else if (_menu == Menu.Quit) {
                Label.Put("Quit Menu");
                if (Button.Put("Yes").Clicked) Exit();
                if (Button.Put("No").Clicked) _menu = Menu.Main;
            }
            Vertical.Pop();

            FloatingWindow.Push();
            {
                Tooltip.Put();
                // Label.Put("This is a tooltip for a window.");
                Button.Put("Tooltip button");
                Button.Put("Hover me");
                Button.Put("Button 2");
                Button.Put("Button 3");
                if (_menu == Menu.Settings) {
                    var t = Label.Put("Main Menu", id: 1);
                    Label.Put($"{t.Id}");
                }
            }
            FloatingWindow.Pop();

            if (_left.Pressed()) {
            // if (_left.Pressed()) {
                _pressed = true;
                _wasPressed = true;
                _wasReleased = false;
            }
            if (_left.HeldOnly() && _wasPressed) {
                _pressed = false;
                _held = true;
                _wasReleased = false;
            }
            if (_left.Released() || _wasPressed && MouseCondition.Released(MouseButton.LeftButton)) {
                _held = false;
                _released = true;
                _wasPressed = false;
                _wasReleased = true;
            }

            // ICondition.Released(_left);

            _ui.UpdateEnd(gameTime);
            GuiHelper.UpdateCleanup();
            base.Update(gameTime);
        }

        enum Menu {
            Main,
            Settings,
            Quit
        }
        Menu _menu = Menu.Main;

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            _sb.Begin();
            if (_pressed) {
                _sb.FillRectangle(new Vector2(500, 500), new Vector2(100, 100), TWColor.Green500);
            } else if (_held) {
                _sb.FillRectangle(new Vector2(500, 500), new Vector2(100, 100), TWColor.Yellow500);
            } else if (_released) {
                _sb.FillRectangle(new Vector2(500, 500), new Vector2(100, 100), TWColor.Red500);
            } else {
                _sb.FillRectangle(new Vector2(500, 500), new Vector2(100, 100), TWColor.Gray700);
            }
            if (_wasPressed) {
                _sb.FillRectangle(new Vector2(475, 500), new Vector2(25, 25), TWColor.Green300);
            }
            if (_wasReleased) {
                _sb.FillRectangle(new Vector2(475, 525), new Vector2(25, 25), TWColor.Red300);
            }
            _sb.End();

            _ui.Draw(gameTime);

            base.Draw(gameTime);
        }

        readonly GraphicsDeviceManager _graphics;
        private ShapeBatch _sb;

        readonly ICondition _quit =
            new AnyCondition(
                new KeyboardCondition(Keys.Escape),
                new GamePadCondition(GamePadButton.Back, 0)
            );

        IMGUI _ui;

        Texture2DRegion _apos;

        bool _pressed;
        bool _held;
        bool _released;

        bool _wasPressed;
        bool _wasReleased;

        ICondition _left = new MouseCondition(MouseButton.LeftButton);
    }
}
