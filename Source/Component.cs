using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Optional;

namespace Apos.Gui {
    /// <summary>
    /// The core of a user interface.
    /// Handles everything from how to handle updates and inputs, to how something is drawn.
    /// </summary>
    public class Component {

        // Group: Constructors

        /// <summary>
        /// Creates an empty component that doesn't really do anything.
        /// You can script this but usually this is better as a base class for other components.
        /// </summary>
        public Component() { }

        // Group: Public Variables

        /// <value>The component's position in UI coordinates. Defaults to 0, 0.</value>
        public virtual Point Position {
            get;
            set;
        } = new Point(0, 0);
        /// <value>The component's width in UI coordinates. Defaults to 100.</value>
        public virtual int Width {
            get;
            set;
        } = 100;
        /// <value>The component's height in UI coordinates. Defaults to 100.</value>
        public virtual int Height {
            get;
            set;
        } = 100;
        /// <summary>
        /// The components's preferred width. This might or might not be honored by layout managers.
        /// Returns the width by default.
        /// It's a good idea to override this for custom components.
        /// </summary>
        public virtual int PrefWidth => Width;
        /// <summary>
        /// The components's preferred height. This might or might not be honored by layout managers.
        /// Returns the height by default.
        /// It's a good idea to override this for custom components.
        /// </summary>
        public virtual int PrefHeight => Height;
        /// <summary>
        /// The component's left position. This is the same as Position.X.
        /// </summary>
        public virtual int Left => Position.X;
        /// <summary>
        /// The component's top position. This is the same as Position.Y.
        /// </summary>
        public virtual int Top => Position.Y;
        /// <summary>
        /// The component's right position. This is the same as Position.X + Width.
        /// </summary>
        public virtual int Right => Position.X + Width;
        /// <summary>
        /// The component's bottom position. This is the same as Position.Y + Height.
        /// </summary>
        public virtual int Bottom => Position.Y + Height;
        /// <summary>
        /// The component's full bounding rectangle. It is defined by left, top, width, height.
        /// </summary>
        public virtual Rectangle BoundingRect => new Rectangle(Left, Top, Width, Height);
        /// <summary>
        /// Visible area of a component.
        /// </summary>
        public virtual Rectangle ClippingRect {
            get {
                return _clippingRect.ValueOr(() => BoundingRect);
            }
            set {
                _clippingRect = Option.Some(value);
            }
        }
        /// <summary>
        /// Parents usually manage their children. Defaults to none.
        /// </summary>
        public virtual Option<Component> Parent {
            get;
            set;
        } = Option.None<Component>();
        /// <summary>
        /// Represents the component's current hover state.
        /// A component needs a hover condition for this to do anything.
        /// </summary>
        /// <seealso cref="AddHoverCondition(Func{Component, bool})"/>
        public virtual bool IsHovered {
            get;
            set;
        } = false;
        /// <summary>
        /// True when the component can be focused. Defaults to false.
        /// </summary>
        public virtual bool IsFocusable {
            get;
            set;
        } = false;
        /// <summary>
        /// Represents the component's current focus state.
        /// A component needs a focus condition for this to do anything.
        /// </summary>
        /// <seealso cref="AddFocusCondition(Func{Component, bool})"/>
        public virtual bool IsFocused {
            get;
            set;
        } = false;
        /// <summary>
        /// The component can use this to ask to grab focus. By default this does nothing.
        /// </summary>
        public virtual Action<Component> GrabFocus {
            get;
            set;
        } = c => {};

        // Group: Public Functions

