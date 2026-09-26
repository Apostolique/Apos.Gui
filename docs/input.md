# Input

This guide will show you how to change the inputs a UI listens to, where it goes on the screen, and how to run more than one.

## Bindings

Each `IMGUI` has its own `Bindings`. The defaults listen to the keyboard, the mouse, touch and every gamepad. Any of them can be replaced with an [Apos.Input](https://apostolique.github.io/Apos.Input/) condition:

```csharp
_ui.Bindings.Back = new AnyCondition(
    new Track.KeyboardCondition(Keys.Escape),
    new Track.GamePadCondition(GamePadButton.Start, 0)
);
```

Use the `Track` conditions for anything that should be consumed, so that one press doesn't trigger two things. `MoveLeft`, `MoveRight`, `MoveUp` and `MoveDown` repeat while held. Wrap your own condition in a `RepeatCondition` to keep that:

```csharp
_ui.Bindings.MoveDown = new RepeatCondition(new Track.KeyboardCondition(Keys.S), 400, 50);
```

`Bindings.UsesPointer` turns the mouse and touch off for the whole UI. A single component can turn them off with its `Interaction.UsesMouse` and `Interaction.UsesTouch`.

## Scale and area

`Scale` makes the whole UI bigger or smaller. At 2, a 100 unit wide button takes 200 pixels.

`Area` is where the UI goes on the screen, in pixels. By default it's the whole window. The UI lays out in that area divided by the scale.

```csharp
_ui.Scale = 1.5f;
_ui.Area = new RectangleF(0, 0, 400, GraphicsDevice.Viewport.Height);
```

## More than one UI

Each UI has its own components, focus, bindings, scale and area. Components go to the UI that's between `UpdateStart` and `UpdateEnd`:

```csharp
_hud.UpdateStart(gameTime);
ProgressBar.Put(_health);
_hud.UpdateEnd(gameTime);

_menu.UpdateStart(gameTime);
// ...
_menu.UpdateEnd(gameTime);
```

You can also start one inside another. `UpdateEnd` gives the spot back to the one that was there before.

Call `GuiHelper.UpdateSetup` and `GuiHelper.UpdateCleanup` once per frame, not once per UI.

## Split screen

`Bindings.ForGamePad` makes bindings that only listen to one gamepad, with the keyboard, the mouse and touch turned off:

```csharp
_players = [
    new IMGUI { Bindings = Bindings.ForGamePad(0) },
    new IMGUI { Bindings = Bindings.ForGamePad(1) },
];

// Every frame, before UpdateStart:
float half = GraphicsDevice.Viewport.Width / 2f;
_players[0].Area = new RectangleF(0, 0, half, GraphicsDevice.Viewport.Height);
_players[1].Area = new RectangleF(half, 0, half, GraphicsDevice.Viewport.Height);
```

Draw each one after the other.

## UI in a render target

A UI can draw to a render target that you then show somewhere else, like on a screen inside your game world. Set the area to the render target's size and draw it while the render target is set:

```csharp
_monitorUi = new IMGUI { Area = new RectangleF(0, 0, 512, 512) };

GraphicsDevice.SetRenderTarget(_monitor);
GraphicsDevice.Clear(Color.Black);
_monitorUi.Draw(gameTime);
GraphicsDevice.SetRenderTarget(null);
```

The mouse and touch are still in screen pixels though. `PointerTransform` maps them to the render target's pixels, so they hit the right spot. If you draw the render target with a matrix, it's the inverse of that matrix:

```csharp
_monitorUi.PointerTransform = screen => Vector2.Transform(screen, Matrix.Invert(_monitorMatrix));
```

For a render target in 3D, the function can raycast from the camera and return where the ray hits the texture.

## Clipboard

Textboxes copy and paste through `GuiHelper.Clipboard`. MonoGame doesn't have a clipboard that works on every platform, so by default the text stays inside the game. You can plug in the one your platform has with the `IClipboard` interface:

```csharp
public class MyClipboard : IClipboard {
    public string? GetText() => ...;
    public void SetText(string text) => ...;
}

GuiHelper.Clipboard = new MyClipboard();
```

The example has one [for DesktopGL](https://github.com/Apostolique/Apos.Gui/blob/main/Example/Platforms/DesktopGL/SdlClipboard.cs) that goes through SDL, and one [for WindowsDX](https://github.com/Apostolique/Apos.Gui/blob/main/Example/Platforms/WindowsDX/WinFormsClipboard.cs) that goes through WinForms.

## On-screen keyboard

Phones and consoles don't always have a physical keyboard. `GuiHelper.OnScreenKeyboard` opens a keyboard when a textbox is tapped, or when `Bindings.OpenKeyboard` (A by default) is pressed while it has focus. It's off by default.

`SystemKeyboard` uses the platform's own keyboard through MonoGame's `KeyboardInput`:

```csharp
#if ANDROID || IOS
GuiHelper.OnScreenKeyboard = new SystemKeyboard { Title = "Enter your name" };
#endif
```

Desktop platforms don't have one. You can also make your own, for example a keyboard drawn with Apos.Gui itself, with the `IOnScreenKeyboard` interface. It gets the textbox and returns a task that finishes with the text, or with null when the player cancels.
