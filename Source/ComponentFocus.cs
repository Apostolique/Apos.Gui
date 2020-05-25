using System;

namespace Apos.Gui {
    /// <summary>
    /// Goal: Provides a way to control which component has focus.
    ///       This is especially useful for gamepad and keyboard controls.
    /// </summary>
    public class ComponentFocus {

        // Group: Constructors

        public ComponentFocus(Component c) : this(c, () => false, () => false) { }
        public ComponentFocus(Component c, Func<bool> prevFocusAction, Func<bool> nextFocusAction) {
            RootComponent = c;

            Focus = findNext(RootComponent);

            PrevFocusAction = prevFocusAction;
            NextFocusAction = nextFocusAction;
        }

        // Group: Public Variables

        public Component RootComponent {
            get;
            set;
        }
        public Component Focus {
            get {
                if (_focus == null) {
                    if (_oldFocus == null) {
                        return RootComponent;
                    } else {
                        return _oldFocus;
                    }
                }
                return _focus;
            }
            set {
                _oldFocus = _focus;
                if (_focus != null) {
                    _focus.HasFocus = false;
                }
                Component focus = value;
                if (!focus.IsFocusable) {
                    focus = findNext(focus);
                }
                _focus = focus;
                if (_focus != null) {
                    _focus.HasFocus = true;
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
            RootComponent.UpdateSetup();
        }
        public bool UpdateInput() {
            bool usedInput = false;
            if (NextFocusAction()) {
                FocusNext();
                usedInput = true;
            }
            if (PrevFocusAction()) {
                FocusPrev();
                usedInput = true;
            }

            if (!usedInput) {
                usedInput = RootComponent.UpdateInput();
            }

            return usedInput;
        }
        public void Update() {
            RootComponent.Update();
        }
        public void Draw() {
            GuiHelper.DrawGui(RootComponent);
        }
        public void FocusPrev() {
            Focus = findPrev(Focus);
        }
        public void FocusNext() {
            Focus = findNext(Focus);
        }

        // Group: Private Variables

        private Component _oldFocus;
        private Component _focus;

        // Group: Private Functions

        private Component findPrev(Component c) {
            Component currentFocus = c;
            currentFocus.HasFocus = false;

            do {
                currentFocus = currentFocus.GetPrev();
                currentFocus = findFinalInverse(currentFocus);
            } while (!currentFocus.IsFocusable && currentFocus != c);

            if (currentFocus.IsFocusable) {
                currentFocus.HasFocus = true;
                return currentFocus;
            }
            return null;
        }
        private Component findNext(Component c) {
            Component currentFocus = c;
            currentFocus.HasFocus = false;

            do {
                currentFocus = currentFocus.GetNext();
                currentFocus = findFinal(currentFocus);
            } while (!currentFocus.IsFocusable && currentFocus != c);

            if (currentFocus.IsFocusable) {
                currentFocus.HasFocus = true;
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