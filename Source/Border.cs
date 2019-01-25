using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AposGui {
    /// <summary>
    /// Goal: Adds padding around a component.
    /// </summary>
    public class Border : Component {
        //constructors
        public Border() : this(new Component()) { }
        public Border(Component c) : this(c, 0, 0, 0, 0) { }
        public Border(Component c, int iMarginLeft, int iMarginTop, int iMarginRight, int iMarginBottom) {
            Item = c;
            MarginLeft = iMarginLeft;
            MarginTop = iMarginTop;
            MarginRight = iMarginRight;
            MarginBottom = iMarginBottom;
        }

        //public vars
        public virtual Component Item {
            get => _item;
            set {
                _item = value;
                _item.Parent = this;
            }
        }
        public override Point Position {
            get => base.Position;
            set {
                base.Position = value;
                Item.Position = base.Position + new Point(MarginLeft, MarginTop);
            }
        }
        public override int Width {
            get => base.Width;
            set {
                base.Width = value;
                Item.Width = base.Width - MarginLeft - MarginRight;
            }
        }
        public override int Height {
            get => base.Height;
            set {
                base.Height = value;
                Item.Height = base.Height - MarginTop - MarginBottom;
            }
        }
        public override int PrefWidth => Item.PrefWidth + MarginLeft + MarginRight;
        public override int PrefHeight => Item.PrefHeight + MarginTop + MarginBottom;
        public virtual int MarginLeft {
            get;
            set;
        }
        public virtual int MarginTop {
            get;
            set;
        }
        public virtual int MarginRight {
            get;
            set;
        }
        public virtual int MarginBottom {
            get;
            set;
        }
        public override Rectangle ClippingRect {
            get {
                return base.ClippingRect;
            }
            set {
                base.ClippingRect = value;

                Item.ClippingRect = base.ClippingRect;
            }
        }
        public override bool IsHovered {
            get => _isHovered;
            set {
                OldIsHovered = _isHovered;
                _isHovered = value;

                Item.IsHovered = _isHovered;
            }
        }

        //public functions
        public override Component GetFinal() {
            return Item;
        }
        public override Component GetFinalInverse() {
            return Item;
        }
        public override void UpdateSetup() {
            Item.UpdateSetup();
        }
        public override bool UpdateInput() {
            bool isUsed = base.UpdateInput();

            isUsed = isUsed || Item.UpdateInput();

            return isUsed;
        }
        public override void Update() {
            Item.Update();
        }
        public override void Draw(SpriteBatch s) {
            Item.Draw(s);
        }

        //private vars
        protected Component _item;
    }
}