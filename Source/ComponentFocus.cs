using System;
using Microsoft.Xna.Framework.Graphics;

namespace AposGui {
    /// <summary>
    /// Goal: Provides a way to control which component has focus.
    ///       This is especially useful for gamepad and keyboard controls.
    /// </summary>
    public class ComponentFocus {
        //constructors
        public ComponentFocus(Component c) : this(c, () => false, () => false) { }
        public ComponentFocus(Component c, Func<bool> previousFocusAction, Func<bool> nextFocusAction) {
            RootComponent = c;

            Focus = findNext(RootComponent);

            PreviousFocusAction = previousFocusAction;
            NextFocusAction = nextFocusAction;
        }

        //public vars
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
                _focus = value;
                if (_focus != null) {
                    _focus.HasFocus = true;
                }
            }
        }
        public Func<bool> PreviousFocusAction {
            get;
            set;
        }
        public Func<bool> NextFocusAction {
            get;
            set;
        }

        //public functions
        public void UpdateSetup() {
            GuiHelper.UpdateSetup();
            RootComponent.UpdateSetup();
        }
        public bool UpdateInput() {
            bool usedInput = false;
            if (NextFocusAction()) {
                FocusNext();
                usedInput = true;
            }
            if (PreviousFocusAction()) {
                FocusPrevious();
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
        public void Draw(SpriteBatch s) {
            GuiHelper.DrawGui(s, RootComponent);
        }
        public void FocusPrevious() {
            Focus = findPrevious(Focus);
        }
        public void FocusNext() {
            Focus = findNext(Focus);
        }

        //private vars
        private Component _oldFocus;
        private Component _focus;

        //private functions
        private Component findPrevious(Component c) {
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