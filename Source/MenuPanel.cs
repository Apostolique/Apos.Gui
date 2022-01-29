using System.Runtime.CompilerServices;
using Apos.Input;
using Apos.Tweens;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    public class MenuPanel : Panel {
        public MenuPanel(int id) : base(id) { }

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
        public override void UpdateSetup(GameTime gameTime) {
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

                c.UpdateSetup(gameTime);

                currentY += c.Height;
            }
        }

        protected override float ClampOffsetY(float y) {
            return MathHelper.Min(MathHelper.Max(y, Height - FullHeight), FullHeight < Height ? Height / 2f - FullHeight / 2f : 0f);
        }

        public static MenuPanel Push([CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            // 1. Check if ScreenPanel with id already exists.
            //      a. If already exists. Get it.
            //      b  If not, create it.
            // 3. Push it on the stack.
            // 4. Ping it.
            id = GuiHelper.CurrentIMGUI.CreateId(id, isAbsoluteId);
            GuiHelper.CurrentIMGUI.TryGetValue(id, out IComponent c);

            MenuPanel a;
            if (c is MenuPanel) {
                a = (MenuPanel)c;
            } else {
                a = new MenuPanel(id);
            }

            IParent parent = GuiHelper.CurrentIMGUI.GrabParent(a);

            if (a.LastPing != InputHelper.CurrentFrame) {
                a.Reset();
                a.LastPing = InputHelper.CurrentFrame;
                a.Index = parent.NextIndex();
            }

            GuiHelper.CurrentIMGUI.Push(a);

            return a;
        }
        public static void Pop() {
            GuiHelper.CurrentIMGUI.Pop();
        }

        protected bool _snap = false;
    }
}
