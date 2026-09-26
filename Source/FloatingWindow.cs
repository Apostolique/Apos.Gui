using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    public class FloatingWindow(long id) : Vertical(id) {
        public override bool IsFocusable { get; set; } = true;
        public override bool IsFocusScope { get; set; } = true;
        public override bool IsFloatable { get; set; } = true;

        public Interaction Interaction { get; } = new() { UsesFocusButton = false };
        /// <summary>Height of the space under the content, in UI units.</summary>
        public float BarHeight { get; set; } = 20f;
        /// <summary>Draws this window under its content. Null uses <see cref="DefaultLook"/>.</summary>
        public Action<FloatingWindow>? Look { get; set; }
        /// <summary>Draws every window that doesn't have its own <see cref="Look"/>.</summary>
        public static Action<FloatingWindow> DefaultLook { get; set; } = DrawDefault;

        public override void UpdateInput(GameTime gameTime) {
            base.UpdateInput(gameTime);
            Interaction.UpdateInput(this);

            if (Interaction.JustPressed) {
                _dragStart = XY;
            }
            if (Interaction.Pressed) {
                XY = _dragStart + Interaction.Position - Interaction.PressPosition;
            }
        }
        public override void UpdatePrefSize(GameTime gameTime) {
            base.UpdatePrefSize(gameTime);

            PrefHeight += BarHeight;
        }

        public override void Draw(GameTime gameTime) {
            (Look ?? DefaultLook)(this);
            base.Draw(gameTime);
        }

        /// <summary>A background with a bar under the content.</summary>
        public static void DrawDefault(FloatingWindow w) {
            GuiHelper.ShapeBatch.FillRectangle(w.XY, w.Size, Theme.Background, Theme.Rounding);
            GuiHelper.ShapeBatch.FillRectangle(new Vector2(w.Left, w.Bottom - w.BarHeight), new Vector2(w.Width, w.BarHeight), new Color(Theme.Highlight, 0.5f));
        }

        protected Vector2 _dragStart = Vector2.Zero;

        public static new FloatingWindow Push([CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            return IMGUI.Push(id, isAbsoluteId, file, id => new FloatingWindow(id));
        }
        public static new void Pop() {
            GuiHelper.CurrentIMGUI.Pop();
        }
    }
}
