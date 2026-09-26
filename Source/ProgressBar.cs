using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    /// <summary>
    /// Shows how far along something is, from 0 to 1.
    /// </summary>
    public class ProgressBar(long id) : Component(id) {
        public float Value {
            get => _value;
            set => _value = MathHelper.Clamp(value, 0f, 1f);
        }
        /// <summary>The size it asks its parent for, in UI units.</summary>
        public Vector2 BarSize { get; set; } = new(200f, 20f);
        /// <summary>Draws this progress bar. Null uses <see cref="DefaultLook"/>.</summary>
        public Action<ProgressBar>? Look { get; set; }
        /// <summary>Draws every progress bar that doesn't have its own <see cref="Look"/>.</summary>
        public static Action<ProgressBar> DefaultLook { get; set; } = DrawDefault;

        public override void UpdatePrefSize(GameTime gameTime) {
            PrefSize = BarSize;
        }
        public override void Draw(GameTime gameTime) {
            (Look ?? DefaultLook)(this);
        }

        /// <summary>A frame that fills from the left.</summary>
        public static void DrawDefault(ProgressBar p) {
            float inset = Theme.BorderThickness * 2f;
            Vector2 inner = p.Size - new Vector2(inset * 2f);
            if (p.Value > 0f && inner.X > 0f && inner.Y > 0f) {
                GuiHelper.ShapeBatch.FillRectangle(p.XY + new Vector2(inset), new Vector2(inner.X * p.Value, inner.Y), Theme.Focus, MathF.Max(Theme.Rounding - inset, 0f));
            }
            GuiHelper.ShapeBatch.BorderRectangle(p.XY, p.Size, Theme.Border, Theme.BorderThickness, Theme.Rounding);
        }

        protected float _value;

        /// <param name="value">From 0 to 1.</param>
        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="file">The caller's file. The same line in two files gives two different ids.</param>
        public static ProgressBar Put(float value, [CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            ProgressBar a = IMGUI.Put(id, isAbsoluteId, file, id => new ProgressBar(id));
            a.Value = value;

            return a;
        }
    }
}
