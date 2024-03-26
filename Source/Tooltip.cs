using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Apos.Gui {
    public class Tooltip(int id) : Component(id), IParent {
        public IComponent? Main { get; set; }
        public IComponent? Tip { get; set; }
        public override bool IsFocusable { get; set; } = false;
        public override bool IsFloatable { get; set; } = true;

        public override void UpdateSetup(GameTime gameTime) {
            Main?.UpdateSetup(gameTime);
            Tip?.UpdateSetup(gameTime);
        }
        public override void UpdateInput(GameTime gameTime) {
            if (Main != null) {
                if (Main.Clip.Contains(GuiHelper.OldMouse)) {
                    if (!_hovered) {
                        SendToTop(this);
                    }
                    _hovered = true;
                } else {
                    _hovered = false;
                }
            }

            if (Tip != null && _hovered) {
                Tip.UpdateInput(gameTime);
            }

            Main?.UpdateInput(gameTime);
        }
        public override void Update(GameTime gameTime) {
            Main?.Update(gameTime);
            if (Tip != null && _hovered) {
                Tip.Update(gameTime);
            }
        }

        public override void UpdatePrefSize(GameTime gameTime) {
            if (Main != null) {
                Main.UpdatePrefSize(gameTime);

                PrefWidth = Main.PrefWidth;
                PrefHeight = Main.PrefHeight;
            }
            if (Tip != null && _hovered) {
                Tip.UpdatePrefSize(gameTime);
            }
        }
        public virtual void UpdateLayout(GameTime gameTime) {
            if (Main != null) {
                Main.X = X;
                Main.Y = Y;
                Main.Width = Width;
                Main.Height = Height;
                Main.Clip = Main.Bounds.Intersection(Clip);

                if (Main is IParent p) {
                    p.UpdateLayout(gameTime);
                }
            }
            if (Tip != null && _hovered) {
                Tip.X = X;
                Tip.Y = Y;
                Tip.Width = Tip.PrefWidth;
                Tip.Height = Tip.PrefHeight;

                if (Tip is IParent p) {
                    p.UpdateLayout(gameTime);
                }
            }
        }

        public override void Draw(GameTime gameTime) {
            Main?.Draw(gameTime);

            if (Tip != null && _hovered) {
                GuiHelper.PushScissor(Tip.Clip);
                GuiHelper.SpriteBatch.FillRectangle(Tip.Bounds, Color.Black * 0.7f);
                GuiHelper.PopScissor();
                Tip.Draw(gameTime);
            }
        }

        public void Add(IComponent c) {
            if (c.Index == 0) {
                if (Tip != null) {
                    Tip.Parent = null;
                }
                Tip = c;
                Tip.Parent = this;
            } else if (c.Index == 1) {
                if (Main != null) {
                    Main.Parent = null;
                }
                Main = c;
                Main.Parent = this;
            }
        }
        public void Remove(IComponent c) {
            if (Tip == c) {
                Tip.Parent = null;
                Tip = null;
            } else if (Main == c) {
                Main.Parent = null;
                Main = null;
            }
        }
        public virtual void Reset() {
            _nextChildIndex = 0;
        }
        public virtual int PeekNextIndex() => _nextChildIndex + 1;
        public virtual int NextIndex() => _nextChildIndex++;

        /// <summary>
        /// If this component has a parent, it will ask the parent to return this component's previous neighbor.
        /// If it has children, it will return the last one.
        /// Otherwise it will return itself.
        /// </summary>
        public override IComponent GetPrev() {
            return Parent?.GetPrev(this) ?? Tip?.GetLast() ?? Main?.GetLast() ?? this;
        }
        /// <summary>
        /// If this component has children, it will return the first one.
        /// If it has a parent it will ask the parent to return this component's next neighbor.
        /// Otherwise, it will return itself.
        /// </summary>
        public override IComponent GetNext() {
            return Main ?? Tip ?? Parent?.GetNext(this) ?? this;
        }
        /// <summary>
        /// If the child isn't the first one, it will return the child before it.
        /// Otherwise it will return itself.
        /// </summary>
        public virtual IComponent GetPrev(IComponent c) {
            return c == Tip && Main != null ? Main.GetLast() : this;
        }
        /// <summary>
        /// If the child isn't the last one, it will return the child after it.
        /// If it has a parent, it will ask the parent to return this component's next neighbor.
        /// Otherwise it will return itself.
        /// </summary>
        public virtual IComponent GetNext(IComponent c) {
            return c == Main && Tip != null ? Tip : c == Tip ? Parent?.GetNext(this) ?? this : this;
        }
        /// <summary>
        /// Returns the last child in this component tree.
        /// </summary>
        public override IComponent GetLast() {
            return Tip ?? Main ?? this;
        }

        public virtual void SendToTop(IComponent c) {
            Parent?.SendToTop(this);
        }

        protected bool _hovered = false;
        protected int _nextChildIndex = 0;
        protected float _x;
        protected float _y;

        public static Tooltip Put([CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            id = GuiHelper.CurrentIMGUI.TryCreateId(id, isAbsoluteId, out IComponent c);

            Tooltip a;
            if (c is Tooltip d) {
                a = d;
            } else {
                a = new Tooltip(id);
            }

            GuiHelper.CurrentIMGUI.GrabParent(a);

            GuiHelper.CurrentIMGUI.Push(a, 2);

            return a;
        }
    }
}
