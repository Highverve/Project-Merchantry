using AetaLibrary.Elements.Images;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Merchantry.UI.Elements
{
    public class DividerElement : ImageElement
    {
        public int EdgeSize { get; set; }
        public int StretchSize { get; set; }
        public int MiddleSize { get; set; }
        public int Width { get; set; }

        public DividerElement(int RenderOrder, Vector2 Offset, Texture2D Texture, Color Color, Vector2 Origin, int EdgeSize, int StretchSize, int MiddleSize, int Width)
            : base(RenderOrder, Offset, Texture, Rectangle.Empty, Color, Origin, Vector2.One, 0, SpriteEffects.None)
        {
            this.EdgeSize = EdgeSize;
            this.StretchSize = StretchSize;
            this.MiddleSize = MiddleSize;
            this.Width = Width;
        }

        public override void Load(ContentManager cm)
        {
            base.Load(cm);
        }
        public override void Update(GameTime gt)
        {
            base.Update(gt);
        }

        public override void Draw(SpriteBatch sb)
        {
            //Left edge
            sb.Draw(Texture, new Rectangle((int)Position.X, (int)Position.Y, EdgeSize, Texture.Height),
                new Rectangle(0, 0, EdgeSize, Texture.Height),
                RenderColor.CurrentColor, 0, Vector2.Zero, SpriteEffects.None, 0);

            //Left stretch
            sb.Draw(Texture, new Rectangle((int)Position.X + EdgeSize, (int)Position.Y, Width / 2, Texture.Height),
                new Rectangle(EdgeSize, 0, StretchSize, Texture.Height),
                RenderColor.CurrentColor, 0, Vector2.Zero, SpriteEffects.None, 0);

            //Middle
            sb.Draw(Texture, new Rectangle((int)Position.X + EdgeSize + (Width / 2), (int)Position.Y, MiddleSize, Texture.Height),
                new Rectangle(EdgeSize + StretchSize, 0, MiddleSize, Texture.Height),
                RenderColor.CurrentColor, 0, Vector2.Zero, SpriteEffects.None, 0);

            //Right stretch
            sb.Draw(Texture, new Rectangle((int)Position.X + EdgeSize + (Width / 2) + MiddleSize, (int)Position.Y, Width / 2, Texture.Height),
                new Rectangle(EdgeSize + StretchSize + MiddleSize, 0, StretchSize, Texture.Height),
                RenderColor.CurrentColor, 0, Vector2.Zero, SpriteEffects.None, 0);

            //Right edge
            sb.Draw(Texture, new Rectangle((int)Position.X + EdgeSize + (Width / 2) + MiddleSize + (Width / 2), (int)Position.Y, EdgeSize, Texture.Height),
                new Rectangle(EdgeSize + StretchSize + MiddleSize + StretchSize, 0, EdgeSize, Texture.Height),
                RenderColor.CurrentColor, 0, Vector2.Zero, SpriteEffects.None, 0);

            /*
            Rectangle source = new Rectangle(0, 0, EdgeSize, Texture.Height);
            //Left Corner
            sb.Draw(Texture, Position, source, RenderColor.CurrentColor, 0, Origin, Scale, SpriteEffects.None, 0);

            //Left stretch
            source.X += EdgeSize;
            source.Width = StretchSize;
            sb.Draw(Texture, Position + new Vector2(EdgeSize, 0), source, RenderColor.CurrentColor, 0, Origin, Scale, SpriteEffects.None, 0);

            sb.Draw(Texture, Position, new Rectangle(0, 0, SideSize, Texture.Height), RenderColor.CurrentColor, 0, Origin, Vector2.One, SpriteEffects.None, 0);
            sb.Draw(Texture, Position + new Vector2(SideSize, 0), new Rectangle(SideSize, 0, MiddleSize, Texture.Height), RenderColor.CurrentColor, 0, Origin, Vector2.One, SpriteEffects.None, 0);
            sb.Draw(Texture, Position + new Vector2(SideSize + MiddleSize, 0), new Rectangle(SideSize + MiddleSize, 0, SideSize, Texture.Height), RenderColor.CurrentColor, 0, Origin, Vector2.One, SpriteEffects.None, 0);*/
        }
    }
}
