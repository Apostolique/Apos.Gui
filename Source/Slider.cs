using System;
using System.Runtime.CompilerServices;
using Apos.Input;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    /// <summary>Picks a number between a min and a max. Drag the thumb along the track, or use the arrows while it has focus.</summary>
    public class Slider : Component {
        /// <summary>Creates the slider. <see cref="Put"/> calls this on the first frame it's put.</summary>
        public Slider(long id, float value, float min, float max, float? step) : base(id) {
            Value = value;
            Min = min;
            Max = max;
            Step = step;
        }

        /// <summary>The current value, from <see cref="Min"/> to <see cref="Max"/>.</summary>
        public float Value { get; set; }
        /// <summary>Where the value sits between Min and Max, from 0 to 1.</summary>
        public float Percent => Max == Min ? 0f : (Value - Min) / (Max - Min);
        /// <summary>The value at the left end.</summary>
        public float Min { get; set; }
        /// <summary>The value at the right end.</summary>
        public float Max { get; set; }
        /// <summary>
        /// How far the arrows move the value. Dragging snaps to it too when you set one.
        /// Without one, the arrows move by 1% of the range and dragging is smooth.
        /// </summary>
        public float? Step {
            get => _step ?? (Max - Min) / 100f;
            set {
                _step = value;
            }
        }
        /// <inheritdoc/>
        public override bool IsFocusable { get; set; } = true;
        /// <summary>The size it asks its parent for, in UI units.</summary>
        public Vector2 BarSize { get; set; } = new(200f, 30f);
        /// <summary>In UI units.</summary>
        public float ThumbRadius { get; set; } = 10f;
        /// <summary>In UI units.</summary>
        public float TrackThickness { get; set; } = 6f;
        /// <summary>Draws this slider. Null uses <see cref="DefaultLook"/>.</summary>
        public Action<Slider>? Look { get; set; }
        /// <summary>Draws every slider that doesn't have its own <see cref="Look"/>.</summary>
        public static Action<Slider> DefaultLook { get; set; } = DrawDefault;
        /// <summary>Hover, press and drag for this slider.</summary>
        public Interaction Interaction { get; } = new() { UsesFocusButton = false };

        /// <inheritdoc/>
        public override void UpdateInput(GameTime gameTime) {
            Interaction.UpdateInput(this);

            if (Interaction.Pressed && Interaction.Source != InteractionSource.Button) {
                float percent = Travel > 0f ? (Interaction.Position.X - TravelStart) / Travel : 0f;
                float value = percent * (Max - Min) + Min;
                if (_step is float step && step > 0f) {
                    value = Min + MathF.Round((value - Min) / step) * step;
                }
                Value = MathHelper.Clamp(value, Min, Max);
            }
            if (IsFocused) {
                if (Root.Bindings.MoveLeft.Pressed()) Slide(-1);
                if (Root.Bindings.MoveRight.Pressed()) Slide(1);
            }
        }

        /// <inheritdoc/>
        public override void UpdatePrefSize(GameTime gameTime) {
            PrefSize = BarSize;
        }

        /// <inheritdoc/>
        public override void Draw(GameTime gameTime) {
            (Look ?? DefaultLook)(this);
        }

        /// <summary>A track filled up to a round thumb.</summary>
        public static void DrawDefault(Slider s) {
            float y = s.Top + s.Height / 2f;
            float left = s.TravelStart;
            float right = left + s.Travel;
            float half = s.TrackThickness / 2f;
            Vector2 thumb = new(s.ThumbX, y);

            Color track = Theme.Surface.A > 0 ? Theme.Surface : Theme.Border;
            GuiHelper.ShapeBatch.FillRectangle(new Vector2(left - half, y - half), new Vector2(right - left + half * 2f, half * 2f), track, half);
            GuiHelper.ShapeBatch.FillRectangle(new Vector2(left - half, y - half), new Vector2(thumb.X - left + half * 2f, half * 2f), Theme.Accent, half);

            float highlight = 0.1f * s.Interaction.Hover + 0.15f * s.Interaction.Press;
            if (highlight > 0f) {
                GuiHelper.ShapeBatch.FillCircle(thumb, s.ThumbRadius + 5f, new Color(Theme.Highlight, highlight));
            }
            GuiHelper.ShapeBatch.FillCircle(thumb, s.ThumbRadius, Theme.Accent);
            if (s.IsFocused) {
                GuiHelper.ShapeBatch.BorderCircle(thumb, s.ThumbRadius + 4f, Theme.Focus, Theme.BorderThickness);
            }
        }

        /// <summary>Where the middle of the thumb is, in UI coordinates.</summary>
        public float ThumbX => TravelStart + Travel * Percent;
        /// <summary>The thumb stays this far from the ends so that it stays inside the slider.</summary>
        protected float Inset => ThumbRadius + 5f;
        /// <summary>Where the thumb's center is at <see cref="Min"/>, in UI coordinates.</summary>
        protected float TravelStart => Left + Inset;
        /// <summary>How far the thumb's center moves from <see cref="Min"/> to <see cref="Max"/>, in UI units.</summary>
        protected float Travel => MathF.Max(Width - Inset * 2f, 0f);

        /// <summary>Moves the value by one <see cref="Step"/>. -1 goes toward <see cref="Min"/> and 1 toward <see cref="Max"/>.</summary>
        protected void Slide(int direction) {
            Value = MathHelper.Clamp(Value + direction * Step!.Value, Min, Max);
        }

        /// <summary>Backs <see cref="Step"/>. Null when no step was set.</summary>
        protected float? _step;

        /// <summary>A slider from min to max.</summary>
        /// <param name="value">Your value. Changes while the slider is dragged or moved with the arrows.</param>
        /// <param name="min">The value at the left end.</param>
        /// <param name="max">The value at the right end.</param>
        /// <param name="step">How far the arrows move the value, and what dragging snaps to. Null moves by 1% of the range and drags smoothly.</param>
        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="file">The caller's file. The same line in two files gives two different ids.</param>
        public static Slider Put(ref float value, float min, float max, float? step = null, [CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            float initial = value;
            Slider a = IMGUI.Put(id, isAbsoluteId, file, id => new Slider(id, initial, min, max, step));
            if (a.IsFocused) {
                value = a.Value;
            } else {
                a.Value = value;
                a.Min = min;
                a.Max = max;
                a.Step = step;
            }

            return a;
        }
    }
}
