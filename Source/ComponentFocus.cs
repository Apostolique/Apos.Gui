using System;

namespace AposGui {
    /// <summary>
    /// Goal: Provides a way to control which component has focus.
    ///       This is especially useful for gamepad and keyboard controls.
    /// </summary>
    public class ComponentFocus {
        public ComponentFocus(Component c, Func<bool> previousFocusAction, Func<bool> nextFocusAction) {
            RootComponent = c;

            Focus = findNext(RootComponent);

            _previousFocusAction = previousFocusAction;
            _nextFocusAction = nextFocusAction;
        }
        public Component RootComponent {
            get;
            set;
        }
        public Component Focus {
            get => _focus;
            set {
                if (_focus != null) {
                    _focus.HasFocus = false;
                }
                _focus = value;
                if (_focus != null) {
                    _focus.HasFocus = true;
                }
            }
        }

        private Component _focus;
        private Func<bool> _previousFocusAction;
        private Func<bool> _nextFocusAction;

        public bool UpdateInput() {
            bool usedInput = false;
            if (_nextFocusAction()) {
                FocusNext();
                usedInput = true;
            }
            if (_previousFocusAction()) {
                FocusPrevious();
                usedInput = true;
            }

            return usedInput;
        }

        public void FocusPrevious() {
            Focus = findPrevious(Focus);
        }
        public void FocusNext() {
            Focus = findNext(Focus);
        }
        private Component findPrevious(Component c) {
            if (c == null) {
                c = RootComponent;
            }
            Component currentFocus = c;
            currentFocus.HasFocus = false;

            do {
                currentFocus = currentFocus.GetPrevious();
                currentFocus = findFinalInverse(currentFocus);
            } while (!currentFocus.IsFocusable && currentFocus != c);

            if (currentFocus.IsFocusable) {
                currentFocus.HasFocus = true;
                return currentFocus;
            }
            return null;
        }
        private Component findNext(Component c) {
            if (c == null) {
                c = RootComponent;
            }
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
            Component previousFinal;
            Component currentFinal = c;
            do {
                previousFinal = currentFinal;
                currentFinal = previousFinal.GetFinal();
            } while (currentFinal != previousFinal && currentFinal != c);

            return currentFinal;
        }
        private Component findFinalInverse(Component c) {
            Component previousFinal;
            Component currentFinal = c;
            do {
                previousFinal = currentFinal;
                currentFinal = previousFinal.GetFinalInverse();
            } while (currentFinal != previousFinal && currentFinal != c);

            return currentFinal;
        }
    }
}