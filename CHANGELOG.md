# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/), and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- GuiHelper.Clock, the clock that UI animations and delays run on, separate from TweenHelper.Clock

### Changed

- Uses Apos.Shapes 0.9.3 and Apos.Tweens 0.3.0
- GuiHelper.UpdateSetup no longer moves TweenHelper.Clock
- Nested clips keep their own rounding and rotation, so a rounded stack inside another rounded stack is cut by both
- Scrolling again before a scroll ends keeps its speed
- HoverDuration, PressDuration and ScrollMaxDuration are doubles
- StackPanel's protected offset tweens are EasedValues, and Textbox's caret blink is a TweenPlayer

### Removed

- StackPanel.SetOffset

### Fixed

- Floating window content getting rounded corners under the title bar

## [3.0.0] - 2026-10-10

### Added

- Touch input for every component
- Interaction, the hover, press, click, drag and focus handling that components share
- Directional focus with the arrow keys and the d-pad, with IParent.Navigate for custom layouts and focus scopes for popups and windows
- Bindings, the inputs of one UI, with Bindings.ForGamePad and Bindings.ForKeyboardAndMouse for split screen
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
- Text on checkboxes and radio buttons, pressing it works like pressing the box
- IMGUI.InvalidateLayout for components whose size depends on the size they get
- IMGUI.DrawOnTop to draw over the whole UI, and IMGUI.Focused
- Room for the scrollbar beside the children of a stack that scrolls
- Theme.UseOutline and Theme.UseFilled, and Surface, Panel and Accent colors
- A title bar, a close button, Content, StartAlign and StartOffset on FloatingWindow
- Delay, Gap, Padding and Bubble on Tooltip

### Changed

- Targets .NET 10
- Uses Apos.Shapes 0.8.5 and Apos.Input 3.2.0
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
- Tooltip takes the component first and the tip second, shows after a delay, goes next to the component and stays inside the UI
- FloatingWindow is dragged by its title bar, opens centered and stays inside the UI
- Slider is a track with a thumb, 200 wide by default
- MenuPanel has 8 units of spacing

### Removed

- FontStashSharp
- GuiHelper.FontSystem, GetFont, MeasureStringTight and FontScale
- PushScissor and PopScissor

### Fixed

- Floating windows losing their order every frame
- Components getting another component's state when their ids collided

## [2.3.2-alpha] - 2026-01-23

### Changed

- Uses Apos.Shapes 0.6.3-alpha, so games don't need to reference Apos.Shapes themselves

## [2.3.1-alpha] - 2026-01-21

### Added

- Block, a component with a fixed size
- CenteredVertical for menus
- Root on every component, the IMGUI it belongs to

### Changed

- Components grab focus through Root.GrabFocus

### Fixed

- Non-premultiplied colors with Apos.Shapes

## [2.3.0-alpha] - 2026-01-13

### Changed

- Draws with Apos.Shapes
- Uses FontStashSharp 1.5.2

## [2.2.0] - 2026-01-07

### Added

- Apos.Gui.KNI, a package for KNI

### Changed

- Uses MonoGame.Extended 5.3.1, Apos.Input 2.5.0 and Apos.Tweens 0.2.0

## [2.1.0] - 2024-10-14

### Added

- Tooltip

### Changed

- Targets .NET 8
- Uses MonoGame.Extended 4.0.3

## [2.0.3] - 2024-02-06

### Fixed

- Textbox with null text

## [2.0.2] - 2024-02-06

### Fixed

- Pop on components that inherited the wrong one from their base class

## [2.0.1] - 2024-01-29

### Fixed

- Dock not updating its clip

## [2.0.0] - 2023-04-21

### Changed

- Layout happens before draw, so components show up on the frame they're put

## [1.0.12] - 2023-04-10

### Changed

- Uses FontStashSharp 1.2.8

### Fixed

- Wrong gamepad buttons in the defaults

## [1.0.11] - 2022-03-27

### Changed

- A button's label isn't part of the focus cycle

### Fixed

- Dock not updating its child
- Constructor and override warnings

## [1.0.10] - 2022-02-07

