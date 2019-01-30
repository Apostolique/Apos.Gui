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
                Item.Position = base.Position + _topLeftOffset;
                Item.Width = base.Width - MarginLeft - MarginRight;
                Item.Height = base.Height - MarginTop - MarginBottom;
            }
        }
        public override Point Position {
            get => base.Position;
            set {
                base.Position = value;
                Item.Position = base.Position + _topLeftOffset;
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
            get => _marginLeft;
            set {
                _marginLeft = value;
                _topLeftOffset = new Point(MarginLeft, _topLeftOffset.Y);
                Item.Position = base.Position + _topLeftOffset;
                Item.Width = base.Width - MarginLeft - MarginRight;
            }
        }
        public virtual int MarginTop {
            get => _marginTop;
            set {
                _marginTop = value;
                _topLeftOffset = new Point(_topLeftOffset.X, MarginTop);
                Item.Position = base.Position + _topLeftOffset;
                Item.Height = base.Height - MarginTop - MarginBottom;
            }
        }
        public virtual int MarginRight {
            get => _marginRight;
            set {
                _marginRight = value;
                Item.Width = base.Width - MarginLeft - MarginRight;
            }
        }
        public virtual int MarginBottom {
            get => _marginBottom;
            set {
                _marginBottom = value;
                Item.Height = base.Height - MarginTop - MarginBottom;
            }
        }
        public override Rectangle ClippingRect {
            get {
                return base.ClippingRect;
            }
            set {
                base.ClippingRect = value;
                Item.ClippingRect = value;
            }
        }
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
        public override bool IsFocusable => Item.IsFocusable;

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
        protected int _marginLeft;
        protected int _marginTop;
        protected int _marginRight;
        protected int _marginBottom;
        protected Point _topLeftOffset = new Point(0, 0);
    }
}