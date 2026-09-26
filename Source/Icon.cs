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
            GuiHelper.PushClip(Clip);

            int halfWidth = (int)(Width / 2);
            int iconHalfWidth = (int)(PrefWidth / 2);

            int halfHeight = (int)(Height / 2);
            int iconHalfHeight = (int)(PrefHeight / 2);

            Vector2 pos = new(Left + halfWidth - iconHalfWidth, Top + halfHeight - iconHalfHeight);

            GuiHelper.ShapeBatch.Draw(Region.Texture, pos, Region.Bounds, Color.White);

            GuiHelper.PopClip();
        }

        public static Icon Put(Texture2DRegion region, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            Icon a = IMGUI.Put(id, isAbsoluteId, id => new Icon(id, region));
            a.Region = region;

            return a;
        }
    }
}
