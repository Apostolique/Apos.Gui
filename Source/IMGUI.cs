using System;
using System.Collections.Generic;
using System.Linq;
using Apos.Input;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    // NOTE: IMGUI is NOT recursive. It should always be the top level component.
    public class IMGUI : Component, IParent {
        public IMGUI() : base(0) {
            _currentParent = this;
            _activeComponents.Add(Id, this);

            GuiHelper.CurrentIMGUI = this;
        }

        /// <summary>
        /// Only call this if you didn't call UpdateStart. This should be called after UpdatePrefSize.
        /// </summary>
        public override void UpdateSetup(GameTime gameTime) {
            // 1. Ping ourself to prevent cleanup.
            // 2. Cleanup last cycle
            // 3. Pending components become active.
            //      a. Set parenting.
            // 4. Update pref sizes.
            // 5. Apply pref sizes.
            // 6. Update setup.
            LastPing = InputHelper.CurrentFrame;
            _idsUsedThisFrame.Add(Id, 1);

            foreach (var c in _children) {
                c.UpdateSetup(gameTime);
            }
        }
        /// <summary>
        /// Only call this if you didn't call UpdateStart. This should be called after UpdateSetup.
        /// </summary>
        /// <param name="gameTime">Current gametime.</param>
        public override void UpdateInput(GameTime gameTime) {
            for (int i = _childrenRenderOrder.Count - 1; i >= 0; i--) {
                _childrenRenderOrder[i].UpdateInput(gameTime);
            }

            if (!_nextPressed && Default.FocusPrev.Pressed()) {
                _prevPressed = true;
            } else if (_prevPressed) {
                if (Default.FocusPrev.Released()) {
                    FindPrevFocus();
                    _prevPressed = false;
                } else {
                    Default.FocusPrev.Consume();
                }
            }

            if (!_prevPressed && Default.FocusNext.Pressed()) {
                _nextPressed = true;
            } else if (_nextPressed) {
                if (Default.FocusNext.Released()) {
                    FindNextFocus();
                    _nextPressed = false;
                } else {
                    Default.FocusNext.Consume();
                }
            }
        }
        /// <summary>
        /// Only call this if you didn't call UpdateStart. This should be called after UpdateInput.
        /// </summary>
        /// <param name="gameTime">Current gametime</param>
        public override void Update(GameTime gameTime) {
            foreach (var c in _children)
                c.Update(gameTime);
        }

        /// <summary>
        /// Call this at the start of your update loop.
        /// It calls UpdateSetup, UpdateInput, and Update.
        /// </summary>
        /// <param name="gameTime">Current gametime.</param>
        /// <param name="callUpdateInput">false will skip UpdateInput.</param>
        public void UpdateStart(GameTime gameTime, bool callUpdateInput = true) {
            UpdateSetup(gameTime);
            if (callUpdateInput)
                UpdateInput(gameTime);
            Update(gameTime);
        }

        /// <summary>
        /// Don't call this.
        /// </summary>
        public override void UpdatePrefSize(GameTime gameTime) { }
        /// <summary>
        /// Only call this if you didn't call UpdateEnd. This should be called after UpdateInput.
        /// </summary>
        /// <param name="gameTime">Current gametime</param>
        public void UpdateLayout(GameTime gameTime) {
            // IMGUI manages itself so it can set it's own position and size.
            X = 0f;
            Y = 0f;
            Width = GuiHelper.WindowWidth;
            Height = GuiHelper.WindowHeight;

            foreach (var c in _children) {
                c.UpdatePrefSize(gameTime);
                c.Width = c.PrefWidth;
                c.Height = c.PrefHeight;

                c.Clip = c.Bounds.Intersection(Clip);

                if (c is IParent p) {
                    p.UpdateLayout(gameTime);
                }
            }
        }

        /// <summary>
        /// This should be called at the end of your update loop.
        /// </summary>
        public void UpdateEnd(GameTime gameTime) {
            Cleanup();
            UpdateLayout(gameTime);
        }

        /// <summary>
        /// Draws all components in the UI.
        /// </summary>
        /// <param name="gameTime">Current gametime.</param>
        public override void Draw(GameTime gameTime) {
            foreach (var c in _childrenRenderOrder)
                c.Draw(gameTime);
        }

        /// <summary>
        /// Inserts a parent at the top of the parent stack.
        /// </summary>
        /// <param name="p">The parent to insert.</param>
        /// <param name="maxChildren">The parent's max amount of children, 0 means infinite.</param>
        public void Push(IParent p, int maxChildren = 0) {
            _parents.Push((_currentParent, _maxChildren, _childrenCount));
            _currentParent = p;
            _maxChildren = maxChildren;
            _childrenCount = 0;
            PushId(p.Id);
        }
        /// <summary>
        /// Removes a parent from the top of the parent stack.
        /// </summary>
        public void Pop() {
            var pop = _parents.Pop();
            _currentParent = pop.Parent;
            _maxChildren = pop.MaxChildren;
            _childrenCount = pop.ChildrenCount;
            PopId();
        }
        /// <summary>
        /// Inserts an id at the top of the id stack.
        /// </summary>
        /// <param name="id">The id to push onto the id stack.</param>
        public void PushId(int id) {
            _idStack.Push(id);

            ComputeIdStack();
        }
        /// <summary>
        /// Removes an id from the top of the id stack.
        /// </summary>
        public void PopId() {
            int id = _idStack.Pop();

            ComputeIdStack();
        }
        /// <summary>
        /// Tries to get the component with the given id if it exists.
        /// </summary>
        /// <param name="id">The id of the component to get.</param>
        /// <param name="c">When this method returns, contains the component associated with the specified id.</param>
        /// <returns>true if the component with the specified id is found; otherwise false.</returns>
        public bool TryGetValue(int id, out IComponent c) {
            return _activeComponents.TryGetValue(id, out c);
        }
        /// <summary>
        /// Used for parenting a child component to a parent.
        /// If a child already has a parent, it will be adopted by the current parent if the parent is different.
        /// </summary>
        /// <param name="c">The child that will be parented.</param>
        public void GrabParent(IComponent c) {
            IParent current = _currentParent;

            if (c.Parent == null) {
                _activeComponents.Add(c.Id, c);
            }

            // Note: This might be considered a hack. Is it more proper to reset parent components during their LastPing?
            if (c is IParent p) {
                p.Reset();
            }

            if (c.Parent != current || c.Parent.PeekNextIndex() != c.Index) {
                c.Parent?.Remove(c);
                c.Index = current.NextIndex();
                c.GrabFocus = GrabFocus;

                current.Add(c);
            }

            if (_maxChildren > 0 && ++_childrenCount >= _maxChildren) {
                Pop();
            }

            c.LastPing = InputHelper.CurrentFrame;
        }
        /// <summary>
        /// Gives focus to a component. Can also clear the focus if null is passed.
        /// </summary>
        /// <param name="c">The component that should receive focus.</param>
        public new void GrabFocus(IComponent? c) {
            if (c == null) {
                Focus = null;
            } else if (Focus != c.Id) {
                Focus = c.Id;
                c.SendToTop();
            }
        }
        /// <summary>
        /// Guaranteed to return a unique id during the span of the current frame.
        /// </summary>
        /// <param name="id">An id that should be part of the generation process.</param>
        /// <param name="isAbsoluteId">Whether to use the current parent for the id generation.</param>
        public int CreateId(int id, bool isAbsoluteId) {
            if (!isAbsoluteId) {
                id = CombineHash(_idHash, id);
            }

            if (_idsUsedThisFrame.TryGetValue(id, out int count)) {
                count++;
                _idsUsedThisFrame[id] = count;
                id = CombineHash(id, count);
            } else {
                _idsUsedThisFrame.Add(id, 1);
            }

            return id;
        }

        public int TryCreateId(int id, bool isAbsoluteId, out IComponent c) {
            id = GuiHelper.CurrentIMGUI.CreateId(id, isAbsoluteId);
            GuiHelper.CurrentIMGUI.TryGetValue(id, out c);

            return id;
        }

        public void Add(IComponent c) {
            c.Parent = this;
            _children.Insert(c.Index, c);

            // TODO: Optimize this?
            _childrenRenderOrder.Add(c);
            _childrenRenderOrder.Sort((a, b) => {
                if (a.IsFloatable && b.IsFloatable) {
                    return 0;
                } else if (!a.IsFloatable && !b.IsFloatable) {
                    return a.Index.CompareTo(b.Index);
                } else if (a.IsFloatable) {
                    return 1;
                } else {
                    return -1;
                }
            });
        }
        public void Remove(IComponent c) {
            c.Parent = null;
            _children.Remove(c);
            _childrenRenderOrder.Remove(c);
        }
        public void Reset() {
            _nextChildIndex = 0;
        }
        public int PeekNextIndex() => _nextChildIndex + 1;
        public int NextIndex() => _nextChildIndex++;

        /// <summary>
        /// If this component has a parent, it will ask the parent to return this component's previous neighbor.
        /// If it has children, it will return the last one.
        /// Otherwise it will return itself.
        /// </summary>
        public override IComponent GetPrev() {
            return Parent?.GetPrev(this) ?? (_children.Count > 0 ? _children.Last().GetLast() : this);
        }
        /// <summary>
        /// If this component has children, it will return the first one.
        /// If it has a parent it will ask the parent to return this component's next neighbor.
        /// Otherwise, it will return itself.
        /// </summary>
        public override IComponent GetNext() {
            return _children.Count > 0 ? _children.First() : Parent?.GetNext(this) ?? this;
        }
        /// <summary>
        /// If the child isn't the first one, it will return the child before it.
        /// Otherwise it will return itself.
        /// </summary>
        public IComponent GetPrev(IComponent c) {
            int index = c.Index - 1;
            return index >= 0 ? _children[index].GetLast() : this;
        }
        /// <summary>
        /// If the child isn't the last one, it will return the child after it.
        /// If it has a parent, it will ask the parent to return this component's next neighbor.
        /// Otherwise it will return itself.
        /// </summary>
        public IComponent GetNext(IComponent c) {
            int index = c.Index + 1;
            return index < _children.Count ? _children[index] : Parent?.GetNext(this) ?? this;
        }
        /// <summary>
        /// Returns the last child in this component tree.
        /// </summary>
        public override IComponent GetLast() {
            return _children.Count > 0 ? _children.Last().GetLast() : this;
        }

        public void SendToTop(IComponent c) {
            if (c.IsFloatable) {
                _childrenRenderOrder.Remove(c);
                _childrenRenderOrder.Add(c);
            }
        }

        private void Cleanup() {
            Reset();
            foreach (var kc in _activeComponents.Reverse()) {
                if (kc.Value.LastPing != InputHelper.CurrentFrame) {
                    Remove(kc.Key, kc.Value);
                }
            }
            _currentParent = this;
            _maxChildren = 0;
            _childrenCount = 0;
            _parents.Clear();
            _idsUsedThisFrame.Clear();
        }
        private void Remove(int id, IComponent c) {
            if (Focus == id) {
                Focus = null;
            }

            _activeComponents.Remove(id);
            c.Parent?.Remove(c);
        }
        private void FindPrevFocus() {
            FindFocus(ExtractPrev);
        }
        private void FindNextFocus() {
            FindFocus(ExtractNext);
        }
        private IComponent ExtractPrev(int id) => _activeComponents[id].GetPrev();
        private IComponent ExtractNext(int id) => _activeComponents[id].GetNext();
        private void FindFocus(Func<int, IComponent> getNeighbor) {
            int initialFocus = Id;
            if (Focus != null) {
                initialFocus = Focus.Value;
            }
            int newFocus = initialFocus;
            do {
                var c = getNeighbor(newFocus);
                newFocus = c.Id;
                if (c.IsFocusable) {
                    GrabFocus(c);
                    return;
                }
            } while (initialFocus != newFocus);
        }
        private static int CombineHash<T1, T2>(T1 value1, T2 value2) {
            unchecked {
                int hash = 17;
                hash *= 31 + value1!.GetHashCode();
                hash *= 31 + value2!.GetHashCode();

                return hash;
            }
        }
        private void ComputeIdStack() {
            unchecked {
                int hash = 17;

                foreach (var e in _idStack) {
                    hash *= 31 + e.GetHashCode();
                }

                _idHash = hash;
            }
        }

        private Stack<int> _idStack = new Stack<int>();
        private int _idHash = 17;

        private Stack<(IParent Parent, int MaxChildren, int ChildrenCount)> _parents = new Stack<(IParent, int, int)>();
        private Dictionary<int, IComponent> _activeComponents = new Dictionary<int, IComponent>();
        private int? Focus {
            get => _focus;
            set {
                if (_focus != null) {
                    _activeComponents[_focus.Value].IsFocused = false;
                }
                _focus = value;
                if (_focus != null) {
                    _activeComponents[_focus.Value].IsFocused = true;
                }
            }
        }
        private int? _focus;
        private IParent _currentParent;
        private int _maxChildren = 0;
        private int _childrenCount = 0;
        private Dictionary<int, int> _idsUsedThisFrame = new Dictionary<int, int>();

        private bool _prevPressed = false;
        private bool _nextPressed = false;

        private int _nextChildIndex = 0;
        private List<IComponent> _children = new List<IComponent>();
        private List<IComponent> _childrenRenderOrder = new List<IComponent>();
    }
}
