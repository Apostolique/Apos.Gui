using Apos.Input;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Apos.Gui {
	/// <summary>
	/// Panel Object that puts Components next to each other.
	/// </summary>
	public class GridPanel : Component, IParent {

		#region Properties

		/// <summary>
		/// The amount of components that should be put next to each other.
		/// </summary>
		private int _ComponentsInWidth = 2;

		/// <summary>
		/// Indexer for the next child in the _Children list.
		/// </summary>
		protected int _nextChildIndex = 0;
		/// <summary>
		/// List of child components in this parent Panel.
		/// </summary>
		protected List<IComponent> _Children = new();

		/// <summary>
		/// The Panel in form of floatable Ractangle.
		/// </summary>
		public RectangleF Panel { get; set; } = new(0, 0, 100, 100);

		/// <summary>
		/// Helper to set both the X and Y relative to the Parent.
		/// </summary>
		public Point2 OffsetXY {
			get => new(Panel.X, Panel.Y);
			private set { }
		}

		/// <summary>
		/// Helper to set the width and height of the Panel.
		/// </summary>
		public Point2 FullSize {
			get => new(Panel.Width, Panel.Height);
			private set { } 
		}

		#endregion

		/// <summary>
		/// Initialize the Panel.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="componentsInWidth"></param>
		public GridPanel(int id, int componentsInWidth = 2) : base(id) {
			_ComponentsInWidth = componentsInWidth;
		}

		/// <summary>
		/// To update all children based on new size.
		/// Should run every frame.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void UpdatePrefSize(GameTime gameTime) {
			float maxWidth = 0;
			float maxHeight = 0;

			var count = 0;
			foreach (var c in _Children) {
				c.UpdatePrefSize(gameTime);
				UpdateWidthBasedOnComponentSize(gameTime, ref maxWidth, ref count, c);

				// Add all sizes together to get the max height.
				// We are missing some checks to see if we dont go outside the game screen window?
				maxHeight += c.PrefHeight;
			}

			PrefHeight = maxHeight;
		}

		/// <summary>
		///for every 2 items. Add their width together.
		/// then check if the prefWidth is lower than the cur maxwidth
		/// </summary>
		/// <param name="gameTime"></param>
		/// <param name="maxWidth"></param>
		/// <param name="count"></param>
		/// <param name="c"></param>
		private void UpdateWidthBasedOnComponentSize(GameTime gameTime, ref float maxWidth, ref int count, IComponent c) {
			count++;
			if (count < _ComponentsInWidth) {
				maxWidth += c.PrefWidth;
			} else {
				PrefWidth = MathHelper.Max(maxWidth, PrefWidth);
				count = 0;
				maxWidth = 0;
			}
		}

		/// <summary>
		/// Set all the components positions based on the parent position.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void UpdateSetup(GameTime gameTime) {
			var maxWidth = Width;
			var maxHeight = Height;

			var count = 0;

			var currentX = X + OffsetXY.X;
			var currentY = Y + OffsetXY.Y;
			foreach (var c in _Children) {

				// to update the Y pos
				count++;
				if (count < _ComponentsInWidth) {
					currentX += c.Width; // this might need  + Y + offset.Y
				} else {
					count = 0;
					currentX = X + OffsetXY.X;
				}

				c.X = currentX;
				c.Y = currentY + Y + OffsetXY.Y;
				c.Width = c.PrefWidth;
				c.Height = c.PrefHeight;

				c.Clip = c.Bounds.Intersection(Clip);
				c.UpdateSetup(gameTime);

				currentY += c.Height;
			}

			Panel = new RectangleF(OffsetXY, new Point2(MathHelper.Max(currentX, maxWidth), MathHelper.Max(currentY, maxHeight)));
		}

		#region Component updaters
		public override void UpdateInput(GameTime gameTime) {
			foreach (var c in _Children) c.UpdateInput(gameTime);
		}

		public override void Update(GameTime gameTime) {
			foreach (var c in _Children) c.Update(gameTime);
		}

		public override void Draw(GameTime gameTime) {
			foreach (var c in _Children) c.Draw(gameTime);
		}
		#endregion

		public IComponent GetNext(IComponent c) => throw new NotImplementedException();
		public IComponent GetPrev(IComponent c) => throw new NotImplementedException();

		#region IParent overrites

		/// <summary>
		/// Add the component to this parent Panel.
		/// </summary>
		/// <param name="c"></param>
		public virtual void Add(IComponent c) {
			c.Parent = this;
			_Children.Insert(c.Index, c);
		}

		/// <summary>
		/// Remove the component child from this parent.
		/// </summary>
		/// <param name="c"></param>
		public virtual void Remove(IComponent c) {
			c.Parent = null;
			_Children.Remove(c);
		}

		/// <summary>
		/// Reset the child index back to 0.
		/// </summary>
		public virtual void Reset() => _nextChildIndex = 0;

		/// <summary>
		/// Helper to get the next index.
		/// </summary>
		/// <returns></returns>
		public virtual int NextIndex() => _nextChildIndex++;

		#endregion

		#region statics
		/// <summary>
		/// Since first in last out this pops the last one in the ImGUI.
		/// </summary>
		public static void Pop() {
			GuiHelper.CurrentIMGUI.Pop();
		}

		/// <summary>
		/// Check if GridPanel exists. create or get it. Ping it and push it to the GUI.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="isAbsoluteId"></param>
		/// <returns></returns>
		public static GridPanel Push([CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
			// create the hash and check if a component with this hash exists.
			id = GuiHelper.CurrentIMGUI.CreateId(id, isAbsoluteId);
			GuiHelper.CurrentIMGUI.TryGetValue(id, out var c);

			// if component is a panel. return it else create a new panel.
			var a = c is GridPanel panel ? panel : new GridPanel(id);

			a.PingIfNotDoneThisFrame(a);

			GuiHelper.CurrentIMGUI.Push(a);

			return a;
		}

		/// <summary>
		/// Not sure why we do this yet. but hey. Panel obj does it. c:
		/// </summary>
		/// <param name="a"></param>
		private void PingIfNotDoneThisFrame(GridPanel a) {
			if (a.LastPing != InputHelper.CurrentFrame) {
				var parent = GuiHelper.CurrentIMGUI.GrabParent(a);
				if (parent == null) throw new Exception("Asumed here is that Parent could never be null.");

				a.Reset();
				a.LastPing = InputHelper.CurrentFrame;
				a.Index = parent.NextIndex();
			}
		}

		#endregion
	}
}
