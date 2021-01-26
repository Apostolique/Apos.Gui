using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Apos.Gui {
    /// <summary>
    /// An empty component that doesn't really do anything.
    /// Used as a base class for other components.
    /// </summary>
    public class Component : IComponent {
        public virtual uint LastPing {
            get;
            set;
        } = 0;
        public virtual int Index {
            get;
            set;
        } = 0;

        public virtual float X {
            get;
            set;
        } = 0;
        public virtual float Y {
            get;
            set;
        } = 0;
        public virtual float Width {
            get;
            set;
        } = 100;
        public virtual float Height {
            get;
            set;
        } = 100;

        public virtual float PrefWidth {
            get;
            set;
        } = 100;
        public virtual float PrefHeight {
            get;
            set;
        } = 100;

        public virtual IParent? Parent {
            get;
            set;
        }

        public virtual RectangleF Clip {
            get => _clip != null ? _clip.Value : Bounds;
            set {
                _clip = value;
            }
        }

        public virtual void UpdatePrefSize() { }
        public virtual void UpdateSetup() { }
        public virtual void UpdateInput() { }
        public virtual void Update() { }
        public virtual void Draw() { }

        /// <summary>
        /// If this component has a parent, it will ask the parent to return this component's previous neighbor.
        /// Otherwise, it will return itself.
        /// </summary>
        public virtual IComponent GetPrev() {
            return Parent == null ? this : Parent.GetPrev(this);
        }
        /// <summary>
        /// If this component has a parent, it will ask the parent to return this component's next neighbor.
        /// Otherwise, it will return itself.
        /// </summary>
        public virtual IComponent GetNext() {
            return Parent == null ? this : Parent.GetNext(this);
        }
        /// <summary>
        /// This function is used by components that manage children.
        /// It will try to return a child that is previous to another component.
        /// </summary>
        public virtual IComponent GetPrev(IComponent c) {
            return GetPrev();
        }
        /// <summary>
        /// This function is used by components that manage children.
        /// It will try to return a child that is next to another component.
        /// </summary>
        public virtual IComponent GetNext(IComponent c) {
            return GetNext();
        }
        /// <summary>
        /// This is used to sink down a component hierarchy from a parent down to a child.
        /// If a parent has children, it will return the first one.
        /// </summary>
        public virtual IComponent GetFirst() {
            return this;
        }
        /// <summary>
        /// This is used to sink down a component hierarchy from a parent down to a child.
        /// If a parent has children, it will return the last one.
        /// </summary>
        public virtual IComponent GetLast() {
            return this;
        }

        public virtual Vector2 XY {
            get => new Vector2(X, Y);
            set {
                X = value.X;
                Y = value.Y;
            }
        }
        public virtual Vector2 Size {
            get => new Vector2(Width, Height);
            set {
                Width = value.X;
                Height = value.Y;
            }
        }
        public virtual Vector2 PrefSize {
            get => new Vector2(PrefWidth, PrefHeight);
            set {
                PrefWidth = value.X;
                PrefHeight = value.Y;
            }
        }
        public virtual RectangleF Bounds {
            get => new RectangleF(XY, Size);
            set {
                XY = value.Position;
                Size = value.Size;
            }
        }
        public virtual float Left {
            get => X;
            set {
                X = value;
            }
        }
        public virtual float Top {
            get => Y;
            set {
                Y = value;
            }
        }
        public virtual float Right {
            get => X + Width;
            set {
                Width = value - Left;
            }
        }
        public virtual float Bottom {
            get => Y + Height;
            set {
                Height = value - Top;
            }
        }

        protected RectangleF? _clip;
    }
}