        /// <summary>
        /// When the function is true, the component is considered hovered.
        /// </summary>
        /// <param name="hc">A function that takes a component and returns a bool.</param>
        public void AddHoverCondition(Func<Component, bool> hc) {
            _hoverConditions.Add(hc);
        }
        /// <summary>
        /// When the function is true, the component is considered focused.
        /// </summary>
        /// <param name="fc">A function that takes a component and returns a bool.</param>
        public void AddFocusCondition(Func<Component, bool> fc) {
            _focusConditions.Add(fc);
            IsFocusable = true;
        }
        /// <summary>
        /// Provides a way to script components. Associates a condition to an action.
        /// It is possible for a component to have many conditions and many actions.
        /// For example, a button could do two different actions on a left mouse click and on a right mouse click.
        /// </summary>
        /// <param name="c">A condition that will trigger an action.</param>
        /// <param name="o">An action that will do something when triggered.</param>
        public void AddAction(Func<Component, bool> c, Func<Component, bool> o) {
            _conditionOperations.Add(new ConditionOperation(c, o));
        }
        /// <summary>
        /// If this component has a parent, it will ask the parent to return this component's previous neighbor.
        /// Otherwise, it will return itself.
        /// </summary>
        public virtual Component GetPrev() {
            return Parent.Map(parent => parent.GetPrev(this)).ValueOr(this);
        }
        /// <summary>
        /// If this component has a parent, it will ask the parent to return this component's next neighbor.
        /// Otherwise, it will return itself.
        /// </summary>
        public virtual Component GetNext() {
            return Parent.Map(parent => parent.GetNext(this)).ValueOr(this);
        }
        /// <summary>
        /// This function is used by components that manage children.
        /// It will try to return a child that is previous to another component.
        /// </summary>
        public virtual Component GetPrev(Component c) {
            return GetPrev();
        }
        /// <summary>
        /// This function is used by components that manage children.
        /// It will try to return a child that is next to another component.
        /// </summary>
        public virtual Component GetNext(Component c) {
            return GetNext();
        }
        /// <summary>
        /// This is used to sink down a component hierarchy from a parent down to a child.
        /// If a parent has children, it will return the first one.
        /// </summary>
        public virtual Component GetFinal() {
            return this;
        }
        /// <summary>
        /// This is used to sink down a component hierarchy from a parent down to a child.
        /// If a parent has children, it will return the last one.
        /// </summary>
        public virtual Component GetFinalInverse() {
            return this;
        }
        /// <summary>
        /// Clips a rectangle against this component's clipping rectangle.
        /// </summary>
        public Rectangle ClipRectangle(Rectangle rect1) {
            return ClipRectangle(rect1, ClippingRect);
        }
        /// <summary>
        /// Checks if a point is within this component's bounding rectangle.
        /// </summary>
        /// <param name="p">A Point in UI coordinates.</param>
        public virtual bool IsInside(Point p) {
            return IsInside(BoundingRect, p);
        }
        /// <summary>
        /// Checks if a point is within this component's clipping rectangle.
        /// </summary>
        /// <param name="p">A Point in UI coordinates.</param>
        public virtual bool IsInsideClip(Point p) {
            return IsInside(ClippingRect, p);
        }
        /// <summary>
        /// Sets the drawing limits for this component.
        /// </summary>
        public virtual void SetScissor() {
            _oldScissor = _s.GraphicsDevice.ScissorRectangle;
            GuiHelper.SetScissor(ClippingRect);
        }
        /// <summary>
        /// Cleans up the drawing limits that were set by SetScissor.
        /// </summary>
        public virtual void ResetScissor() {
            GuiHelper.ResetScissor(_oldScissor);
        }
        /// <summary>
        /// Called at the start of the update loop.
        /// This is generally used to call layout managers or
        /// other updates that don't rely on inputs but are used by inputs.
        /// </summary>
        public virtual void UpdateSetup() { }
        /// <summary>
        /// Called at the start of the UpdateInput step. This is used to find which component should get the hover.
        /// </summary>
        /// <returns>Returns true if the component wants to be in focus.</returns>
        public virtual Option<Component> FindHover() {
            foreach (Func<Component, bool> c in _hoverConditions) {
                if (c(this)) {
                    return Option.Some(this);
                }
            }

            return Option.None<Component>();
        }
        /// <summary>
        /// Called to process user inputs.
        /// Separating the logic for inputs makes it easy to freeze inputs on components or the whole UI.
        /// </summary>
        /// <returns>Returns true when an input has been consumed.</returns>
        public virtual void UpdateInput() {
            foreach (Func<Component, bool> c in _focusConditions) {
                if (c(this)) {
                    GrabFocus(this);
                    break;
                }
            }

            foreach (ConditionOperation co in _conditionOperations) {
                if (co.Condition(this)) {
                    co.Operation(this);
                }
            }
        }
        /// <summary>
        /// The final update step.
        /// This is useful for updates that need to run after inputs but aren't inputs themselves.
        /// For example animations.
        /// </summary>
        public virtual void Update() { }
        /// <summary>
        /// The component's draw function.
        /// </summary>
        public virtual void Draw() { }

