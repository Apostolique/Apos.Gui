namespace Apos.Gui {
    public interface IParent : IComponent {
        void Add(IComponent c);
        void Remove(IComponent c);
        void Reset();
        int NextIndex();
    }
}
