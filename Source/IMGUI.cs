using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    // NOTE: IMGUI is NOT recursive. It should always be the top level component.
    public class IMGUI : ListParent {
        public IMGUI() : base(0) {
            Root = this;
            _currentParent = this;
            _activeComponents.Add(Id, this);

            GuiHelper.CurrentIMGUI = this;
        }

        /// <summary>
        /// Counts the UI's frames. Starts a new frame in UpdateSetup. Components that weren't put during the
        /// current frame are removed in Cleanup.
        /// </summary>
        public uint CurrentFrame { get; private set; } = 0;

        /// <summary>
        /// Only call this if you didn't call UpdateStart. Starts a new frame.
        /// </summary>
        public override void UpdateSetup(GameTime gameTime) {
            CurrentFrame++;
            LastPing = CurrentFrame;
            _idsUsedThisFrame.Add(Id, 1);

            base.UpdateSetup(gameTime);
        }
        /// <summary>
        /// Only call this if you didn't call UpdateStart. This should be called after UpdateSetup.
        /// </summary>
        /// <param name="gameTime">Current gametime.</param>
        public override void UpdateInput(GameTime gameTime) {
            base.UpdateInput(gameTime);

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
            base.Update(gameTime);
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
        /// Only call this if you didn't call UpdateEnd. This should be called after Cleanup.
        /// </summary>
        /// <param name="gameTime">Current gametime</param>
        public override void UpdateLayout(GameTime gameTime) {
            // IMGUI manages itself so it can set its own position and size.
            X = 0f;
            Y = 0f;
            Width = GuiHelper.WindowWidth;
            Height = GuiHelper.WindowHeight;

            foreach (var c in _children) {
                c.UpdatePrefSize(gameTime);
            }
            base.UpdateLayout(gameTime);
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
            GuiHelper.Begin();
            base.Draw(gameTime);
            GuiHelper.End();
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
            int _ = _idStack.Pop();

            ComputeIdStack();
        }
        /// <summary>
        /// Tries to get the component with the given id if it exists.
        /// </summary>
        /// <param name="id">The id of the component to get.</param>
        /// <param name="c">When this method returns, contains the component associated with the specified id.</param>
        /// <returns>true if the component with the specified id is found; otherwise false.</returns>
        public bool TryGetValue(int id, out IComponent? c) {
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

            if (c.Parent != current || current.PeekNextIndex() != c.Index) {
                c.Parent?.Remove(c);
                c.Index = current.NextIndex();
                c.Root = this;

                current.Add(c);
            } else {
                // Same parent, same slot as last frame. Leave it in place so that its render order is kept.
                current.NextIndex();
            }

            if (_maxChildren > 0 && ++_childrenCount >= _maxChildren) {
                Pop();
            }

            c.LastPing = CurrentFrame;
        }
        /// <summary>
        /// Gives focus to a component. Can also clear the focus if null is passed.
        /// </summary>
        /// <param name="c">The component that should receive focus.</param>
        public void GrabFocus(IComponent? c) {
            if (c == null) {
                Focus = null;
            } else if (Focus != c.Id) {
                Focus = c.Id;
                c.SendToTop();
            }
        }
        /// <summary>
        /// Claims the mouse hover for this frame. Components check from the top of the draw order down,
        /// so the first one under the mouse gets it and the ones below it don't.
        /// </summary>
        /// <returns>True if the component got the hover or already had it this frame.</returns>
        public bool TryClaimHover(IComponent c) {
            if (_hoverFrame == CurrentFrame) {
                return _hover == c;
            }
            _hover = c;
            _hoverFrame = CurrentFrame;
            return true;
        }
        /// <summary>The component under the mouse this frame, if one claimed it.</summary>
        public IComponent? Hover => _hoverFrame == CurrentFrame ? _hover : null;
        /// <summary>The component holding a mouse press. While it's set, other components don't get hovered.</summary>
        public IComponent? MouseCapture { get; set; }

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

        public static int TryCreateId(int id, bool isAbsoluteId, out IComponent? c) {
            id = GuiHelper.CurrentIMGUI.CreateId(id, isAbsoluteId);
            GuiHelper.CurrentIMGUI.TryGetValue(id, out c);

            return id;
        }

        /// <summary>
        /// Gets the component that was put with this id last frame, or creates it, then parents it to the current parent.
        /// This is what a component's static Put method is made of.
        /// </summary>
        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="create">Creates the component when there isn't one yet. Gets the final id.</param>
        public static T Put<T>(int id, bool isAbsoluteId, Func<int, T> create) where T : IComponent {
            IMGUI ui = GuiHelper.CurrentIMGUI;
            id = TryCreateId(id, isAbsoluteId, out IComponent? c);

            if (c is not T a) {
                if (c != null) {
                    // A different kind of component had this id last frame.
                    ui.Remove(id, c);
                }
                a = create(id);
            }

            ui.GrabParent(a);
            return a;
        }
        /// <summary>
        /// Same as <see cref="Put{T}"/> for a parent. The components put after it become its children until it's popped.
        /// </summary>
        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="create">Creates the component when there isn't one yet. Gets the final id.</param>
        /// <param name="maxChildren">Pops the parent by itself after this many children. 0 means no limit.</param>
        public static T Push<T>(int id, bool isAbsoluteId, Func<int, T> create, int maxChildren = 0) where T : IParent {
            T a = Put(id, isAbsoluteId, create);
            GuiHelper.CurrentIMGUI.Push(a, maxChildren);
            return a;
        }

        public override uint LastFocus { get; set; } = uint.MaxValue;

        /// <summary>
        /// Only call this if you didn't call UpdateEnd. Removes the components that weren't put during this frame.
        /// This should be called after all your components were put, before UpdateLayout.
        /// </summary>
        public void Cleanup() {
            Reset();
            foreach (var kc in _activeComponents.Reverse()) {
                if (kc.Value.LastPing != CurrentFrame) {
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
            if (MouseCapture == c) {
                MouseCapture = null;
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
                hash = hash * 31 + value1!.GetHashCode();
                hash = hash * 31 + value2!.GetHashCode();

                return hash;
            }
        }
        private void ComputeIdStack() {
            unchecked {
                int hash = 17;

                foreach (var e in _idStack) {
                    hash = hash * 31 + e.GetHashCode();
                }

                _idHash = hash;
            }
        }

        private readonly Stack<int> _idStack = new();
        private int _idHash = 17;

        private readonly Stack<(IParent Parent, int MaxChildren, int ChildrenCount)> _parents = new();
        private readonly Dictionary<int, IComponent> _activeComponents = [];
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
        private IComponent? _hover;
        private uint _hoverFrame;
        private IParent _currentParent;
        private int _maxChildren = 0;
        private int _childrenCount = 0;
        private readonly Dictionary<int, int> _idsUsedThisFrame = [];

        private bool _prevPressed = false;
        private bool _nextPressed = false;
    }
}
