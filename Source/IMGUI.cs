using System;
using System.Collections.Generic;
using System.Linq;
using Apos.Input;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Apos.Gui {
    // NOTE: IMGUI is NOT recursive. It should always be the top level component.
    public class IMGUI : ListParent {
        public IMGUI() : base(0) {
            Root = this;
            _currentParent = this;
            _activeComponents.Add(Id, this);

            GuiHelper.CurrentIMGUI = this;
        }

        /// <summary>How big the UI is drawn. 2f makes everything twice as big. Defaults to 1f.</summary>
        public float Scale {
            get => _scale;
            set {
                if (value > 0f) {
                    _scale = value;
                }
            }
        }
        /// <summary>
        /// Where the UI goes on the screen, in pixels. The UI lays out in this area divided by <see cref="Scale"/>.
        /// Null fills the window. When you draw the UI to a render target, set it to the render target's size.
        /// </summary>
        public RectangleF? Area { get; set; }
        /// <summary>
        /// Maps a screen position to the space the UI is drawn in, before <see cref="Area"/> and <see cref="Scale"/> apply.
        /// Null means the UI is drawn straight to the screen. Set it when the UI is drawn to a render target that's shown somewhere else,
        /// like on a screen in the game world, so the mouse and touch still hit the right spot.
        /// </summary>
        public Func<Vector2, Vector2>? PointerTransform { get; set; }
        /// <summary>The inputs this UI listens to.</summary>
        public Bindings Bindings { get; set; } = new();

        /// <summary>Goes from the UI's coordinates to the pixels it's drawn to.</summary>
        public Matrix Matrix {
            get {
                RectangleF area = AreaOrWindow;
                return Matrix.CreateScale(Scale, Scale, 1f) * Matrix.CreateTranslation(area.X, area.Y, 0f);
            }
        }
        /// <summary>Converts a position from the screen to the UI's coordinates.</summary>
        public Vector2 ToUI(Vector2 screen) {
            if (PointerTransform != null) {
                screen = PointerTransform(screen);
            }
            return Vector2.Transform(screen, Matrix.Invert(Matrix));
        }
        /// <summary>Position of the mouse or of the oldest touch contact in the UI's coordinates.</summary>
        public Vector2 Pointer => ToUI(Apos.Input.Pointer.Position);
        /// <summary>Position of InputHelper.OldMouse in the UI's coordinates.</summary>
        public Vector2 OldMouse => ToUI(InputHelper.OldMouse.Position.ToVector2());
        /// <summary>Position of InputHelper.NewMouse in the UI's coordinates.</summary>
        public Vector2 Mouse => ToUI(InputHelper.NewMouse.Position.ToVector2());

        /// <summary>
        /// Counts the UI's frames. Starts a new frame in UpdateSetup. Components that weren't put during the
        /// current frame are removed in Cleanup.
        /// </summary>
        public uint CurrentFrame { get; private set; } = 0;

        /// <summary>
        /// Only call this if you didn't call UpdateStart. Starts a new frame.
        /// </summary>
        public override void UpdateSetup(GameTime gameTime) {
            _previous = GuiHelper.CurrentIMGUI;
            GuiHelper.CurrentIMGUI = this;

            CurrentFrame++;
            LastPing = CurrentFrame;
            _idsUsedThisFrame.Add(Id, 1);

            base.UpdateSetup(gameTime);
        }
        /// <summary>
        /// Only call this if you didn't call UpdateStart. This should be called after UpdateSetup.
        /// </summary>
        /// <param name="gameTime">Current gametime.</param>
        public override void UpdateInput(GameTime gameTime) {
            base.UpdateInput(gameTime);

            if (!_nextPressed && Bindings.FocusPrev.Pressed()) {
                _prevPressed = true;
            } else if (_prevPressed) {
                if (Bindings.FocusPrev.Released()) {
                    FindPrevFocus();
                    _prevPressed = false;
                } else {
                    Bindings.FocusPrev.Consume();
                }
            }

            if (!_prevPressed && Bindings.FocusNext.Pressed()) {
                _nextPressed = true;
            } else if (_nextPressed) {
                if (Bindings.FocusNext.Released()) {
                    FindNextFocus();
                    _nextPressed = false;
                } else {
                    Bindings.FocusNext.Consume();
                }
            }
        }
        /// <summary>
        /// Only call this if you didn't call UpdateStart. This should be called after UpdateInput.
        /// </summary>
        /// <param name="gameTime">Current gametime</param>
        public override void Update(GameTime gameTime) {
            base.Update(gameTime);
        }

        /// <summary>
        /// Call this at the start of your update loop.
        /// It calls UpdateSetup, UpdateInput, and Update.
        /// </summary>
        /// <param name="gameTime">Current gametime.</param>
        /// <param name="callUpdateInput">false will skip UpdateInput.</param>
        public void UpdateStart(GameTime gameTime, bool callUpdateInput = true) {
            UpdateSetup(gameTime);
            if (callUpdateInput)
                UpdateInput(gameTime);
            Update(gameTime);
        }

        /// <summary>
        /// Don't call this.
        /// </summary>
        public override void UpdatePrefSize(GameTime gameTime) { }
        /// <summary>
        /// Only call this if you didn't call UpdateEnd. This should be called after Cleanup.
        /// </summary>
        /// <param name="gameTime">Current gametime</param>
        public override void UpdateLayout(GameTime gameTime) {
            IMGUI previous = MakeCurrent();

            // IMGUI manages itself so it can set its own position and size.
            RectangleF area = AreaOrWindow;
            X = 0f;
            Y = 0f;
            Width = area.Width / Scale;
            Height = area.Height / Scale;

            foreach (var c in _children) {
                c.UpdatePrefSize(gameTime);
            }
            base.UpdateLayout(gameTime);

            GuiHelper.CurrentIMGUI = previous;
        }

        /// <summary>
        /// This should be called at the end of your update loop.
        /// </summary>
        public void UpdateEnd(GameTime gameTime) {
            Cleanup();
            UpdateLayout(gameTime);
        }

        /// <summary>
        /// Draws all components in the UI.
        /// </summary>
        /// <param name="gameTime">Current gametime.</param>
        public override void Draw(GameTime gameTime) {
            IMGUI previous = MakeCurrent();

            GuiHelper.Begin();
            base.Draw(gameTime);
            GuiHelper.End();

            GuiHelper.CurrentIMGUI = previous;
        }

        /// <summary>
        /// Inserts a parent at the top of the parent stack.
        /// </summary>
        /// <param name="p">The parent to insert.</param>
        /// <param name="maxChildren">The parent's max amount of children, 0 means infinite.</param>
        public void Push(IParent p, int maxChildren = 0) {
            _parents.Push((_currentParent, _maxChildren, _childrenCount));
            _currentParent = p;
            _maxChildren = maxChildren;
            _childrenCount = 0;
            PushId(p.Id);
        }
        /// <summary>
        /// Removes a parent from the top of the parent stack.
        /// </summary>
        public void Pop() {
            var pop = _parents.Pop();
            _currentParent = pop.Parent;
            _maxChildren = pop.MaxChildren;
            _childrenCount = pop.ChildrenCount;
            PopId();
        }
        /// <summary>
        /// Inserts an id at the top of the id stack.
        /// </summary>
        /// <param name="id">The id to push onto the id stack.</param>
        public void PushId(long id) {
            _idStack.Push(_idHash);
            _idHash = Combine(_idHash, id);
        }
        /// <summary>
        /// Removes an id from the top of the id stack.
        /// </summary>
        public void PopId() {
            _idHash = _idStack.Pop();
        }
        /// <summary>
        /// Tries to get the component with the given id if it exists.
        /// </summary>
        /// <param name="id">The id of the component to get.</param>
        /// <param name="c">When this method returns, contains the component associated with the specified id.</param>
        /// <returns>true if the component with the specified id is found; otherwise false.</returns>
        public bool TryGetValue(long id, out IComponent? c) {
            return _activeComponents.TryGetValue(id, out c);
        }
        /// <summary>
        /// Used for parenting a child component to a parent.
        /// If a child already has a parent, it will be adopted by the current parent if the parent is different.
        /// </summary>
        /// <param name="c">The child that will be parented.</param>
        public void GrabParent(IComponent c) {
            IParent current = _currentParent;

            if (c.Parent == null) {
                _activeComponents.Add(c.Id, c);
            }

            // Note: This might be considered a hack. Is it more proper to reset parent components during their LastPing?
            if (c is IParent p) {
                p.Reset();
            }

            if (c.Parent != current || current.PeekNextIndex() != c.Index) {
                c.Parent?.Remove(c);
                c.Index = current.NextIndex();
                c.Root = this;

                current.Add(c);
            } else {
                // Same parent, same slot as last frame. Leave it in place so that its render order is kept.
                current.NextIndex();
            }

            if (_maxChildren > 0 && ++_childrenCount >= _maxChildren) {
                Pop();
            }

            c.LastPing = CurrentFrame;
        }
        /// <summary>
        /// Gives focus to a component. Can also clear the focus if null is passed.
        /// </summary>
        /// <param name="c">The component that should receive focus.</param>
        public void GrabFocus(IComponent? c) {
            if (c == null) {
                Focus = null;
            } else if (Focus != c.Id) {
                Focus = c.Id;
                c.SendToTop();
            }
        }
        /// <summary>
        /// Claims the mouse hover for this frame. Components check from the top of the draw order down,
        /// so the first one under the mouse gets it and the ones below it don't.
        /// </summary>
        /// <returns>True if the component got the hover or already had it this frame.</returns>
        public bool TryClaimHover(IComponent c) {
            if (_hoverFrame == CurrentFrame) {
                return _hover == c;
            }
            _hover = c;
            _hoverFrame = CurrentFrame;
            return true;
        }
        /// <summary>The component under the mouse this frame, if one claimed it.</summary>
        public IComponent? Hover => _hoverFrame == CurrentFrame ? _hover : null;
        /// <summary>The component holding a mouse press. While it's set, other components don't get hovered.</summary>
        public IComponent? MouseCapture { get; set; }

        /// <summary>
        /// Guaranteed to return a unique id during the span of the current frame.
        /// Calls that would get the same id, like a loop or two calls on one line, are told apart by their order.
        /// </summary>
        /// <param name="id">An id that should be part of the generation process.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the id stack and the file.</param>
        /// <param name="file">The caller's file.</param>
        public long CreateId(int id, bool isAbsoluteId, string file) {
            long result = isAbsoluteId ? id : Combine(Combine(_idHash, file.GetHashCode()), id);

            if (_idsUsedThisFrame.TryGetValue(result, out int count)) {
                long unique;
                do {
                    count++;
                    unique = Combine(result, count);
                } while (_idsUsedThisFrame.ContainsKey(unique));
                _idsUsedThisFrame[result] = count;
                _idsUsedThisFrame.Add(unique, 1);
                result = unique;
            } else {
                _idsUsedThisFrame.Add(result, 1);
            }

            return result;
        }

        public static long TryCreateId(int id, bool isAbsoluteId, string file, out IComponent? c) {
            long result = GuiHelper.CurrentIMGUI.CreateId(id, isAbsoluteId, file);
            GuiHelper.CurrentIMGUI.TryGetValue(result, out c);

            return result;
        }

        /// <summary>
        /// Gets the component that was put with this id last frame, or creates it, then parents it to the current parent.
        /// This is what a component's static Put method is made of.
        /// </summary>
        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="file">The caller's file. Forward the one your Put method got from [CallerFilePath].</param>
        /// <param name="create">Creates the component when there isn't one yet. Gets the final id.</param>
        public static T Put<T>(int id, bool isAbsoluteId, string file, Func<long, T> create) where T : IComponent {
            IMGUI ui = GuiHelper.CurrentIMGUI;
            long key = TryCreateId(id, isAbsoluteId, file, out IComponent? c);

            if (c is not T a) {
                if (c != null) {
                    // A different kind of component had this id last frame.
                    ui.Remove(key, c);
                }
                a = create(key);
            }

            ui.GrabParent(a);
            return a;
        }
        /// <summary>
        /// Same as <see cref="Put{T}"/> for a parent. The components put after it become its children until it's popped.
        /// </summary>
        /// <param name="id">Usually the caller's line number.</param>
        /// <param name="isAbsoluteId">Whether to use the id as is instead of combining it with the current parent's.</param>
        /// <param name="file">The caller's file. Forward the one your Push method got from [CallerFilePath].</param>
        /// <param name="create">Creates the component when there isn't one yet. Gets the final id.</param>
        /// <param name="maxChildren">Pops the parent by itself after this many children. 0 means no limit.</param>
        public static T Push<T>(int id, bool isAbsoluteId, string file, Func<long, T> create, int maxChildren = 0) where T : IParent {
            T a = Put(id, isAbsoluteId, file, create);
            GuiHelper.CurrentIMGUI.Push(a, maxChildren);
            return a;
        }

        public override uint LastFocus { get; set; } = uint.MaxValue;

        /// <summary>
        /// Only call this if you didn't call UpdateEnd. Removes the components that weren't put during this frame.
        /// This should be called after all your components were put, before UpdateLayout.
        /// Gives back the current UI spot to the one that had it before UpdateSetup.
        /// </summary>
        public void Cleanup() {
            Reset();
            foreach (var kc in _activeComponents.Reverse()) {
                if (kc.Value.LastPing != CurrentFrame) {
                    Remove(kc.Key, kc.Value);
                }
            }
            _currentParent = this;
            _maxChildren = 0;
            _childrenCount = 0;
            _parents.Clear();
            _idStack.Clear();
            _idHash = 0;
            _idsUsedThisFrame.Clear();

            GuiHelper.CurrentIMGUI = _previous ?? this;
            _previous = null;
        }
        private void Remove(long id, IComponent c) {
            if (Focus == id) {
                Focus = null;
            }
            if (MouseCapture == c) {
                MouseCapture = null;
            }

            _activeComponents.Remove(id);
            c.Parent?.Remove(c);
        }
        private IMGUI MakeCurrent() {
            IMGUI previous = GuiHelper.CurrentIMGUI;
            GuiHelper.CurrentIMGUI = this;
            return previous ?? this;
        }
        private RectangleF AreaOrWindow => Area ?? new RectangleF(0f, 0f, InputHelper.WindowWidth, InputHelper.WindowHeight);

        private void FindPrevFocus() {
            FindFocus(ExtractPrev);
        }
        private void FindNextFocus() {
            FindFocus(ExtractNext);
        }
        private IComponent ExtractPrev(long id) => _activeComponents[id].GetPrev();
        private IComponent ExtractNext(long id) => _activeComponents[id].GetNext();
        private void FindFocus(Func<long, IComponent> getNeighbor) {
            long initialFocus = Id;
            if (Focus != null) {
                initialFocus = Focus.Value;
            }
            long newFocus = initialFocus;
            do {
                var c = getNeighbor(newFocus);
                newFocus = c.Id;
                if (c.IsFocusable) {
                    GrabFocus(c);
                    return;
                }
            } while (initialFocus != newFocus);
        }
        /// <summary>
        /// Mixes every bit of both values into the result with Murmur3's 64 bit finalizer.
        /// A linear combine gives neighbor parents neighbor ids, which lines up their children's ids.
        /// </summary>
        private static long Combine(long seed, long value) {
            unchecked {
                ulong h = (ulong)seed * 0x9E3779B97F4A7C15UL + (ulong)value;
                h ^= h >> 33;
                h *= 0xFF51AFD7ED558CCDUL;
                h ^= h >> 33;
                h *= 0xC4CEB9FE1A85EC53UL;
                h ^= h >> 33;
                return (long)h;
            }
        }

        /// <summary>The id hash that each PushId saved so that PopId can restore it.</summary>
        private readonly Stack<long> _idStack = new();
        private long _idHash = 0;

        private readonly Stack<(IParent Parent, int MaxChildren, int ChildrenCount)> _parents = new();
        private readonly Dictionary<long, IComponent> _activeComponents = [];
        private long? Focus {
            get => _focus;
            set {
                if (_focus != null) {
                    _activeComponents[_focus.Value].IsFocused = false;
                }
                _focus = value;
                if (_focus != null) {
                    _activeComponents[_focus.Value].IsFocused = true;
                }
            }
        }
        private long? _focus;
        private IComponent? _hover;
        private uint _hoverFrame;
        private IParent _currentParent;
        private int _maxChildren = 0;
        private int _childrenCount = 0;
        private readonly Dictionary<long, int> _idsUsedThisFrame = [];

        private float _scale = 1f;
        private IMGUI? _previous;

        private bool _prevPressed = false;
        private bool _nextPressed = false;
    }
}
