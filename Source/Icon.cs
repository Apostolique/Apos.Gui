using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Graphics;

namespace Apos.Gui {
    public class Icon(int id, Texture2DRegion region) : Component(id) {
        public Texture2DRegion Region { get; set; } = region;

        public override void UpdatePrefSize(GameTime gametime) {
            PrefWidth = Region.Width;
            PrefHeight = Region.Height;
        }
        public override void Draw(GameTime gameTime) {
            GuiHelper.PushScissor(Clip);

            int halfWidth = (int)(Width / 2);
            int iconHalfWidth = (int)(PrefWidth / 2);

            int halfHeight = (int)(Height / 2);
            int iconHalfHeight = (int)(PrefHeight / 2);

            Vector2 pos = new(Left + halfWidth - iconHalfWidth, Top + halfHeight - iconHalfHeight);

            GuiHelper.SpriteBatch.Draw(Region, pos, Color.White);

            GuiHelper.PopScissor();
        }

        public static Icon Put(Texture2DRegion region, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            id = GuiHelper.CurrentIMGUI.TryCreateId(id, isAbsoluteId, out IComponent c);

            Icon a;
            if (c is Icon d) {
                a = d;
                a.Region = region;
            } else {
                a = new Icon(id, region);
            }

            GuiHelper.CurrentIMGUI.GrabParent(a);

            return a;
        }
    }
}
