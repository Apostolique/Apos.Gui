# Apos.Gui
UI library for MonoGame.

[![Discord](https://img.shields.io/discord/355231098122272778.svg)](https://discord.gg/N9t26Uv)

## Documentation

* [API](https://apostolique.github.io/Apos.Gui/)
* [Wiki](https://github.com/Apostolique/Apos.Gui/wiki)


## Build

### [NuGet](https://www.nuget.org/packages/Apos.Gui/) [![NuGet](https://img.shields.io/nuget/v/Apos.Gui.svg)](https://www.nuget.org/packages/Apos.Gui/) [![NuGet](https://img.shields.io/nuget/dt/Apos.Gui.svg)](https://www.nuget.org/packages/Apos.Gui/)

## Features

* Mouse, Keyboard, Gamepad, Touchscreen
* UI scaling

## Showcase

<img src="Images/Showcase.gif" alt="Apos GUI Loop" width="800" height="480" />

## Usage samples

First, find a font ttf file to use (Call it FontFile.ttf), put it in a subfolder called "Fonts" in your content folder. Add it to the MonoGame content pipeline and select ***copy*** in "Build Action" instead of ~build~.

In your game's `LoadContent()`, get the Helper classes ready:

```csharp
protected override void LoadContent() {
    using MemoryStream ms = new MemoryStream();
    TitleContainer.OpenStream($"{Content.RootDirectory}/Fonts/FontFile.ttf").CopyTo(ms);
    byte[] fontBytes = ms.ToArray();
    var font = DynamicSpriteFont.FromTtf(fontBytes, 30);

    GuiHelper.Setup(this, font);
}
```

You can create a simple UI with the following code:

```csharp
ComponentFocus focus;

Action<Component> grabFocus = c => {
    focus.Focus = c;
};

var screen = new ScreenPanel();
screen.Layout = new LayoutVerticalCenter();

var p = new Panel();
p.Layout = new LayoutVerticalCenter();
p.AddHoverCondition(Default.ConditionHoverMouse);
p.AddAction(Default.IsScrolled, Default.ScrollVertically);

p.Add(Default.CreateButton("Fun", c => {
    Console.WriteLine("This is fun.");
    return true;
}, grabFocus));
p.Add(Default.CreateButton("Quit", c => {
    Console.WriteLine("Quitting the game.");
    Exit();
    return true;
}, grabFocus));
screen.Add(p);

focus = new ComponentFocus(screen, Default.ConditionPreviousFocus, Default.ConditionNextFocus);
```

The above code will create 2 buttons, "Fun" and "Quit". You can use your mouse, keyboard, or gamepad to interact with them.

In your `Update(GameTime gameTime)`, call the following functions:

```csharp
protected override void Update(GameTime gametime) {
    //Call UpdateSetup at the start.
    GuiHelper.UpdateSetup();

    focus.UpdateSetup();
    focus.UpdateInput();
    focus.Update();

    //Call UpdateCleanup at the end.
    GuiHelper.UpdateCleanup();
}
```

In your `Draw(GameTime gameTime)`, call:

```csharp
protected override void Draw(GameTime gameTime) {
    GraphicsDevice.Clear(Color.Black);
    focus.Draw();
    base.Draw(gameTime);
}
```

Working usage code can be found in the [AposGameStarter](https://github.com/Apostolique/AposGameStarter) project. Look into [menu.cs](https://github.com/Apostolique/AposGameStarter/blob/master/Menu.cs).

## Other projects you might like

* [Apos.Input](https://github.com/Apostolique/Apos.Input) - Input library for MonoGame.
* [Apos.History](https://github.com/Apostolique/Apos.History) - A C# library that makes it easy to handle undo and redo.
* [Apos.Content](https://github.com/Apostolique/Apos.Content) - Content builder library for MonoGame.
* [Apos.Framework](https://github.com/Apostolique/Apos.Framework) - Game architecture for MonoGame.
* [AposGameStarter](https://github.com/Apostolique/AposGameStarter) - MonoGame project starter. Common files to help create a game faster.