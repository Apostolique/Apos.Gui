using Microsoft.Xna.Framework;
using MonoGame.Extended.TextureAtlases;

namespace Apos.Gui {
    /// <summary>
    /// Goal: Just a simple Gui element that displays a texture.
    /// </summary>
    public class Icon : Component {

        // Group: Constructors

        public Icon(TextureRegion2D iRegion) {
            _region = iRegion;
        }

        // Group: Public Variables

        public override int PrefWidth => _region.Width;
        public override int PrefHeight => _region.Height;

        // Group: Public Functions

        public override void Draw() {
            SetScissor();

            int halfWidth = Width / 2;
            int iconHalfWidth = PrefWidth / 2;

            int halfHeight = Height / 2;
            int iconHalfHeight = PrefHeight / 2;

            Vector2 pos = new Vector2(Left + halfWidth - iconHalfWidth, Top + halfHeight - iconHalfHeight);

            _s.Draw(_region, pos, Color.White);

            ResetScissor();
        }

        // Group: Private Variables

        protected TextureRegion2D _region;
    }
}