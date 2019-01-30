using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using SpriteFontPlus;
using Optional;
using System.Linq;

namespace AposGui {
    /// <summary>
    /// Goal: A Switch component that works like a tab.
    /// </summary>
    public class Switch : Component {
        //constructors
        public Switch() { }

        //public vars
        public string Key {
            get {
                if (_children.Count > 0) {
                    if (_children.ContainsKey(_key)) {
                        return _key;
                    }
                    _key = _children.Keys.First();
                    return _key;
                }
                return null;
            }
            set {
                if (_children.ContainsKey(value)) {
                    _key = value;
                }
            }
        }
        public override Rectangle ClippingRect {
            get {
                return base.ClippingRect;
            }
            set {
                base.ClippingRect = value;
                if (_children.Count > 0) {
                    _children[Key].ClippingRect = value;
                }
            }
        }
        public override bool OldIsHovered {
            get => base.OldIsHovered;
            set {
                base.OldIsHovered = value;
                if (_children.Count > 0) {
                    _children[Key].OldIsHovered = value;
                }
            }
        }
        public override bool IsHovered {
            get => base.IsHovered;
            set {
                base.IsHovered = value;
                if (_children.Count > 0) {
                    _children[Key].IsHovered = value;
                }
            }
        }
        public override bool IsFocusable { 
            get {
                if (_children.Count > 0) {
                    return _children[Key].IsFocusable;
                }
                return false;
            }
        }
        public override bool HasFocus {
            get => base.HasFocus;
            set {
                base.HasFocus = value;
                if (_children.Count > 0) {
                    _children[Key].HasFocus = value;
                }
            }
        }
        public override Point Position {
            get => base.Position;
            set {
                base.Position = value;
                if (_children.Count > 0) {
                    _children[Key].Position = base.Position;
                }
            }
        }
        public override int Width {
            get => base.Width;
            set {
                base.Width = value;
                if (_children.Count > 0) {
                    _children[Key].Width = base.Width;
                }
            }
        }
        public override int Height {
            get => base.Height;
            set {
                base.Height = value;
                if (_children.Count > 0) {
                    _children[Key].Height = base.Height;
                }
            }
        }
        public override int PrefWidth {
            get {
                if (_children.Count > 0) {
                    return _children[Key].PrefWidth;
                }
                return Width;
            }
        }
        public override int PrefHeight {
            get {
                if (_children.Count > 0) {
                    return _children[Key].PrefHeight;
                }
                return Height;
            }
        }

        //public functions
        public void Add(string key, Component c) {
            _children.Add(key, c);
        }
        public override Component GetPrevious(Component c) {
            if (_children.Count > 0) {
                return _children[Key];
            } else if (Parent != null) {
                return Parent.GetPrevious(this);
            }
            return this;
        }
        public override Component GetNext(Component c) {
            if (_children.Count > 0) {
                return _children[Key];
            } else if (Parent != null) {
                return Parent.GetNext(this);
            }
            return this;
        }
        public override Component GetFinal() {
            if (_children.Count > 0) {
                return _children[Key];
            }
            return this;
        }
        public override Component GetFinalInverse() {
            if (_children.Count > 0) {
                return _children[Key];
            }
            return this;
        }
        public override void UpdateSetup() {
            base.UpdateSetup();
            if (_children.Count > 0) {
                _children[Key].UpdateSetup();
            }
        }
        public override bool UpdateInput() {
            bool isUsed = false;

            if (_children.Count > 0) {
                isUsed = _children[Key].UpdateInput();
            }

            if (!isUsed) {
                isUsed = base.UpdateInput();
            }

            return isUsed;
        }
        public override void Update() {
            base.Update();
            if (_children.Count > 0) {
                _children[Key].Update();
            }
        }

        //private vars
        protected string _key;
        protected Dictionary<string, Component> _children = new Dictionary<string, Component>();
    }
}