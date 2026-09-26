using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Xunit;

// GuiHelper.CurrentIMGUI is static so tests can't share the process concurrently.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Apos.Gui.Tests {
    public class IdentityTests {
        [Fact]
        public void SameCallsReturnSameComponents() {
            var ui = new IMGUI();
            Label a1 = null!, a2 = null!;

            Frame(ui, () => a1 = Label.Put("A", id: 1));
            Frame(ui, () => a2 = Label.Put("A", id: 1));

            Assert.Same(a1, a2);
        }

        [Fact]
        public void ComponentsThatWerentPutAreRemoved() {
            var ui = new IMGUI();
            Label a = null!;

            Frame(ui, () => {
                a = Label.Put("A", id: 1);
                Label.Put("B", id: 2);
            });
            Frame(ui, () => Label.Put("B", id: 2));

            Assert.False(ui.TryGetValue(a.Id, out _));
            Assert.Null(a.Parent);
            Assert.Equal(["B"], Texts(ui));
        }

        [Fact]
        public void InsertingInTheMiddleKeepsOrder() {
            var ui = new IMGUI();
            Label c1 = null!, c2 = null!;

            Frame(ui, () => {
                Label.Put("A", id: 1);
                c1 = Label.Put("C", id: 3);
            });
            Frame(ui, () => {
                Label.Put("A", id: 1);
                Label.Put("B", id: 2);
                c2 = Label.Put("C", id: 3);
            });

            Assert.Same(c1, c2);
            Assert.Equal(["A", "B", "C"], Texts(ui));
        }

        [Fact]
        public void ReorderingAndAddingKeepsOrder() {
            var ui = new IMGUI();

            Frame(ui, () => {
                Label.Put("A", id: 1);
                Label.Put("B", id: 2);
                Label.Put("C", id: 3);
            });
            Frame(ui, () => {
                Label.Put("C", id: 3);
                Label.Put("B", id: 2);
                Label.Put("D", id: 4);
            });

            Assert.Equal(["C", "B", "D"], Texts(ui));
        }

        [Fact]
        public void IndexMatchesPosition() {
            var ui = new IMGUI();
            var labels = new List<Label>();

            Frame(ui, () => {
                Label.Put("A", id: 1);
                Label.Put("B", id: 2);
                Label.Put("C", id: 3);
                Label.Put("D", id: 4);
            });
            Frame(ui, () => {
                labels.Clear();
                labels.Add(Label.Put("D", id: 4));
                labels.Add(Label.Put("B", id: 2));
                labels.Add(Label.Put("E", id: 5));
            });

            Assert.Equal([0, 1, 2], labels.Select(l => l.Index));
        }

        [Fact]
        public void ChildMovesToAnotherParent() {
            var ui = new IMGUI();
            Vertical v = null!;
            Label a1 = null!, a2 = null!;

            Frame(ui, () => {
                v = Vertical.Push(id: 10);
                a1 = Label.Put("A", id: 1, isAbsoluteId: true);
                Vertical.Pop();
            });
            Frame(ui, () => {
                Vertical.Push(id: 10);
                Vertical.Pop();
                a2 = Label.Put("A", id: 1, isAbsoluteId: true);
            });

            Assert.Same(a1, a2);
            Assert.Same(ui, a2.Parent);
            Assert.Same(v, v.GetLast());
            Assert.Equal(["A"], Texts(ui));
        }

        [Fact]
        public void SameIdWithAnotherTypeReplacesTheComponent() {
            var ui = new IMGUI();
            Label label = null!;
            Icon icon = null!;

            Frame(ui, () => label = Label.Put("A", id: 1));
            Frame(ui, () => icon = Icon.Put(null!, id: 1));

            Assert.Null(label.Parent);
            Assert.Same(ui, icon.Parent);
            Assert.True(ui.TryGetValue(icon.Id, out IComponent? c) && c == icon);
        }

        [Fact]
        public void PopupGoesToTheRootAndBackToTheParent() {
            var ui = new IMGUI();
            Vertical v = null!;
            Popup p = null!;
            Label inside = null!, after = null!;

            Frame(ui, () => {
                v = Vertical.Push(id: 1);
                bool open = true;
                p = Popup.Push(ref open, id: 2);
                inside = Label.Put("In the popup", id: 3);
                Popup.Pop();
                after = Label.Put("Back in the stack", id: 4);
                Vertical.Pop();
            });

            Assert.Same(ui, p.Parent);
            Assert.Same(p, inside.Parent);
            Assert.Same(v, after.Parent);
            Assert.Same(p, RenderOrder(ui).Last());
        }

        [Fact]
        public void FloatingWindowStaysOnTopAfterFocus() {
            var ui = new IMGUI();
            FloatingWindow w1 = null!, w2 = null!;

            void Build() {
                w1 = FloatingWindow.Push(id: 1);
                FloatingWindow.Pop();
                w2 = FloatingWindow.Push(id: 2);
                FloatingWindow.Pop();
            }

            Frame(ui, Build);
            Assert.Same(w2, RenderOrder(ui).Last());

            ui.GrabFocus(w1);
            Assert.Same(w1, RenderOrder(ui).Last());

            Frame(ui, Build);
            Frame(ui, Build);
            Assert.Same(w1, RenderOrder(ui).Last());
        }

        [Fact]
        public void ChildrenOfNeighborParentsDontCollide() {
            // With a linear hash, parents at lines 10 and 11 get consecutive ids, so line 32 under the first
            // and line 1 under the second end up with the same id.
            var ui = new IMGUI();
            Label b1 = null!, b2 = null!;

            Frame(ui, () => {
                Vertical.Push(id: 10);
                Label.Put("a", id: 32);
                Vertical.Pop();
                Vertical.Push(id: 11);
                b1 = Label.Put("b", id: 1);
                Vertical.Pop();
            });
            Frame(ui, () => {
                Vertical.Push(id: 10);
                Vertical.Pop();
                Vertical.Push(id: 11);
                b2 = Label.Put("b", id: 1);
                Vertical.Pop();
            });

            Assert.Same(b1, b2);
        }

        [Fact]
        public void SameLineInAnotherFileDoesntCollide() {
            var ui = new IMGUI();
            Label b1 = null!, b2 = null!;

            Frame(ui, () => {
                Label.Put("a", id: 5, file: "A.cs");
                b1 = Label.Put("b", id: 5, file: "B.cs");
            });
            Frame(ui, () => b2 = Label.Put("b", id: 5, file: "B.cs"));

            Assert.Same(b1, b2);
        }

        [Fact]
        public void PushIdKeepsListItemsWhenOneIsRemoved() {
            var ui = new IMGUI();
            var first = new Dictionary<int, Label>();
            var second = new Dictionary<int, Label>();

            void Build(int[] keys, Dictionary<int, Label> labels) {
                foreach (int key in keys) {
                    ui.PushId(key);
                    labels[key] = Label.Put($"{key}", id: 1);
                    ui.PopId();
                }
            }

            Frame(ui, () => Build([1, 2, 3], first));
            Frame(ui, () => Build([2, 3], second));

            Assert.Same(first[2], second[2]);
            Assert.Same(first[3], second[3]);
        }

        private static void Frame(IMGUI ui, Action build) {
            ui.UpdateSetup(_gameTime);
            build();
            ui.Cleanup();
        }

        /// <summary>Walks the root's direct children in order using the same links focus cycling uses.</summary>
        private static List<string> Texts(IMGUI ui) {
            var result = new List<string>();
            IComponent c = ui.GetNext();
            while (c != ui) {
                if (c is Label l) result.Add(l.Text);
                c = c.GetNext();
            }
            return result;
        }

        private static List<IComponent> RenderOrder(IMGUI ui) {
            var field = typeof(ListParent).GetField("_childrenRenderOrder", BindingFlags.NonPublic | BindingFlags.Instance)!;
            return (List<IComponent>)field.GetValue(ui)!;
        }

        private static readonly GameTime _gameTime = new();
    }
}