### Fixed

- Dock GetPrev

## [1.0.9] - 2022-02-04

### Added

- Horizontal and Dock

### Changed

- Panel is now Vertical

### Fixed

- Clips extending outside their component's bounds
- Child sizes in Vertical and Horizontal, and horizontal scrolling

## [1.0.8] - 2022-01-29

### Added

- Font size and color on Textbox

### Changed

- Labels only measure their text again when it changes

## [1.0.7] - 2022-01-29

### Added

- Back in the defaults: Escape, B, Back on a gamepad and the mouse's XButton2
- XML docs on IComponent

### Changed

- Panels only draw the children they show

### Fixed

- Window size with the UI scale
- Queue next tick
- Scroll handling

## [1.0.6-alpha] - 2022-01-28

### Added

- MenuPanel

### Changed

- Animations use Apos.Tweens

## [1.0.5-alpha] - 2021-12-07

### Added

- Smooth scrolling in panels, and scrolling a focused component into view
- A background on floating windows

### Fixed

- Button children not being clipped
- Textbox size

## [1.0.4-alpha] - 2021-08-03

### Added

- Checkbox
- Floating windows
- GrabFocus takes null

### Changed

- Uses Apos.Input 2.4.1

### Fixed

- Focus switching to the component that already had it
- Inputs processed in the opposite order

## [1.0.3-alpha] - 2021-03-22

### Added

- Absolute ids
- Reparenting
- Queue next tick

### Changed

- Panel.Put is now Panel.Push

### Fixed

- Button clicks
- Previous focus handling
- Id generation, and the id of a button's label
- Focus switching in the middle of a frame

## [1.0.2-alpha] - 2021-02-17

### Added

- MeasureString with line height, cached
- GrabFocus

### Changed

- Private component members are protected

## [1.0.1-alpha] - 2021-02-06

### Added

- Keyboard and gamepad controls on Button, and gamepad in more defaults
- A new Slider

### Fixed

- Focus staying on a component that was removed
- Textbox cursor blink not resetting on focus

## [1.0.0-alpha] - 2021-02-05

### Changed

- New IMGUI-style API
- Uses MonoGame.Extended 3.8.0, FontStashSharp 0.9.2 and Apos.Input 2.3.1

### Removed

- The Optional dependency

## [0.16.2] - 2020-11-27

### Changed

- Scroll operation signatures

## [0.16.1] - 2020-11-27

### Changed

- Operation signatures

## [0.16.0] - 2020-11-27

### Changed

- Reworked input, hover and focus

## [0.15.0] - 2020-10-20

### Added

- SourceLink

### Changed

- Uses FontStashXNA instead of SpriteFontPlus, and Apos.Input 1.0.1

## [0.14.3] - 2020-10-01

### Fixed

- Infinite loop when scaling

## [0.14.2] - 2020-09-06

### Changed

- Uses newer SpriteFontPlus and MonoGame.Framework

## [0.14.1] - 2020-05-25

### Fixed

- ComponentFocus setting the focus to null

## [0.14.0] - 2020-05-25

### Changed

- GetPrevious is now GetPrev
- Uses SpriteFontPlus 0.8.0.37

## [0.13.0] - 2020-05-25

### Changed

- Uses Apos.Input 1.0.0

## [0.12.2] - 2020-03-22

### Changed

- Uses Apos.Input 0.8.2

## [0.12.1] - 2020-01-12

### Added

- GuiHelper.Setup, which sets up Apos.Input and the font

### Changed

- DrawString and MeasureString are back on GuiHelper
- Uses Apos.Input 0.8.0

## [0.11.5] - 2020-01-12

### Changed

- Uses Apos.Input 0.7.0

## [0.11.4] - 2020-01-05

### Fixed

- Space, Enter and the gamepad's A each press a button on their own again

## [0.11.3] - 2020-01-05 [YANKED]

### Fixed

- Space and Enter each press a button on their own, while the gamepad's A doesn't press it at all

## [0.11.2] - 2020-01-05 [YANKED]

### Changed

