using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Meta.Shapes
{
    public class Circle : Shape
    {
        public float Radius { get; set; }

        public Circle(Vector2 Offset, float Radius) : base(Offset)
        {
            this.Radius = Radius;
        }

        public override bool Contains(Vector2 position)
        {
            return Vector2.Distance(Position(), position) <= Radius;
        }
        public override bool Intersects(Shape shape)
        {
            if (shape is Box)
            {
                Box box = (Box)shape;
                return Vector2.Distance(Position(), shape.Closest(Position())) <= Radius;
            }
            if (shape is Circle)
            {
                Circle circle = (Circle)shape;
                return Vector2.Distance(Position(), circle.Position()) < (Radius + circle.Radius);
            }

            return false;
        }
        public override Vector2 Closest(Vector2 position)
        {
            Vector2 direction = position - Position();
            if (direction != Vector2.Zero)
                direction.Normalize();

            return direction * Math.Min(Vector2.Distance(Position(), position), Radius);
        }
        public override float Gap(Shape shape)
        {
            return Vector2.Distance(Closest(shape.Position()), shape.Closest(Position()));
        }
    }
}
