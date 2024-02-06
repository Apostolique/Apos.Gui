using System.Runtime.CompilerServices;
using Apos.Tweens;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    public class MenuPanel(int id) : Vertical(id) {
        public override void UpdatePrefSize(GameTime gameTime) {
            base.UpdatePrefSize(gameTime);

            FullWidth = PrefWidth;
            FullHeight = PrefHeight;

            if (PrefWidth != GuiHelper.WindowWidth) {
                PrefWidth = GuiHelper.WindowWidth;
                _snap = true;
            }
            if (PrefHeight != GuiHelper.WindowHeight) {
                PrefHeight = GuiHelper.WindowHeight;
                _snap = true;
            }
        }
        public override void UpdateLayout(GameTime gameTime) {
            // MenuPanel is a root component so it can set it's own position.
            X = 0f;
            Y = 0f;

            if (_offsetYTween.B != ClampOffsetY(_offsetYTween.B)) {
                if (!_snap) {
                    SetOffset(_offsetYTween, ClampOffsetY(_offsetYTween.B));
                } else {
                    _offsetYTween.A = ClampOffsetY(_offsetYTween.B);
                    _offsetYTween.B = ClampOffsetY(_offsetYTween.B);
                    _offsetYTween.StartTime = TweenHelper.TotalMS;
                    _offsetYTween.Duration = 0;
                    _snap = false;
                }
            }

            _offsetX = _offsetXTween.Value;
            _offsetY = _offsetYTween.Value;

            float halfWidth = Width / 2f;

            float currentY = 0f;
            foreach (var c in _children) {
                c.Width = c.PrefWidth;
                c.Height = c.PrefHeight;

                c.X = X + OffsetX + halfWidth - c.Width / 2f;
                c.Y = currentY + Y + OffsetY;

                c.Clip = c.Bounds.Intersection(Clip);

                if (c is IParent p) {
                    p.UpdateLayout(gameTime);
                }

                currentY += c.Height;
            }
        }

        protected override float ClampOffsetY(float y) {
            return MathHelper.Min(MathHelper.Max(y, Height - FullHeight), FullHeight < Height ? Height / 2f - FullHeight / 2f : 0f);
        }

        protected bool _snap = false;

        public static new MenuPanel Push([CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            id = GuiHelper.CurrentIMGUI.TryCreateId(id, isAbsoluteId, out IComponent c);

            MenuPanel a;
            if (c is MenuPanel d) {
                a = d;
            } else {
                a = new MenuPanel(id);
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
