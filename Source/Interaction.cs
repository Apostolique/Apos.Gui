using Apos.Input;
using Apos.Tweens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using MonoGame.Extended;
using Track = Apos.Input.Track;

namespace Apos.Gui {
    /// <summary>What started the current press.</summary>
    public enum InteractionSource {
        /// <summary>Nothing is pressing.</summary>
        None,
        /// <summary>The mouse button in <see cref="Bindings.MouseInteraction"/>.</summary>
        Mouse,
        /// <summary>A touch screen contact.</summary>
        Touch,
        /// <summary>The focus button in <see cref="Bindings.ButtonInteraction"/>, while the component has focus.</summary>
        Button
    }

    /// <summary>
    /// Hover, press, click and drag for one component, from the mouse, a touch screen, or the focus button.
    /// Call <see cref="UpdateInput"/> from the component's UpdateInput. The rest is read when the component updates or draws.
    /// </summary>
    /// <remarks>
    /// A press captures its pointer. It stays pressed when the pointer leaves the component and only clicks if it's released inside.
    /// Each touch contact is captured on its own, so two components can be pressed by two fingers at the same time.
    /// </remarks>
    public class Interaction {
        /// <summary>The mouse is over the component and nothing drawn above it claimed the mouse first. Touch never hovers.</summary>
        public bool Hovered { get; private set; }
        /// <summary>A press is in progress, even if the pointer moved outside.</summary>
        public bool Pressed => Source != InteractionSource.None;
        /// <summary>The pressing pointer is over the component. Always true for the focus button.</summary>
        public bool Inside { get; private set; }
        /// <summary>What started the current press.</summary>
        public InteractionSource Source { get; private set; } = InteractionSource.None;

        /// <summary>True on the frame a press is released inside the component.</summary>
        public bool Clicked => IsThisFrame(_clickFrame);
        /// <summary>True on the frame a press starts.</summary>
        public bool JustPressed => IsThisFrame(_pressFrame);
        /// <summary>True on the frame a press ends, clicked or not.</summary>
        public bool JustReleased => IsThisFrame(_releaseFrame);

        /// <summary>Where the pressing pointer is, in UI coordinates. The component's center for the focus button.</summary>
        public Vector2 Position { get; private set; }
        /// <summary>Where the current press started, in UI coordinates.</summary>
        public Vector2 PressPosition { get; private set; }
        /// <summary>How far the pressing pointer moved since the last frame.</summary>
        public Vector2 Delta { get; private set; }

        /// <summary>Goes from 0 to 1 while hovered and back to 0 after. Useful for animating a look.</summary>
        public float Hover => _hover.Value;
        /// <summary>Goes from 0 to 1 while pressed inside and back to 0 after. Useful for animating a look.</summary>
        public float Press => _press.Value;
        /// <summary>Milliseconds for <see cref="Hover"/> to go all the way from 0 to 1.</summary>
        public long HoverDuration { get; set; } = 150;
        /// <summary>Milliseconds for <see cref="Press"/> to go all the way from 0 to 1.</summary>
        public long PressDuration { get; set; } = 80;

        /// <summary>Whether a mouse or touch press gives focus to the component. Only applies when the component is focusable.</summary>
        public bool GrabsFocus { get; set; } = true;
        /// <summary>Whether the focus button presses the component while it has focus. A textbox turns this off so that space and enter stay text.</summary>
        public bool UsesFocusButton { get; set; } = true;
        /// <summary>Whether the mouse can hover and press the component.</summary>
        public bool UsesMouse { get; set; } = true;
        /// <summary>Whether touch contacts can press the component.</summary>
        public bool UsesTouch { get; set; } = true;

