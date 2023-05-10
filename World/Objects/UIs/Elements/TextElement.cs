using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Objects.UIs.Elements
{
    public class TextElement : Element
    {
        public SpriteFont Font { get; set; }
        public string Text { get; set; }

        public TextElement(Vector2 Offset, SpriteFont Font, string Text, Color Color)
            : this(Offset, Font, Text, Color, Vector2.One, 0, Vector2.Zero) { }
        public TextElement(Vector2 Offset, SpriteFont Font, string Text, Color Color, Vector2 Scale)
            : this(Offset, Font, Text, Color, Scale, 0, Vector2.Zero) { }
        public TextElement(Vector2 Offset, SpriteFont Font, string Text, Color Color, Vector2 Scale, float Rotation, Vector2 Origin) : base(Offset)
        {
            this.Font = Font;
            this.Text = Text;
            this.Color = Color;
            this.Scale = Scale;
            this.Rotation = Rotation;
            this.Origin = Origin;
        }
        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Draw(SpriteBatch sb)
        {
            sb.DrawString(Font, Text, CombinedPosition(), Color, CombinedRotation(), Origin, Scale, SpriteEffects.None, UI.Depth + DepthOffset);
        }
    }
}
