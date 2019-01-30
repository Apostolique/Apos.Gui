using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using SpriteFontPlus;

namespace AposGui {
    /// <summary>
    /// Goal: A Switch component that works like a tab.
    /// </summary>
    public class Switch : Component {
        //constructors
        public Switch() {
            
        }

        //public vars
        public string Key {
            get => _key;
            set {
                if (children.ContainsKey(value)) {
                    _key = value;
                }
            }
        }

        //public functions
        public void Add(string key, Component c) {
            children.Add(key, c);
        }
        public override void UpdateSetup() {
            children[Key].UpdateSetup();
        }
        public override bool UpdateInput() {
            bool isUsed = false;

            isUsed = children[Key].UpdateInput();

            if (!isUsed) {
                isUsed = base.UpdateInput();
            }

            return isUsed;
        }
        public override void Update() {
            children[Key].Update();
        }

        //private functions

        //private vars
        protected string _key;
        protected Dictionary<string, Component> children;
    }
}