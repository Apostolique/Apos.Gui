# Getting started

## Install

Install using the following dotnet command:

```
dotnet add package Apos.Gui --prerelease
```

## Setup

Import the library with:

```csharp
using Apos.Gui;
using FontStashSharp;
```

Find a font ttf file, call it `font-file.ttf` and put it in your content folder. Add it to the MonoGame content pipeline and select ***copy*** in "Build Action" instead of ~~build~~.

In your game's `LoadContent()`, give your game instance and font to GuiHelper and create an `IMGUI` object:

```csharp
protected override void LoadContent() {
    FontSystem fontSystem = FontSystemFactory.Create(GraphicsDevice, 2048, 2048);
    fontSystem.AddFont(TitleContainer.OpenStream($"{Content.RootDirectory}/font-file.ttf"));

    GuiHelper.Setup(this, fontSystem);

    _ui = new IMGUI();
}

IMGUI _ui;
```

In your update loop, call the following functions:

```csharp
protected override void Update(GameTime gameTime) {
    // Call UpdateSetup at the start.
    GuiHelper.UpdateSetup(gameTime);
    _ui.UpdateAll(gameTime);

    // Create your UI.
    Panel.Push();
    if (Button.Put("Show fun").Clicked) {
        _showFun = !_showFun;
    }
    if (_showFun) {
        Label.Put("This is fun!");
    }
    if (Button.Put("Quit").Clicked) {
        Exit();
    }
    Panel.Pop();

    // Call UpdateCleanup at the end.
    GuiHelper.UpdateCleanup();

    base.Update(gameTime);
}

bool _showFun = false;
```

This last code shows a simple menu with two buttons and a conditional label.

Finally, in your draw loop:

```csharp
protected override void Draw(GameTime gameTime) {
    GraphicsDevice.Clear(Color.Black);

    _ui.Draw(gameTime);

    base.Draw(gameTime);
}
```

## Read more

Check the [Design choices](./design-choices.md) to understand the thinking process behind this library.
