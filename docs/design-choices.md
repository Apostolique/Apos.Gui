# Design choices

This library's goal is to make it easy to create custom user-facing GUIs. It is a hybrid between the IMGUI paradigm and regular retained GUIs. When using the library, it looks like IMGUI. When implementing new components, it looks retained.

This library is built on top of [Apos.Input](https://apostolique.github.io/Apos.Input/) which is a polling-based input library. This is what powers conflict resolution in the interface.
