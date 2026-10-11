# Layout

This guide will show you the containers that arrange components.

A container is pushed, then the components you put become its children until you pop it:

```csharp
Vertical.Push();
Label.Put("Name");
Textbox.Put(ref _name);
Vertical.Pop();
```

Each component asks for a preferred size, then its parent gives it a spot. Positions and sizes are in UI units, which are pixels at a [scale](./input.md#scale-and-area) of 1.

## Vertical and Horizontal

A `Vertical` stacks its children from top to bottom and a `Horizontal` from left to right. Both are a `StackPanel`, so they have the same settings:

```csharp
Vertical v = Vertical.Push();
v.Spacing = 10f;
v.Padding = 20f;
v.Align = CrossAlign.Center;
```

`Spacing` goes between children and `Padding` around all of them. `Align` places the children across the stack: `Start`, `Center`, `End`, or `Stretch` to give them the full width of a vertical or the full height of a horizontal.

A row of components with different heights usually wants `CrossAlign.Center` so they line up:

```csharp
Horizontal.Push().Align = CrossAlign.Center;
Icon.Put(_coin);
Label.Put($"{_coins}");
Horizontal.Pop();
```

## Scrolling

A stack scrolls when its parent gives it less room than its children need. The mouse wheel, dragging the scrollbar and dragging with a finger all scroll it. A component that gets focus inside it scrolls into view.

A `Block` is a simple way to give it a fixed size:

```csharp
Block.Put(400f, 300f);
Vertical.Push();
foreach (Item item in _inventory) {
    // ...
}
Vertical.Pop();
```

The scrollbar sits inside the stack's padding. While the stack scrolls, it keeps room for it beside the children, with `ScrollbarGap` between them.

`ScrollIncrement` sets how far a wheel notch goes, `ShowScrollbar` hides the scrollbar, and `OffsetX` and `OffsetY` let you scroll from your own code.

## MenuPanel

A vertical stack that fills the whole UI, centers its children, and scrolls when they don't fit. It's the usual root for a menu screen.

```csharp
MenuPanel.Push();
// ...
MenuPanel.Pop();
```

## Block

Gives its one child a fixed size, or the child's own size plus some padding. It pops itself after that child.

```csharp
Block.Put(width: 200f, height: 50f);
Label.Put("Fits in 200 by 50");
```

## Dock

Places its one child at a fixed rectangle, given as its left, top, right and bottom edges.

```csharp
Dock.Put(20f, 20f, 300f, 100f);
Label.Put("Top left corner");
```

## FloatingWindow

A window with a title bar you drag it by. It opens in the middle of the UI, stays inside it, and comes to the front when it's pressed or gets focus.

```csharp
FloatingWindow.Push("Inventory");
Label.Put("Sword");
// ...
FloatingWindow.Pop();
```

With `ref` to your open state, it gets a close button, and back closes it while the focus is in it:

```csharp
if (_inventoryOpen) {
    FloatingWindow.Push(ref _inventoryOpen, "Inventory");
    // ...
    FloatingWindow.Pop();
}
```

Its components go in `Content`, a vertical stack, so you can change its spacing and padding there. It scrolls when the window is taller than the UI.

The arrow keys and the d-pad stay inside a floating window once the focus is in it. [Focus](./focus.md) explains why.

## Clipping

Every component has a `Clip`, the part of it that its parents leave visible. Stacks cut what their children draw to that rectangle. Labels, icons and paragraphs also cut themselves when their text or texture is bigger than they are. A [custom component](./custom-components.md) that draws past its bounds can do the same with `GuiHelper.PushClip(Clip)` and `GuiHelper.PopClip()`. A clip you push stays inside the clips that are already there, so a component in a stack that scrolls can't draw outside of it. Floating windows and popups keep their content inside their border.

`ClipRounding` on a stack rounds the corners of that cut. It only applies while the stack is fully visible, since a stack that's cut by its own parent has straight edges where it was cut. Only one rounded clip is active at a time though, so a stack inside a rounded one cuts with square corners. Padding at least as big as the rounding keeps the inner stacks away from the corners.
