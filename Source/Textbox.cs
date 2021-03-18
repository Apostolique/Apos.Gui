using Apos.Input;
using Track = Apos.Input.Track;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using FontStashSharp;
using MonoGame.Extended;
using System.Runtime.CompilerServices;

namespace Apos.Gui {
    // TODO: Scroll / Follow cursor when there's too much text.
    public class Textbox : Component {
        public Textbox(int id, string text) : base(id) {
            Text = text;
        }

        public string Text {
            get => _text;
            set {
                // TODO: Only modify the text on the next loop.
                if (_text != value) {
                    _text = value;
                    _size = GuiHelper.MeasureString(_text, 30);
                    if (Cursor > _text.Length) {
                        Cursor = _text.Length;
                    }
                }
            }
        }
        public int Padding { get; set; } = 10;
        public override bool IsFocused {
            get => base.IsFocused;
            set {
                base.IsFocused = value;
                _cursorBlink = _cursorBlinkSpeed;
            }
        }
        public override bool IsFocusable { get; set; } = true;

        public override void UpdatePrefSize(GameTime gameTime) {
            PrefWidth = MathHelper.Max(_size.X, 100) + Padding * 2;
            PrefHeight = _size.Y + Padding * 2;
        }
        public override void UpdateInput(GameTime gameTime) {
            if (Clip.Contains(GuiHelper.Mouse) && Default.MouseInteraction.Pressed()) {
                _pressed = true;
                Cursor = MouseToCursor(GuiHelper.Mouse.X, _text);
                GrabFocus(this);
            }

            if (IsFocused) {
                if (_pressed && Default.MouseInteraction.HeldOnly()) {
                    Cursor = MouseToCursor(GuiHelper.Mouse.X, _text);
                }
                if (_pressed && Default.MouseInteraction.Released()) {
                    _pressed = false;
                    Cursor = MouseToCursor(GuiHelper.Mouse.X, _text);
                }

                MoveCursor(Default.MoveLeft, -1);
                MoveCursor(Default.MoveRight, 1);

                if (Default.MoveUp.Released()) {
                    Cursor = 0;
                }
                if (Default.MoveDown.Released()) {
                    Cursor = _text.Length;
                }

                foreach (var te in InputHelper.TextEvents) {
                    if (te.Key == Keys.Tab) {
                        continue;
                    } else if (te.Key == Keys.Enter) {
                    } else if (te.Key == Keys.Back) {
                        if (Cursor > 0 && _text.Length > 0) {
                            _text = _text.Remove(Cursor - 1, 1);
                            Cursor--;
                        }
                    } else if (te.Key == Keys.Delete) {
                        if (_text.Length > 0 && Cursor < _text.Length) {
                            _text = _text.Remove(Cursor, 1);
                            _cursorBlink = _cursorBlinkSpeed;
                        }
                    } else {
                        _text = _text.Insert(Cursor, $"{te.Character}");
                        Cursor++;
                    }
                    Track.KeyboardCondition.Consume(te.Key);
                }
            }
        }
        public override void Update(GameTime gameTime) {
            if (IsFocused) {
                if (_inputDelay > 0) {
                    _inputDelay -= (int)gameTime.ElapsedGameTime.TotalMilliseconds;
                }
                if (_cursorBlink > 0) {
                    _cursorBlink -= (int)gameTime.ElapsedGameTime.TotalMilliseconds;
                } else {
                    _cursorBlink = _cursorBlinkSpeed;
                }
            }
        }
        public override void Draw(GameTime gameTime) {
            GuiHelper.SetScissor(Clip);

            if (IsFocused) {
                GuiHelper.SpriteBatch.DrawRectangle(Bounds, Color.White, 2f);

                float alignLeft = Left + Padding;

                float cursorLeft = alignLeft;
                if (Cursor > 0 && Cursor <= _text.Length) {
                    cursorLeft = alignLeft + GuiHelper.MeasureStringTight(_text.Substring(0, Cursor), _fontSize).X;
                }
                if (_cursorBlink >= _cursorBlinkSpeed * 0.5) {
                    GuiHelper.SpriteBatch.FillRectangle(new RectangleF(cursorLeft, Top, 2, Height), Color.White);
                }
            } else {
                GuiHelper.SpriteBatch.DrawRectangle(Bounds, new Color(76, 76, 76), 2f);
            }

            var font = GuiHelper.GetFont(_fontSize);
            GuiHelper.SpriteBatch.DrawString(font, _text, XY + new Vector2(Padding), new Color(200, 200, 200), GuiHelper.FontScale);

            GuiHelper.ResetScissor();
        }

        public static Textbox Put(ref string text, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            // 1. Check if Textbox with id already exists.
            //      a. If already exists. Get it.
            //      b  If not, create it.
            // 4. Ping it.
            id = GuiHelper.CurrentIMGUI.CreateId(id, isAbsoluteId);
            GuiHelper.CurrentIMGUI.TryGetValue(id, out IComponent c);

            Textbox a;
            if (c is Textbox) {
                a = (Textbox)c;
                if (a.IsFocused) {
                    text = a.Text;
                } else {
                    a.Text = text;
                }
            } else {
                a = new Textbox(id, text);
            }

            IParent? parent = GuiHelper.CurrentIMGUI.GrabParent(a);

            if (a.LastPing != InputHelper.CurrentFrame) {
                a.LastPing = InputHelper.CurrentFrame;
                if (parent != null) {
                    a.Index = parent.NextIndex();
                }
            }

            return a;
        }

        protected void MoveCursor(ICondition condition, int direction) {
            if (condition.Pressed()) {
                Cursor += direction;
                _inputDelay = _inputDelayInitialSpeed;
            }
            if (condition.HeldOnly()) {
                if (_inputDelay <= 0) {
                    Cursor += direction;
                    _inputDelay = _inputDelaySpeed;
                }
            }
        }
        protected int MouseToCursor(float x, string text) {
            float left = Left + Padding;
            float currentOffset = left;
            int currentPosition = 0;
            for (int i = 0; i < text.Length; i++) {
                if (x < currentOffset) {
                    break;
                }
                currentPosition++;
                currentOffset = left + GuiHelper.MeasureStringTight(text.Substring(0, currentPosition), _fontSize).X;
            }
            return currentPosition;
        }

        protected string _text;
        protected Vector2 _size;

        protected int _fontSize = 30;
        protected RectangleF _cursorRect;
        protected int Cursor {
            get => _cursor;
            set {
                if (value >= 0 && value <= _text.Length && _cursor != value) {
                    _cursor = value;
                    _cursorBlink = _cursorBlinkSpeed;
                }
            }
        }
        protected int _cursor;

        protected int _inputDelay = 0;
        protected int _inputDelaySpeed = 50;
        protected int _inputDelayInitialSpeed = 400;
        protected int _cursorBlink = 0;
        protected int _cursorBlinkSpeed = 1500;

        protected bool _pressed = false;
    }
}
