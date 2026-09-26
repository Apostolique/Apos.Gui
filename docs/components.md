# Components

This guide lists the components that come with Apos.Gui. Containers that arrange other components are in [Layout](./layout.md).

Components that edit a value take it by `ref`. While the component has focus it owns the value and writes it back to your variable. The rest of the time it copies your variable, so you can change it from your own code whenever you want.

## Label

One line of text.

```csharp
Label.Put("Score: 100");
Label.Put("Game over", fontSize: 50, color: Color.Red);
```

Font sizes are em sizes, the same as `ShapeFont`.

## Paragraph

Text that wraps to the space its parent gives it. `MaxWidth` keeps it from getting wider than that, even when there's more room.

```csharp
Paragraph p = Paragraph.Put(_story, align: TextAlign.Center);
p.MaxWidth = 500f;
```

`TextLayout` does the wrapping, and you can use it for your own components. It also has `Ellipsize` to cut a line short with "...".

## Button

```csharp
if (Button.Put("Play").Clicked) StartGame();
```

A button holds one child. `Button.Put("Play")` is a shortcut for a button with a label in it. To put something else in it, push the button yourself:

```csharp
Button b = Button.Put();
Icon.Put(_playIcon);
if (b.Clicked) StartGame();
```

The button pops itself after its child.

## Checkbox

```csharp
Checkbox.Put(ref _music, "Music");
```

The text is part of the checkbox, so pressing it toggles the checkbox too. Leave it out for a box on its own.

## RadioButton

Each radio button has a value, and it's checked when your variable equals that value:

```csharp
RadioButton.Put(ref _difficulty, 0, "Easy");
RadioButton.Put(ref _difficulty, 1, "Normal");
RadioButton.Put(ref _difficulty, 2, "Hard");
```

Like the checkbox, the text is optional and pressing it picks the radio button.

## Slider

```csharp
Slider.Put(ref _volume, 0f, 1f, step: 0.05f);
```

Left and right move it while it has focus. `BarSize` sets how big it wants to be.

## ProgressBar

```csharp
ProgressBar.Put(_loaded / (float)_total);
```

It goes from 0 to 1. `BarSize` sets how big it wants to be.

## Textbox

```csharp
Textbox.Put(ref _name);
Textbox notes = Textbox.Put(ref _notes);
notes.Multiline = true;
notes.MaxLength = 500;
```

It has a caret, selection with the mouse or Shift, word jumps with Ctrl, select all, copy, cut and paste. It scrolls when the text doesn't fit. [Input](./input.md) shows how to connect the system clipboard and an on-screen keyboard.

## Dropdown

```csharp
Dropdown.Put(ref _resolution, ["1280x720", "1920x1080", "2560x1440"]);
```

It opens a popup under itself with a button for each option.

## Tabs

```csharp
Tabs.Put(ref _tab, ["Video", "Audio", "Controls"]);
if (_tab == 0) VideoSettings();
```

## Icon

Draws a texture region.

```csharp
Icon.Put(new Texture2DRegion(_texture, 0, 0, 64, 64));
```

## Tooltip

A tooltip takes two children. The first one is the component, the second one is the tip:

```csharp
Tooltip.Put();
if (Button.Put("Delete").Clicked) Delete();
Label.Put("Deletes the save forever.");
```

The tip shows up after a short `Delay` while the mouse is over the component or while it has focus, so it works with a gamepad too. It goes under the component, or over it when there isn't room, and it stays inside the UI. It draws on top of everything else.

## Popup

A popup draws above everything else. It's open while your variable is true:

```csharp
if (_confirmOpen) {
    Popup.Push(ref _confirmOpen, modal: true);
    Label.Put("Quit the game?");
    if (Button.Put("Yes").Clicked) Exit();
    if (Button.Put("No").Clicked) _confirmOpen = false;
    Popup.Pop();
}
```

Pressing outside of it or pressing back sets your variable to false. `modal: true` darkens the screen and blocks everything under it. You can pass an `anchor` rectangle to open it next to something, the way the dropdown does. A popup keeps the arrow keys and the d-pad inside itself, so when you open one with a keyboard or a gamepad, give focus to something in it with `_ui.GrabFocus`.
