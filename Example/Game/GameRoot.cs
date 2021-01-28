using Apos.Gui;
using Apos.Input;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

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
        }

        protected override void Update(GameTime gameTime) {
            GuiHelper.UpdateSetup();

            if (_quit.Pressed())
                Exit();

            _ui.UpdateAll();

            Panel.Use();
            if (Button.Use("Reset").Clicked) _counter = 0;
            if (Button.Use("Add one").Clicked) _counter++;
            Label.Use($"Counter: {_counter}");
            if (10 <= _counter && _counter < 12) Label.Use("Why hello, world! there.");
            if (12 <= _counter && _counter < 17) Label.Use("I see you like clicking.");
            if (20 <= _counter && _counter < 23) Label.Use("There is really no point though...");
            Panel.Pop();

            GuiHelper.UpdateCleanup();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            _ui.Draw();

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
    }
}
