using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Apos.Input;
using Apos.Tweens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using Track = Apos.Input.Track;

namespace Apos.Gui {
    /// <summary>
    /// Editable text. One line that scrolls sideways by default, or wrapped lines that scroll up and down when <see cref="Multiline"/> is on.
    /// Supports selection with the mouse, touch and shift, word jumps, and copy, cut and paste through <see cref="GuiHelper.Clipboard"/>.
    /// </summary>
    public class Textbox : Component {
        public Textbox(long id, string text, int fontSize, Color c) : base(id) {
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
                    _cursor = Math.Min(_cursor, _text.Length);
                    _anchor = Math.Min(_anchor, _text.Length);
                    _isDirty = true;
                }
            }
        }
        public int FontSize {
            get => _fontSize;
            set {
                if (value != _fontSize) {
                    _fontSize = value;
                    _isDirty = true;
                }
            }
        }
        public Color Color { get; set; }
        public int Padding { get; set; } = 10;
        /// <summary>Width the textbox asks its parent for, padding included. It doesn't grow with its text.</summary>
        public float BoxWidth { get; set; } = 300f;
        /// <summary>Wraps the text, lets enter start a new line, and scrolls up and down instead of sideways.</summary>
        public bool Multiline {
            get => _multiline;
            set {
                if (value != _multiline) {
                    _multiline = value;
                    _isDirty = true;
                }
            }
        }
        /// <summary>How many lines tall a multiline textbox is.</summary>
        public int VisibleLines { get; set; } = 4;
        /// <summary>Typing and pasting stop at this many chars.</summary>
        public int MaxLength { get; set; } = int.MaxValue;

        /// <summary>Where the caret is, as a char index.</summary>
        public int Cursor => _cursor;
        /// <summary>The other end of the selection. Same as <see cref="Cursor"/> when nothing is selected.</summary>
        public int Anchor => _anchor;
        public int SelectionStart => Math.Min(_cursor, _anchor);
        public int SelectionEnd => Math.Max(_cursor, _anchor);
        public bool HasSelection => _cursor != _anchor;
        public string SelectedText => _text[SelectionStart..SelectionEnd];
        /// <summary>The lines from the last layout. A single line textbox has one.</summary>
        public IReadOnlyList<TextLine> Lines => _lines;
        /// <summary>How far the text is scrolled, in UI units.</summary>
        public Vector2 Scroll => _scroll;
        /// <summary>The area inside the padding, where the text draws.</summary>
        public RectangleF ContentBounds => new(X + Padding, Y + Padding, MathF.Max(Width - Padding * 2, 0f), MathF.Max(Height - Padding * 2, 0f));
        /// <summary>Where the top left of the text draws once scrolled.</summary>
        public Vector2 TextOrigin => ContentBounds.Position - _scroll;
        public float LineHeight => GuiHelper.Font.LineHeight * FontSize;
        /// <summary>True while focused and the blinking cursor is in its visible half.</summary>
        public bool IsCursorVisible => IsFocused && _blink.Value <= 0.5f;

        public override bool IsFocused {
            get => base.IsFocused;
            set {
                base.IsFocused = value;
                _blink.StartTime = TweenHelper.TotalMS;
            }
        }
        public override bool IsFocusable { get; set; } = true;
        public Interaction Interaction { get; } = new() { UsesFocusButton = false };
        /// <summary>Draws this textbox. Null uses <see cref="DefaultLook"/>.</summary>
        public Action<Textbox>? Look { get; set; }
        /// <summary>Draws every textbox that doesn't have its own <see cref="Look"/>.</summary>
        public static Action<Textbox> DefaultLook { get; set; } = DrawDefault;

        /// <summary>Moves the caret. Keeps the selection's other end when extend is true, and clears the selection otherwise.</summary>
        public void MoveCursor(int index, bool extend = false) {
            _cursor = Math.Clamp(index, 0, _text.Length);
            if (!extend) {
                _anchor = _cursor;
            }
            _blink.StartTime = TweenHelper.TotalMS;
        }
        public void Select(int start, int end) {
            _anchor = Math.Clamp(start, 0, _text.Length);
            MoveCursor(end, true);
        }
        /// <summary>Replaces the selection, or inserts at the caret when nothing is selected.</summary>
        public void Insert(string s) {
            int room = MaxLength - (_text.Length - (SelectionEnd - SelectionStart));
            if (room < s.Length) {
                s = s[..Math.Max(room, 0)];
            }
            int start = SelectionStart;
            Text = _text.Remove(start, SelectionEnd - start).Insert(start, s);
            MoveCursor(start + s.Length);
        }

        /// <summary>Where the caret is, in UI coordinates. It's the top of the caret, one line tall.</summary>
        public Vector2 PositionOf(int index) {
            Relayout();
            int line = TextLayout.LineAt(_lines, index);
            float x = TextLayout.CaretX(GuiHelper.Font, _text, _lines[line], index, FontSize);
            return TextOrigin + new Vector2(x, line * LineHeight);
        }
        /// <summary>The char index closest to a position in UI coordinates.</summary>
        public int IndexAt(Vector2 position) {
            Relayout();
            Vector2 local = position - TextOrigin;
            int line = Math.Clamp((int)MathF.Floor(local.Y / LineHeight), 0, _lines.Count - 1);
            return TextLayout.HitTest(GuiHelper.Font, _text, _lines[line], local.X, FontSize);
        }

        public override void UpdateInput(GameTime gameTime) {
            Interaction.UpdateInput(this);

            if (!IsFocused) {
                return;
            }

            Bindings bindings = Root.Bindings;
            bool extend = bindings.ExtendSelection.Held(false);
            bool byWord = bindings.ByWord.Held(false);

            if (Interaction.Pressed && Interaction.Source != InteractionSource.Button) {
                MoveCursor(IndexAt(Interaction.Position), extend || !Interaction.JustPressed);
            }

            if (bindings.MoveLeft.Pressed()) {
                int to = byWord ? PrevWord(_cursor) : HasSelection && !extend ? SelectionStart : _cursor - 1;
                MoveCursor(to, extend);
            }
            if (bindings.MoveRight.Pressed()) {
                int to = byWord ? NextWord(_cursor) : HasSelection && !extend ? SelectionEnd : _cursor + 1;
                MoveCursor(to, extend);
            }
            // A single line textbox leaves up and down alone so that they still move the focus.
            if (Multiline && bindings.MoveUp.Pressed()) {
                MoveCursor(LineStep(-1), extend);
            }
            if (Multiline && bindings.MoveDown.Pressed()) {
                MoveCursor(LineStep(1), extend);
            }
            if (bindings.LineStart.Pressed()) {
                MoveCursor(byWord ? 0 : _lines[CurrentLine].Start, extend);
            }
            if (bindings.LineEnd.Pressed()) {
                MoveCursor(byWord ? _text.Length : TextLayout.LineContentEnd(_text, _lines[CurrentLine]), extend);
            }

            if (bindings.SelectAll.Pressed()) {
                Select(0, _text.Length);
            }
            if (bindings.Copy.Pressed() && HasSelection) {
                GuiHelper.Clipboard.SetText(SelectedText);
            }
            if (bindings.Cut.Pressed() && HasSelection) {
                GuiHelper.Clipboard.SetText(SelectedText);
                Insert("");
            }
            if (bindings.Paste.Pressed()) {
                string? pasted = GuiHelper.Clipboard.GetText();
                if (pasted != null) {
                    pasted = pasted.Replace("\r\n", "\n");
                    Insert(Multiline ? pasted : pasted.Replace("\n", " "));
                }
            }

            foreach (var te in InputHelper.TextEvents) {
                if (te.Key == Keys.Back) {
                    if (!HasSelection) {
                        MoveCursor(byWord ? PrevWord(_cursor) : _cursor - 1, true);
                    }
                    Insert("");
                } else if (te.Key == Keys.Delete) {
                    if (!HasSelection) {
                        MoveCursor(byWord ? NextWord(_cursor) : _cursor + 1, true);
                    }
                    Insert("");
                } else if (te.Key == Keys.Enter && Multiline) {
                    Insert("\n");
                } else if (!char.IsControl(te.Character)) {
                    Insert(te.Character.ToString());
                } else {
                    // Tab, and enter on a single line, stay free for focus and buttons.
                    continue;
                }
                Track.KeyboardCondition.Consume(te.Key);
            }
        }
        public override void Update(GameTime gameTime) {
            if (IsFocused && TweenHelper.TotalMS > _blink.StartTime + _blink.Duration) {
                _blink.StartTime = TweenHelper.TotalMS;
            }
        }

        public override void UpdatePrefSize(GameTime gameTime) {
            Relayout();
            PrefWidth = BoxWidth;
            PrefHeight = (Multiline ? VisibleLines : 1) * LineHeight + Padding * 2f;
        }

        public override void Draw(GameTime gameTime) {
            ScrollToCursor();
            (Look ?? DefaultLook)(this);
        }

        /// <summary>A border that lights up with focus, the selection, the text, and a blinking cursor.</summary>
        public static void DrawDefault(Textbox t) {
            GuiHelper.ShapeBatch.BorderRectangle(t.XY, t.Size, t.IsFocused ? Theme.Focus : Theme.Border, Theme.BorderThickness, Theme.Rounding);

            GuiHelper.PushClip(t.ContentBounds.Intersect(t.Clip));
            if (t.HasSelection) {
                DrawSelection(t, new Color(Theme.Highlight, 0.3f));
            }
            DrawText(t, t.Color);
            if (t.IsCursorVisible) {
                GuiHelper.ShapeBatch.FillRectangle(t.PositionOf(t.Cursor), new Vector2(2f, t.LineHeight), Theme.Focus);
            }
            GuiHelper.PopClip();
        }
        /// <summary>Draws each line of text where it sits after scrolling.</summary>
        public static void DrawText(Textbox t, Color color) {
            for (int i = 0; i < t._lines.Count; i++) {
                TextLine line = t._lines[i];
                int end = TextLayout.LineContentEnd(t._text, line);
                Vector2 pos = t.TextOrigin + new Vector2(0f, i * t.LineHeight);
                GuiHelper.ShapeBatch.DrawString(GuiHelper.Font, t._text.AsSpan(line.Start, end - line.Start), pos, t.FontSize, color);
            }
        }
        /// <summary>Fills the selected part of each line.</summary>
        public static void DrawSelection(Textbox t, Color color) {
            int start = t.SelectionStart;
            int end = t.SelectionEnd;
            for (int i = 0; i < t._lines.Count; i++) {
                TextLine line = t._lines[i];
                if (line.End < start || line.Start > end) continue;

                float left = TextLayout.CaretX(GuiHelper.Font, t._text, line, start, t.FontSize);
                float right = TextLayout.CaretX(GuiHelper.Font, t._text, line, end, t.FontSize);
                if (right <= left) continue;

                Vector2 pos = t.TextOrigin + new Vector2(left, i * t.LineHeight);
                GuiHelper.ShapeBatch.FillRectangle(pos, new Vector2(right - left, t.LineHeight), color);
            }
        }

        protected int CurrentLine => TextLayout.LineAt(_lines, _cursor);

        protected void Relayout() {
            float wrap = _multiline ? MathF.Max(BoxWidth - Padding * 2f, 0f) : float.PositiveInfinity;
            if (_isDirty || wrap != _wrapWidth) {
                TextLayout.Wrap(GuiHelper.Font, _text, _fontSize, wrap, _lines);
                _wrapWidth = wrap;
                _isDirty = false;
            }
        }
        /// <summary>Scrolls just enough to keep the caret inside the content area.</summary>
        protected void ScrollToCursor() {
            Relayout();
            RectangleF content = ContentBounds;
            int line = CurrentLine;
            float x = TextLayout.CaretX(GuiHelper.Font, _text, _lines[line], _cursor, FontSize);
            float y = line * LineHeight;

            if (x - _scroll.X > content.Width - 2f) _scroll.X = x - content.Width + 2f;
            if (x - _scroll.X < 0f) _scroll.X = x;
            if (y + LineHeight - _scroll.Y > content.Height) _scroll.Y = y + LineHeight - content.Height;
            if (y - _scroll.Y < 0f) _scroll.Y = y;

            _scroll.X = MathF.Max(_scroll.X, 0f);
            _scroll.Y = MathF.Max(_scroll.Y, 0f);
        }
        /// <summary>The index one line up or down, at about the same x.</summary>
        protected int LineStep(int direction) {
            int line = CurrentLine + direction;
            if (line < 0) return 0;
            if (line >= _lines.Count) return _text.Length;

            float x = TextLayout.CaretX(GuiHelper.Font, _text, _lines[CurrentLine], _cursor, FontSize);
            return TextLayout.HitTest(GuiHelper.Font, _text, _lines[line], x, FontSize);
        }
        protected int PrevWord(int i) {
            while (i > 0 && char.IsWhiteSpace(_text[i - 1])) i--;
            while (i > 0 && !char.IsWhiteSpace(_text[i - 1])) i--;
            return i;
        }
        protected int NextWord(int i) {
            while (i < _text.Length && !char.IsWhiteSpace(_text[i])) i++;
            while (i < _text.Length && char.IsWhiteSpace(_text[i])) i++;
            return i;
        }

        protected string _text = "";
        protected int _fontSize;
        protected bool _multiline = false;
        protected bool _isDirty = true;
        protected float _wrapWidth = -1f;
        protected readonly List<TextLine> _lines = [];
        protected Vector2 _scroll = Vector2.Zero;

        protected int _cursor;
        protected int _anchor;

        protected FloatTween _blink = new(0f, 1f, 1500, Easing.Linear);

        public static Textbox Put(ref string text, int fontSize = 30, Color? color = null, [CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            Color c = color ?? Theme.Text;

            string initial = text;
            Textbox a = IMGUI.Put(id, isAbsoluteId, file, id => new Textbox(id, initial, fontSize, c));
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
