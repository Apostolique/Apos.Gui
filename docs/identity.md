# Identity

This guide will show you how a component is remembered from one frame to the next, and what to do in loops.

## Where a component comes from

Every `Put` and `Push` has these optional parameters at the end:

```csharp
Button.Put("Play", id: ..., isAbsoluteId: ..., file: ...);
```

You don't pass them most of the time. `id` is filled with the caller's line number and `file` with the caller's file path. Together with the parents the call is nested in, they make the component's `Id`. The first frame a call happens, the component gets created. The next frames, the same call gets the same component back, with its focus, its scroll position, its animations and anything else it keeps.

A component that wasn't put during a frame is removed at the end of that frame.

## Same line, same parent

Let's say you put buttons in a loop:

```csharp
foreach (string name in _saves) {
    if (Button.Put(name).Clicked) Load(name);
}
```

Every call has the same line and the same parent. Those get told apart by the order they happen in, so the first button is always the first call, the second button the second call, and so on.

That's fine while the list doesn't change. If you remove the first save though, every button moves up one spot and takes the state of the one before it. A focused button would suddenly be a different save.

## Keys for lists that change

You can give each item a key with `PushId`. The components put until the matching `PopId` belong to that key instead of to their position:

```csharp
foreach (Save save in _saves) {
    _ui.PushId(save.Id);
    if (Button.Put(save.Name).Clicked) Load(save);
    _ui.PopId();
}
```

Now removing a save removes its button, and the others keep their state. The key can be any `long` that stays the same for that item, like a database id or an index into a list that only grows.

You can also pass the key as the `id` directly when there's a single component per item:

```csharp
for (int i = 0; i < _slots.Length; i++) {
    Checkbox.Put(ref _slots[i].Enabled, id: i);
}
```

## Absolute ids

`isAbsoluteId: true` uses `id` as is, without the parents or the file. The component keeps its state when it moves from one parent to another:

```csharp
if (_docked) Vertical.Push(); else FloatingWindow.Push();
Textbox.Put(ref _chat, id: 1, isAbsoluteId: true);
if (_docked) Vertical.Pop(); else FloatingWindow.Pop();
```

Two absolute ids that are the same in the same frame get told apart by their order, like the loop above.

## When a call changes type

If a call that used to put a `Label` now puts an `Icon` with the same id, the label gets removed and an icon gets created. That happens when you change which component a line of code puts, for example with hot reload.
