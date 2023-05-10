using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Merchantry.World.Objects.Various
{
    public class Expression : WorldObject
    {
        public Texture2D Expressions { get; set; }
        public WorldObject Object { get; private set; }

        public Expression(string ID, Vector2 Position, WorldObject Object) : base(ID, Position)
        {
            this.Object = Object;

            IsSectoring = false;
            IsAcceptingItems = false;
            IsSelectable = false;

            Scale = Vector2.Zero;
            SCALE_Speed = 5;
        }

        public override void Load(ContentManager cm)
        {
            base.Load(cm);

            Expressions = cm.Load<Texture2D>("UI/HUD/expressions");

            Animation.FrameSpeed = 100;
            Animation.FrameSize = new Point(64, 64);
            Animation.AddState("Exclamation", () =>
            {
                Animation.CurrentFrame = Point.Zero;
                Animation.FrameSpeed = 300;
                Origin = new Vector2(30, 45);

                Reset();
            }, () =>
            {
                if (Scale.Y > 1.25f)
                    SCALE_Loose(new Vector2(1f, 1f));
                else
                    SCALE_Loose(new Vector2(1.25f, 1.5f));
                SCALE_Speed = 5f;
            });
            Animation.AddState("Question", () =>
            {
                Animation.CurrentFrame = new Point(1, 0);
                Origin = new Vector2(30, 47);
                Animation.FrameSpeed = 300;

                Reset();
            }, () =>
            {
                if (Rotation > 0)
                    ROTATE_Loose(-.25f);
                else
                    ROTATE_Loose(.25f);
                ROTATE_Speed = 2.5f;
            });

            Queue.Add(0, Outro);
        }
        private void Reset()
        {
            SCALE_Loose(1, 1);
            ROTATE_Loose(0);
        }

        public override void Update(GameTime gt)
        {
            base.Update(gt);
        }

        public override void Draw(SpriteBatch sb)
        {
            Depth = Object.Depth;
            sb.Draw(Expressions, Physics.AltitudePosition(), SourceRect, Color, Rotation, Origin, Scale, SpriteEffects.None, Depth);

            base.Draw(sb);
        }
    }
}
