using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Graphics;

namespace Apos.Gui {
    /// <summary>Draws a texture region.</summary>
    public class Icon(long id, Texture2DRegion region) : Component(id) {
        /// <summary>The texture region to draw. Its size is the icon's preferred size.</summary>
        public Texture2DRegion Region { get; set; } = region;
        /// <summary>Draws this icon. Null uses <see cref="DefaultLook"/>.</summary>
        public Action<Icon>? Look { get; set; }
        /// <summary>Draws every icon that doesn't have its own <see cref="Look"/>.</summary>
        public static Action<Icon> DefaultLook { get; set; } = DrawDefault;

        /// <inheritdoc/>
        public override void UpdatePrefSize(GameTime gametime) {
            PrefWidth = Region.Width;
            PrefHeight = Region.Height;
        }
        /// <inheritdoc/>
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

        /// <summary>An icon that draws a texture region.</summary>
        /// <param name="region">The texture region to draw.</param>
        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="file">The caller's file. The same line in two files gives two different ids.</param>
        public static Icon Put(Texture2DRegion region, [CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            Icon a = IMGUI.Put(id, isAbsoluteId, file, id => new Icon(id, region));
            a.Region = region;

            return a;
        }
    }
}
