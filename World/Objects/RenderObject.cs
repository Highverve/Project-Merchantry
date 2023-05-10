using Merchantry.Extensions;
using Merchantry.World.Meta;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Objects
{
    public class RenderObject : WorldObject
    {
        public Texture2D Texture { get; set; }
        public Rectangle WorldRect()
        {
            return new Rectangle((int)(Physics.AltitudePosition().X - Origin.X),
                                 (int)(Physics.AltitudePosition().Y - Origin.Y),
                                 SourceRect.Width, SourceRect.Height);
        }

        public RenderObject(string ID, Vector2 Position, Texture2D Texture) : base(ID, Position)
        {
            this.Texture = Texture;
            if (Texture != null)
                SourceRect = Texture.Bounds;
        }

        public override void Initialize()
        {
            base.Initialize();

            Queue.Add(500, () =>
            {
                float xPct = Origin.X / SourceRect.Width;
                float yPct = Origin.Y / SourceRect.Height;

                Shape.Add("MAIN_" + SHAPE_Select, new Meta.Shapes.Box(
                                     -(SourceRect.Width * xPct),
                                     -(SourceRect.Height * yPct),
                                     SourceRect.Width, SourceRect.Height)
                { Group = SHAPE_Select });
            });
        }

        public override void Update(GameTime gt)
        {
            base.Update(gt);
        }
        public override void Draw(SpriteBatch sb)
        {
            if (Texture != null)
                sb.Draw(Texture, Physics.AltitudePosition(), SourceRect, Color, Rotation, Origin, Scale, SpriteEffects.None, Depth);

            base.Draw(sb);
        }

        public override void HoverEnter(WorldObject user)
        {
            Color = Color.Lerp(Color.White, Color.Black, .25f);
            base.HoverEnter(user);
        }
        public override void HoverExit(WorldObject user)
        {
            Color = Color.White;
            base.HoverExit(user);
        }
        public override void LeftClick(WorldObject user)
        {
            base.LeftClick(user);
        }

        public override WorldObject Copy()
        {
            RenderObject obj = (RenderObject)base.Copy();

            //obj.smoothRotation = null;
            //obj.smoothScaleX = null;
            //obj.smoothScaleY = null;
            //obj.animation = animation.Copy();

            return obj;
        }
    }
}
