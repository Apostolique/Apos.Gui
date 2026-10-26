using System;
using System.Runtime.CompilerServices;
using Apos.Input;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Track = Apos.Input.Track;

namespace Apos.Gui {
    /// <summary>
    /// Shows a tip next to a component while the mouse is over it or while it has focus.
    /// The first child is the component, the second one is the tip.
    /// </summary>
    public class Tooltip(long id) : Component(id), IParent {
        /// <summary>The component the tip explains.</summary>
        public IComponent? Main { get; set; }
        /// <summary>Shown next to <see cref="Main"/>.</summary>
        public IComponent? Tip { get; set; }
        public override bool IsFocusable { get; set; } = false;
        public virtual bool IsFocusScope { get; set; } = false;
        public virtual IComponent? Navigate(IComponent from, Direction direction) => null;

        /// <summary>How long the mouse has to stay over the component, or the component has to have focus, before the tip shows. In milliseconds.</summary>
        public double Delay { get; set; } = 400;
        /// <summary>Space between the component and the tip's background, in UI units.</summary>
        public float Gap { get; set; } = 6f;
        /// <summary>Space between the tip's background and the tip, in UI units.</summary>
        public float Padding { get; set; } = 8f;
        /// <summary>True while the tip is showing.</summary>
        public bool IsOpen { get; protected set; }
        /// <summary>The tip's background, in UI coordinates. It goes under the component, or over it when there isn't room, and stays inside the UI.</summary>
        public RectangleF Bubble { get; protected set; }

        /// <summary>Draws the tip's background. Null uses <see cref="DefaultLook"/>.</summary>
        public Action<Tooltip>? Look { get; set; }
        /// <summary>Draws every tooltip's background that doesn't have its own <see cref="Look"/>.</summary>
        public static Action<Tooltip> DefaultLook { get; set; } = DrawDefault;

        public override void UpdateSetup(GameTime gameTime) {
            Main?.UpdateSetup(gameTime);
            Tip?.UpdateSetup(gameTime);
        }
        public override void UpdateInput(GameTime gameTime) {
            bool wanted = false;
            if (Main != null && Tip != null) {
                // Checked before the component takes the hover, so something drawn over it still blocks the tip.
                bool mouseOver = Root.Bindings.UsesPointer && Pointer.Source == PointerSource.Mouse && Track.MouseCondition.IsUnique(MouseSensor.Pointer) && Main.Clip.Contains(Root.Mouse);
                wanted = mouseOver || HasFocus(Main);
            }

            if (!wanted) {
                _since = -1;
            } else if (_since < 0) {
                _since = GuiHelper.Clock.TotalMS;
            }
            IsOpen = wanted && GuiHelper.Clock.TotalMS - _since >= Delay;

            Main?.UpdateInput(gameTime);
        }
        public override void Update(GameTime gameTime) {
            Main?.Update(gameTime);
            if (Tip != null && IsOpen) {
                Tip.Update(gameTime);
            }
        }

        public override void UpdatePrefSize(GameTime gameTime) {
            if (Main != null) {
                Main.UpdatePrefSize(gameTime);

                PrefWidth = Main.PrefWidth;
                PrefHeight = Main.PrefHeight;
            }
            if (Tip != null && IsOpen) {
                Tip.UpdatePrefSize(gameTime);
            }
        }
        public virtual void UpdateLayout(GameTime gameTime) {
            if (Main != null) {
                Main.Bounds = Bounds;
                Main.Clip = Main.Bounds.Intersect(Clip);

                if (Main is IParent p) {
                    p.UpdateLayout(gameTime);
                }
            }
            if (Main != null && Tip != null && IsOpen) {
                // The tip isn't bound by the parents, only by the UI.
                RectangleF area = new(0f, 0f, Root.Width, Root.Height);
                float width = MathF.Min(Tip.PrefWidth + Padding * 2f, area.Width);
                float height = MathF.Min(Tip.PrefHeight + Padding * 2f, area.Height);

                float x = MathHelper.Clamp(Main.X, area.Left, area.Right - width);
                float y = Main.Bottom + Gap;
                if (y + height > area.Bottom) {
                    y = Main.Top - Gap - height;
                }
                y = MathHelper.Clamp(y, area.Top, area.Bottom - height);

                Bubble = new RectangleF(x, y, width, height);
                Tip.Bounds = new RectangleF(x + Padding, y + Padding, width - Padding * 2f, height - Padding * 2f);
                Tip.Clip = Tip.Bounds.Intersect(area);

                if (Tip is IParent p) {
                    p.UpdateLayout(gameTime);
                }
            }
        }

        public override void Draw(GameTime gameTime) {
            Main?.Draw(gameTime);

            if (Tip != null && IsOpen) {
                // On top of the whole UI, so that what comes after the tooltip doesn't cover the tip.
                Root.DrawOnTop(DrawTip);
            }
        }
        protected virtual void DrawTip(GameTime gameTime) {
            if (Tip == null) return;

            GuiHelper.PushClip(Bubble.Intersect(new RectangleF(0f, 0f, Root.Width, Root.Height)));
            (Look ?? DefaultLook)(this);
            Tip.Draw(gameTime);
            GuiHelper.PopClip();
        }

        /// <summary>A background with a border behind the tip.</summary>
        public static void DrawDefault(Tooltip t) {
            GuiHelper.ShapeBatch.DrawRectangle(t.Bubble.Position, new Vector2(t.Bubble.Width, t.Bubble.Height), Theme.Panel, Theme.Border, Theme.BorderThickness, Theme.Rounding);
        }

        public void Add(IComponent c) {
            if (c.Index == 0) {
                if (Main != null) {
                    Main.Parent = null;
                }
                Main = c;
                Main.Parent = this;
            } else if (c.Index == 1) {
                if (Tip != null) {
                    Tip.Parent = null;
                }
                Tip = c;
                Tip.Parent = this;
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
        public virtual int PeekNextIndex() => _nextChildIndex;
        public virtual int NextIndex() => _nextChildIndex++;

        public override IComponent GetPrev() {
            return Parent?.GetPrev(this) ?? Tip?.GetLast() ?? Main?.GetLast() ?? this;
        }
        public override IComponent GetNext() {
            return Main ?? Tip ?? Parent?.GetNext(this) ?? this;
        }
        public virtual IComponent GetPrev(IComponent c) {
            return c == Tip && Main != null ? Main.GetLast() : this;
        }
        public virtual IComponent GetNext(IComponent c) {
            return c == Main && Tip != null ? Tip : Parent?.GetNext(this) ?? this;
        }
        public override IComponent GetLast() {
            return Tip ?? Main ?? this;
        }

        public virtual void SendToTop(IComponent c) {
            LastFocus = Root.CurrentFrame;
            Parent?.SendToTop(this);
        }

        /// <summary>The component, or a component inside it, has focus.</summary>
        protected static bool HasFocus(IComponent c) {
            for (IComponent? f = c.Root.Focused; f != null; f = f.Parent) {
                if (f == c) return true;
            }
            return false;
        }

        protected int _nextChildIndex = 0;
        protected double _since = -1;

        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="file">The caller's file. The same line in two files gives two different ids.</param>
        public static Tooltip Put([CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            return IMGUI.Push(id, isAbsoluteId, file, id => new Tooltip(id), 2);
        }
        /// <summary>Ends the tooltip early. It ends by itself after its second child.</summary>
        public static void Pop() {
            GuiHelper.CurrentIMGUI.Pop();
        }
    }
}
