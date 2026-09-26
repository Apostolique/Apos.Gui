using System;
using System.Runtime.CompilerServices;
using Apos.Input;
using Apos.Tweens;
using Microsoft.Xna.Framework;
using Track = Apos.Input.Track;

namespace Apos.Gui {
    /// <summary>
    /// Stacks its children from left to right. Scrolls with the mouse wheel when they don't fit.
    /// </summary>
    public class Horizontal(int id) : ListParent(id) {
        public float OffsetX {
            get => _offsetX;
            set {
                SetOffset(_offsetXTween, ClampOffsetX(value));
            }
        }
        public float OffsetY {
            get => _offsetY;
            set {
                SetOffset(_offsetYTween, ClampOffsetY(value));
            }
        }
        public float FullWidth { get; set; } = 100;
        public float FullHeight { get; set; } = 100;

        public Vector2 OffsetXY {
            get => new(OffsetX, OffsetY);
            set {
                OffsetX = value.X;
                OffsetY = value.Y;
            }
        }
        public Vector2 FullSize {
            get => new(FullWidth, FullHeight);
            set {
                FullWidth = value.X;
                FullHeight = value.Y;
            }
        }

        public float ScrollIncrement { get; set; } = 50f;
        public float ScrollSpeed { get; set; } = 0.25f;
        public long ScrollMaxDuration { get; set; } = 1000;

        public override void UpdateInput(GameTime gameTime) {
            base.UpdateInput(gameTime);

            // TODO: If we don't scroll, don't consume the scroll to bubble the event up. Allows recursive scrolling.
            if (Clip.Contains(GuiHelper.Mouse) && Track.MouseCondition.Scrolled()) {
                SetOffset(_offsetXTween, ClampOffsetX(_offsetXTween.B + Math.Sign(MouseCondition.ScrollDelta) * ScrollIncrement));
            }

            // TODO: Consume clicks on the panel? Otherwise it's possible to click stuff under it.
        }

        public override void UpdatePrefSize(GameTime gameTime) {
            float maxWidth = 0;
            float maxHeight = 0;

            foreach (var c in _children) {
                c.UpdatePrefSize(gameTime);
                maxWidth += c.PrefWidth;
                maxHeight = MathHelper.Max(c.PrefHeight, maxHeight);
            }

            PrefWidth = maxWidth;
            PrefHeight = maxHeight;
        }
        public override void UpdateLayout(GameTime gameTime) {
            // TODO: Keep current focus in view if it's in view?

            if (_offsetXTween.B != ClampOffsetX(_offsetXTween.B)) {
                SetOffset(_offsetXTween, ClampOffsetX(_offsetXTween.B));
            }

            _offsetX = _offsetXTween.Value;
            _offsetY = _offsetYTween.Value;

            float maxWidth = Width;
            float maxHeight = Height;

            float currentX = 0f;
            foreach (var c in _children) {
                c.X = currentX + X + OffsetX;
                c.Y = Y + OffsetY;
                c.Width = c.PrefWidth;
                c.Height = MathHelper.Min(c.PrefHeight, Height);

                maxHeight = MathHelper.Max(c.Height, maxHeight);
                LayoutChild(c, gameTime);

                currentX += c.Width;
            }

            FullWidth = MathHelper.Max(currentX, maxWidth);
            FullHeight = maxHeight;
        }

        // TODO: Draw scrollbars if needed?

        public override void SendToTop(IComponent c) {
            if (c.X < X) {
                float diff = X - c.X;
                float oDiff = _offsetXTween.B - _offsetX;
                SetOffset(_offsetXTween, ClampOffsetX(_offsetXTween.B + diff - oDiff));
            }
            if (c.Right > Right) {
                float diff = Right - c.Right;
                float oDiff = _offsetXTween.B - _offsetX;
                SetOffset(_offsetXTween, ClampOffsetX(_offsetXTween.B + diff - oDiff));
            }

            base.SendToTop(c);
        }

        protected virtual float ClampOffsetX(float x) {
            return MathHelper.Min(MathHelper.Max(x, Width - FullWidth), 0f);
        }
        protected virtual float ClampOffsetY(float y) {
            return MathHelper.Min(MathHelper.Max(y, Height - FullHeight), 0f);
        }

        protected void SetOffset(FloatTween ft, float b) {
            var a = ft.Value;
            ft.StartTime = TweenHelper.TotalMS;
            ft.A = a;
            ft.B = b;
            ft.Duration = GetDuration(a, b, ScrollSpeed, ScrollMaxDuration);
        }

        protected static long GetDuration(float a, float b, float speed, long maxDuration) {
            return (long)Math.Min(Math.Abs((b - a) / speed), maxDuration);
        }

        protected FloatTween _offsetXTween = new(0f, 0f, 0, Easing.ExpoOut);
        protected FloatTween _offsetYTween = new(0f, 0f, 0, Easing.ExpoOut);

        protected float _offsetX = 0;
        protected float _offsetY = 0;

        public static Horizontal Push([CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            return IMGUI.Push(id, isAbsoluteId, id => new Horizontal(id));
        }
        public static void Pop() {
            GuiHelper.CurrentIMGUI.Pop();
        }
    }
}
