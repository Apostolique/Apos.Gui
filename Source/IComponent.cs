using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Apos.Gui {
    public interface IComponent {
        /// <summary>
        /// Managed automatically when a component is created. Used by an IParent to keep track of it's children.
        /// </summary>
        int Index { get; set; }
        /// <summary>
        /// Used by IMGUI to track of a component was pinged and is kept for this frame.
        /// </summary>
        uint LastPing { get; set; }
        /// <summary>
        /// Used by IMGUI to efficiently access any component in the UI.
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// X position in the UI coordinate system.
        /// </summary>
        float X { get; set; }
        /// <summary>
        /// Y position in the UI coordinate system.
        /// </summary>
        float Y { get; set; }
        /// <summary>
        /// Width of the component given by it's parent.
        /// </summary>
        float Width { get; set; }
        /// <summary>
        /// Height of the component given by it's parent.
        /// </summary>
        float Height { get; set; }
        /// <summary>
        /// The component's preferred width. It's up to the parent to respect that.
        /// </summary>
        float PrefWidth { get; set; }
        /// <summary>
        /// The component's preferred height. It's up to the parent to respect that.
        /// </summary>
        float PrefHeight { get; set; }

        bool IsFocused { get; set; }
        bool IsFocusable { get; set; }
        bool IsFloatable { get; set; }

        RectangleF Bounds { get; set; }
        RectangleF Clip { get; set; }

        Vector2 XY { get; set; }
        Vector2 Size { get; set; }
        Vector2 PrefSize { get; set; }
        float Left { get; set; }
        float Top { get; set; }
        float Right { get; set; }
        float Bottom { get; set; }

        void UpdatePrefSize(GameTime gameTime);
        void UpdateSetup(GameTime gameTime);
        void UpdateInput(GameTime gameTime);
        void Update(GameTime gameTime);
        void Draw(GameTime gameTime);

        IParent? Parent { get; set; }
        IComponent GetPrev();
        IComponent GetNext();
        IComponent GetLast();

        Action<IComponent?> GrabFocus { get; set; }
        void SendToTop();
    }
}
