using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    /// <summary>
    /// A parent with any number of children, kept in the order they were put.
    /// Children draw in that order, except floatables which stay on top in the order they were brought there.
    /// By default each child keeps its own position and gets its preferred size. Override UpdatePrefSize and UpdateLayout to arrange them.
    /// </summary>
    public class ListParent(long id) : Component(id), IParent {
        public override void UpdateSetup(GameTime gameTime) {
            foreach (var c in _children)
                c.UpdateSetup(gameTime);
        }
        public override void UpdateInput(GameTime gameTime) {
            for (int i = _childrenRenderOrder.Count - 1; i >= 0; i--) {
                _childrenRenderOrder[i].UpdateInput(gameTime);
            }
        }
        public override void Update(GameTime gameTime) {
            foreach (var c in _children)
                c.Update(gameTime);
        }

        public override void UpdatePrefSize(GameTime gameTime) {
            float maxWidth = 0;
            float maxHeight = 0;

            foreach (var c in _children) {
                c.UpdatePrefSize(gameTime);
                maxWidth = MathHelper.Max(c.PrefWidth, maxWidth);
                maxHeight = MathHelper.Max(c.PrefHeight, maxHeight);
            }

            PrefWidth = maxWidth;
            PrefHeight = maxHeight;
        }
        public virtual void UpdateLayout(GameTime gameTime) {
            foreach (var c in _children) {
                c.Width = c.PrefWidth;
                c.Height = c.PrefHeight;
                LayoutChild(c, gameTime);
            }
        }
        /// <summary>
        /// Clips a child to this parent and lays out the child's own children. Call it after placing the child.
        /// </summary>
        protected void LayoutChild(IComponent c, GameTime gameTime) {
            c.Clip = c.Bounds.Intersect(Clip);

            if (c is IParent p) {
                p.UpdateLayout(gameTime);
            }
        }

        /// <summary>
        /// Corner radius of the clip the children draw in. Only used while this parent is fully visible,
        /// since a clip that was cut by a parent has straight corners where it was cut.
        /// </summary>
        public float ClipRounding { get; set; } = 0f;

        public override void Draw(GameTime gameTime) {
            // The clip stack cuts the bounds down to what the parents leave visible.
            GuiHelper.PushClip(Bounds, ClipRounding);
            foreach (var c in _childrenRenderOrder) {
                if (Clip.Intersects(c.Clip)) {
                    c.Draw(gameTime);
                }
            }
            GuiHelper.PopClip();
        }

        public virtual void Add(IComponent c) {
            c.Parent = this;
            ChildList.Insert(_children, _childrenRenderOrder, c);
        }
        public virtual void Remove(IComponent c) {
            c.Parent = null;
            ChildList.Remove(_children, _childrenRenderOrder, c);
        }
        public virtual void Reset() {
            _nextChildIndex = 0;
        }
        public virtual int PeekNextIndex() => _nextChildIndex;
        public virtual int NextIndex() => _nextChildIndex++;

        /// <summary>
        /// If this component has a parent, it will ask the parent to return this component's previous neighbor.
        /// If it has children, it will return the last one.
        /// Otherwise it will return itself.
        /// </summary>
        public override IComponent GetPrev() {
            return Parent?.GetPrev(this) ?? (_children.Count > 0 ? _children[^1].GetLast() : this);
        }
        /// <summary>
        /// If this component has children, it will return the first one.
        /// If it has a parent it will ask the parent to return this component's next neighbor.
        /// Otherwise, it will return itself.
        /// </summary>
        public override IComponent GetNext() {
            return _children.Count > 0 ? _children[0] : Parent?.GetNext(this) ?? this;
        }
        /// <summary>
        /// If the child isn't the first one, it will return the child before it.
        /// Otherwise it will return itself.
        /// </summary>
        public virtual IComponent GetPrev(IComponent c) {
            int index = c.Index - 1;
            return index >= 0 ? _children[index].GetLast() : this;
        }
        /// <summary>
        /// If the child isn't the last one, it will return the child after it.
        /// If it has a parent, it will ask the parent to return this component's next neighbor.
        /// Otherwise it will return itself.
        /// </summary>
        public virtual IComponent GetNext(IComponent c) {
            int index = c.Index + 1;
            return index < _children.Count ? _children[index] : Parent?.GetNext(this) ?? this;
        }
        /// <summary>
        /// Returns the last child in this component tree.
        /// </summary>
        public override IComponent GetLast() {
            return _children.Count > 0 ? _children[^1].GetLast() : this;
        }

        public virtual void SendToTop(IComponent c) {
            LastFocus = Root.CurrentFrame;
            if (c.IsFloatable) {
                _childrenRenderOrder.Remove(c);
                _childrenRenderOrder.Add(c);
            }

            Parent?.SendToTop(this);
        }

        public virtual bool IsFocusScope { get; set; } = false;
        public virtual IComponent? Navigate(IComponent from, Direction direction) => null;

        protected int _nextChildIndex = 0;
        protected List<IComponent> _children = [];
        protected List<IComponent> _childrenRenderOrder = [];
    }
}
