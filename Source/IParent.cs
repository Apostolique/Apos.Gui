namespace Apos.Gui {
    public interface IParent : IComponent {
        void Add(Component c);
        void Remove(Component c);
        void Reset();
        int NextIndex();
    }
}
