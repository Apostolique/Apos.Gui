using System;
using System.Collections.Generic;
using System.Linq;
using Apos.Input;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    // TODO: Make IMGUI implement IParent?
    public class IMGUI {
        public void UpdateSetup(GameTime gameTime) {
            // 1. Cleanup last cycle
            // 2. Pending components become active.
            //      a. Set parenting.
            //      b. No parent means root parent.
            // 3. Update pref sizes.
            // 4. Apply pref sizes.
            // 5. Update setup.
            Cleanup();
            while (PendingComponents.Count > 0) {
                var pc = PendingComponents.Dequeue();
                ActiveComponents.Add(pc.Id, pc.Component);
                if (pc.Parent != null) {
                    pc.Parent.Add(pc.Component);
                } else {
                    Roots.Add(pc.Component);
                }
            }

            // TODO: Process pending action queue. (It doesn't exist yet.)

            foreach (var c in Roots) {
                c.UpdatePrefSize(gameTime);
                // TODO: Update position?
                c.Width = c.PrefWidth;
                c.Height = c.PrefHeight;
                c.UpdateSetup(gameTime);
            }
        }
        public void UpdateInput(GameTime gameTime) {
            foreach (var c in Roots)
                c.UpdateInput(gameTime);

            // TODO: Need to handle the whole lifecycle of FocusPrev and FocusNext, same as buttons. (Pressed, HeldOnly, Released)
            if (Default.FocusPrev.Released()) {
                FindPrevFocus();
            }
            if (Default.FocusNext.Released()) {
                FindNextFocus();
            }
        }
        public void Update(GameTime gameTime) {
            foreach (var c in Roots)
                c.Update(gameTime);
        }
        public void UpdateAll(GameTime gameTime) {
            UpdateSetup(gameTime);
            UpdateInput(gameTime);
            Update(gameTime);
        }
        public void Draw(GameTime gameTime) {
            foreach (var c in Roots)
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
        public void Add(int id, IComponent c) {
            // NOTE: This should only be called if the component hasn't already been added.
            PendingComponents.Enqueue((id, CurrentParent, c));
            c.GrabFocus = GrabFocus;
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
        public IParent? GrabParent() {
            ChildrenCount++;

            IParent? current = CurrentParent;

            if (CurrentParent != null && MaxChildren > 0 && ChildrenCount >= MaxChildren) {
                Pop();
            }

            return current;
        }
        private void Cleanup() {
            foreach (var kc in ActiveComponents.Reverse()) {
                if (kc.Value.LastPing != InputHelper.CurrentFrame - 1) {
                    Remove(kc.Key, kc.Value);
                }
            }
            CurrentParent = null;
            MaxChildren = 0;
            ChildrenCount = 0;
            Parents.Clear();
        }
        private void Remove(int id, IComponent c) {
            if (_focus == id) {
                _focus = null;
            }

            ActiveComponents.Remove(id);
            if (c.Parent != null) {
                c.Parent.Remove(c);
            } else {
                Roots.Remove(c);
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
            int? initialFocus = null;
            if (Focus != null) {
                initialFocus = Focus;
            } else if (Roots.Count > 0) {
                // TODO: Figure out what should be done if there are multiple Roots.
                //       This is why it might be a good idea for IMGUI to implement IParent.
                initialFocus = Roots.First().Id;
            }
            if (initialFocus != null) {
                int newFocus = initialFocus.Value;
                do {
                    var c = getNeighbor(newFocus);
                    newFocus = c.Id;
                    if (c.IsFocusable) {
                        Focus = newFocus;
                        break;
                    }
                } while (initialFocus != newFocus);
            }
        }
        public void GrabFocus(IComponent? c) {
            if (c == null) {
                Focus = null;
            } else {
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

        private Stack<int> IdStack = new Stack<int>();

        private Stack<(IParent? Parent, int MaxChildren, int ChildrenCount)> Parents = new Stack<(IParent?, int, int)>();
        private List<IComponent> Roots = new List<IComponent>();
        private Dictionary<int, IComponent> ActiveComponents = new Dictionary<int, IComponent>();
        private Queue<(int Id, IParent? Parent, IComponent Component)> PendingComponents = new Queue<(int, IParent?, IComponent)>();
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
        private IParent? CurrentParent = null;
        private int MaxChildren = 0;
        private int ChildrenCount = 0;
    }
}
