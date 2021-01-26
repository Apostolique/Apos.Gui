using System.Collections.Generic;
using System.Linq;
using Apos.Input;

namespace Apos.Gui {
    // TODO: Ping components to keep them alive.
    public class IMGUI {
        public void UpdateSetup() {
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
                ActiveComponents.Add(pc.Name, pc.Component);
                if (pc.Parent != null) {
                    pc.Parent.Add(pc.Component);
                } else {
                    RootParents.Add(pc.Component);
                }
            }

            foreach (var c in RootParents) {
                c.UpdatePrefSize();
                // TODO: Update position?
                c.Width = c.PrefWidth;
                c.Height = c.PrefHeight;
                c.UpdateSetup();
            }
        }
        public void UpdateInput() {
            foreach (var c in RootParents)
                c.UpdateInput();
        }
        public void Update() {
            foreach (var c in RootParents)
                c.Update();
        }
        public void Draw() {
            foreach (var c in RootParents)
                c.Draw();
        }

        public void Push(IParent p) {
            if (CurrentParent != null) {
                Parents.Push(CurrentParent);
            }
            CurrentParent = p;
        }
        public void Pop() {
            if (Parents.Count > 0) {
                CurrentParent = Parents.Pop();
            } else {
                CurrentParent = null;
            }
        }
        public void Add(string name, Component c) {
            if (!ActiveComponents.TryGetValue(name, out Component current)) {
                PendingComponents.Enqueue((name, CurrentParent, c));
            }
        }
        public bool TryGetValue(string name, out Component c) {
            if (ActiveComponents.TryGetValue(name, out c)) {
                return true;
            }

            foreach (var pc in PendingComponents) {
                if (pc.Name == name) {
                    c = pc.Component;
                    return true;
                }
            }

            return false;
        }

        public IParent? CurrentParent;
        private Stack<IParent> Parents = new Stack<IParent>();
        private List<Component> RootParents = new List<Component>();
        private Dictionary<string, Component> ActiveComponents = new Dictionary<string, Component>();
        private Queue<(string Name, IParent? Parent, Component Component)> PendingComponents = new Queue<(string, IParent?, Component)>();

        private void Cleanup() {
            foreach (var kc in ActiveComponents.Reverse()) {
                if (kc.Value.LastPing != InputHelper.CurrentFrame - 1) {
                    Remove(kc.Key, kc.Value);
                }
            }
            CurrentParent = null;
            Parents.Clear();
        }
        private void Remove(string name, Component c) {
            ActiveComponents.Remove(name);
            if (c.Parent != null) {
                c.Parent.Remove(c);
            } else {
                RootParents.Remove(c);
            }
            // TODO: Remove from PendingComponents?
        }
    }
}
