using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    public class CenteredVertical(int id) : Vertical(id) {
        public override void UpdateLayout(GameTime gameTime) {
            // TODO: Keep current focus in view if it's in view?

            if (_offsetYTween.B != ClampOffsetY(_offsetYTween.B)) {
                SetOffset(_offsetYTween, ClampOffsetY(_offsetYTween.B));
            }

            _offsetX = _offsetXTween.Value;
            _offsetY = _offsetYTween.Value;

            float maxWidth = Width;
            float maxHeight = Height;

            float halfWidth = Width / 2f;

            float currentY = 0f;
            foreach (var c in _children) {
                c.Width = MathHelper.Min(c.PrefWidth, Width);
                c.Height = c.PrefHeight;

                c.X = X + OffsetX + halfWidth - c.Width / 2f;
                c.Y = currentY + Y + OffsetY;

                maxWidth = MathHelper.Max(c.Width, maxWidth);
                c.Clip = c.Bounds.Intersect(Clip);

                if (c is IParent p) {
                    p.UpdateLayout(gameTime);
                }

                currentY += c.Height;
            }

            FullWidth = maxWidth;
            FullHeight = MathHelper.Max(currentY, maxHeight);
        }

        public static new CenteredVertical Push([CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            id = IMGUI.TryCreateId(id, isAbsoluteId, out IComponent? c);

            CenteredVertical a;
            if (c is CenteredVertical d) {
                a = d;
            } else {
                a = new CenteredVertical(id);
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
