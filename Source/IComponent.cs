using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace Apos.Gui {
    public interface IComponent {
        int Index { get; set; }
        uint LastPing { get; set; }
        int Id { get; set; }

        float X { get; set; }
        float Y { get; set; }
        float Width { get; set; }
        float Height { get; set; }
        float PrefWidth { get; set; }
        float PrefHeight { get; set; }

        bool IsFocused { get; set; }
        bool IsFocusable { get; set; }

        RectangleF Bounds { get; set; }
        RectangleF Clip { get; set; }

        Vector2 XY { get; set; }
        Vector2 Size { get; set; }
        Vector2 PrefSize { get; set; }
        float Left { get; set; }
        float Top { get; set; }
        float Right { get; set; }
        float Bottom { get; set; }

        void UpdatePrefSize(GameTime gameTime);
        void UpdateSetup(GameTime gameTime);
        void UpdateInput(GameTime gameTime);
        void Update(GameTime gameTime);
        void Draw(GameTime gameTime);

        IParent? Parent { get; set; }
        IComponent GetPrev();
        IComponent GetNext();
        IComponent GetLast();

        Action<IComponent> GrabFocus { get; set; }
    }
}
