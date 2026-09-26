# Moving from 2.x

This guide lists what you need to change in a game that used Apos.Gui 2.x.

## Packages

Apos.Gui targets .NET 10 and depends on Apos.Shapes 0.8 and Apos.Input 3.1. It doesn't use FontStashSharp anymore.

## Fonts

`GuiHelper.Setup` takes a `ShapeFont` instead of a `FontSystem`:

```csharp
using var fontStream = TitleContainer.OpenStream($"{Content.RootDirectory}/font-file.ttf");
GuiHelper.Setup(this, new ShapeFont(fontStream));
```

Font sizes are em sizes now, which makes text about 25% bigger than it was at the same number. Multiply your sizes by ~0.8 to keep the old look.

`GuiHelper.FontSystem`, `GetFont`, `MeasureStringTight` and `FontScale` are gone. `GuiHelper.MeasureString` and `GuiHelper.Font` replace them.

## Scale and input

`GuiHelper.Scale`, `WindowWidth`, `WindowHeight` and `UIMatrix` moved to each UI as `Scale`, `Width`, `Height` and `Matrix`:

```csharp
_ui.Scale = 2f;
```

The `Default` class is now `Bindings`, and each UI has its own:

```csharp
_ui.Bindings.Back = ...;
```

`GuiHelper.Mouse` still works. It's the mouse in the current UI's coordinates.

Up and down used to go to the previous and next component. The arrows and the d-pad now move the focus in their direction, and Tab and Shift+Tab go through the components in order. [Focus](./focus.md) has the details.

The defaults listen to every gamepad instead of only the first one.

## Drawing

`IMGUI.Draw` begins and ends the batch itself. Remove any `GuiHelper.Begin` and `GuiHelper.End` around it.

`PushScissor` and `PopScissor` are now `PushClip` and `PopClip`. They're drawn by the shader, so they don't break the batch.

Buttons don't cut their content anymore. Stacks still do.

## Ids

`IComponent.Id` is a `long`. Every `Put` and `Push` has a `[CallerFilePath] string file` parameter after `isAbsoluteId`. If you pass `id` and `isAbsoluteId` by name, nothing changes.

Custom components need two changes. Their constructor takes a `long` id, and their `Put` gets the file parameter and forwards it:

```csharp
public static MyThing Put([CallerLineNumber] int id = 0, bool isAbsoluteId = false, [CallerFilePath] string file = "") {
    return IMGUI.Put(id, isAbsoluteId, file, id => new MyThing(id));
}
```

## Removed setters

`Clicked` on buttons and checkboxes is read only, it comes from their `Interaction`. `FullWidth` and `FullHeight` on stacks are read only, they come from the size of the content.

## Layout

`Vertical` and `Horizontal` are both a `StackPanel` now, with `Spacing`, `Padding`, `Align` and scrollbars. [Layout](./layout.md) shows them.

`MenuPanel` puts 8 units of space between its children.

`FloatingWindow` isn't a stack anymore. It has a title bar, and its components go in its `Content`, which is. Set spacing and padding there.

## Tooltips

A tooltip takes the component first and the tip second, the other way around from before:

```csharp
Tooltip.Put();
Button.Put("Delete");
Label.Put("Deletes the save forever.");
```
