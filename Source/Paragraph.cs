using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    /// <summary>
    /// Text that wraps to the width its parent gives it.
    /// </summary>
    public class Paragraph(long id, string text, int fontSize, Color color) : Component(id) {
        /// <summary>The text to wrap and draw.</summary>
        public string Text { get; set; } = text;
        /// <summary>Size of the text.</summary>
        public int FontSize { get; set; } = fontSize;
        /// <summary>Color of the text.</summary>
        public Color Color { get; set; } = color;
        /// <summary>How each line lines up in the paragraph's width.</summary>
        public TextAlign Align { get; set; } = TextAlign.Left;
        /// <summary>Space around the text on each side, in UI units.</summary>
        public int Padding { get; set; } = 10;
        /// <summary>Wraps at this width, padding included, or earlier when the parent gives it less. Null follows the parent.</summary>
        public float? MaxWidth { get; set; }

        /// <inheritdoc/>
        public override float Width {
            get => base.Width;
            set {
                // The height comes from the width, so a new width needs another layout pass.
                if (value != base.Width) {
                    base.Width = value;
                    Root?.InvalidateLayout();
                }
            }
        }
        /// <summary>The lines from the last layout.</summary>
        public IReadOnlyList<TextLine> Lines => _lines;

        /// <summary>Draws this paragraph. Null uses <see cref="DefaultLook"/>.</summary>
        public Action<Paragraph>? Look { get; set; }
        /// <summary>Draws every paragraph that doesn't have its own <see cref="Look"/>.</summary>
        public static Action<Paragraph> DefaultLook { get; set; } = DrawDefault;

        /// <inheritdoc/>
        public override void UpdatePrefSize(GameTime gameTime) {
            float given = MathF.Min(MaxWidth ?? float.PositiveInfinity, _hasWidth ? Width : float.PositiveInfinity);
            _hasWidth = true;

            float wrapWidth = MathF.Max(given - Padding * 2, 0f);
            if (Text != _cachedText || FontSize != _cachedSize || wrapWidth != _cachedWrap) {
                TextLayout.Wrap(GuiHelper.Font, Text, FontSize, wrapWidth, _lines);
                _natural = GuiHelper.Font.MeasureString(Text, FontSize).X;
                _cachedText = Text;
                _cachedSize = FontSize;
                _cachedWrap = wrapWidth;
            }

            // The unwrapped width lets the parent widen it again when there's more room.
            PrefWidth = MathF.Min(_natural + Padding * 2, MaxWidth ?? float.PositiveInfinity);
            PrefHeight = _lines.Count * GuiHelper.Font.LineHeight * FontSize + Padding * 2;
        }

        /// <inheritdoc/>
        public override void Draw(GameTime gameTime) {
            bool overflows = PrefHeight > Height;
            if (overflows) GuiHelper.PushClip(Clip);
            (Look ?? DefaultLook)(this);
            if (overflows) GuiHelper.PopClip();
        }

        /// <summary>Each line in the paragraph's color, lined up with <see cref="Align"/>.</summary>
        public static void DrawDefault(Paragraph p) {
            float lineHeight = GuiHelper.Font.LineHeight * p.FontSize;
            float width = p.Width - p.Padding * 2;
            Vector2 start = p.XY + new Vector2(p.Padding);

            for (int i = 0; i < p._lines.Count; i++) {
                TextLine line = p._lines[i];
                int end = TextLayout.LineContentEnd(p.Text, line);
                Vector2 pos = start + new Vector2(TextLayout.AlignOffset(line, width, p.Align), i * lineHeight);
                GuiHelper.ShapeBatch.DrawString(GuiHelper.Font, p.Text.AsSpan(line.Start, end - line.Start), pos, p.FontSize, p.Color);
            }
        }

        /// <summary>Backs <see cref="Lines"/>.</summary>
        protected readonly List<TextLine> _lines = [];
        /// <summary>Whether the parent gave it a width yet. Until then the text is measured without wrapping.</summary>
        protected bool _hasWidth = false;
        /// <summary>The text from the last wrap.</summary>
        protected string? _cachedText;
        /// <summary>The font size from the last wrap.</summary>
        protected int _cachedSize;
        /// <summary>The width from the last wrap. -1 before the first one.</summary>
        protected float _cachedWrap = -1f;
        /// <summary>How wide the text is without wrapping.</summary>
        protected float _natural;

        /// <summary>Text that wraps to the width its parent gives it.</summary>
        /// <param name="text">The text to wrap and draw.</param>
        /// <param name="fontSize">Size of the text.</param>
        /// <param name="color">Color of the text. Null uses <see cref="Theme.Text"/>.</param>
        /// <param name="align">How each line lines up in the paragraph's width.</param>
        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="file">The caller's file. The same line in two files gives two different ids.</param>
        public static Paragraph Put(string text, int fontSize = 30, Color? color = null, TextAlign align = TextAlign.Left, [CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            Color c = color ?? Theme.Text;

            Paragraph a = IMGUI.Put(id, isAbsoluteId, file, id => new Paragraph(id, text, fontSize, c));
            a.Text = text;
            a.FontSize = fontSize;
            a.Color = c;
            a.Align = align;

            return a;
        }
    }
}
