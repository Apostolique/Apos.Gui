using Apos.Input;
using Microsoft.Xna.Framework.Input;
using Track = Apos.Input.Track;

namespace Apos.Gui {
    public static class Default {
            public static ICondition MouseInteraction { get; set; } =
                new Track.MouseCondition(MouseButton.LeftButton);
            public static ICondition FocusPrev { get; set; } =
                new AnyCondition(
                    new Track.KeyboardCondition(Keys.Up),
                    new AllCondition(
                        new AnyCondition(
                            new KeyboardCondition(Keys.LeftShift),
                            new KeyboardCondition(Keys.RightShift)
                        ),
                        new Track.KeyboardCondition(Keys.Tab)
                    )
                );
            public static ICondition FocusNext { get; set; } =
                new AnyCondition(
                    new Track.KeyboardCondition(Keys.Down),
                    new Track.KeyboardCondition(Keys.Tab)
                );
            public static ICondition MoveLeft { get; set; } =
                new Track.KeyboardCondition(Keys.Left);
            public static ICondition MoveRight { get; set; } =
                new Track.KeyboardCondition(Keys.Right);
    }
}
