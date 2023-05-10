using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Objects.UIs.Elements
{
    public class Element
    {
        public string Name { get; set; }
        public UIObject UI { get; set; }

        public Vector2 Offset { get; set; }
        public Vector2 CombinedPosition() { return UI.Physics.AltitudePosition() + UI.ToMatrix(Offset); }

        public Vector2 Size { get; set; }
        public Func<Vector2, bool> IsContains { get; set; } = (v) => { return false; };
        public void SetBoxContains(float x, float y, float width, float height)
        {
            IsContains = (position) =>
            {
                return position.X >= x &&
                       position.Y >= y &&
                       position.X < x + width &&
                       position.Y < y + height;
            };
        }
        public void SetBoxContains(Vector2 position, Vector2 size)
        {
            IsContains = (p) =>
            {
                return p.X >= position.X &&
                       p.Y >= position.Y &&
                       p.X < position.X + size.X &&
                       p.Y < position.Y + size.Y;
            };
        }
        public void SetBoxContains(Vector2 size)
        {
            IsContains = (p) =>
            {
                Vector2 position = CombinedPosition();
                return p.X >= position.X &&
                       p.Y >= position.Y &&
                       p.X < position.X + size.X &&
                       p.Y < position.Y + size.Y;
            };
        }
        public void SetBoxContains(float offsetX, float offsetY, Vector2 size)
        {
            IsContains = (p) =>
            {
                Vector2 position = CombinedPosition() + new Vector2(offsetX, offsetY);
                return p.X >= position.X &&
                       p.Y >= position.Y &&
                       p.X < position.X + size.X &&
                       p.Y < position.Y + size.Y;
            };
        }

        public Color Color { get; set; } = Color.White;
        public Vector2 Origin { get; set; }
        public float Rotation { get; set; }
        public Vector2 Scale { get; set; }
        public float DepthOffset { get; set; }

        protected Vector2 CombinedScale() { return UI.Scale + Scale; }
        protected float CombinedRotation() { return UI.Rotation + Rotation; }

        public bool IsVisible { get; set; } = true;
        public bool IsEnabled { get; set; } = true;

        #region Events

        private event Action onLeftClick, onRightClick, onHoverEnter, onHoverExit;
        public event Action OnLeftClick { add { onLeftClick += value; } remove { onLeftClick -= value; } }
        public event Action OnRightClick { add { onRightClick += value; } remove { onRightClick -= value; } }
        public event Action OnHoverEnter { add { onHoverEnter += value; } remove { onHoverEnter -= value; } }
        public event Action OnHoverExit { add { onHoverExit += value; } remove { onHoverExit -= value; } }

        public void CallLeftClick() { onLeftClick?.Invoke(); }
        public void CallRightClick() { onRightClick?.Invoke(); }
        public void CallHoverEnter() { onHoverEnter?.Invoke(); }
        public void CallHoverExit() { onHoverExit?.Invoke(); }

        public void ClearLeftClick() { onLeftClick = null; }
        public void ClearRightClick() { onRightClick = null; }
        public void ClearHoverEnter() { onHoverEnter = null; }
        public void ClearHoverExit() { onHoverExit = null; }
        
        public bool IsLeftClickNull() { return onLeftClick == null; }
        public bool IsRightClickNull() { return onRightClick == null; }

        #endregion

        public bool IsHovering { get; set; }

        public Element(Vector2 Offset)
        {
            this.Offset = Offset;
        }

        public virtual void Initialize()
        {
            DepthOffset = UI.World.PixelDepth();
            ScaleToUI();
        }
        bool IsScaled = false;
        public void ScaleToUI()
        {
            if (IsScaled == false)
            {
                Scale = new Vector2(UI.ToScale(Scale.X), UI.ToScale(Scale.Y));
                IsScaled = true;
            }
        }

        public virtual void Update(GameTime gt) { }
        public virtual void Draw(SpriteBatch sb) { }
    }
}
