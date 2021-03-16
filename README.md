# Apos.Gui
UI library for MonoGame.

[![Discord](https://img.shields.io/discord/355231098122272778.svg)](https://discord.gg/N9t26Uv)

Note: Version 1 is currently in alpha. It is a full rewrite. This readme is updated, but the API is not yet fully stabilized. The library is not yet fully stable.

## Documentation

* [Getting started](https://apostolique.github.io/Apos.Gui/getting-started/)

## Build

[![NuGet](https://img.shields.io/nuget/v/Apos.Gui.svg)](https://www.nuget.org/packages/Apos.Gui/) [![NuGet](https://img.shields.io/nuget/dt/Apos.Gui.svg)](https://www.nuget.org/packages/Apos.Gui/)

## Features

* Mouse, Keyboard, Gamepad, Touchscreen
* UI scaling
* Used like IMGUI but components can be coded like a retained UI

## Showcase

![Apos.GUI Showcase](Images/Showcase.gif)

## Usage Example

First, find a font ttf file to use (Call it FontFile.ttf), put it in a subfolder called "Fonts" in your content folder. Add it to the MonoGame content pipeline and select ***copy*** in "Build Action" instead of ~build~.

In your game's `LoadContent()`, get the Helper classes ready:

```csharp
protected override void LoadContent() {
    var fontSystem = FontSystemFactory.Create(GraphicsDevice, 2048, 2048);
    fontSystem.AddFont(TitleContainer.OpenStream($"{Content.RootDirectory}/FontFile.ttf"));

    GuiHelper.Setup(this, fontSystem);

    _ui = new IMGUI();
    GuiHelper.CurrentIMGUI = _ui;
}

IMGUI _ui;
```

You can create a simple UI with the following code that you'll put in the Update call:

```csharp
Panel.Push();
if (Button.Put("Fun")) Console.WriteLine("This is fun.");
if (Button.Put("Quit")) {
    Console.WriteLine("This is fun.");
    Exit();
}
Panel.Pop();
```

The above code will create 2 buttons, "Fun" and "Quit". You can use your mouse, keyboard, or gamepad to interact with them.

In your `Update(GameTime gameTime)`, call the following functions:

```csharp
protected override void Update(GameTime gametime) {
    // Call UpdateSetup at the start.
    GuiHelper.UpdateSetup();
    _ui.UpdateAll();

    // Create your UI.
    Panel.Push();
    if (Button.Put("Fun")) Console.WriteLine("This is fun.");
    if (Button.Put("Quit")) {
        Console.WriteLine("This is fun.");
        Exit();
    }
    Panel.Pop();

    // Call UpdateCleanup at the end.
    GuiHelper.UpdateCleanup();
}
```

In your `Draw(GameTime gameTime)`, call:

```csharp
protected override void Draw(GameTime gameTime) {
    GraphicsDevice.Clear(Color.Black);

    _ui.Draw();

    base.Draw(gameTime);
}
```

Working usage code can be found in the [AposGameStarter](https://github.com/Apostolique/AposGameStarter) project. Look into [menu.cs](https://github.com/Apostolique/AposGameStarter/blob/master/Game/Layer1/Menu.cs).

## Other projects you might like

* [Apos.Input](https://github.com/Apostolique/Apos.Input) - Input library for MonoGame.
* [Apos.History](https://github.com/Apostolique/Apos.History) - A C# library that makes it easy to handle undo and redo.
* [Apos.Content](https://github.com/Apostolique/Apos.Content) - Content builder library for MonoGame.
* [Apos.Framework](https://github.com/Apostolique/Apos.Framework) - Game architecture for MonoGame.
* [AposGameStarter](https://github.com/Apostolique/AposGameStarter) - MonoGame project starter. Common files to help create a game faster.
