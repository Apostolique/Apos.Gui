using System.Runtime.CompilerServices;
using Apos.Input;
using Microsoft.Xna.Framework;

namespace Apos.Gui {
    public class Button(int id) : Component(id), IParent {
        public Interaction Interaction { get; } = new();
        public bool Clicked => Interaction.Clicked;
        public IComponent? Child { get; set; }
        public override bool IsFocusable { get; set; } = true;

        public override void UpdateSetup(GameTime gameTime) {
            Child?.UpdateSetup(gameTime);
        }
        public override void UpdateInput(GameTime gameTime) {
            Child?.UpdateInput(gameTime);
            Interaction.UpdateInput(this);
        }
        public override void Update(GameTime gameTime) {
            Child?.Update(gameTime);
        }

        public override void UpdatePrefSize(GameTime gameTime) {
            if (Child != null) {
                Child.UpdatePrefSize(gameTime);

                PrefWidth = Child.PrefWidth;
                PrefHeight = Child.PrefHeight;
            }
        }
        public virtual void UpdateLayout(GameTime gameTime) {
            if (Child != null) {
                Child.X = X;
                Child.Y = Y;
                Child.Width = Width;
                Child.Height = Height;
                Child.Clip = Child.Bounds.Intersect(Clip);

                if (Child is IParent p) {
                    p.UpdateLayout(gameTime);
                }
            }
        }

        public override void Draw(GameTime gameTime) {
            GuiHelper.PushClip(Clip);

            float highlight = Clicked ? 0.5f : 0.08f * Interaction.Hover + 0.15f * Interaction.Press;
            if (highlight > 0f) {
                GuiHelper.ShapeBatch.FillRectangle(Bounds.Position, Bounds.Size, new Color(Color.White, highlight));
            }
            if (IsFocused) {
                GuiHelper.ShapeBatch.BorderRectangle(Bounds.Position, Bounds.Size, Color.White, 2f);
            } else {
                GuiHelper.ShapeBatch.BorderRectangle(Bounds.Position, Bounds.Size, new Color(76, 76, 76), 2f);
            }

            Child?.Draw(gameTime);

            GuiHelper.PopClip();
        }

        public void Add(IComponent c) {
            if (c != Child) {
                if (Child != null) {
                    Child.Parent = null;
                }
                Child = c;
                Child.Parent = this;
            }
        }
        public void Remove(IComponent c) {
            if (Child == c) {
                Child.Parent = null;
                Child = null;
            }
        }
        public virtual void Reset() { }
        public virtual int PeekNextIndex() => 0;
        public virtual int NextIndex() => 0;

        public virtual IComponent GetPrev(IComponent c) {
            return this;
        }
        public virtual IComponent GetNext(IComponent c) {
            return Parent?.GetNext(this) ?? this;
        }

        public virtual void SendToTop(IComponent c) {
            LastFocus = Root.CurrentFrame;
            Parent?.SendToTop(this);
        }

        public static Button Put(string text, int fontSize = 30, Color? color = null, [CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            Button b = Put(id, isAbsoluteId);
            Label.Put(text, fontSize: fontSize, color: color, id: id, isAbsoluteId: isAbsoluteId);

            return b;
        }
        public static Button Put([CallerLineNumber] int id = 0, bool isAbsoluteId = false) {
            id = IMGUI.TryCreateId(id, isAbsoluteId, out IComponent? c);

            Button a;
            if (c is Button d) {
                a = d;
            } else {
                a = new Button(id);
            }

            GuiHelper.CurrentIMGUI.GrabParent(a);

            GuiHelper.CurrentIMGUI.Push(a, 1);

            return a;
        }
    }
}
