using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    public class Checkbox(int id, bool isChecked) : Component(id) {
        public Interaction Interaction { get; } = new();
        public bool Clicked => Interaction.Clicked;
        public bool IsChecked { get; set; } = isChecked;
        public override bool IsFocusable { get; set; } = true;

        public override void UpdateInput(GameTime gameTime) {
            Interaction.UpdateInput(this);
            if (Clicked) {
                IsChecked = !IsChecked;
            }
        }

        public override void UpdatePrefSize(GameTime gameTime) {
            PrefWidth = 30f;
            PrefHeight = 30f;
        }

        public override void Draw(GameTime gameTime) {
            GuiHelper.PushClip(Clip);

            if (IsChecked) {
                GuiHelper.ShapeBatch.FillRectangle(new Vector2(Left + 6, Top + 6), new Vector2(Width - 12, Height - 12), Color.White);
            }

            float highlight = Clicked ? 0.5f : 0.08f * Interaction.Hover + 0.15f * Interaction.Press;
            if (highlight > 0f) {
                GuiHelper.ShapeBatch.FillRectangle(Bounds.Position, Bounds.Size, new Color(Color.White, highlight));
            }
            if (IsFocused) {
                GuiHelper.ShapeBatch.BorderRectangle(Bounds.Position, Bounds.Size, Color.White, 2f);
                GuiHelper.ShapeBatch.BorderRectangle(new Vector2(Left + 6, Top + 6), new Vector2(Width - 12, Height - 12), Color.White, 2f);
            } else {
                GuiHelper.ShapeBatch.BorderRectangle(Bounds.Position, Bounds.Size, new Color(76, 76, 76), 2f);
                GuiHelper.ShapeBatch.BorderRectangle(new Vector2(Left + 6, Top + 6), new Vector2(Width - 12, Height - 12), new Color(76, 76, 76), 2f);
            }

            GuiHelper.PopClip();
        }


        public static Checkbox Put(ref bool isChecked, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            id = IMGUI.TryCreateId(id, isAbsoluteId, out IComponent? c);

            Checkbox a;
            if (c is Checkbox d) {
                a = d;
                if (a.IsFocused) {
                    isChecked = a.IsChecked;
                } else {
                    a.IsChecked = isChecked;
                }
            } else {
                a = new Checkbox(id, isChecked);
            }

            GuiHelper.CurrentIMGUI.GrabParent(a);

            return a;
        }
    }
}
