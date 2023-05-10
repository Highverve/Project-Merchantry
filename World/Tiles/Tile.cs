using Merchantry.World.Meta;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Tiles
{
    public class Tile
    {
        public Texture2D Texture { get; set; }
        public TileLayer Layer { get; set; }
        
        #region Spatial

        private Vector2 position, lastPosition, origin;
        private float top, bottom, left, right;
        protected event Action onPositionChange;

        public Vector2 Position
        {
            get { return position; }
            set
            {
                position = value;
                if (lastPosition != position)
                {
                    onPositionChange?.Invoke();
                    lastPosition = position;
                }

                top = position.Y;
                bottom = position.Y + WorldManager.TileHeight;
                left = position.X;
                right = position.X + WorldManager.TileWidth;
            }
        }
        public float Top() { return top; }
        public float Bottom() { return bottom; }
        public float Left() { return left; }
        public float Right() { return right; }

        public void SetCoords(Point coords)
        {
            SetCoords(coords.X, coords.Y);
        }
        public void SetCoords(int x, int y)
        {
            Position = new Vector2((WorldManager.TileWidth * x) + origin.X, (WorldManager.TileHeight * y) + origin.Y);
        }
        public bool Contains(Vector2 position) { return Contains(position.X, position.Y); }
        public bool Contains(float x, float y)
        {
            Vector2 tl = Layer.ToScreen(new Vector2(left - origin.X, top - origin.Y));
            Vector2 br = Layer.ToScreen(new Vector2(right - origin.X, bottom - origin.Y));

            return x >= tl.X &&
                   y >= tl.Y &&
                   x <= br.X &&
                   y <= br.Y;
        }

        #endregion

        #region Rendering

        private ObjectAnimation animation;
        public ObjectAnimation Animation
        {
            get
            {
                if (animation == null)
                {
                    animation = new ObjectAnimation();
                    animation.FrameSize = new Point(WorldManager.TileWidth, WorldManager.TileHeight);
                }
                return animation;
            }
            set { animation = value; }
        }

        public Color Color { get; set; } = Color.White;
        public Vector2 Scale { get; set; } = Vector2.Zero;
        public SpriteEffects Effects { get; set; } = SpriteEffects.None;
        public float DepthOffset { get; set; }
        /// <summary>
        /// Only works if animation is not in use (== null).
        /// </summary>
        public Point Frame { get; set; }
        public bool IsRender { get; set; } = true;

        public Rectangle Source()
        {
            if (animation != null)
                return animation.Source();
            else
            {
                return new Rectangle(Frame.X * (WorldManager.TileWidth + 2) + 1,
                                     Frame.Y * (WorldManager.TileHeight + 2) + 1,
                                     WorldManager.TileWidth, WorldManager.TileHeight);
            }
        }

        public float Rotation { get; set; } = 0;
        public void RotateClockwise() { Rotation += MathHelper.PiOver2; }
        public void RotateCounterClockwise() { Rotation -= MathHelper.PiOver2; }

        #endregion

        public Tile(Texture2D Texture)
        {
            this.Texture = Texture;

            origin = new Vector2(WorldManager.TileWidth / 2, WorldManager.TileHeight / 2);
        }
        public Tile(Texture2D Texture, Point Frame, Point Coords) : this(Texture)
        {
            this.Frame = Frame;
            SetCoords(Coords);
        }
        public Tile(Texture2D Texture, int FrameX, int FrameY, int CoordsX, int CoordsY)
            : this(Texture, new Point(FrameX, FrameY), new Point(CoordsX, CoordsY)) { }

        public virtual void Initialize() { }
        public virtual void Update(GameTime gt)
        {
            if (animation != null)
                animation.Update(gt);
        }
        public virtual void Draw(SpriteBatch sb)
        {
            if (IsRender == true)
                sb.Draw(Texture, Layer.ToScreen(Position) + Layer.Position, Source(), Color,
                    Layer.Rotation + Rotation, origin, Scale + Layer.Scale, Effects, Layer.Depth + DepthOffset);
        }
    }
}
