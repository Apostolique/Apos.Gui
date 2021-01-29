using Apos.Input;
using Microsoft.Xna.Framework;
using MonoGame.Extended.TextureAtlases;

namespace Apos.Gui {
    public class Icon : Component {
        public Icon(TextureRegion2D region) {
            Region = region;
        }

        public TextureRegion2D Region {
            get;
            set;
        }

        public override void UpdatePrefSize() {
            PrefWidth = Region.Width;
            PrefHeight = Region.Height;
        }
        public override void Draw() {
            GuiHelper.SetScissor(Clip);

            int halfWidth = (int)(Width / 2);
            int iconHalfWidth = (int)(PrefWidth / 2);

            int halfHeight = (int)(Height / 2);
            int iconHalfHeight = (int)(PrefHeight / 2);

            Vector2 pos = new Vector2(Left + halfWidth - iconHalfWidth, Top + halfHeight - iconHalfHeight);

            GuiHelper.SpriteBatch.Draw(Region, pos, Color.White);

            GuiHelper.ResetScissor();
        }

        public static Icon Put(TextureRegion2D region, int id = 0) {
            // 1. Check if Icon with id already exists.
            //      a. If already exists. Get it.
            //      b  If not, create it.
            // 4. Ping it.
            var fullName = $"icon{(id == 0 ? GuiHelper.CurrentIMGUI.NextId() : id)}";

            IParent? parent = GuiHelper.CurrentIMGUI.CurrentParent;
            GuiHelper.CurrentIMGUI.TryGetValue(fullName, out IComponent c);

            Icon a;
            if (c is Icon) {
                a = (Icon)c;
            } else {
                a = new Icon(region);
                GuiHelper.CurrentIMGUI.Add(fullName, a);
            }

            a.Region = region;
            if (a.LastPing != InputHelper.CurrentFrame) {
                a.LastPing = InputHelper.CurrentFrame;
                if (parent != null) {
                    a.Index = parent.NextIndex();
                }
            }

            return a;
        }
    }
}