- The defaults use Apos.Input's conditions, and a button needs Space, Enter and A held together
- Uses SpriteFontPlus 0.7.1.25 and Apos.Input 0.6.2

## [0.11.1] - 2019-11-24

### Changed

- MonoGame.Extended is a regular dependency
- Uses MonoGame.Framework.DesktopGL.Core 3.7.0.7 and Apos.Input 0.6.1

## [0.10.1] - 2019-09-25

### Changed

- Uses Apos.Input 0.5.1

## [0.10.0] - 2019-09-18

### Changed

- Uses MonoGame.Extended 3.7.0, SpriteFontPlus 0.7.0.22 and Apos.Input 0.4.0

### Removed

- The .NET Framework 4.5 target

## [0.9.14] - 2019-03-07

### Fixed

- Centering in LayoutVerticalCenter

## [0.9.13] - 2019-03-05

### Changed

- Uses Apos.Input 0.2.2

## [0.9.12] - 2019-03-05

### Changed

- Uses Apos.Input 0.2.1

## [0.9.11] - 2019-02-23

### Fixed

- LayoutVertical
- ClippingRect's get and set calling each other
- Border's ClippingRect

## [0.9.10] - 2019-02-21

### Added

- XML docs on GuiHelper, Default and the components

### Fixed

- Icon not being public

## [0.9.8] - 2019-02-19

### Added

- Icon
- An example project

### Changed

- Uses SpriteFontPlus 0.6.2.14 and Apos.Input 0.1.4

### Removed

- The Newtonsoft.Json dependency

## [0.9.7] - 2019-02-18

### Added

- ScreenToUI and UIToScreen on a single axis
- Scrolling over panels in the defaults
- XML documentation in the package

### Changed

- Input is ignored while the game isn't active
- Uses SpriteFontPlus 0.6.1.14

## [0.9.5] - 2019-02-06

### Changed

- DefaultInput is now Default

## [0.9.2] - 2019-02-06 [YANKED]

### Changed

- DefaultInput is now Defaults, with more defaults
- Uses Apos.Input 0.1.1

## [0.9.1] - 2019-02-04

### Added

- Documentation

### Changed

- Input goes through Apos.Input 0.1.0

## [0.9.0] - 2019-02-01

### Added

- Button, Label, LabelDynamic, Border, Panel, PanelVerticalScroll, ScreenPanel, LayoutVertical, LayoutVerticalCenter and Switcher
- Focus that moves with the keyboard and a gamepad, and mouse input

