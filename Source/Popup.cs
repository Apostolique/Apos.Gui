using System;
using System.Runtime.CompilerServices;
using Apos.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.Extended;
using Track = Apos.Input.Track;

namespace Apos.Gui {
    /// <summary>
    /// A stack that floats above the rest of the UI. It's put at the root, so no parent clips it, and it gets input first.
    /// Opens under an anchor rectangle, or above it when there isn't room, or in the middle of the screen without one.
    /// Dropdowns and dialogs are made of it.
    /// </summary>
    public class Popup : Vertical {
        /// <inheritdoc/>
        public override bool IsFocusScope { get; set; } = true;
        /// <summary>Creates the popup. <see cref="Push"/> calls this on the first frame it's put.</summary>
        public Popup(long id) : base(id) {
            Padding = 4f;
            Align = CrossAlign.Stretch;
        }

        /// <inheritdoc/>
        public override bool IsFloatable { get; set; } = true;
        /// <summary>The rectangle it opens next to, in UI coordinates. Null centers it on the screen.</summary>
        public RectangleF? Anchor { get; set; }
        /// <summary>Darkens everything under it and keeps the pointer from reaching it. For dialogs.</summary>
        public bool Modal { get; set; }
        /// <summary>A press outside the popup and its anchor dismisses it.</summary>
        public bool CloseOnOutsidePress { get; set; } = true;
        /// <summary>The popup is at least this wide, in UI units.</summary>
        public float MinWidth { get; set; }
        /// <summary>True on a frame where something asked the popup to close: a press outside it, or <see cref="Bindings.Back"/>.</summary>
        public bool Dismissed => _dismissed;

        /// <summary>Draws this popup under its content. Null uses <see cref="DefaultLook"/>.</summary>
        public Action<Popup>? Look { get; set; }
        /// <summary>Draws every popup that doesn't have its own <see cref="Look"/>.</summary>
        public static Action<Popup> DefaultLook { get; set; } = DrawDefault;

        /// <inheritdoc/>
        public override void UpdateSetup(GameTime gameTime) {
            _dismissed = false;
            base.UpdateSetup(gameTime);
        }
        /// <inheritdoc/>
        public override void UpdateInput(GameTime gameTime) {
            base.UpdateInput(gameTime);

            IMGUI ui = Root;
            Bindings bindings = ui.Bindings;
            if (bindings.Back.Pressed()) {
                _dismissed = true;
            }
            if (!bindings.UsesPointer) {
                return;
            }

            Vector2 mouse = ui.Mouse;
            if (CloseOnOutsidePress) {
                bool outside = bindings.MouseInteraction.Pressed(false) && IsOutside(mouse);
                foreach (TouchLocation t in InputHelper.NewTouch) {
                    outside |= Input.TouchCondition.Pressed(t) && Track.TouchCondition.IsUnique(t.Id) && IsOutside(ui.ToUI(t.Position));
                }
                _dismissed |= outside;
            }

            // Whatever is under the popup shouldn't react to it. A modal popup covers the whole screen.
            bool covers = Modal || Clip.Contains(mouse);
            if (covers) {
                Track.MouseCondition.Consume(MouseSensor.Pointer);
                bindings.MouseInteraction.Pressed();
                Track.MouseCondition.Scrolled();
            }
            foreach (TouchLocation t in InputHelper.NewTouch) {
                if (Input.TouchCondition.Pressed(t) && (Modal || Clip.Contains(ui.ToUI(t.Position)))) {
                    Track.TouchCondition.Consume(t.Id);
                }
            }
        }

        /// <inheritdoc/>
        public override void UpdatePrefSize(GameTime gameTime) {
            base.UpdatePrefSize(gameTime);
            PrefWidth = MathF.Max(PrefWidth, MinWidth);
        }
        /// <inheritdoc/>
        public override void UpdateLayout(GameTime gameTime) {
            RectangleF screen = Root.Bounds;
            Width = MathF.Min(Width, screen.Width);
            Height = MathF.Min(Height, screen.Height);

            if (Anchor is RectangleF a) {
                X = MathHelper.Clamp(a.Left, screen.Left, screen.Right - Width);
                bool fitsBelow = a.Bottom + Height <= screen.Bottom;
                bool fitsAbove = a.Top - Height >= screen.Top;
                Y = fitsBelow || !fitsAbove ? MathF.Min(a.Bottom, screen.Bottom - Height) : a.Top - Height;
            } else {
                XY = screen.Center - Size / 2f;
            }

            // The parent clipped the popup before it moved itself.
            Clip = Bounds.Intersect(screen);

            base.UpdateLayout(gameTime);
        }

        /// <inheritdoc/>
        public override void Draw(GameTime gameTime) {
            if (Modal) {
                GuiHelper.ShapeBatch.FillRectangle(Root.XY, Root.Size, Theme.Backdrop);
            }
            (Look ?? DefaultLook)(this);
            // Content that scrolls stays inside the border.
            float b = Theme.BorderThickness;
            RectangleF inside = new(Clip.X + b, Clip.Y + b, MathF.Max(Clip.Width - b * 2f, 0f), MathF.Max(Clip.Height - b * 2f, 0f));
            GuiHelper.PushClip(inside, MathF.Max(Theme.Rounding - b, 0f));
            base.Draw(gameTime);
            GuiHelper.PopClip();
        }

        /// <summary>A background with a border.</summary>
        public static void DrawDefault(Popup p) {
            GuiHelper.ShapeBatch.DrawRectangle(p.XY, p.Size, Theme.Panel, Theme.Border, Theme.BorderThickness, Theme.Rounding);
        }

        /// <summary>Whether a position in UI coordinates is outside both the popup and its anchor.</summary>
        protected bool IsOutside(Vector2 p) => !Clip.Contains(p) && !(Anchor?.Contains(p) ?? false);

        /// <summary>Backs <see cref="Dismissed"/>. Cleared at the start of every frame.</summary>
        protected bool _dismissed = false;

        /// <summary>
        /// Opens the popup. Put its content after this, then call <see cref="Pop"/>.
        /// Sets isOpen to false on a frame where it got dismissed, so you can stop putting it.
        /// </summary>
        /// <param name="isOpen">Your open state.</param>
        /// <param name="anchor">The rectangle to open next to. Null centers the popup.</param>
        /// <param name="modal">Darkens and blocks everything under it.</param>
        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="file">The caller's file. The same line in two files gives two different ids.</param>
        public static Popup Push(ref bool isOpen, RectangleF? anchor = null, bool modal = false, [CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            IMGUI ui = GuiHelper.CurrentIMGUI;
            ui.Push(ui);

            Popup a = IMGUI.Push(id, isAbsoluteId, file, id => new Popup(id));
            a.Anchor = anchor;
            a.Modal = modal;
            if (a.Dismissed) {
                isOpen = false;
            }

            return a;
        }
        /// <summary>Closes the popup started with <see cref="Push"/>.</summary>
        public static new void Pop() {
            IMGUI ui = GuiHelper.CurrentIMGUI;
            ui.Pop();
            ui.Pop();
        }
    }
}
