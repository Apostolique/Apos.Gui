using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    public class FloatingWindow(int id) : Vertical(id) {
        public override bool IsFocusable { get; set; } = true;
        public override bool IsFloatable { get; set; } = true;

        public Interaction Interaction { get; } = new() { UsesFocusButton = false };

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

            PrefHeight += 20;
        }

        public override void Draw(GameTime gameTime) {
            GuiHelper.PushClip(Clip);
            GuiHelper.ShapeBatch.FillRectangle(Bounds.Position, Bounds.Size, Color.Black);
            GuiHelper.ShapeBatch.FillRectangle(new Vector2(Left, Bottom - 20), new Vector2(Width, 20), new Color(Color.White, 0.5f));
            GuiHelper.PopClip();

            base.Draw(gameTime);
        }

        protected Vector2 _dragStart = Vector2.Zero;

        public static new FloatingWindow Push([CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            id = IMGUI.TryCreateId(id, isAbsoluteId, out IComponent? c);

            FloatingWindow a;
            if (c is FloatingWindow d) {
                a = d;
            } else {
                a = new FloatingWindow(id);
            }

            GuiHelper.CurrentIMGUI.GrabParent(a);

            GuiHelper.CurrentIMGUI.Push(a);

            return a;
        }
        public static new void Pop() {
            GuiHelper.CurrentIMGUI.Pop();
        }
    }
}
