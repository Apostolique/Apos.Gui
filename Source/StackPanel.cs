using System;
using Apos.Input;
using Apos.Tweens;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Track = Apos.Input.Track;

namespace Apos.Gui {
    public enum Orientation {
        Horizontal,
        Vertical
    }

    /// <summary>How children line up across a stack. Across a vertical stack is left to right, across a horizontal one is top to bottom.</summary>
    public enum CrossAlign {
        Start,
        Center,
        End,
        /// <summary>Children take the stack's whole width, or its whole height for a horizontal stack.</summary>
        Stretch
    }

    /// <summary>
    /// Lines up its children one after the other, with spacing between them and padding around them.
    /// When they don't fit it scrolls with the mouse wheel, a scrollbar, or a touch drag that starts on the stack itself.
    /// </summary>
    public class StackPanel(long id, Orientation orientation) : ListParent(id) {
        public Orientation Orientation { get; set; } = orientation;
        /// <summary>Space between two children, in UI units.</summary>
        public float Spacing { get; set; } = 0f;
        /// <summary>Space around the children, in UI units.</summary>
        public float Padding { get; set; } = 0f;
        public CrossAlign Align { get; set; } = CrossAlign.Start;

        public float OffsetX {
            get => _offsetX;
            set => SetOffset(_offsetXTween, ClampOffsetX(value));
        }
        public float OffsetY {
            get => _offsetY;
            set => SetOffset(_offsetYTween, ClampOffsetY(value));
        }
        public Vector2 OffsetXY {
            get => new(OffsetX, OffsetY);
            set {
                OffsetX = value.X;
                OffsetY = value.Y;
            }
        }
        /// <summary>How big the children are together, padding and spacing included. Can be bigger than the stack.</summary>
        public Vector2 ContentSize { get; protected set; }
        public float FullWidth => MathF.Max(ContentSize.X, Width);
        public float FullHeight => MathF.Max(ContentSize.Y, Height);
        public Vector2 FullSize => new(FullWidth, FullHeight);
        /// <summary>The children don't fit along the stack's direction.</summary>
        public bool CanScroll => MainOf(ContentSize) > MainOf(Size) + 0.5f;

        /// <summary>How far one wheel notch scrolls, in UI units.</summary>
        public float ScrollIncrement { get; set; } = 50f;
        public float ScrollSpeed { get; set; } = 0.25f;
        public long ScrollMaxDuration { get; set; } = 1000;

        public bool ShowScrollbar { get; set; } = true;
        public float ScrollbarThickness { get; set; } = 6f;
        /// <summary>Space between the children and the scrollbar, in UI units.</summary>
        public float ScrollbarGap { get; set; } = 4f;
        /// <summary>
        /// Room kept beside the children for the scrollbar while the stack scrolls, so it doesn't cover them.
        /// It's part of the preferred size too, so a parent with room to spare widens the stack instead.
        /// </summary>
        public float ScrollbarRoom => _showsScrollbar ? ScrollbarThickness + ScrollbarGap : 0f;
        /// <summary>Dragging the scrollbar thumb.</summary>
        public Interaction ScrollbarInteraction { get; } = new() { GrabsFocus = false, UsesFocusButton = false };
        /// <summary>A touch drag that starts on the stack and not on a child.</summary>
        public Interaction DragInteraction { get; } = new() { GrabsFocus = false, UsesFocusButton = false, UsesMouse = false };

        /// <summary>Draws this stack's scrollbar. Null uses <see cref="DefaultScrollbarLook"/>.</summary>
        public Action<StackPanel>? ScrollbarLook { get; set; }
        /// <summary>Draws every scrollbar that doesn't have its own <see cref="ScrollbarLook"/>.</summary>
        public static Action<StackPanel> DefaultScrollbarLook { get; set; } = DrawDefaultScrollbar;

        /// <summary>Where the scrollbar thumb is, in UI coordinates. Empty when nothing scrolls.</summary>
        public RectangleF ScrollbarThumb {
            get {
                if (!CanScroll) return RectangleF.Empty;

                float track = MainOf(Size);
                float length = MathF.Max(track * track / MainOf(ContentSize), 20f);
                float along = -MainOffset / (MainOf(ContentSize) - track) * (track - length);
                return IsVertical
                    ? new RectangleF(Right - ScrollbarThickness, Top + along, ScrollbarThickness, length)
                    : new RectangleF(Left + along, Bottom - ScrollbarThickness, length, ScrollbarThickness);
            }
        }

        public override void UpdateInput(GameTime gameTime) {
            base.UpdateInput(gameTime);

            // Only a stack that can scroll takes the wheel, so an outer stack still gets it.
            if (CanScroll && Root.Bindings.UsesPointer && Clip.Contains(Root.Mouse) && Track.MouseCondition.Scrolled()) {
                // A wheel notch is 120. Touchpads send smaller steps, which scroll smoothly.
                SetOffset(MainTween, ClampMain(MainTween.B + MouseCondition.ScrollDelta / 120f * ScrollIncrement));
            }

            if (ShowScrollbar && CanScroll || ScrollbarInteraction.Pressed) {
                RectangleF thumb = ScrollbarThumb;
                ScrollbarInteraction.UpdateInput(this, thumb);
                if (ScrollbarInteraction.JustPressed) {
                    _scrollDragStart = MainOffset;
                }
                if (ScrollbarInteraction.Pressed && CanScroll) {
                    float track = MainOf(Size);
                    float moved = MainOf(ScrollbarInteraction.Position - ScrollbarInteraction.PressPosition);
                    SnapOffset(_scrollDragStart - moved * (MainOf(ContentSize) - track) / MathF.Max(track - MainOf(thumb.Size), 1f));
                }
            }

            if (CanScroll || DragInteraction.Pressed) {
                DragInteraction.UpdateInput(this);
                if (DragInteraction.JustPressed) {
                    _scrollDragStart = MainOffset;
                }
                if (DragInteraction.Pressed) {
                    SnapOffset(_scrollDragStart + MainOf(DragInteraction.Position - DragInteraction.PressPosition));
                }
            }
        }

