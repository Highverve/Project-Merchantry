using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using ExtensionsLibrary.Extensions;

namespace Merchantry.World.Meta.Shapes
{
    public class Box : Shape
    {
        public float Width { get; set; }
        public float Height { get; set; }

        public float Right() { return Object.Position.X + Offset.X + Width; }
        public float Bottom() { return Object.Position.Y + Offset.Y + Height; }

        public Box(Vector2 Offset, Vector2 Size) : base(Offset)
        {
            Width = Size.X;
            Height = Size.Y;
        }
        public Box(float X, float Y, float Width, float Height) : base(new Vector2(X, Y))
        {
            this.Width = Width;
            this.Height = Height;
        }

        public override bool Contains(Vector2 position)
        {
            return position.X >= Position().X &&
                   position.Y >= Position().Y &&
                   position.X <= Position().X + Width &&
                   position.Y <= Position().Y + Height;
        }
        public override bool Intersects(Shape shape)
        {
            if (shape is Circle)
            {
                Circle circle = (Circle)shape;
                return Vector2.Distance(circle.Position(), Closest(circle.Position())) <= circle.Radius;
            }
            if (shape is Box)
            {
                Box box = (Box)shape;
                return box.Contains(Closest(box.Position()));
            }

            return false;
        }
        public override Vector2 Closest(Vector2 position)
        {
            return new Vector2(MathHelper.Clamp(position.X, Position().X, Right()), MathHelper.Clamp(position.Y, Position().Y, Bottom()));
        }
        public override float Gap(Shape shape)
        {
            return Vector2.Distance(Closest(shape.Position()), shape.Closest(Position()));
        }

        public override void Draw(SpriteBatch sb, Texture2D pixel)
        {
            base.Draw(sb, pixel);

            sb.DrawSquare(pixel, pixel.Bounds, Position(), new Vector2(Width, Height), Color.White, 1, 1);
        }
    }
}
