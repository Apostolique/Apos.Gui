using System;
using System.Collections.Generic;
using System.Linq;
using Apos.Gui;
using Apos.Shapes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
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
            using var fontStream = TitleContainer.OpenStream($"{Content.RootDirectory}/source-code-pro-medium.ttf");
            GuiHelper.Setup(this, new ShapeFont(fontStream));

            _ui = new IMGUI();
            // Each split screen player gets a UI of their own that only listens to their gamepad.
            _players = [
                new IMGUI { Bindings = Bindings.ForGamePad(0) },
                new IMGUI { Bindings = Bindings.ForGamePad(1) },
            ];

            var texture = Content.Load<Texture2D>("apos");
            _apos = new Texture2DRegion(texture, 0, 0, texture.Width, texture.Height);
        }

        protected override void Update(GameTime gameTime) {
            GuiHelper.UpdateSetup(gameTime);

            // The players' gamepads belong to their own UIs while the screen is split.
            _ui.Bindings = _screen == Screen.SplitScreen ? _withoutGamePads : _allInputs;
            _ui.UpdateStart(gameTime);
            switch (_screen) {
                case Screen.Main: MainMenu(); break;
                case Screen.Components: ComponentsScreen(); break;
                case Screen.Settings: SettingsScreen(); break;
                case Screen.SplitScreen: SplitScreenBar(); break;
            }
            QuitPopup();
            if (_showWindow) FloatingWindowDemo();
            _ui.UpdateEnd(gameTime);

            if (_screen == Screen.SplitScreen) {
                for (int i = 0; i < _players.Length; i++) {
                    Player(gameTime, i);
                }
            }

            GuiHelper.UpdateCleanup();
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            _ui.Draw(gameTime);
            if (_screen == Screen.SplitScreen) {
                foreach (IMGUI p in _players) {
                    p.Draw(gameTime);
                }
            }

            base.Draw(gameTime);
        }

        private void MainMenu() {
            MenuPanel.Push();
            Label.Put("Apos.Gui", 50);
            Label.Put("Arrows, Tab, a gamepad, the mouse or touch all work.", 20);
            if (Button.Put("Components").Clicked) _screen = Screen.Components;
            if (Button.Put("Settings").Clicked) _screen = Screen.Settings;
            if (Button.Put("Split screen").Clicked) _screen = Screen.SplitScreen;
            if (Button.Put("Quit").Clicked) OpenQuit();
            MenuPanel.Pop();

            if (!_quitOpen && _ui.Bindings.Back.Pressed()) OpenQuit();
        }

        private void ComponentsScreen() {
            MenuPanel.Push();
            Tabs.Put(ref _tab, ["Controls", "Text", "List", "Custom"]);
            switch (_tab) {
                case 0: ControlsTab(); break;
                case 1: TextTab(); break;
                case 2: ListTab(); break;
                case 3: CustomTab(); break;
            }
            BackButton();
            MenuPanel.Pop();
        }

        private void ControlsTab() {
            Checkbox.Put(ref _music, "Music");

            Horizontal.Push().Spacing = 20f;
            string[] difficulties = ["Easy", "Normal", "Hard"];
            for (int i = 0; i < difficulties.Length; i++) {
                RadioButton.Put(ref _difficulty, i, difficulties[i]);
            }
            Horizontal.Pop();

            Label.Put($"Volume {MathF.Round(_volume * 100)}%");
            Slider.Put(ref _volume, 0f, 1f, 0.05f);
            ProgressBar.Put(_volume);

            Dropdown.Put(ref _resolution, _resolutions);
            // Taller than the screen, so the list scrolls.
            Dropdown.Put(ref _level, _levels);

            Tooltip.Put();
            Button.Put("Hover me");
            Label.Put("Tooltips show up next to what they explain.", 20);
        }

        private void TextTab() {
            Paragraph p = Paragraph.Put("A paragraph wraps its text to the space it's given. Resize the window to see it flow. Textboxes have selection, copy and paste, and Ctrl moves a word at a time.", 22);
            p.MaxWidth = 600f;

            Label.Put("Name");
            Textbox.Put(ref _name);
            Label.Put("Notes");
            Textbox.Put(ref _notes, 22).Multiline = true;
        }

        private void ListTab() {
            Horizontal.Push();
            if (Button.Put("Add").Clicked) _quests.Add(new Quest(_nextQuest++));
            if (Button.Put("Remove first").Clicked && _quests.Count > 0) _quests.RemoveAt(0);
            Horizontal.Pop();

            // Each row is keyed by its quest, so removing one doesn't move the checkboxes to other rows.
            Block.Put(420f, 250f);
            Vertical.Push();
            foreach (Quest q in _quests) {
                _ui.PushId(q.Id);
                Checkbox.Put(ref q.Done, $"Quest {q.Id}");
                _ui.PopId();
            }
            Vertical.Pop();
        }

        private void CustomTab() {
            Label.Put("Knobs are a custom component, see Knob.cs.", 20);
            Horizontal.Push();
            Knob.Put(ref _bass);
            Knob.Put(ref _treble);
            Horizontal.Pop();
            Label.Put($"Bass {MathF.Round(_bass * 10)} Treble {MathF.Round(_treble * 10)}");

            Label.Put("A button with its own look:", 20);
            Button danger = Button.Put("Reset the knobs");
            danger.Look = DrawDanger;
            if (danger.Clicked) {
                _bass = 0.5f;
                _treble = 0.5f;
            }

            Icon.Put(_apos);
        }

        private void SettingsScreen() {
            MenuPanel.Push();
            Label.Put("Style");
            Horizontal.Push().Spacing = 20f;
            if (RadioButton.Put(ref _style, 0, "Outline").Clicked) Theme.UseOutline();
            if (RadioButton.Put(ref _style, 1, "Filled").Clicked) Theme.UseFilled();
            Horizontal.Pop();

            Label.Put($"UI scale {_scale:0.00}");
            Slider.Put(ref _scale, 1f, 2f, 0.25f);
            _ui.Scale = _scale;

            Label.Put($"Rounding {Theme.Rounding:0}");
            float rounding = Theme.Rounding;
            Slider.Put(ref rounding, 0f, 12f, 1f);
            Theme.Rounding = rounding;

            Checkbox.Put(ref _showWindow, "Floating window");

            BackButton();
            MenuPanel.Pop();
        }

        private void SplitScreenBar() {
            Horizontal.Push().Align = CrossAlign.Center;
            BackButton();
            Label.Put("Each half has its own gamepad. B comes back here.", 20);
            Horizontal.Pop();

            foreach (IMGUI p in _players) {
                if (p.Bindings.Back.Pressed()) _screen = Screen.Main;
            }
        }

        private void Player(GameTime gameTime, int index) {
            IMGUI ui = _players[index];
            float top = 70f * _ui.Scale;
            float half = GraphicsDevice.Viewport.Width / 2f;
            ui.Area = new RectangleF(half * index, top, half, GraphicsDevice.Viewport.Height - top);
            ui.Scale = _ui.Scale;

            ui.UpdateStart(gameTime);
            MenuPanel.Push();
            Label.Put($"Player {index + 1}");
            Label.Put($"Gamepad {index + 1}", 20);
            Checkbox.Put(ref _ready[index], _ready[index] ? "Ready!" : "Not ready");
            Slider.Put(ref _sensitivity[index], 0f, 1f, 0.1f);
            MenuPanel.Pop();
            ui.UpdateEnd(gameTime);
        }

        private void FloatingWindowDemo() {
            FloatingWindow w = FloatingWindow.Push(ref _showWindow, "Window");
            // Opens in the top right corner, away from the settings.
            w.StartAlign = new Vector2(1f, 0f);
            w.StartOffset = new Vector2(-20f, 20f);
            Label.Put("Drag me by the title.", 20);
            Button.Put("A button");
            Checkbox.Put(ref _longWindow, "Long content");
            if (_longWindow) {
                // Taller than the screen, so the window scrolls.
                for (int i = 1; i <= 30; i++) {
                    Label.Put($"Line {i}", 20, id: i);
                }
            }
            FloatingWindow.Pop();
        }

        private void BackButton() {
            if (Button.Put("Back").Clicked || !_quitOpen && _ui.Bindings.Back.Pressed()) {
                _screen = Screen.Main;
            }
        }

        private void OpenQuit() {
            _quitOpen = true;
            _focusQuit = true;
        }

        private void QuitPopup() {
            if (!_quitOpen) return;

            Popup.Push(ref _quitOpen, modal: true);
            Label.Put("Quit the game?");
            Horizontal.Push();
            if (Button.Put("Yes").Clicked) Exit();
            Button no = Button.Put("No");
            if (no.Clicked) _quitOpen = false;
            Horizontal.Pop();
            Popup.Pop();

            // Opening it with a keyboard or a gamepad should put the focus inside.
            if (_focusQuit) {
                _ui.GrabFocus(no);
                _focusQuit = false;
            }
        }

        private static void DrawDanger(Button b) {
            Color red = TWColor.Red500;
            float highlight = b.Clicked ? 0.6f : 0.2f + 0.2f * b.Interaction.Hover + 0.2f * b.Interaction.Press;
            GuiHelper.ShapeBatch.FillRectangle(b.XY, b.Size, new Color(red, highlight), Theme.Rounding);
            GuiHelper.ShapeBatch.BorderRectangle(b.XY, b.Size, b.IsFocused ? Theme.Focus : red, Theme.BorderThickness, Theme.Rounding);
        }

        private enum Screen {
            Main,
            Components,
            Settings,
            SplitScreen,
        }

        private class Quest(int id) {
            public int Id { get; } = id;
            public bool Done;
        }

        private readonly GraphicsDeviceManager _graphics;
        private IMGUI _ui = null!;
        private IMGUI[] _players = [];
        private readonly Bindings _allInputs = new();
        private readonly Bindings _withoutGamePads = Bindings.ForKeyboardAndMouse();
        private Texture2DRegion _apos = null!;

        private Screen _screen = Screen.Main;
        private bool _quitOpen;
        private bool _focusQuit;
        private bool _showWindow;
        private float _scale = 1f;
        private int _style = 1;

        private int _tab;
        private bool _music = true;
        private int _difficulty = 1;
        private float _volume = 0.8f;
        private int _resolution;
        private readonly string[] _resolutions = ["1280x720", "1920x1080", "2560x1440"];
        private int _level;
        private readonly string[] _levels = [.. Enumerable.Range(1, 40).Select(i => $"Level {i}")];
        private bool _longWindow;
        private string _name = "";
        private string _notes = "";
        private readonly List<Quest> _quests = [new(1), new(2), new(3), new(4), new(5), new(6), new(7), new(8)];
        private int _nextQuest = 9;
        private float _bass = 0.5f;
        private float _treble = 0.5f;

        private readonly bool[] _ready = new bool[2];
        private readonly float[] _sensitivity = [0.5f, 0.5f];
    }
}
