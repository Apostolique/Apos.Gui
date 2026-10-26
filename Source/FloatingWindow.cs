using System;
using System.Runtime.CompilerServices;
using Apos.Shapes;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Apos.Gui {
    /// <summary>
    /// A window you drag by its title bar. The components put after it go in its <see cref="Content"/>, a vertical stack.
    /// It opens where <see cref="StartAlign"/> and <see cref="StartOffset"/> put it, stays inside the UI, and comes to the front when it's pressed or gets focus.
    /// </summary>
    public class FloatingWindow(long id) : SingleParent(id) {
        public override bool IsFocusable { get; set; } = true;
        public override bool IsFocusScope { get; set; } = true;
        public override bool IsFloatable { get; set; } = true;

        /// <summary>The stack that holds the window's components.</summary>
        public Vertical? Content => Child as Vertical;
        /// <summary>Drawn in the title bar.</summary>
        public string Title { get; set; } = "";
        public int TitleFontSize { get; set; } = 24;
        /// <summary>Space around the title, in UI units.</summary>
        public float TitlePadding { get; set; } = 8f;
        /// <summary>
        /// Where the window opens in the UI, from 0 to 1 on each axis. 0 is the left or the top, 1 is the right or the bottom.
        /// The default, 0.5, centers it. Only read when the window opens.
        /// </summary>
        public Vector2 StartAlign { get; set; } = new(0.5f, 0.5f);
        /// <summary>Moves the window from where <see cref="StartAlign"/> puts it when it opens, in UI units.</summary>
        public Vector2 StartOffset { get; set; } = Vector2.Zero;
        /// <summary>Shows a close button in the title bar, and lets <see cref="Bindings.Back"/> close the window while the focus is in it.</summary>
        public bool HasCloseButton { get; set; }
        /// <summary>True on the frame something asked the window to close.</summary>
        public bool Closed => _closedFrame != 0 && _closedFrame == Root.CurrentFrame;

        /// <summary>Dragging the title bar.</summary>
        public Interaction TitleInteraction { get; } = new() { UsesFocusButton = false };
        /// <summary>The close button.</summary>
        public Interaction CloseInteraction { get; } = new() { UsesFocusButton = false, GrabsFocus = false };
        /// <summary>Pressing anywhere else on the window. It brings the window to the front and keeps the press from reaching what's under it.</summary>
        public Interaction BodyInteraction { get; } = new() { UsesFocusButton = false };

        public float TitleHeight => GuiHelper.Font.LineHeight * TitleFontSize + TitlePadding * 2f;
        public RectangleF TitleBar => new(X, Y, Width, TitleHeight);
        public RectangleF CloseButton => HasCloseButton ? new(Right - TitleHeight, Y, TitleHeight, TitleHeight) : RectangleF.Empty;
        /// <summary>Where the content goes: under the title bar and inside the border.</summary>
        public RectangleF ContentArea {
            get {
                float b = Theme.BorderThickness;
                return new(X + b, Y + TitleHeight, MathF.Max(Width - b * 2f, 0f), MathF.Max(Height - TitleHeight - b, 0f));
            }
        }

        /// <summary>Draws this window under its content. Null uses <see cref="DefaultLook"/>.</summary>
        public Action<FloatingWindow>? Look { get; set; }
        /// <summary>Draws every window that doesn't have its own <see cref="Look"/>.</summary>
        public static Action<FloatingWindow> DefaultLook { get; set; } = DrawDefault;

        public override void UpdateInput(GameTime gameTime) {
            base.UpdateInput(gameTime);

            if (HasCloseButton) {
                CloseInteraction.UpdateInput(this, CloseButton);
                if (CloseInteraction.Clicked || HasFocusInside() && Root.Bindings.Back.Pressed()) {
                    _closedFrame = Root.CurrentFrame;
                }
            }

            RectangleF title = TitleBar;
            if (HasCloseButton) {
                title.Width -= TitleHeight;
            }
            TitleInteraction.UpdateInput(this, title);
            if (TitleInteraction.JustPressed) {
                _dragStart = XY;
            }
            if (TitleInteraction.Pressed) {
                XY = _dragStart + TitleInteraction.Position - TitleInteraction.PressPosition;
            }

            BodyInteraction.UpdateInput(this);
        }

        public override void UpdatePrefSize(GameTime gameTime) {
            base.UpdatePrefSize(gameTime);

            float titleWidth = GuiHelper.MeasureString(Title, TitleFontSize).X + TitlePadding * 2f + (HasCloseButton ? TitleHeight : 0f);
            float b = Theme.BorderThickness;
            PrefWidth = MathF.Max(Child != null ? PrefWidth + b * 2f : 0f, titleWidth);
            PrefHeight = (Child != null ? PrefHeight : 0f) + TitleHeight + b;
        }
        public override void UpdateLayout(GameTime gameTime) {
            // A window manages its own position. It isn't bigger than the UI, and scrolls its content when it doesn't fit.
            Width = MathF.Min(PrefWidth, Root.Width);
            Height = MathF.Min(PrefHeight, Root.Height);
            if (!_placed) {
                XY = new Vector2(Root.Width - Width, Root.Height - Height) * StartAlign + StartOffset;
                _placed = true;
            }
            X = MathHelper.Clamp(X, 0f, MathF.Max(Root.Width - Width, 0f));
            Y = MathHelper.Clamp(Y, 0f, MathF.Max(Root.Height - Height, 0f));
            if (Parent != null) {
                Clip = Bounds.Intersect(Parent.Clip);
            }

            if (Child != null) {
                Child.Bounds = ContentArea;
                LayoutChild(gameTime);
            }
        }

        public override void Draw(GameTime gameTime) {
            (Look ?? DefaultLook)(this);
            // The window's rounded inside cuts the bottom corners while the title bar's edge stays straight.
            float b = Theme.BorderThickness;
            GuiHelper.PushClip(new RectangleF(X + b, Y + b, MathF.Max(Width - b * 2f, 0f), MathF.Max(Height - b * 2f, 0f)), MathF.Max(Theme.Rounding - b, 0f));
            GuiHelper.PushClip(ContentArea);
            base.Draw(gameTime);
            GuiHelper.PopClip();
            GuiHelper.PopClip();
        }

        /// <summary>A panel with a title bar, and a close button when it has one.</summary>
        public static void DrawDefault(FloatingWindow w) {
            bool focused = w.HasFocusInside();
            RectangleF bar = w.TitleBar;
            Vector2 size = new(w.Width, w.Height);

            GuiHelper.ShapeBatch.FillRectangle(w.XY, size, Theme.Panel, Theme.Rounding);
            GuiHelper.ShapeBatch.FillRectangle(bar.Position, new Vector2(bar.Width, bar.Height), Theme.Surface, new CornerRadii(Theme.Rounding, Theme.Rounding, 0f, 0f));
            float drag = 0.08f * w.TitleInteraction.Hover + 0.1f * w.TitleInteraction.Press;
            if (drag > 0f) {
                GuiHelper.ShapeBatch.FillRectangle(bar.Position, new Vector2(bar.Width, bar.Height), new Color(Theme.Highlight, drag), new CornerRadii(Theme.Rounding, Theme.Rounding, 0f, 0f));
            }
            GuiHelper.ShapeBatch.FillRectangle(new Vector2(bar.Left, bar.Bottom - Theme.BorderThickness), new Vector2(bar.Width, Theme.BorderThickness), Theme.Border);
            GuiHelper.ShapeBatch.DrawString(GuiHelper.Font, w.Title, bar.Position + new Vector2(w.TitlePadding), w.TitleFontSize, Theme.Text);

            if (w.HasCloseButton) {
                RectangleF close = w.CloseButton;
                float light = 0.15f * w.CloseInteraction.Hover + 0.2f * w.CloseInteraction.Press;
                if (light > 0f) {
                    GuiHelper.ShapeBatch.FillRectangle(close.Position, new Vector2(close.Width, close.Height), new Color(Theme.Highlight, light), new CornerRadii(0f, Theme.Rounding, 0f, 0f));
                }
                Vector2 c = close.Center;
                float arm = close.Width * 0.18f;
                GuiHelper.ShapeBatch.FillLine(c - new Vector2(arm), c + new Vector2(arm), 1f, Theme.Text);
                GuiHelper.ShapeBatch.FillLine(c + new Vector2(-arm, arm), c + new Vector2(arm, -arm), 1f, Theme.Text);
            }

            GuiHelper.ShapeBatch.BorderRectangle(w.XY, size, focused ? Theme.Focus : Theme.Border, Theme.BorderThickness, Theme.Rounding);
        }

        /// <summary>The window, or a component in it, has focus.</summary>
        public bool HasFocusInside() {
            for (IComponent? f = Root.Focused; f != null; f = f.Parent) {
                if (f == this) return true;
            }
            return false;
        }

        protected Vector2 _dragStart = Vector2.Zero;
        protected bool _placed;
        protected uint _closedFrame;

        /// <summary>A window without a close button.</summary>
        /// <param name="title">Drawn in the title bar.</param>
        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="file">The caller's file. The same line in two files gives two different ids.</param>
        public static FloatingWindow Push(string title = "", [CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            FloatingWindow w = IMGUI.Push(id, isAbsoluteId, file, id => new FloatingWindow(id), 1);
            w.Title = title;
            w.HasCloseButton = false;
            PushContent();
            return w;
        }
        /// <summary>A window with a close button. It's open while isOpen is true, and pressing the close button sets it to false.</summary>
        /// <param name="isOpen">Your open state.</param>
        /// <param name="title">Drawn in the title bar.</param>
        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="file">The caller's file. The same line in two files gives two different ids.</param>
        public static FloatingWindow Push(ref bool isOpen, string title = "", [CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            FloatingWindow w = IMGUI.Push(id, isAbsoluteId, file, id => new FloatingWindow(id), 1);
            w.Title = title;
            w.HasCloseButton = true;
            if (w.Closed) {
                isOpen = false;
            }
            PushContent();
            return w;
        }
        private static void PushContent() {
            IMGUI.Push(0, false, "", id => new Vertical(id) { Padding = 8f, Spacing = 8f });
        }
        /// <summary>Ends the window's content.</summary>
        public static void Pop() {
            GuiHelper.CurrentIMGUI.Pop();
        }
    }
}
