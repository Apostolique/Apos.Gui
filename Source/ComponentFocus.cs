using System;

namespace Apos.Gui {
    /// <summary>
    /// Goal: Provides a way to control which component has focus.
    ///       This is especially useful for gamepad and keyboard controls.
    /// </summary>
    public class ComponentFocus {

        // Group: Constructors

        public ComponentFocus() : this(() => false, () => false) { }
        public ComponentFocus(Func<bool> prevFocusAction, Func<bool> nextFocusAction) {
            PrevFocusAction = prevFocusAction;
            NextFocusAction = nextFocusAction;
        }

        // Group: Public Variables

        public Component Root {
            get => _root;
            set {
                _root = value;
                Focus = findNext(Root);
            }
        }
        public Component Hover {
            get {
                return _hover;
            }
            set {
                if (_hover != null) {
                    _hover.IsHovered = false;
                }
                _hover = value;
                if (_hover != null) {
                    _hover.IsHovered = true;
                }
            }
        }
        public Component Focus {
            get {
                if (_focus == null) {
                    if (_oldFocus == null) {
                        return Root;
                    } else {
                        return _oldFocus;
                    }
                }
                return _focus;
            }
            set {
                _oldFocus = _focus;
                if (_focus != null) {
                    _focus.IsFocused = false;
                }
                Component focus = value;
                if (focus != null && !focus.IsFocusable) {
                    focus = findNext(focus);
                }
                _focus = focus;
                if (_focus != null) {
                    _focus.IsFocused = true;
                }
            }
        }
        public Func<bool> PrevFocusAction {
            get;
            set;
        }
        public Func<bool> NextFocusAction {
            get;
            set;
        }

        // Group: Public Functions

        public void UpdateSetup() {
            Root.UpdateSetup();
        }
        public void UpdateInput() {
            if (NextFocusAction()) {
                FocusNext();
            }
            if (PrevFocusAction()) {
                FocusPrev();
            }

            findHover();

            Root.UpdateInput();
        }
        public void Update() {
            Root.Update();
        }
        public void Draw() {
            GuiHelper.DrawGui(Root);
        }
        public void FocusPrev() {
            Focus = findPrev(Focus);
        }
        public void FocusNext() {
            Focus = findNext(Focus);
        }
        public void GrabFocus(Component c) {
            Focus = c;
        }

        // Group: Private Variables

        private Component _root;
        private Component _oldFocus;
        private Component _focus;
        private Component _hover;

        // Group: Private Functions

        private void findHover() {
            if (Hover != null) {
                var hover = Hover.FindHover();
                hover.Match(c => {
                    Hover = c;
                }, () => {
                    Hover = null;
                });
            }

            if (Hover == null) {
                var hover = Root.FindHover();
                hover.MatchSome(c => {
                    Hover = c;
                });
            }
        }
        private Component findPrev(Component c) {
            Component prevFocus;
            Component currentFocus = c;
            currentFocus.IsFocused = false;

            do {
                prevFocus = currentFocus;
                currentFocus = currentFocus.GetPrev();
                currentFocus = findFinalInverse(currentFocus);
            } while (!currentFocus.IsFocusable && currentFocus != prevFocus && currentFocus != c);

            if (currentFocus.IsFocusable) {
                currentFocus.IsFocused = true;
                return currentFocus;
            }
            return null;
        }
        private Component findNext(Component c) {
            Component prevFocus;
            Component currentFocus = c;
            currentFocus.IsFocused = false;

            do {
                prevFocus = currentFocus;
                currentFocus = currentFocus.GetNext();
                currentFocus = findFinal(currentFocus);
            } while (!currentFocus.IsFocusable && currentFocus != prevFocus && currentFocus != c);

            if (currentFocus.IsFocusable) {
                currentFocus.IsFocused = true;
                return currentFocus;
            }
            return null;
        }
        private Component findFinal(Component c) {
            Component prevFinal;
            Component currentFinal = c;
            do {
                prevFinal = currentFinal;
                currentFinal = prevFinal.GetFinal();
            } while (currentFinal != prevFinal && currentFinal != c);

            return currentFinal;
        }
        private Component findFinalInverse(Component c) {
            Component prevFinal;
            Component currentFinal = c;
            do {
                prevFinal = currentFinal;
                currentFinal = prevFinal.GetFinalInverse();
            } while (currentFinal != prevFinal && currentFinal != c);

            return currentFinal;
        }
    }
}