        /// <summary>
        /// Processes this frame's input for a component. Parents call their children's UpdateInput from the top of the draw order down,
        /// so the first component under the pointer gets it.
        /// </summary>
        /// <param name="owner">The component this interaction belongs to.</param>
        /// <param name="area">Where the pointer can press, in UI coordinates. Defaults to the owner's clip. A scrollbar uses it for its thumb.</param>
        public void UpdateInput(IComponent owner, RectangleF? area = null) {
            _owner = owner;
            RectangleF hit = area is RectangleF a ? a.Intersect(owner.Clip) : owner.Clip;
            IMGUI ui = owner.Root;
            Bindings bindings = ui.Bindings;
            Vector2 mouse = ui.Mouse;
            bool mouseInside = UsesMouse && bindings.UsesPointer && Apos.Input.Pointer.Source == PointerSource.Mouse && InputHelper.IsActive && hit.Contains(mouse);

            IComponent? capture = ui.MouseCapture;
            Hovered = mouseInside && (capture == null || capture == owner) && ui.TryClaimHover(owner);

            Vector2 old = Position;
            if (Source == InteractionSource.None) {
                if (Hovered && bindings.MouseInteraction.Pressed()) {
                    Start(InteractionSource.Mouse, mouse);
                } else if (UsesTouch && bindings.UsesPointer && TryClaimTouch(ui, hit, out TouchLocation touch)) {
                    _touchId = touch.Id;
                    Start(InteractionSource.Touch, ui.ToUI(touch.Position));
                } else if (UsesFocusButton && owner.IsFocused && bindings.ButtonInteraction.Pressed()) {
                    Start(InteractionSource.Button, owner.Bounds.Center);
                }
                old = Position;
            } else if ((_grabbedFocus && !owner.IsFocused) || !InputHelper.IsActive) {
                End(false);
            } else if (Source == InteractionSource.Mouse) {
                Position = mouse;
                if (bindings.MouseInteraction.Released()) {
                    End(hit.Contains(Position));
                } else {
                    bindings.MouseInteraction.Consume();
                }
            } else if (Source == InteractionSource.Touch) {
                if (!InputHelper.NewTouch.FindById(_touchId, out TouchLocation touch)) {
                    End(false);
                } else {
                    Position = ui.ToUI(touch.Position);
                    if (Input.TouchCondition.Released(touch)) {
                        End(hit.Contains(Position));
                    } else {
                        Track.TouchCondition.Consume(_touchId);
                    }
                }
            } else if (Source == InteractionSource.Button) {
                Position = owner.Bounds.Center;
                if (bindings.ButtonInteraction.Released()) {
                    End(true);
                } else {
                    bindings.ButtonInteraction.Consume();
                }
            }

            Delta = Position - old;
            Inside = Source == InteractionSource.Button || Pressed && hit.Contains(Position);

            Animate(_hover, Hovered ? 1f : 0f, HoverDuration);
            Animate(_press, Pressed && Inside ? 1f : 0f, PressDuration);
        }

        /// <summary>Drops the current press without clicking.</summary>
        public void Cancel() {
            if (Pressed) {
                End(false);
            }
        }

        private void Start(InteractionSource source, Vector2 position) {
            Source = source;
            Position = position;
            PressPosition = position;
            _pressFrame = _owner!.Root.CurrentFrame;
            if (source == InteractionSource.Mouse) {
                _owner.Root.MouseCapture = _owner;
            }

            _grabbedFocus = false;
            if (source != InteractionSource.Button && GrabsFocus && _owner.IsFocusable) {
                _owner.Root.GrabFocus(_owner);
                _grabbedFocus = true;
            }
        }
        private void End(bool clicked) {
            if (Source == InteractionSource.Mouse && _owner!.Root.MouseCapture == _owner) {
                _owner.Root.MouseCapture = null;
            }
            Source = InteractionSource.None;
            _releaseFrame = _owner!.Root.CurrentFrame;
            if (clicked) {
                _clickFrame = _releaseFrame;
            }
            _grabbedFocus = false;
        }

        private static bool TryClaimTouch(IMGUI ui, RectangleF hit, out TouchLocation touch) {
            foreach (TouchLocation t in InputHelper.NewTouch) {
                if (Input.TouchCondition.Pressed(t) && Track.TouchCondition.IsUnique(t.Id) && hit.Contains(ui.ToUI(t.Position))) {
                    Track.TouchCondition.Consume(t.Id);
                    touch = t;
                    return true;
                }
            }
            touch = default;
            return false;
        }

        private static void Animate(FloatTween tween, float target, long duration) {
            if (tween.B == target) return;

            float current = tween.Value;
            tween.A = current;
            tween.B = target;
            tween.StartTime = TweenHelper.TotalMS;
            tween.Duration = (long)(duration * MathHelper.Distance(current, target));
        }

        private bool IsThisFrame(uint frame) => frame != 0 && _owner != null && frame == _owner.Root.CurrentFrame;

        private IComponent? _owner;
        private int _touchId;
        private bool _grabbedFocus;
        private uint _pressFrame;
        private uint _releaseFrame;
        private uint _clickFrame;

        private readonly FloatTween _hover = new(0f, 0f, 0, Easing.QuadOut);
        private readonly FloatTween _press = new(0f, 0f, 0, Easing.QuadOut);
    }
}