        /// <summary>
        /// Clips a rectangle against another rectangle.
        /// </summary>
        /// <returns>Returns a new rectangle that corresponds to the two rectangle's intersection.</returns>
        public static Rectangle ClipRectangle(Rectangle rect1, Rectangle rect2) {
            var left = Math.Max(rect1.Left, rect2.Left);
            var top = Math.Max(rect1.Top, rect2.Top);
            var right = Math.Min(rect1.Right, rect2.Right);
            var bottom = Math.Min(rect1.Bottom, rect2.Bottom);

            int clipWidth = Math.Max(right - left, 0);
            int clipHeight = Math.Max(bottom - top, 0);

            return new Rectangle(left, top, clipWidth, clipHeight);
        }
        /// <summary>
        /// Checks if a point is within a rectangle's boundaries exclusively.
        /// </summary>
        public static bool IsInside(Rectangle r, Point p) {
            return r.Left < p.X && r.Right > p.X && r.Top < p.Y && r.Bottom > p.Y;
        }

        // Group: Private Functions

        /// <summary>
        /// Draws a string using the Font, FontSize and UI scale.
        /// </summary>
        /// <param name="t">The string to draw.</param>
        /// <param name="p">The position for the string.</param>
        /// <param name="c">The color for the string.</param>
        protected void DrawString(string t, Vector2 p, Color c) => GuiHelper.DrawString(t, p, c);
        /// <param name="t">The string to measure.</param>
        protected Vector2 MeasureString(string t) => GuiHelper.MeasureString(t);

        // Group: Private Variables

        /// <summary>
        /// SpriteBatch to use for drawing.
        /// </summary>
        protected SpriteBatch _s => GuiHelper.SpriteBatch;
        /// <summary>
        /// The clipping rectangle that this component might or might not have.
        /// </summary>
        protected Option<Rectangle> _clippingRect = Option.None<Rectangle>();
        /// <summary>
        /// A field to store the scissor information before overwriting it.
        /// </summary>
        protected Rectangle _oldScissor;
        /// <summary>
        /// A list with all the condition for when a component is hovered.
        /// </summary>
        protected List<Func<Component, bool>> _hoverConditions = new List<Func<Component, bool>>();
        /// <summary>
        /// A list with all the condition for when a component is focused.
        /// </summary>
        protected List<Func<Component, bool>> _focusConditions = new List<Func<Component, bool>>();
        /// <summary>
        /// A list that holds conditions and their associated actions.
        /// </summary>
        protected List<ConditionOperation> _conditionOperations = new List<ConditionOperation>();
        /// <summary>
        /// Associates a condition to an action.
        /// </summary>
        protected struct ConditionOperation {

            // Group: Constructors

            /// <param name="c">Condition</param>
            /// <param name="o">Action</param>
            public ConditionOperation(Func<Component, bool> c, Func<Component, bool> o) {
                Condition = c;
                Operation = o;
            }

            // Group: Public Variables

            /// <summary>
            /// Used to build a Boolean condition over a component.
            /// </summary>
            public Func<Component, bool> Condition;
            /// <summary>
            /// Used to build an action over a component. Returns true when an action is blocking.
            /// </summary>
            public Func<Component, bool> Operation;
        }
    }
}
