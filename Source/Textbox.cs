using Apos.Input;
using Track = Apos.Input.Track;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using FontStashSharp;
using MonoGame.Extended;
using System.Runtime.CompilerServices;
using Apos.Tweens;

namespace Apos.Gui {
    // TODO: Textbox shouldn't change it's size. It should take a "preferred width" and just use that.
    //       Will need to add scrolling.

    public class Textbox : Component {
        public Textbox(int id, string text, int fontSize, Color c) : base(id) {
            Text = text;
            FontSize = fontSize;
            Color = c;
        }

        public string Text {
            get => _text;
            set {
                value ??= "";
                if (_text != value) {
                    _text = value;
                    _isDirty = true;
                }
            }
        }
        public int Padding { get; set; } = 10;
        public Color Color { get; set; }
        public int FontSize {
            get => _fontSize;
            set {
                if (value != _fontSize) {
                    _fontSize = value;
                    _isDirty = true;
                }
            }
        }

        public override bool IsFocused {
            get => base.IsFocused;
            set {
                base.IsFocused = value;
                _blink.StartTime = TweenHelper.TotalMS;
            }
        }
        public override bool IsFocusable { get; set; } = true;

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
                            Cursor--;
                            Text = _text.Remove(Cursor, 1);
                        }
                    } else if (te.Key == Keys.Delete) {
                        if (_text.Length > 0 && Cursor < _text.Length) {
                            Text = _text.Remove(Cursor, 1);
                            _blink.StartTime = TweenHelper.TotalMS;
                        }
                    } else {
                        Text = _text.Insert(Cursor, $"{te.Character}");
                        Cursor++;
                    }
                    Track.KeyboardCondition.Consume(te.Key);
                }
            }
        }
        public override void Update(GameTime gameTime) {
            if (IsFocused) {
                if (TweenHelper.TotalMS > _blink.StartTime + _blink.Duration) {
                    _blink.StartTime = TweenHelper.TotalMS;
                }
            }
        }

        public override void UpdatePrefSize(GameTime gameTime) {
            Cache();
            PrefWidth = MathHelper.Max(_cachedSize.X, 100f) + Padding * 2f;
            PrefHeight = _cachedSize.Y + Padding * 2f;
        }

        public override void Draw(GameTime gameTime) {
            GuiHelper.PushScissor(Clip);

            if (IsFocused) {
                GuiHelper.SpriteBatch.DrawRectangle(Bounds, Color.White, 2f);

                float alignLeft = Left + Padding;

                float cursorLeft = alignLeft;
                if (Cursor > 0 && Cursor <= _text.Length) {
                    cursorLeft = alignLeft + GuiHelper.MeasureStringTight(_text.Substring(0, Cursor), _fontSize).X;
                }
                if (_blink.Value <= 0.5f) {
                    GuiHelper.SpriteBatch.FillRectangle(new RectangleF(cursorLeft, Top, 2f, Height), Color.White);
                }
            } else {
                GuiHelper.SpriteBatch.DrawRectangle(Bounds, new Color(76, 76, 76), 2f);
            }

            var font = GuiHelper.GetFont(_fontSize);
            GuiHelper.SpriteBatch.DrawString(font, _text, XY + new Vector2(Padding), Color, GuiHelper.FontScale);

            GuiHelper.PopScissor();
        }

        protected void Cache() {
            if (_isDirty) {
                _cachedSize = GuiHelper.MeasureString(_text, _fontSize);
                if (Cursor > _text.Length) {
                    Cursor = _text.Length;
                }
                _isDirty = false;
            }
        }

        protected void MoveCursor(ICondition condition, int direction) {
            if (condition.Pressed()) {
                Cursor += direction;
                _input.StartTime = TweenHelper.TotalMS;
                _input.Duration = _inputInitialDelay;
            }
            if (condition.HeldOnly()) {
                if (TweenHelper.TotalMS > _input.StartTime + _input.Duration) {
                    Cursor += direction;
                    _input.StartTime = TweenHelper.TotalMS;
                    _input.Duration = _inputDelay;
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
        protected Vector2 _cachedSize;

        protected int _fontSize;
        protected bool _isDirty = false;

        protected RectangleF _cursorRect;
        protected int Cursor {
            get => _cursor;
            set {
                if (value >= 0 && value <= _text.Length && _cursor != value) {
                    _cursor = value;
                    _blink.StartTime = TweenHelper.TotalMS;
                }
            }
        }
        protected int _cursor;

        protected int _inputDelay = 50;
        protected int _inputInitialDelay = 400;
        protected FloatTween _input = new(0f, 1f, 50, Easing.Linear);
        protected FloatTween _blink = new(0f, 1f, 1500, Easing.Linear);

        protected bool _pressed = false;

        public static Textbox Put(ref string text, int fontSize = 30, Color? color = null, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            id = GuiHelper.CurrentIMGUI.TryCreateId(id, isAbsoluteId, out IComponent c);

            color ??= new Color(200, 200, 200);

            Textbox a;
            if (c is Textbox d) {
                a = d;
                if (a.IsFocused) {
                    text = a.Text;
                } else {
                    a.Text = text;
                }

                a.Color = color.Value;
                a.FontSize = fontSize;
            } else {
                a = new Textbox(id, text, fontSize, color.Value);
            }

            GuiHelper.CurrentIMGUI.GrabParent(a);

            return a;
        }
    }
}
