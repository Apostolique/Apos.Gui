using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Graphics;

namespace Apos.Gui {
    public class Icon(int id, Texture2DRegion region) : Component(id) {
        public Texture2DRegion Region { get; set; } = region;
        /// <summary>Draws this icon. Null uses <see cref="DefaultLook"/>.</summary>
        public Action<Icon>? Look { get; set; }
        /// <summary>Draws every icon that doesn't have its own <see cref="Look"/>.</summary>
        public static Action<Icon> DefaultLook { get; set; } = DrawDefault;

        public override void UpdatePrefSize(GameTime gametime) {
            PrefWidth = Region.Width;
            PrefHeight = Region.Height;
        }
        public override void Draw(GameTime gameTime) {
            bool overflows = Region.Width > Width || Region.Height > Height;
            if (overflows) GuiHelper.PushClip(Clip);
            (Look ?? DefaultLook)(this);
            if (overflows) GuiHelper.PopClip();
        }

        /// <summary>The texture region, centered.</summary>
        public static void DrawDefault(Icon i) {
            Vector2 pos = new(MathF.Floor(i.Left + i.Width / 2f - i.Region.Width / 2f), MathF.Floor(i.Top + i.Height / 2f - i.Region.Height / 2f));
            GuiHelper.ShapeBatch.Draw(i.Region.Texture, pos, i.Region.Bounds, Color.White);
        }

        public static Icon Put(Texture2DRegion region, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            Icon a = IMGUI.Put(id, isAbsoluteId, id => new Icon(id, region));
            a.Region = region;

            return a;
        }
    }
}
