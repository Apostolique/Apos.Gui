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
    public class StackPanel : ListParent {
        public StackPanel(long id, Orientation orientation) : base(id) {
            Orientation = orientation;
            // Smooth keeps the speed of a scroll that's still going, so quick wheel notches flow into each other.
            _easedOffsetX = new(0f, (from, to) => GetDuration(from, to, ScrollSpeed, ScrollMaxDuration), Easing.ExpoOut, GuiHelper.Clock) { Smooth = true };
            _easedOffsetY = new(0f, (from, to) => GetDuration(from, to, ScrollSpeed, ScrollMaxDuration), Easing.ExpoOut, GuiHelper.Clock) { Smooth = true };
        }

        public Orientation Orientation { get; set; }
        /// <summary>Space between two children, in UI units.</summary>
        public float Spacing { get; set; } = 0f;
        /// <summary>Space around the children, in UI units.</summary>
        public float Padding { get; set; } = 0f;
        public CrossAlign Align { get; set; } = CrossAlign.Start;

        public float OffsetX {
            get => _offsetX;
            set => _easedOffsetX.Target = ClampOffsetX(value);
        }
        public float OffsetY {
            get => _offsetY;
            set => _easedOffsetY.Target = ClampOffsetY(value);
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
        public double ScrollMaxDuration { get; set; } = 1000;

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

                // The thumb moves inside the padding, in the room kept for it beside the children.
                float track = MathF.Max(MainOf(Size) - Padding * 2f, 0f);
                float length = MathF.Min(MathF.Max(track * MainOf(Size) / MainOf(ContentSize), 20f), track);
                float along = Padding + -MainOffset / (MainOf(ContentSize) - MainOf(Size)) * (track - length);
                return IsVertical
                    ? new RectangleF(Right - Padding - ScrollbarThickness, Top + along, ScrollbarThickness, length)
                    : new RectangleF(Left + along, Bottom - Padding - ScrollbarThickness, length, ScrollbarThickness);
            }
        }

        public override void UpdateInput(GameTime gameTime) {
            base.UpdateInput(gameTime);

            // Only a stack that can scroll takes the wheel, so an outer stack still gets it.
            if (CanScroll && Root.Bindings.UsesPointer && Clip.Contains(Root.Mouse) && Track.MouseCondition.Scrolled()) {
                // A wheel notch is 120. Touchpads send smaller steps, which scroll smoothly.
                MainEasedOffset.Target = ClampMain(MainEasedOffset.Target + MouseCondition.ScrollDelta / 120f * ScrollIncrement);
            }

            if (ShowScrollbar && CanScroll || ScrollbarInteraction.Pressed) {
                RectangleF thumb = ScrollbarThumb;
                ScrollbarInteraction.UpdateInput(this, thumb);
                if (ScrollbarInteraction.JustPressed) {
                    _scrollDragStart = MainOffset;
                }
                if (ScrollbarInteraction.Pressed && CanScroll) {
                    float track = MainOf(Size) - Padding * 2f;
                    float moved = MainOf(ScrollbarInteraction.Position - ScrollbarInteraction.PressPosition);
                    SnapOffset(_scrollDragStart - moved * (MainOf(ContentSize) - MainOf(Size)) / MathF.Max(track - MainOf(thumb.Size), 1f));
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
            MainEasedOffset.Target = ClampMain(MainEasedOffset.Target);

            // That speed can carry a scroll a bit past its target, which shouldn't show past the ends of the content.
            _offsetX = ClampOffsetX(_easedOffsetX.Value);
            _offsetY = ClampOffsetY(_easedOffsetY.Value);

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
            if (start < 0f) {
                MainEasedOffset.Target = ClampMain(MainOffset - start);
            } else if (end > MainOf(Size)) {
                MainEasedOffset.Target = ClampMain(MainOffset - (end - MainOf(Size)));
            }

            base.SendToTop(c);
        }

        protected bool IsVertical => Orientation == Orientation.Vertical;
        protected float MainOf(Vector2 v) => IsVertical ? v.Y : v.X;
        protected float CrossOf(Vector2 v) => IsVertical ? v.X : v.Y;
        protected Vector2 FromMainCross(float main, float cross) => IsVertical ? new Vector2(cross, main) : new Vector2(main, cross);
        protected EasedValue<float> MainEasedOffset => IsVertical ? _easedOffsetY : _easedOffsetX;
        protected float MainOffset => IsVertical ? _offsetY : _offsetX;
        protected float ClampMain(float offset) => IsVertical ? ClampOffsetY(offset) : ClampOffsetX(offset);

        protected virtual float ClampOffsetX(float x) {
            return MathF.Min(MathF.Max(x, Width - ContentSize.X), 0f);
        }
        protected virtual float ClampOffsetY(float y) {
            return MathF.Min(MathF.Max(y, Height - ContentSize.Y), 0f);
        }

        /// <summary>Jumps the offset along the stack's direction, for drags that should follow the pointer exactly.</summary>
        protected void SnapOffset(float offset) {
            MainEasedOffset.Snap(ClampMain(offset));
        }

        protected static double GetDuration(float a, float b, float speed, double maxDuration) {
            return Math.Min(Math.Abs((b - a) / speed), maxDuration);
        }

        protected EasedValue<float> _easedOffsetX;
        protected EasedValue<float> _easedOffsetY;

        protected float _offsetX = 0;
        protected float _offsetY = 0;
        protected float _scrollDragStart = 0;
        protected bool _showsScrollbar = false;
    }
}
