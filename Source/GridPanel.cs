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
		protected List<IComponent> _Children = new List<IComponent>();

		/// <summary>
		/// The Panel in form of floatable Ractangle.
		/// </summary>
		public RectangleF Panel { get; set; } = new RectangleF(0, 0, 100, 100);

		/// <summary>
		/// Helper to set both the X and Y relative to the Parent.
		/// </summary>
		public Point2 OffsetXY {
			get => new Point2(Panel.X, Panel.Y);
			private set { }
		}

		/// <summary>
		/// Helper to set the width and height of the Panel.
		/// </summary>
		public Point2 FullSize {
			get => new Point2(Panel.Width, Panel.Height);
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
		/// Update the Preffered with and Height of the GridPanel based on how the sizes of the children.
		/// Should run every frame.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void UpdatePrefSize(GameTime gameTime) {
			foreach (var c in _Children) {
				// set the sizes of components to be their children atleast. Default 100
				c.UpdatePrefSize(gameTime);
			}

			UpdateSetup(gameTime);
		}

		/// <summary>
		/// Set all the components positions based on the parent position.
		/// </summary>
		/// <param name="gameTime"></param>
		public override void UpdateSetup(GameTime gameTime) {
			var colWidths = GetColumnSizes();
			SetEachComponent(colWidths, gameTime);
		}

		/// <summary>
		/// Set the x & y coordinate based on the columnWidths and max rows.
		/// </summary>
		/// <param name="colWidths"></param>
		private void SetEachComponent(float[] colWidths, GameTime gameTime) {

			float posX = 0;
			float posY = 0;

			for (var i = 0; i < _Children.Count; i++) {
				var col = i % _ComponentsInWidth;

				_Children[i].Width = colWidths[col];
				_Children[i].Height = _Children[i].PrefHeight;

				if (col == 0 && i != 0) { // not the first row.
					posX = 0;
					posY += _Children[i].PrefHeight;
				}

				_Children[i].Y = posY;
				_Children[i].X = posX;
				_Children[i].Clip = _Children[i].Bounds.Intersection(Clip);
				_Children[i].UpdateSetup(gameTime);

				posX += colWidths[col];
			}

			var maxWidth = 0f;
			for (var i = 0; i < colWidths.Length; i++) {
				maxWidth += colWidths[i];
			}

			var maxHeight = 0f;
			var rowSizes = GetRowSizes();
			for (var i = 0; i < rowSizes.Length; i++) {
				maxHeight += rowSizes[i];
			}

			PrefWidth = maxWidth;
			PrefHeight = maxHeight;

			Panel = new RectangleF(OffsetXY, new Point2(maxWidth, maxHeight));
		}

		/// <summary>
		/// Get the size of each column
		/// </summary>
		/// <returns></returns>
		private float[] GetColumnSizes() {
			var colWidths = new float[_ComponentsInWidth];
			for (var i = 0; i < _Children.Count; i++) {
				var col = i % _ComponentsInWidth;
				colWidths[col] = MathHelper.Max(_Children[i].PrefWidth, colWidths[col]);
			}
			return colWidths;
		}

		/// <summary>
		/// Get the size of each Row
		/// </summary>
		/// <returns></returns>
		private float[] GetRowSizes() {
			var count = (int)MathF.Ceiling((float)_Children.Count / _ComponentsInWidth);
			var rowsHeights = new float[count];

			for (var i = 0; i < _Children.Count; i++) {
				var row = i / _ComponentsInWidth; // row index
				rowsHeights[row] = MathHelper.Max(_Children[i].PrefHeight, rowsHeights[row]);
			}
			return rowsHeights;
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
