using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    /// <summary>
    /// A button that shows the selected option and opens a <see cref="Popup"/> with every option when clicked.
    /// </summary>
    public class Dropdown(long id) : SingleParent(id) {
        /// <summary>Hover, press and click for this dropdown.</summary>
        public Interaction Interaction { get; } = new();
        /// <summary>True on the frame the dropdown is clicked.</summary>
        public bool Clicked => Interaction.Clicked;
        /// <inheritdoc/>
        public override bool IsFocusable { get; set; } = true;
        /// <summary>Whether the popup with the options is showing.</summary>
        public bool IsOpen { get; set; }
        /// <summary>Room on the right for the arrow, in UI units.</summary>
        public float ArrowWidth { get; set; } = 30f;

        /// <summary>Draws this dropdown under its child. Null uses <see cref="DefaultLook"/>.</summary>
        public Action<Dropdown>? Look { get; set; }
        /// <summary>Draws every dropdown that doesn't have its own <see cref="Look"/>.</summary>
        public static Action<Dropdown> DefaultLook { get; set; } = DrawDefault;

        /// <inheritdoc/>
        public override void UpdateInput(GameTime gameTime) {
            base.UpdateInput(gameTime);
            Interaction.UpdateInput(this);
        }

        /// <inheritdoc/>
        public override void UpdatePrefSize(GameTime gameTime) {
            base.UpdatePrefSize(gameTime);
            PrefWidth += ArrowWidth;
        }
        /// <inheritdoc/>
        public override void UpdateLayout(GameTime gameTime) {
            if (Child != null) {
                Child.XY = XY;
                Child.Size = new Vector2(MathF.Max(Width - ArrowWidth, 0f), Height);
                LayoutChild(gameTime);
            }
        }

        /// <inheritdoc/>
        public override void Draw(GameTime gameTime) {
            (Look ?? DefaultLook)(this);
            base.Draw(gameTime);
        }

        /// <summary>A button's look with an arrow on the right that flips while open.</summary>
        public static void DrawDefault(Dropdown d) {
            Button.DrawPressable(d, d.Interaction);

            Vector2 center = new(d.Right - d.ArrowWidth / 2f, d.Top + d.Height / 2f);
            float size = 6f;
            float flip = d.IsOpen ? -1f : 1f;
            GuiHelper.ShapeBatch.FillTriangle(
                center + new Vector2(-size, -size / 2f * flip),
                center + new Vector2(size, -size / 2f * flip),
                center + new Vector2(0f, size / 2f * flip),
                d.IsFocused ? Theme.Focus : Theme.Text
            );
        }

        /// <summary>Gives focus to the selected option on the frame the popup opens.</summary>
        protected bool _focusSelected = false;

        /// <summary>Picks one option out of a list.</summary>
        /// <param name="selected">Index of the selected option. Changes when a new one is picked.</param>
        /// <param name="options">The text of each option.</param>
        /// <param name="fontSize">Size of the text.</param>
        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="file">The caller's file. The same line in two files gives two different ids.</param>
        public static Dropdown Put(ref int selected, IReadOnlyList<string> options, int fontSize = 30, [CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            IMGUI ui = GuiHelper.CurrentIMGUI;
            Dropdown d = IMGUI.Push(id, isAbsoluteId, file, id => new Dropdown(id), 1);
            Label.Put(selected >= 0 && selected < options.Count ? options[selected] : "", fontSize);

            if (d.Clicked) {
                d.IsOpen = !d.IsOpen;
                d._focusSelected = d.IsOpen;
            }

            if (d.IsOpen) {
                bool open = true;
                ui.PushId(d.Id);
                Popup p = Popup.Push(ref open, d.Bounds);
                p.MinWidth = d.Width;

                for (int i = 0; i < options.Count; i++) {
                    Button b = Button.Put(options[i], fontSize, id: i);
                    if (d._focusSelected && i == selected) {
                        ui.GrabFocus(b);
                        d._focusSelected = false;
                    }
                    if (b.Clicked) {
                        selected = i;
                        open = false;
                    }
                }

                Popup.Pop();
                ui.PopId();

                if (!open) {
                    d.IsOpen = false;
                    ui.GrabFocus(d);
                }
            }

            return d;
        }
    }
}
