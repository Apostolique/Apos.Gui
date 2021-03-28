using System;
using System.Collections.Generic;
using System.Linq;
using Apos.Input;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    // NOTE: IMGUI is NOT recursive. It should always be the top level component.
    public class IMGUI : Panel {
        public IMGUI() : base(0) {
            CurrentParent = this;
            ActiveComponents.Add(Id, this);

            GuiHelper.CurrentIMGUI = this;
        }

        public override void UpdatePrefSize(GameTime gameTime) { }
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
            while (PendingComponents.Count > 0) {
                var pc = PendingComponents.Dequeue();
                if (pc.Component.Parent == null) {
                    ActiveComponents.Add(pc.Id, pc.Component);
                } else {
                    pc.Component.Parent.Remove(pc.Component);
                }
                pc.Parent.Add(pc.Component);
                pc.Component.GrabFocus = GrabFocus;
            }

            while (_nextTick.Count > 0) {
                _nextTick.Dequeue().Invoke();
            }

            foreach (var c in _children) {
                c.UpdatePrefSize(gameTime);
                // TODO: Update position?
                c.Width = c.PrefWidth;
                c.Height = c.PrefHeight;
                c.UpdateSetup(gameTime);
            }
        }
        public override void UpdateInput(GameTime gameTime) {
            foreach (var c in _children)
                c.UpdateInput(gameTime);

            if (!_nextPressed && Default.FocusPrev.Pressed()) {
                _prevPressed = true;
            }
            if (_prevPressed) {
                if (Default.FocusPrev.Released()) {
                    FindPrevFocus();
                    _prevPressed = false;
                } else {
                    Default.FocusPrev.Consume();
                }
            }

            if (!_prevPressed && Default.FocusNext.Pressed()) {
                _nextPressed = true;
            }
            if (_nextPressed) {
                if (Default.FocusNext.Released()) {
                    FindNextFocus();
                    _nextPressed = false;
                } else {
                    Default.FocusNext.Consume();
                }
            }
        }
        public override void Update(GameTime gameTime) {
            foreach (var c in _children)
                c.Update(gameTime);
        }
        public void UpdateAll(GameTime gameTime) {
            UpdateSetup(gameTime);
            UpdateInput(gameTime);
            Update(gameTime);
        }
        public override void Draw(GameTime gameTime) {
            foreach (var c in _children)
                c.Draw(gameTime);
        }

        public void Push(IParent p, int maxChildren = 0) {
            Parents.Push((CurrentParent, MaxChildren, ChildrenCount));
            CurrentParent = p;
            MaxChildren = maxChildren;
            ChildrenCount = 0;
            PushId(p.Id);
        }
        public void Pop() {
            var pop = Parents.Pop();
            CurrentParent = pop.Parent;
            MaxChildren = pop.MaxChildren;
            ChildrenCount = pop.ChildrenCount;
            PopId();
        }
        public void PushId(int id) {
            IdStack.Push(id);
            // TODO: Compute the top id.
        }
        public void PopId() {
            IdStack.Pop();
            // TODO: Compute the top id.
        }
        public bool TryGetValue(int id, out IComponent c) {
            if (ActiveComponents.TryGetValue(id, out c)) {
                return true;
            }

            foreach (var pc in PendingComponents) {
                if (pc.Id == id) {
                    c = pc.Component;
                    return true;
                }
            }

            return false;
        }
        public IParent? GrabParent(IComponent c) {
            IParent current = CurrentParent;

            if (c.Parent != current) {
                PendingComponents.Enqueue((c.Id, CurrentParent, c));
            }

            ChildrenCount++;

            if (MaxChildren > 0 && ChildrenCount >= MaxChildren) {
                Pop();
            }

            return current;
        }
        public void GrabFocus(IComponent? c) {
            if (c == null) {
                Focus = null;
            } else if (Focus != c.Id) {
                Focus = c.Id;
            }
        }
        public int GetIdStack() {
            // TODO: We can precompute the top id.
            unchecked {
                int hash = 17;

                foreach (var e in IdStack) {
                    hash *= 31 + e.GetHashCode();
                }

                return hash;
            }
        }
        /// <summary>
        /// Garenteed to return a unique id during the span of the current frame.
        /// </summary>
        /// <param name="id">An id that should be part of the generation process.</param>
        /// <param name="isAbsoluteId">Whether to use the current parent for the id generation.</param>
        public int CreateId(int id, bool isAbsoluteId) {
            if (!isAbsoluteId) {
                id = CombineHash(GetIdStack(), id);
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
        public void QueueNextTick(Action a) {
            _nextTick.Enqueue(a);
        }

        private void Cleanup() {
            Reset();
            foreach (var kc in ActiveComponents.Reverse()) {
                if (kc.Value.LastPing != InputHelper.CurrentFrame - 1) {
                    Remove(kc.Key, kc.Value);
                }
            }
            CurrentParent = this;
            MaxChildren = 0;
            ChildrenCount = 0;
            Parents.Clear();
            _idsUsedThisFrame.Clear();
        }
        private void Remove(int id, IComponent c) {
            if (Focus == id) {
                Focus = null;
            }

            ActiveComponents.Remove(id);
            if (c.Parent != null) {
                c.Parent.Remove(c);
            } else {
                _children.Remove(c);
            }
            // TODO: Remove from PendingComponents? Probably not since that case can't happen?
        }
        private void FindPrevFocus() {
            FindFocus(ExtractPrev);
        }
        private void FindNextFocus() {
            FindFocus(ExtractNext);
        }
        private IComponent ExtractPrev(int id) => ActiveComponents[id].GetPrev();
        private IComponent ExtractNext(int id) => ActiveComponents[id].GetNext();
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
                    Focus = newFocus;
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

        private Stack<int> IdStack = new Stack<int>();

        private Stack<(IParent Parent, int MaxChildren, int ChildrenCount)> Parents = new Stack<(IParent, int, int)>();
        private Dictionary<int, IComponent> ActiveComponents = new Dictionary<int, IComponent>();
        private Queue<(int Id, IParent Parent, IComponent Component)> PendingComponents = new Queue<(int, IParent, IComponent)>();
        private int _lastId = 0;
        private int? Focus {
            get => _focus;
            set {
                if (_focus != null) {
                    ActiveComponents[_focus.Value].IsFocused = false;
                }
                _focus = value;
                if (_focus != null) {
                    ActiveComponents[_focus.Value].IsFocused = true;
                }
            }
        }
        private int? _focus;
        private IParent CurrentParent;
        private int MaxChildren = 0;
        private int ChildrenCount = 0;
        private Dictionary<int, int> _idsUsedThisFrame = new Dictionary<int, int>();

        private bool _prevPressed = false;
        private bool _nextPressed = false;

        private Queue<Action> _nextTick = new Queue<Action>();
    }
}