        public override void UpdatePrefSize(GameTime gameTime) {
            float main = 0f;
            float cross = 0f;
            int count = 0;

            foreach (var c in _children) {
                c.UpdatePrefSize(gameTime);
                main += MainOf(c.PrefSize);
                cross = MathF.Max(CrossOf(c.PrefSize), cross);
                count++;
            }

            main += Spacing * Math.Max(count - 1, 0) + Padding * 2f;
            cross += Padding * 2f;

            ContentSize = FromMainCross(main, cross);
            PrefSize = FromMainCross(main, cross + ScrollbarRoom);
        }
        public override void UpdateLayout(GameTime gameTime) {
            if (MainTween.B != ClampMain(MainTween.B)) {
                SetOffset(MainTween, ClampMain(MainTween.B));
            }

            _offsetX = _offsetXTween.Value;
            _offsetY = _offsetYTween.Value;

            // Whether it scrolls is only known now, and the room for the scrollbar changes the preferred size.
            bool showsScrollbar = ShowScrollbar && CanScroll;
            if (showsScrollbar != _showsScrollbar) {
                _showsScrollbar = showsScrollbar;
                Root?.InvalidateLayout();
            }

            float innerCross = MathF.Max(CrossOf(Size) - Padding * 2f - ScrollbarRoom, 0f);
            float current = Padding;
            foreach (var c in _children) {
                float main = MainOf(c.PrefSize);
                float cross = Align == CrossAlign.Stretch ? innerCross : MathF.Min(CrossOf(c.PrefSize), innerCross);
                float crossAt = Padding + Align switch {
                    CrossAlign.Center => (innerCross - cross) / 2f,
                    CrossAlign.End => innerCross - cross,
                    _ => 0f,
                };

                c.XY = XY + OffsetXY + FromMainCross(current, crossAt);
                c.Size = FromMainCross(main, cross);
                LayoutChild(c, gameTime);

                current += main + Spacing;
            }
        }

        public override void Draw(GameTime gameTime) {
            base.Draw(gameTime);

            if (ShowScrollbar && CanScroll) {
                (ScrollbarLook ?? DefaultScrollbarLook)(this);
            }
        }

        /// <summary>A rounded thumb that brightens while hovered or dragged.</summary>
        public static void DrawDefaultScrollbar(StackPanel s) {
            RectangleF thumb = s.ScrollbarThumb;
            float light = MathF.Max(s.ScrollbarInteraction.Hover, s.ScrollbarInteraction.Press);
            GuiHelper.ShapeBatch.FillRectangle(thumb.Position, thumb.Size, new Color(Theme.Highlight, 0.25f + 0.35f * light), s.ScrollbarThickness / 2f);
        }

        /// <summary>Scrolls just enough to show a child that got focus.</summary>
        public override void SendToTop(IComponent c) {
            float start = MainOf(c.XY) - MainOf(XY);
            float end = start + MainOf(c.Size);
            float pending = MainTween.B - MainOffset;
            if (start < 0f) {
                SetOffset(MainTween, ClampMain(MainTween.B - start - pending));
            } else if (end > MainOf(Size)) {
                SetOffset(MainTween, ClampMain(MainTween.B - (end - MainOf(Size)) - pending));
            }

            base.SendToTop(c);
        }

        protected bool IsVertical => Orientation == Orientation.Vertical;
        protected float MainOf(Vector2 v) => IsVertical ? v.Y : v.X;
        protected float CrossOf(Vector2 v) => IsVertical ? v.X : v.Y;
        protected Vector2 FromMainCross(float main, float cross) => IsVertical ? new Vector2(cross, main) : new Vector2(main, cross);
        protected FloatTween MainTween => IsVertical ? _offsetYTween : _offsetXTween;
        protected float MainOffset => IsVertical ? _offsetY : _offsetX;
        protected float ClampMain(float offset) => IsVertical ? ClampOffsetY(offset) : ClampOffsetX(offset);

        protected virtual float ClampOffsetX(float x) {
            return MathF.Min(MathF.Max(x, Width - ContentSize.X), 0f);
        }
        protected virtual float ClampOffsetY(float y) {
            return MathF.Min(MathF.Max(y, Height - ContentSize.Y), 0f);
        }

        /// <summary>Eases the offset to b.</summary>
        protected void SetOffset(FloatTween ft, float b) {
            var a = ft.Value;
            ft.StartTime = TweenHelper.TotalMS;
            ft.A = a;
            ft.B = b;
            ft.Duration = GetDuration(a, b, ScrollSpeed, ScrollMaxDuration);
        }
        /// <summary>Jumps the offset along the stack's direction, for drags that should follow the pointer exactly.</summary>
        protected void SnapOffset(float offset) {
            FloatTween ft = MainTween;
            ft.A = ClampMain(offset);
            ft.B = ft.A;
            ft.StartTime = TweenHelper.TotalMS;
            ft.Duration = 0;
        }

        protected static long GetDuration(float a, float b, float speed, long maxDuration) {
            return (long)Math.Min(Math.Abs((b - a) / speed), maxDuration);
        }

        protected FloatTween _offsetXTween = new(0f, 0f, 0, Easing.ExpoOut);
        protected FloatTween _offsetYTween = new(0f, 0f, 0, Easing.ExpoOut);

        protected float _offsetX = 0;
        protected float _offsetY = 0;
        protected float _scrollDragStart = 0;
        protected bool _showsScrollbar = false;
    }
}
