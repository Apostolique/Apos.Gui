using Apos.Input;
using Microsoft.Xna.Framework.Input;
using Track = Apos.Input.Track;

namespace Apos.Gui {
    public static class Default {
        public static ICondition MouseInteraction { get; set; } =
            new Track.MouseCondition(MouseButton.LeftButton);
        public static ICondition ButtonInteraction { get; set; } =
            new AnyCondition(
                new Track.KeyboardCondition(Keys.Space),
                new Track.KeyboardCondition(Keys.Enter),
                new Track.GamePadCondition(GamePadButton.A, 0)
            );
        public static ICondition FocusPrev { get; set; } =
            new AnyCondition(
                new Track.KeyboardCondition(Keys.Up),
                new AllCondition(
                    new AnyCondition(
                        new KeyboardCondition(Keys.LeftShift),
                        new KeyboardCondition(Keys.RightShift)
                    ),
                    new Track.KeyboardCondition(Keys.Tab)
                ),
                new Track.GamePadCondition(GamePadButton.Up, 0)
            );
        public static ICondition FocusNext { get; set; } =
            new AnyCondition(
                new Track.KeyboardCondition(Keys.Down),
                new Track.KeyboardCondition(Keys.Tab),
                new Track.GamePadCondition(GamePadButton.Down, 0)
            );
        public static ICondition MoveLeft { get; set; } =
            new AnyCondition(
                new Track.KeyboardCondition(Keys.Left),
                new Track.GamePadCondition(GamePadButton.Left, 0)
            );
        public static ICondition MoveRight { get; set; } =
            new AnyCondition(
                new Track.KeyboardCondition(Keys.Right),
                new Track.GamePadCondition(GamePadButton.Right, 0)
            );
        public static ICondition MoveUp { get; set; } =
            new AnyCondition(
                new Track.KeyboardCondition(Keys.Up),
                new Track.GamePadCondition(GamePadButton.Up, 0)
            );
        public static ICondition MoveDown { get; set; } =
            new AnyCondition(
                new Track.KeyboardCondition(Keys.Down),
                new Track.GamePadCondition(GamePadButton.Down, 0)
            );
        public static ICondition Back { get; set; } =
            new AnyCondition(
                new Track.KeyboardCondition(Keys.Escape),
                new Track.GamePadCondition(GamePadButton.B, 0),
                new Track.GamePadCondition(GamePadButton.Back, 0),
                new Track.MouseCondition(MouseButton.XButton2)
            );
    }
}
