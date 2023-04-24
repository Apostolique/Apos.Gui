using Microsoft.Xna.Framework;

namespace Apos.Gui {
    public interface IParent : IComponent {
        void Add(IComponent c);
        void Remove(IComponent c);
        void Reset();
        int PeekNextIndex();
        int NextIndex();
        IComponent GetPrev(IComponent c);
        IComponent GetNext(IComponent c);
        void SendToTop(IComponent c);

        /// <summary>
        /// Applies a layout to the component
        /// </summary>
        void UpdateLayout(GameTime gameTime);
    }
}
