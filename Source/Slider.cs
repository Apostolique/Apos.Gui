using System;
using System.Runtime.CompilerServices;
using Apos.Input;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    public class Slider : Component {
        public Slider(long id, float value, float min, float max, float? step) : base(id) {
            Value = value;
            Min = min;
            Max = max;
            Step = step;
        }

        public float Value { get; set; }
        /// <summary>Where the value sits between Min and Max, from 0 to 1.</summary>
        public float Percent => Max == Min ? 0f : (Value - Min) / (Max - Min);
        public float Min { get; set; }
        public float Max { get; set; }
        public float? Step {
            get => _step ?? (Max - Min) / 100f;
            set {
                _step = value;
            }
        }
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
        public Interaction Interaction { get; } = new() { UsesFocusButton = false };

        public override void UpdateInput(GameTime gameTime) {
            Interaction.UpdateInput(this);

            if (Interaction.Pressed && Interaction.Source != InteractionSource.Button) {
                float percent = Travel > 0f ? (Interaction.Position.X - TravelStart) / Travel : 0f;
                Value = MathHelper.Clamp(percent * (Max - Min) + Min, Min, Max);
            }
            if (IsFocused) {
                if (Root.Bindings.MoveLeft.Pressed()) Slide(-1);
                if (Root.Bindings.MoveRight.Pressed()) Slide(1);
            }
        }

        public override void UpdatePrefSize(GameTime gameTime) {
            PrefSize = BarSize;
        }

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
        protected float TravelStart => Left + Inset;
        protected float Travel => MathF.Max(Width - Inset * 2f, 0f);

        protected void Slide(int direction) {
            Value = MathHelper.Clamp(Value + direction * Step!.Value, Min, Max);
        }

        protected float? _step;

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
