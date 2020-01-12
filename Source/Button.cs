using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Optional;

namespace Apos.Gui {
    /// <summary>
    /// Goal: A button component that handles actions.
    /// </summary>
    public class Button : Component {

        // Group: Constructors

        public Button() : this(new Component()) { }
        public Button(Component c) {
            Item = c;
            IsFocusable = true;
            Width = Item.PrefWidth;
            Height = Item.PrefHeight;
        }

        // Group: Public Variables

        public virtual bool ShowBox {
            get;
            set;
        } = true;
        public override bool OldIsHovered {
            get => base.OldIsHovered;
            set {
                base.OldIsHovered = value;
                Item.OldIsHovered = value;
            }
        }
        public override bool IsHovered {
            get => base.IsHovered;
            set {
                base.IsHovered = value;
                Item.IsHovered = value;
            }
        }
        public override bool HasFocus {
            get => base.HasFocus;
            set {
                base.HasFocus = value;
                Item.HasFocus = value;
            }
        }
        public virtual Component Item {
            get => _item;
            set {
                _item = value;
                _item.Parent = Option.Some((Component)this);
            }
        }
        public override int PrefWidth => Item.PrefWidth;
        public override int PrefHeight => Item.PrefHeight;

        // Group: Public Functions

        public override void UpdateSetup() {
            base.UpdateSetup();

            Item.Width = Width;
            Item.Height = Height;
            Item.Position = Position;
            Item.ClippingRect = ClippingRect;

            Item.UpdateSetup();
        }
        public override void Draw() {
            SetScissor();
            if (ShowBox) {
                if (IsHovered) {
                    _s.FillRectangle(BoundingRect, new Color(20, 20, 20));
                } else {
                    _s.FillRectangle(BoundingRect, Color.Black);
                }
            }

            Item.Draw();

            if (ShowBox && HasFocus) {
                _s.DrawRectangle(BoundingRect, Color.White, 2);
            }

            ResetScissor();
        }

        // Group: Private Variables

        protected Component _item;
    }
}