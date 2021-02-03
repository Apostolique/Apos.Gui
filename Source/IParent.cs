namespace Apos.Gui {
    public interface IParent : IComponent {
        void Add(IComponent c);
        void Remove(IComponent c);
        void Reset();
        int NextIndex();
        IComponent GetPrev(IComponent c);
        IComponent GetNext(IComponent c);
    }
}
