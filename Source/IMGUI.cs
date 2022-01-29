using System;
using System.Collections.Generic;
using System.Linq;
using Apos.Input;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    // NOTE: IMGUI is NOT recursive. It should always be the top level component.
    public class IMGUI : Panel {
        public IMGUI() : base(0) {
            _currentParent = this;
            _activeComponents.Add(Id, this);

            GuiHelper.CurrentIMGUI = this;
        }

        /// <summary>
        /// Only call this if you didn't call UpdateAll. This should be called at the start of your update loop.
        /// </summary>
        public override void UpdatePrefSize(GameTime gameTime) { }
        /// <summary>
        /// Only call this if you didn't call UpdateAll. This should be called after UpdatePrefSize.
        /// </summary>
        public override void UpdateSetup(GameTime gameTime) {
            // 1. Ping ourself to prevent cleanup.
            // 2. Cleanup last cycle
            // 3. Pending components become active.
            //      a. Set parenting.
            // 4. Update pref sizes.
            // 5. Apply pref sizes.
            // 6. Update setup.
            LastPing = InputHelper.CurrentFrame - 1;
            Cleanup();
            _idsUsedThisFrame.Add(Id, 1);
            while (_pendingComponents.Count > 0) {
                var pc = _pendingComponents.Dequeue();
                if (pc.Component.Parent == null) {
                    _activeComponents.Add(pc.Id, pc.Component);
                } else {
                    pc.Component.Parent.Remove(pc.Component);
                }
                pc.Parent.Add(pc.Component);
                pc.Component.GrabFocus = GrabFocus;
            }

            _isTick0 = !_isTick0;
            if (!_isTick0) {
                while (_nextTick0.Count > 0) {
                    _nextTick0.Dequeue().Invoke();
                }
            } else {
                while (_nextTick1.Count > 0) {
                    _nextTick1.Dequeue().Invoke();
                }
            }

            X = 0f;
            Y = 0f;
            Width = GuiHelper.WindowWidth;
            Height = GuiHelper.WindowHeight;

            foreach (var c in _children) {
                c.UpdatePrefSize(gameTime);
                c.Width = c.PrefWidth;
                c.Height = c.PrefHeight;

                c.Clip = Bounds;

                c.UpdateSetup(gameTime);
            }
        }
        /// <summary>
        /// Only call this if you didn't call UpdateAll. This should be called after UpdateSetup.
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
        /// Only call this if you didn't call UpdateAll. This should be called after UpdateInput.
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
        public void UpdateAll(GameTime gameTime, bool callUpdateInput = true) {
            UpdateSetup(gameTime);
            if (callUpdateInput)
                UpdateInput(gameTime);
            Update(gameTime);
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
        public new void Pop() {
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
            if (_activeComponents.TryGetValue(id, out c)) {
                return true;
            }

            // TODO: Verify that this is still required.
            //       This is usually called after CreateId which will always return a new unique id for this frame.
            foreach (var pc in _pendingComponents) {
                if (pc.Id == id) {
                    c = pc.Component;
                    return true;
                }
            }

            return false;
        }
        /// <summary>
        /// Returns the current top parent. Used for parenting a child component to a parent.
        /// If a child already has a parent, it will be marked for a parent change.
        /// </summary>
        /// <param name="c">The child that will be parented.</param>
        public IParent GrabParent(IComponent c) {
            IParent current = _currentParent;

            if (c.Parent != current) {
                _pendingComponents.Enqueue((c.Id, _currentParent, c));
            }

            if (_maxChildren > 0 && ++_childrenCount >= _maxChildren) {
                Pop();
            }

            return current;
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
        /// <summary>
        /// Used when an action would invalidate the layout which would lead to an invalid draw (flicker).
        /// Using this, you can delay the action until the next UpdateSetup.
        /// </summary>
        /// <param name="a">The action that will be enqueued.</param>
        public void QueueNextTick(Action a) {
            if (_isTick0) {
                _nextTick0.Enqueue(a);
            } else {
                _nextTick1.Enqueue(a);
            }
        }

        private void Cleanup() {
            Reset();
            foreach (var kc in _activeComponents.Reverse()) {
                if (kc.Value.LastPing != InputHelper.CurrentFrame - 1) {
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
            // TODO: Remove from PendingComponents? Probably not since that case can't happen?
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
        private Queue<(int Id, IParent Parent, IComponent Component)> _pendingComponents = new Queue<(int, IParent, IComponent)>();
        private int _lastId = 0;
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

        private bool _isTick0 = true;
        private Queue<Action> _nextTick0 = new Queue<Action>();
        private Queue<Action> _nextTick1 = new Queue<Action>();
    }
}
