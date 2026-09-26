using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    /// <summary>
    /// A button in a row of tabs. The selected one is marked.
    /// </summary>
    public class TabButton(long id) : SingleParent(id) {
        public Interaction Interaction { get; } = new();
        public bool Clicked => Interaction.Clicked;
        public override bool IsFocusable { get; set; } = true;
        public bool IsSelected { get; set; }

        /// <summary>Draws this tab under its child. Null uses <see cref="DefaultLook"/>.</summary>
        public Action<TabButton>? Look { get; set; }
        /// <summary>Draws every tab that doesn't have its own <see cref="Look"/>.</summary>
        public static Action<TabButton> DefaultLook { get; set; } = DrawDefault;

        public override void UpdateInput(GameTime gameTime) {
            base.UpdateInput(gameTime);
            Interaction.UpdateInput(this);
        }

        public override void Draw(GameTime gameTime) {
            (Look ?? DefaultLook)(this);
            base.Draw(gameTime);
        }

        /// <summary>A button's look, with a filled background and a line under it when selected.</summary>
        public static void DrawDefault(TabButton t) {
            if (t.IsSelected) {
                GuiHelper.ShapeBatch.FillRectangle(t.XY, t.Size, new Color(Theme.Highlight, 0.15f), Theme.Rounding);
                GuiHelper.ShapeBatch.FillRectangle(new Vector2(t.Left, t.Bottom - 3f), new Vector2(t.Width, 3f), Theme.Focus);
            }
            Button.DrawPressable(t, t.Interaction);
        }

        public static TabButton Put(string text, int fontSize = 30, Color? color = null, [CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            TabButton t = IMGUI.Push(id, isAbsoluteId, file, id => new TabButton(id), 1);
            Label.Put(text, fontSize: fontSize, color: color);

            return t;
        }
    }

    /// <summary>
    /// A row of tabs. Put the selected tab's content after it.
    /// </summary>
    public static class Tabs {
        /// <param name="selected">Index of the selected tab. Changes when another tab is clicked.</param>
        /// <param name="names">The text of each tab.</param>
        /// <param name="fontSize">Size of the text.</param>
        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="file">The caller's file. The same line in two files gives two different ids.</param>
        public static Horizontal Put(ref int selected, IReadOnlyList<string> names, int fontSize = 30, [CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            Horizontal row = Horizontal.Push(id, isAbsoluteId, file);

            _tabs.Clear();
            for (int i = 0; i < names.Count; i++) {
                TabButton t = TabButton.Put(names[i], fontSize, id: i);
                if (t.Clicked) {
                    selected = i;
                }
                _tabs.Add(t);
            }
            for (int i = 0; i < _tabs.Count; i++) {
                _tabs[i].IsSelected = i == selected;
            }

            Horizontal.Pop();
            return row;
        }

        private static readonly List<TabButton> _tabs = [];
    }
}
