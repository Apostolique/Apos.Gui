using Apos.Input;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Apos.Gui {
    /// <summary>
    /// An empty component that doesn't really do anything.
    /// Used as a base class for other components.
    /// </summary>
    public class Component(long id) : IComponent {
        /// <inheritdoc/>
        public virtual long Id { get; set; } = id;
        /// <inheritdoc/>
        public virtual int Index { get; set; } = 0;
        /// <inheritdoc/>
        public virtual uint LastPing { get; set; } = 0;
        /// <inheritdoc/>
        public virtual uint LastFocus { get; set; }

        /// <inheritdoc/>
        public virtual float X { get; set; } = 0;
        /// <inheritdoc/>
        public virtual float Y { get; set; } = 0;
        /// <inheritdoc/>
        public virtual float Width { get; set; } = 100;
        /// <inheritdoc/>
        public virtual float Height { get; set; } = 100;

        /// <inheritdoc/>
        public virtual float PrefWidth { get; set; } = 100;
        /// <inheritdoc/>
        public virtual float PrefHeight { get; set; } = 100;

        /// <inheritdoc/>
        public virtual bool IsFocused { get; set; } = false;
        /// <inheritdoc/>
        public virtual bool IsFocusable { get; set; } = false;
        /// <inheritdoc/>
        public virtual bool IsFloatable { get; set; } = false;

        /// <inheritdoc/>
        public virtual IParent? Parent { get; set; }

        /// <inheritdoc/>
        public virtual RectangleF Clip {
            get => _clip ?? Bounds;
            set {
                _clip = value;
            }
        }

        /// <inheritdoc/>
        public virtual void UpdateSetup(GameTime gameTime) { }
        /// <inheritdoc/>
        public virtual void UpdateInput(GameTime gameTime) { }
        /// <inheritdoc/>
        public virtual void Update(GameTime gameTime) { }
        /// <inheritdoc/>
        public virtual void UpdatePrefSize(GameTime gameTime) { }
        /// <inheritdoc/>
        public virtual void Draw(GameTime gameTime) { }

        /// <summary>
        /// If this component has a parent, it will ask the parent to return this component's previous neighbor.
        /// Otherwise, it will return itself.
        /// </summary>
        public virtual IComponent GetPrev() {
            return Parent?.GetPrev(this) ?? this;
        }
        /// <summary>
        /// If this component has a parent, it will ask the parent to return this component's next neighbor.
        /// Otherwise, it will return itself.
        /// </summary>
        public virtual IComponent GetNext() {
            return Parent?.GetNext(this) ?? this;
        }
        /// <inheritdoc/>
        public virtual IComponent GetLast() {
            return this;
        }

        /// <inheritdoc/>
        public virtual void SendToTop() {
            LastFocus = Root.CurrentFrame;
            Parent?.SendToTop(this);
        }

        /// <inheritdoc/>
        public virtual IMGUI Root { get; set; } = null!;

        /// <inheritdoc/>
        public virtual Vector2 XY {
            get => new(X, Y);
            set {
                X = value.X;
                Y = value.Y;
            }
        }
        /// <inheritdoc/>
        public virtual Vector2 Size {
            get => new(Width, Height);
            set {
                Width = value.X;
                Height = value.Y;
            }
        }
        /// <inheritdoc/>
        public virtual Vector2 PrefSize {
            get => new(PrefWidth, PrefHeight);
            set {
                PrefWidth = value.X;
                PrefHeight = value.Y;
            }
        }
        /// <inheritdoc/>
        public virtual RectangleF Bounds {
            get => new(XY, Size);
            set {
                XY = value.Position;
                Size = value.Size;
            }
        }
        /// <inheritdoc/>
        public virtual float Left {
            get => X;
            set {
                X = value;
            }
        }
        /// <inheritdoc/>
        public virtual float Top {
            get => Y;
            set {
                Y = value;
            }
        }
        /// <inheritdoc/>
        public virtual float Right {
            get => X + Width;
            set {
                Width = value - Left;
            }
        }
        /// <inheritdoc/>
        public virtual float Bottom {
            get => Y + Height;
            set {
                Height = value - Top;
            }
        }

        /// <summary>Backs <see cref="Clip"/>. Null makes the clip follow <see cref="Bounds"/>.</summary>
        protected RectangleF? _clip;
    }
}
