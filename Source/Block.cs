using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    /// <summary>
    /// Gives its child a fixed size, or the child's size plus padding, and centers the child in it.
    /// </summary>
    public class Block(long id, float? width, float? height, float padding) : SingleParent(id) {
        /// <summary>The width it asks its parent for, in UI units. Null uses the child's width plus padding.</summary>
        public float? ForcedWidth { get; set; } = width;
        /// <summary>The height it asks its parent for, in UI units. Null uses the child's height plus padding.</summary>
        public float? ForcedHeight { get; set; } = height;
        /// <summary>Space around the child on each side, in UI units.</summary>
        public float Padding { get; set; } = padding;

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override void UpdateLayout(GameTime gameTime) {
            if (Child != null) {
                Child.Width = MathHelper.Min(Child.PrefWidth, Width - Padding * 2f);
                Child.Height = MathHelper.Min(Child.PrefHeight, Height - Padding * 2f);
                Child.X = X + Width / 2f - Child.Width / 2f;
                Child.Y = Y + Height / 2f - Child.Height / 2f;
                LayoutChild(gameTime);
            }
        }

        /// <summary>Starts a block. The next component put becomes its child.</summary>
        /// <param name="width">The width it asks for. Null uses the child's width plus padding.</param>
        /// <param name="height">The height it asks for. Null uses the child's height plus padding.</param>
        /// <param name="padding">Space around the child on each side.</param>
        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="file">The caller's file. The same line in two files gives two different ids.</param>
        public static Block Put(float? width = null, float? height = null, float padding = 0f, [CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
            Block a = IMGUI.Push(id, isAbsoluteId, file, id => new Block(id, width, height, padding), 1);
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
