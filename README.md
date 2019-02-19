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
    InputHelper.Game = this;
    GuiHelper.Window = Window;
    GuiHelper.Scale = 1f;
    GuiHelper.Font = DynamicSpriteFont.FromTtf(File.ReadAllBytes(Content.RootDirectory + "/Fonts/FontFile.ttf"), 30);
    GuiHelper.FontSize = 30;
}
```

You can create a simple UI with the following code:

```csharp
ComponentFocus focus;

Action<Component> grabFocus = (Component c) => {
    focus.Focus = c;
}

var screen = new ScreenPanel();
screen.Layout = new LayoutVerticalCenter();

var p = new PanelVerticalScroll();
p.Layout = new LayoutVerticalCenter();
p.AddHoverCondition(Default.ConditionHoverMouse);
p.Add(Default.CreateButton("Fun"), (Component c) => {
    Console.WriteLine("This is fun.");
    return true;
});
p.Add(Default.CreateButton("Quit"), (Component c) => {
    Console.WriteLine("Quitting the game.");
    Exit();
    return true;
});
screen.Add(p);

focus = new ComponentFocus(screen, Default.ConditionPreviousFocus, Default.ConditionNextFocus);
```

In your `Update(GameTime gameTime)`, call the following functions:

```csharp
protected override void Update(GameTime gametime) {
    //Call UpdateSetup at the start.
    InputHelper.UpdateSetup();
    GuiHelper.UpdateSetup();

    focus.UpdateSetup();
    focus.UpdateInput();
    focus.Update();

    //Call Update at the end.
    InputHelper.Update();
}
```

In your `Draw(GameTime gameTime)`, call:

```csharp
protected override void Draw(GameTime gameTime) {
    GraphicsDevice.Clear(Color.Black);
    focus.Draw(spritebatch);
}
```

Working usage code can be found in the [AposGameStarter](https://github.com/Apostolique/AposGameStarter) project. Look into [menu.cs](https://github.com/Apostolique/AposGameStarter/blob/master/Menu.cs).
