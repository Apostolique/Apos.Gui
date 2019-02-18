using Apos.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.TextureAtlases;

namespace Apos.Gui {
    /// <summary>
    /// Goal: Just a simple Gui element that displays a texture.
    /// </summary>
    class Icon : Component {
        // Group: Constructors
        public Icon(TextureRegion2D iRegion) {
            _region = iRegion;
        }

        // Group: Public Variables
        public override int PrefWidth => _region.Width;
        public override int PrefHeight => _region.Height;

        // Group: Public Functions
        public override void Draw(SpriteBatch s) {
            SetScissor(s);

            int halfWidth = Width / 2;
            int iconHalfWidth = PrefWidth / 2;

            int halfHeight = Height / 2;
            int iconHalfHeight = PrefHeight / 2;

            Vector2 pos = new Vector2(Left + halfWidth - iconHalfWidth, Top + halfHeight - iconHalfHeight);

            s.Draw(_region, pos, Color.White);

            ResetScissor(s);
        }

        // Group: Private Variables
        protected TextureRegion2D _region;
    }
}