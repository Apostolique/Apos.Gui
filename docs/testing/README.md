# Testing

This guide will show you how to test a UI, or a component you made, without a window.

Apos.Input can read its input from a `SimulatedInput` instead of the real devices. You set the keyboard, mouse, gamepad and touch states yourself, then run frames the same way your game does. [Apos.Input's testing guide](https://apostolique.github.io/Apos.Input/testing/) has the details.

## Setup

The UI needs a font to measure text, and an area since there's no window to fill:

```csharp
GuiHelper.Font = new ShapeFont(File.ReadAllBytes("font-file.ttf"));

var input = new SimulatedInput();
InputHelper.Setup(input);

var ui = new IMGUI { Area = new RectangleF(0, 0, 800, 480) };
```

Nothing gets drawn, so there's no need for a `GraphicsDevice`.

## Frames

A frame is the same calls as your game's `Update`, with a clock that you step yourself:

```csharp
TimeSpan time = TimeSpan.Zero;

void Frame(Action build) {
    time += TimeSpan.FromMilliseconds(16);
    var gameTime = new GameTime(time, TimeSpan.FromMilliseconds(16));

    GuiHelper.UpdateSetup(gameTime);
    ui.UpdateStart(gameTime);
    build();
    ui.UpdateEnd(gameTime);
    GuiHelper.UpdateCleanup();
}
```

`build` is your UI code. Layout happens in `UpdateEnd`, so a component has its position from the second frame on.

## A click

```csharp
Button b = null!;
void Build() => b = Button.Put("Play");

Frame(Build);
input.Mouse = new MouseState(40, 20, 0, ButtonState.Pressed, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
Frame(Build);
input.Mouse = new MouseState(40, 20, 0, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released, ButtonState.Released);
Frame(Build);

Assert.True(b.Clicked);
```

Keys work the same way with `input.Keyboard = new KeyboardState(Keys.Down)`, and text with `input.Type("hello")`.

## Without input

Identity, layout and focus don't need any input. You can call `ui.GrabFocus` and `ui.Navigate(Direction.Down)` directly, and read a component's `Bounds` after `UpdateEnd`.

`GuiHelper.CurrentIMGUI` and `InputHelper` are static, so these tests can't run at the same time. With xUnit you can turn off parallel tests for the assembly:

```csharp
[assembly: CollectionBehavior(DisableTestParallelization = true)]
```

The library's own [tests](https://github.com/Apostolique/Apos.Gui/tree/main/Tests) are more examples.
