using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    public class Button(int id) : SingleParent(id) {
        public Interaction Interaction { get; } = new();
        public bool Clicked => Interaction.Clicked;
        public override bool IsFocusable { get; set; } = true;

        public override void UpdateInput(GameTime gameTime) {
            base.UpdateInput(gameTime);
            Interaction.UpdateInput(this);
        }

        public override void Draw(GameTime gameTime) {
            GuiHelper.PushClip(Clip);

            float highlight = Clicked ? 0.5f : 0.08f * Interaction.Hover + 0.15f * Interaction.Press;
            if (highlight > 0f) {
                GuiHelper.ShapeBatch.FillRectangle(Bounds.Position, Bounds.Size, new Color(Color.White, highlight));
            }
            if (IsFocused) {
                GuiHelper.ShapeBatch.BorderRectangle(Bounds.Position, Bounds.Size, Color.White, 2f);
            } else {
                GuiHelper.ShapeBatch.BorderRectangle(Bounds.Position, Bounds.Size, new Color(76, 76, 76), 2f);
            }

            base.Draw(gameTime);

            GuiHelper.PopClip();
        }

        public static Button Put(string text, int fontSize = 30, Color? color = null, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            Button b = Put(id, isAbsoluteId);
            Label.Put(text, fontSize: fontSize, color: color, id: id, isAbsoluteId: isAbsoluteId);

            return b;
        }
        public static Button Put([CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            return IMGUI.Push(id, isAbsoluteId, id => new Button(id), 1);
        }
    }
}
