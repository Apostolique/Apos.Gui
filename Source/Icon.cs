using System.Runtime.CompilerServices;
using Apos.Input;
using Microsoft.Xna.Framework;
using MonoGame.Extended.TextureAtlases;

namespace Apos.Gui {
    public class Icon : Component {
        public Icon(int id, TextureRegion2D region) : base(id) {
            Region = region;
        }

        public TextureRegion2D Region { get; set; }

        public override void UpdatePrefSize(GameTime gametime) {
            PrefWidth = Region.Width;
            PrefHeight = Region.Height;
        }
        public override void Draw(GameTime gameTime) {
            GuiHelper.SetScissor(Clip);

            int halfWidth = (int)(Width / 2);
            int iconHalfWidth = (int)(PrefWidth / 2);

            int halfHeight = (int)(Height / 2);
            int iconHalfHeight = (int)(PrefHeight / 2);

            Vector2 pos = new Vector2(Left + halfWidth - iconHalfWidth, Top + halfHeight - iconHalfHeight);

            GuiHelper.SpriteBatch.Draw(Region, pos, Color.White);

            GuiHelper.ResetScissor();
        }

        public static Icon Put(TextureRegion2D region, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            // 1. Check if Icon with id already exists.
            //      a. If already exists. Get it.
            //      b  If not, create it.
            // 4. Ping it.
            id = GuiHelper.CurrentIMGUI.CreateId(id, isAbsoluteId);
            GuiHelper.CurrentIMGUI.TryGetValue(id, out IComponent c);

            Icon a;
            if (c is Icon) {
                a = (Icon)c;
                a.Region = region;
            } else {
                a = new Icon(id, region);
            }

            IParent? parent = GuiHelper.CurrentIMGUI.GrabParent(a);

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
