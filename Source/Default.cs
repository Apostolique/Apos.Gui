using Apos.Input;
using Track = Apos.Input.Track;

namespace Apos.Gui {
    public static class Default {
        public static ICondition MouseInteraction {
            get;
            set;
        } = new Track.MouseCondition(MouseButton.LeftButton);
    }
}
