using Microsoft.Xna.Framework;

namespace Apos.Gui {
    /// <summary>
    /// A parent with at most one child. By default the child gets the parent's size, and the parent prefers the child's size.
    /// Override UpdatePrefSize and UpdateLayout to place the child differently.
    /// </summary>
    public class SingleParent(long id) : Component(id), IParent {
        public IComponent? Child { get; set; }

        public override void UpdateSetup(GameTime gameTime) {
            Child?.UpdateSetup(gameTime);
        }
        public override void UpdateInput(GameTime gameTime) {
            Child?.UpdateInput(gameTime);
        }
        public override void Update(GameTime gameTime) {
            Child?.Update(gameTime);
        }

        public override void UpdatePrefSize(GameTime gameTime) {
            if (Child != null) {
                Child.UpdatePrefSize(gameTime);

                PrefWidth = Child.PrefWidth;
                PrefHeight = Child.PrefHeight;
            }
        }
        public virtual void UpdateLayout(GameTime gameTime) {
            if (Child != null) {
                Child.Bounds = Bounds;
                LayoutChild(gameTime);
            }
        }
        /// <summary>
        /// Clips the child to this parent and lays out the child's own children. Call it after placing the child.
        /// </summary>
        protected void LayoutChild(GameTime gameTime) {
            if (Child != null) {
                Child.Clip = Child.Bounds.Intersect(Clip);

                if (Child is IParent p) {
                    p.UpdateLayout(gameTime);
                }
            }
        }

        public override void Draw(GameTime gameTime) {
            Child?.Draw(gameTime);
        }

        public virtual void Add(IComponent c) {
            if (c != Child) {
                if (Child != null) {
                    Child.Parent = null;
                }
                Child = c;
                Child.Parent = this;
            }
        }
        public virtual void Remove(IComponent c) {
            if (Child == c) {
                Child.Parent = null;
                Child = null;
            }
        }
        public virtual void Reset() { }
        public virtual int PeekNextIndex() => 0;
        public virtual int NextIndex() => 0;

        public override IComponent GetPrev() {
            return Parent?.GetPrev(this) ?? Child?.GetLast() ?? this;
        }
        public override IComponent GetNext() {
            return Child ?? Parent?.GetNext(this) ?? this;
        }
        public virtual IComponent GetPrev(IComponent c) {
            return this;
        }
        public virtual IComponent GetNext(IComponent c) {
            return Parent?.GetNext(this) ?? this;
        }
        public override IComponent GetLast() {
            return Child?.GetLast() ?? this;
        }

        public virtual void SendToTop(IComponent c) {
            LastFocus = Root.CurrentFrame;
            Parent?.SendToTop(this);
        }

        public virtual bool IsFocusScope { get; set; } = false;
        public virtual IComponent? Navigate(IComponent from, Direction direction) => null;
    }
}
