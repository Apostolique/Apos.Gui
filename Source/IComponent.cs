using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Apos.Gui {
    /// <summary>
    /// Base API for components.
    /// </summary>
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

        /// <summary>
        /// The component's current focus state. Managed by IMGUI.
        /// </summary>
        bool IsFocused { get; set; }
        /// <summary>
        /// Used when cycling over components and picking which one should have focus.
        /// </summary>
        bool IsFocusable { get; set; }
        /// <summary>
        /// Allows a component to float to the top so that it can be drawn on top.
        /// </summary>
        bool IsFloatable { get; set; }

        /// <summary>
        /// A rectangle with the component's X, Y, Width, Height.
        /// </summary>
        RectangleF Bounds { get; set; }
        /// <summary>
        /// When a component is not fully visible. Usually because it overflows it's parent.
        /// When the component is fully visible this will be the same as Bounds.
        /// </summary>
        RectangleF Clip { get; set; }

        /// <summary>
        /// X and Y positions in the UI coordinate system.
        /// </summary>
        Vector2 XY { get; set; }
        /// <summary>
        /// Width and Height of the component given by it's parent.
        /// </summary>
        Vector2 Size { get; set; }
        /// <summary>
        /// The component's preferred width and height. It's up to the parent to respect that.
        /// </summary>
        Vector2 PrefSize { get; set; }
        /// <summary>
        /// X
        /// </summary>
        float Left { get; set; }
        /// <summary>
        /// Y
        /// </summary>
        float Top { get; set; }
        /// <summary>
        /// X + Width
        /// </summary>
        float Right { get; set; }
        /// <summary>
        /// Y + Height
        /// </summary>
        float Bottom { get; set; }

        /// <summary>
        /// Good place to setup states before inputs are processed.
        /// </summary>
        void UpdateSetup(GameTime gameTime);
        /// <summary>
        /// Might or might not be called. Handles user inputs.
        /// Inputs are separated to allow for freezing inputs while keeping the UI responsive.
        /// </summary>
        void UpdateInput(GameTime gameTime);
        /// <summary>
        /// Final update step. Usually for doing animations.
        /// This is where everything that doesn't rely on inputs gets updated.
        /// </summary>
        void Update(GameTime gameTime);
        /// <summary>
        /// First pass for layout management.
        /// Components can determine their preferred sizes.
        /// </summary>
        void UpdatePrefSize(GameTime gameTime);
        /// <summary>
        /// Draws the component. The component should draw itself within it's clip rectangle.
        /// </summary>
        void Draw(GameTime gameTime);

        /// <summary>
        /// The component's parent once one has been assigned.
        /// </summary>
        IParent? Parent { get; set; }
        /// <summary>
        /// Used for focus cycling. The component tree is flattened. This returns the component before the current one.
        /// If there is only one component in the tree it will return itself.
        /// </summary>
        IComponent GetPrev();
        /// <summary>
        /// Used for focus cycling. The component tree is flattened. This returns the component after the current one.
        /// If there is only one component in the tree it will return itself.
        /// </summary>
        IComponent GetNext();
        /// <summary>
        /// Used for focus cycling. When a component is a parent, this returns it's last child.
        /// If there is only one component in the tree it will return itself.
        /// </summary>
        IComponent GetLast();

        /// <summary>
        /// Used to request focus on a component. IMGUI will also implicitly call SendToTop.
        /// </summary>
        Action<IComponent?> GrabFocus { get; set; }
        /// <summary>
        /// If the component is floatable, it's parent will reorder it to be drawn on top.
        /// It will also scroll it into view if needed.
        /// </summary>
        void SendToTop();
    }
}
