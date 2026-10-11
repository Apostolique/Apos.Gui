# Design choices

This library's goal is to make it easy to create custom user facing UIs. It's a hybrid between the IMGUI paradigm and regular retained UIs. When using the library, it looks like IMGUI. When implementing new components, it looks retained.

Using it like IMGUI means your game's variables are the source of truth. There's no model to keep in sync with the UI, you put the components every frame from whatever state you already have.

The components themselves are kept from frame to frame though. That's where focus, scrolling, text selection and animations live, so they don't have to be rebuilt or stored by your game. It's also why a component is a regular class with an update and a draw, and writing a new one looks like writing any other part of a game.

The library doesn't try to have every widget a game could want. It has the common ones, and the pieces they're built from (`Interaction`, `IMGUI.Put`, `SingleParent`, `ListParent`, `Navigation`) are public so that your own components work the same way.

This library is built on top of [Apos.Input](https://apostolique.github.io/Apos.Input/), which is a polling based input library. Its conditions can be consumed, which is how a press on a popup doesn't also reach the button under it. Drawing goes through [Apos.Shapes](https://github.com/Apostolique/Apos.Shapes), so shapes, text and textures all end up in the same batch.

MonoGame runs on desktops, phones, consoles and the web, so every feature has to work with a mouse, a touch screen, a keyboard and a gamepad.