[Unreleased]: https://github.com/Apostolique/Apos.Gui/compare/v3.0.0...HEAD
[3.0.0]: https://github.com/Apostolique/Apos.Gui/compare/v2.3.2-alpha...v3.0.0
[2.3.2-alpha]: https://github.com/Apostolique/Apos.Gui/compare/v2.3.1-alpha...v2.3.2-alpha
[2.3.1-alpha]: https://github.com/Apostolique/Apos.Gui/compare/v2.3.0-alpha...v2.3.1-alpha
[2.3.0-alpha]: https://github.com/Apostolique/Apos.Gui/compare/v2.2.0...v2.3.0-alpha
[2.2.0]: https://github.com/Apostolique/Apos.Gui/compare/v2.1.0...v2.2.0
[2.1.0]: https://github.com/Apostolique/Apos.Gui/compare/v2.0.3...v2.1.0
[2.0.3]: https://github.com/Apostolique/Apos.Gui/compare/v2.0.2...v2.0.3
[2.0.2]: https://github.com/Apostolique/Apos.Gui/compare/v2.0.1...v2.0.2
[2.0.1]: https://github.com/Apostolique/Apos.Gui/compare/v2.0.0...v2.0.1
[2.0.0]: https://github.com/Apostolique/Apos.Gui/compare/v1.0.12...v2.0.0
[1.0.12]: https://github.com/Apostolique/Apos.Gui/compare/v1.0.11...v1.0.12
[1.0.11]: https://github.com/Apostolique/Apos.Gui/compare/v1.0.10...v1.0.11
[1.0.10]: https://github.com/Apostolique/Apos.Gui/compare/v1.0.9...v1.0.10
[1.0.9]: https://github.com/Apostolique/Apos.Gui/compare/v1.0.8...v1.0.9
[1.0.8]: https://github.com/Apostolique/Apos.Gui/compare/v1.0.7...v1.0.8
[1.0.7]: https://github.com/Apostolique/Apos.Gui/compare/v1.0.6-alpha...v1.0.7
[1.0.6-alpha]: https://github.com/Apostolique/Apos.Gui/compare/v1.0.5-alpha...v1.0.6-alpha
[1.0.5-alpha]: https://github.com/Apostolique/Apos.Gui/compare/v1.0.4-alpha...v1.0.5-alpha
[1.0.4-alpha]: https://github.com/Apostolique/Apos.Gui/compare/v1.0.3-alpha...v1.0.4-alpha
[1.0.3-alpha]: https://github.com/Apostolique/Apos.Gui/compare/v1.0.2-alpha...v1.0.3-alpha
[1.0.2-alpha]: https://github.com/Apostolique/Apos.Gui/compare/v1.0.1-alpha...v1.0.2-alpha
[1.0.1-alpha]: https://github.com/Apostolique/Apos.Gui/compare/v1.0.0-alpha...v1.0.1-alpha
[1.0.0-alpha]: https://github.com/Apostolique/Apos.Gui/compare/v0.16.2...v1.0.0-alpha
[0.16.2]: https://github.com/Apostolique/Apos.Gui/compare/v0.16.1...v0.16.2
[0.16.1]: https://github.com/Apostolique/Apos.Gui/compare/v0.16.0...v0.16.1
[0.16.0]: https://github.com/Apostolique/Apos.Gui/compare/v0.15.0...v0.16.0
[0.15.0]: https://github.com/Apostolique/Apos.Gui/compare/v0.14.3...v0.15.0
[0.14.3]: https://github.com/Apostolique/Apos.Gui/compare/v0.14.2...v0.14.3
[0.14.2]: https://github.com/Apostolique/Apos.Gui/releases/tag/v0.14.2
[0.14.1]: https://www.nuget.org/packages/Apos.Gui/0.14.1
[0.14.0]: https://www.nuget.org/packages/Apos.Gui/0.14.0
[0.13.0]: https://www.nuget.org/packages/Apos.Gui/0.13.0
[0.12.2]: https://www.nuget.org/packages/Apos.Gui/0.12.2
[0.12.1]: https://www.nuget.org/packages/Apos.Gui/0.12.1
[0.11.5]: https://www.nuget.org/packages/Apos.Gui/0.11.5
[0.11.4]: https://www.nuget.org/packages/Apos.Gui/0.11.4
[0.11.3]: https://www.nuget.org/packages/Apos.Gui/0.11.3
[0.11.2]: https://www.nuget.org/packages/Apos.Gui/0.11.2
[0.11.1]: https://www.nuget.org/packages/Apos.Gui/0.11.1
[0.10.1]: https://www.nuget.org/packages/Apos.Gui/0.10.1
[0.10.0]: https://www.nuget.org/packages/Apos.Gui/0.10.0
[0.9.14]: https://www.nuget.org/packages/Apos.Gui/0.9.14
[0.9.13]: https://www.nuget.org/packages/Apos.Gui/0.9.13
[0.9.12]: https://www.nuget.org/packages/Apos.Gui/0.9.12
[0.9.11]: https://www.nuget.org/packages/Apos.Gui/0.9.11
[0.9.10]: https://www.nuget.org/packages/Apos.Gui/0.9.10
[0.9.8]: https://www.nuget.org/packages/Apos.Gui/0.9.8
[0.9.7]: https://www.nuget.org/packages/Apos.Gui/0.9.7
[0.9.5]: https://www.nuget.org/packages/Apos.Gui/0.9.5
[0.9.2]: https://www.nuget.org/packages/Apos.Gui/0.9.2
[0.9.1]: https://www.nuget.org/packages/Apos.Gui/0.9.1
[0.9.0]: https://www.nuget.org/packages/Apos.Gui/0.9.0
