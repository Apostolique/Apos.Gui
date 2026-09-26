using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    /// <summary>
    /// Gives its child a fixed size, or the child's size plus padding, and centers the child in it.
    /// </summary>
    public class Block(int id, float? width, float? height, float padding) : SingleParent(id) {
        public float? ForcedWidth { get; set; } = width;
        public float? ForcedHeight { get; set; } = height;
        public float Padding { get; set; } = padding;

        public override void UpdatePrefSize(GameTime gameTime) {
            if (Child != null) {
                Child.UpdatePrefSize(gameTime);

                PrefWidth = ForcedWidth ?? (Child.PrefWidth + Padding * 2f);
                PrefHeight = ForcedHeight ?? (Child.PrefHeight + Padding * 2f);
            } else {
                PrefWidth = ForcedWidth ?? 100f;
                PrefHeight = ForcedHeight ?? 100f;
            }
        }

        public override void UpdateLayout(GameTime gameTime) {
            if (Child != null) {
                Child.Width = MathHelper.Min(Child.PrefWidth, Width - Padding * 2f);
                Child.Height = MathHelper.Min(Child.PrefHeight, Height - Padding * 2f);
                Child.X = X + Width / 2f - Child.Width / 2f;
                Child.Y = Y + Height / 2f - Child.Height / 2f;
                LayoutChild(gameTime);
            }
        }

        public static Block Put(float? width = null, float? height = null, float padding = 0f, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            Block a = IMGUI.Push(id, isAbsoluteId, id => new Block(id, width, height, padding), 1);
            a.ForcedWidth = width;
            a.ForcedHeight = height;
            a.Padding = padding;

            return a;
        }
        /// <summary>
        /// Useful for doing an empty block.
        /// </summary>
        public static void Pop() {
            GuiHelper.CurrentIMGUI.Pop();
        }
    }
}
