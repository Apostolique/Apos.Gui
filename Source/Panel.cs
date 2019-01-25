using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace AposGui {
    /// <summary>
    /// Goal: Container that can hold Components.
    /// </summary>
    public class Panel : Component {
        //constructors
        public Panel() : this(new Layout()) {
        }
        public Panel(Layout l) {
            children = new List<Component>();
            Layout = l;
            Offset = new Point(0, 0);
            Size = new Size2(0, 0);
        }

        //public vars
        public Point Offset {
            get;
            set;
        }
        public Size2 Size {
            get;
            set;
        }
        public Layout Layout {
            get => _layout;
            set {
                _layout = value;
                _layout.Panel = this;
            }
        }

        //public functions
        public virtual void Add(Component e) {
            children.Add(e);
            e.Parent = this;
        }
        public virtual void Remove(Component e) {
            children.Remove(e);
            e.Parent = null;
        }
        public override Component GetPrevious(Component c) {
            int index = children.IndexOf(c) - 1;
            if (index >= 0 && children.Count > 0) {
                return children[index];
            } else if (Parent != null) {
                return Parent.GetPrevious(this);
            } else if (children.Count > 0) {
                return children.Last();
            }
            return this;
        }
        public override Component GetNext(Component c) {
            int index = children.IndexOf(c) + 1;
            if (index < children.Count) {
                return children[index];
            } else if (Parent != null) {
                return Parent.GetNext(this);
            } else if (children.Count > 0) {
                return children[0];
            }
            return this;
        }
        public override Component GetFinal() {
            if (children.Count > 0) {
                return children.First();
            }
            return this;
        }
        public override Component GetFinalInverse() {
            if (children.Count > 0) {
                return children.Last();
            }
            return this;
        }
        public override void UpdateSetup() {
            if (Layout != null) {
                Layout.RecomputeChildren(children);
            }
            foreach (Component e in children) {
                e.UpdateSetup();
            }
        }
        public override bool UpdateInput() {
            bool isUsed = false;

            for (int i = children.Count - 1; i >= 0; i--) {
                if (children[i].UpdateInput()) {
                    isUsed = true;
                    break;
                }
            }
            if (!isUsed) {
                isUsed = base.UpdateInput();
            }
            return isUsed;
        }
        public override void Update() {
            base.Update();
            foreach (Component e in children) {
                e.Update();
            }
        }
        public override void Draw(SpriteBatch s) {
            foreach (Component e in children) {
                e.Draw(s);
            }
        }

        //private vars
        protected List<Component> children;
        protected Layout _layout;
    }
}