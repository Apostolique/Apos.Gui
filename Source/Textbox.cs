using System;
using Apos.Input;
using Track = Apos.Input.Track;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System.Runtime.CompilerServices;
using Apos.Tweens;

namespace Apos.Gui {
    // TODO: Textbox shouldn't change its size. It should take a "preferred width" and just use that.
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
        /// <summary>Draws this textbox. Null uses <see cref="DefaultLook"/>.</summary>
        public Action<Textbox>? Look { get; set; }
        /// <summary>Draws every textbox that doesn't have its own <see cref="Look"/>.</summary>
        public static Action<Textbox> DefaultLook { get; set; } = DrawDefault;
        public Interaction Interaction { get; } = new() { UsesFocusButton = false };

        public override void UpdateInput(GameTime gameTime) {
            Interaction.UpdateInput(this);

            if (IsFocused) {
                if (Interaction.Pressed) {
                    Cursor = MouseToCursor(Interaction.Position.X, _text);
                }

                if (_left.Pressed()) Cursor--;
                if (_right.Pressed()) Cursor++;

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

        /// <summary>X of the cursor in UI coordinates.</summary>
        public float CursorX => Left + Padding + (Cursor > 0 ? GuiHelper.MeasureString(_text[..Math.Min(Cursor, _text.Length)], _fontSize).X : 0f);
        /// <summary>True while focused and the blinking cursor is in its visible half.</summary>
        public bool IsCursorVisible => IsFocused && _blink.Value <= 0.5f;

        public override void Draw(GameTime gameTime) {
            // Only clip when the text doesn't fit. The parent's clip already covers the rest.
            bool overflows = _cachedSize.X + Padding * 2 > Width || _cachedSize.Y + Padding * 2 > Height;
            if (overflows) GuiHelper.PushClip(Clip);
            (Look ?? DefaultLook)(this);
            if (overflows) GuiHelper.PopClip();
        }

        /// <summary>A border that lights up with focus, the text, and a blinking cursor.</summary>
        public static void DrawDefault(Textbox t) {
            GuiHelper.ShapeBatch.BorderRectangle(t.XY, t.Size, t.IsFocused ? Theme.Focus : Theme.Border, Theme.BorderThickness, Theme.Rounding);
            if (t.IsCursorVisible) {
                GuiHelper.ShapeBatch.FillRectangle(new Vector2(t.CursorX, t.Top), new Vector2(2f, t.Height), Theme.Focus);
            }
            GuiHelper.ShapeBatch.DrawString(GuiHelper.Font, t.Text, t.XY + new Vector2(t.Padding), t.FontSize, t.Color);
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

        protected int MouseToCursor(float x, string text) {
            float left = Left + Padding;
            float currentOffset = left;
            int currentPosition = 0;
            for (int i = 0; i < text.Length; i++) {
                if (x < currentOffset) {
                    break;
                }
                currentPosition++;
                currentOffset = left + GuiHelper.MeasureString(text[..currentPosition], _fontSize).X;
            }
            return currentPosition;
        }

        protected string _text = null!;
        protected Vector2 _cachedSize;

        protected int _fontSize;
        protected bool _isDirty = false;

        public int Cursor {
            get => _cursor;
            protected set {
                if (value >= 0 && value <= _text.Length && _cursor != value) {
                    _cursor = value;
                    _blink.StartTime = TweenHelper.TotalMS;
                }
            }
        }
        protected int _cursor;

        protected RepeatCondition _left = new(Default.MoveLeft, 400, 50);
        protected RepeatCondition _right = new(Default.MoveRight, 400, 50);
        protected FloatTween _blink = new(0f, 1f, 1500, Easing.Linear);

        public static Textbox Put(ref string text, int fontSize = 30, Color? color = null, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            Color c = color ?? Theme.Text;

            string initial = text;
            Textbox a = IMGUI.Put(id, isAbsoluteId, id => new Textbox(id, initial, fontSize, c));
            if (a.IsFocused) {
                text = a.Text;
            } else {
                a.Text = text;
            }
            a.Color = c;
            a.FontSize = fontSize;

            return a;
        }
    }
}
