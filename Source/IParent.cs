using Microsoft.Xna.Framework;

namespace Apos.Gui {
    public interface IParent : IComponent {
        /// <summary>
        /// Add a component to the parent.
        /// </summary>
        /// <param name="c">The component to add.</param>
        void Add(IComponent c);
        /// <summary>
        /// Remove a specific component from the parent.
        /// </summary>
        /// <param name="c">The component to remove.</param>
        void Remove(IComponent c);
        /// <summary>
        /// Called at the end of a frame so that the parent is ready to accept new children on the next frame.
        /// </summary>
        void Reset();
        /// <summary>
        /// The id that will be given to the next child that will be added to the parent.
        /// </summary>
        /// <returns>The next id.</returns>
        int PeekNextIndex();
        /// <summary>
        /// Allocates an id that hasn't been used yet.
        /// </summary>
        /// <returns>The id to give to the next child that is added to the parent.</returns>
        int NextIndex();

        /// <summary>
        /// Used for focus cycling. The component tree is flattened. This returns the component before the current one.
        /// If the child isn't the first one, it will return the child before it.
        /// Otherwise it will return itself.
        /// </summary>
        /// <param name="c">The child to start from.</param>
        /// <returns>A component that was before the child.</returns>
        IComponent GetPrev(IComponent c);
        /// <summary>
        /// Used for focus cycling. The component tree is flattened. This returns the component after the current one.
        /// If the child isn't the last one, it will return the child after it.
        /// If it has a parent, it will ask the parent to return this component's next neighbor.
        /// Otherwise it will return itself.
        /// </summary>
        /// <param name="c">The child to start from.</param>
        /// <returns>A component that is before the child.</returns>
        IComponent GetNext(IComponent c);

        /// <summary>
        /// If the component is floatable, its parent will reorder it to be drawn on top.
        /// It will also scroll it into view if needed.
        /// </summary>
        /// <param name="c">The child to float to the top.</param>
        void SendToTop(IComponent c);

        /// <summary>
        /// Will apply a layout to its children.
        /// </summary>
        void UpdateLayout(GameTime gameTime);

        /// <summary>
        /// Keeps the arrow keys and the d-pad inside this parent. The focus can still leave with Tab.
        /// Popups and floating windows are focus scopes.
        /// </summary>
        bool IsFocusScope { get; set; }
        /// <summary>
        /// Lets a parent decide where the focus goes when it moves in a direction from one of its descendants.
        /// The focused component's parents get asked from the closest one up to its focus scope. When they all return null,
        /// the focus goes to the closest component in that direction.
        /// Override it for layouts where the closest one isn't right, like a menu that wraps around or a radial menu.
        /// </summary>
        /// <param name="from">The component that has focus.</param>
        /// <param name="direction">Where the focus is moving.</param>
        /// <returns>The component to focus, or null to let the next parent up decide.</returns>
        IComponent? Navigate(IComponent from, Direction direction);
    }
}
