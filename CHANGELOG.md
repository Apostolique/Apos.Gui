# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Touch input for every component
- Interaction, the hover, press, click, drag and focus handling that components share
- Directional focus with the arrow keys and the d-pad, with IParent.Navigate for custom layouts and focus scopes for popups and windows
- Bindings, the inputs of one UI, with Bindings.ForGamePad for split screen
- Scale, Area, PointerTransform and Matrix on IMGUI for scaled, split screen and render target UIs
- IMGUI.Put and IMGUI.Push to write custom components, with SingleParent and ListParent as bases
- Theme, and a Look on every component to restyle it
- StackPanel with spacing, padding, cross alignment, scrollbars and drag to scroll, which Vertical and Horizontal build on
- Popup, Dropdown, RadioButton, ProgressBar and Tabs
- Paragraph for wrapped and aligned text
- TextLayout to wrap, align and ellipsize text
- Selection, scrolling, multiline, word jumps, copy, cut and paste in Textbox
- IClipboard to connect the system clipboard
- IOnScreenKeyboard and SystemKeyboard for phones and consoles
- PushClip and PopClip, clips drawn by the shader that don't break the batch
- ClipRounding on stacks

### Changed

- Targets .NET 10
- Uses Apos.Shapes 0.8.5 and Apos.Input 3.1.0
- GuiHelper.Setup takes a ShapeFont instead of a FontSystem
- Font sizes are em sizes
- Ids are 64 bit and include the caller's file, so every Put and Push has a file parameter
- Up and down move the focus in their direction instead of to the previous and next component
- Default bindings listen to every gamepad
- Default is now Bindings, and each IMGUI has its own
- GuiHelper.Scale, WindowWidth, WindowHeight and UIMatrix moved to IMGUI
- IMGUI.Draw begins and ends the batch
- Buttons don't clip their content
- Clicked, FullWidth and FullHeight are read only

### Removed

- FontStashSharp
- GuiHelper.FontSystem, GetFont, MeasureStringTight and FontScale
- PushScissor and PopScissor

### Fixed

- Floating windows losing their order every frame
- Components getting another component's state when their ids collided
