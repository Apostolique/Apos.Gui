using System;
using Apos.Gui;
using Apos.Input;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.TextureAtlases;

namespace GameProject {
    public class GameRoot : Game {
        public GameRoot() {
            _graphics = new GraphicsDeviceManager(this);
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
            _s = new SpriteBatch(GraphicsDevice);

            FontSystem fontSystem = FontSystemFactory.Create(GraphicsDevice, 2048, 2048);
            fontSystem.AddFont(TitleContainer.OpenStream($"{Content.RootDirectory}/source-code-pro-medium.ttf"));

            GuiHelper.Setup(this, fontSystem);
            _ui = new IMGUI();
            GuiHelper.CurrentIMGUI = _ui;

            var texture = Content.Load<Texture2D>("apos");

            _apos = new TextureRegion2D(texture, 0, 0, texture.Width, texture.Height);
        }

        private string _name = "no name";
        private float _slider = 0.5f;

        protected override void Update(GameTime gameTime) {
            GuiHelper.UpdateSetup(gameTime);

            if (_quit.Pressed())
                Exit();

            _ui.UpdateAll(gameTime);

            MenuPanel.Push().XY = new Vector2(100, 100);
            if (_menu == Menu.Main) {
                Label.Put("Main Menu");
                Label.Put($"Your name is '{_name}'");
                if (Button.Put("Settings").Clicked) _menu = Menu.Settings;
                if (Button.Put("Quit").Clicked) _menu = Menu.Quit;
            } else if (_menu == Menu.Settings) {
                Label.Put("What is your name?");
                Textbox.Put(ref _name);
                Slider.Put(ref _slider, 0f, 1f, 0.1f);
                Label.Put($"{Math.Round(_slider, 3)}");
                Icon.Put(_apos);
                if (Button.Put("Back").Clicked) _menu = Menu.Main;
            } else if (_menu == Menu.Quit) {
                Label.Put("Quit Menu");
                if (Button.Put("Yes").Clicked) Exit();
                if (Button.Put("No").Clicked) _menu = Menu.Main;
            }
            MenuPanel.Pop();

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

            _ui.Draw(gameTime);

            base.Draw(gameTime);
        }

        GraphicsDeviceManager _graphics;
        SpriteBatch _s;

        ICondition _quit =
            new AnyCondition(
                new KeyboardCondition(Keys.Escape),
                new GamePadCondition(GamePadButton.Back, 0)
            );

        IMGUI _ui;
        int _counter = 0;

        TextureRegion2D _apos;
    }
}
