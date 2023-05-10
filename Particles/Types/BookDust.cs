using ExtensionsLibrary.Extensions;
using Merchantry.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.Particles.Types
{
    public class BookDust : ParticleEngine.Particle
    {
        float rotationVelocity;
        float xVel, yVel;
        float velocityMultiplier;

        public BookDust(Vector2 Position, float VelocityMultiplier) : base()
        {
            position = Position;
            velocityMultiplier = VelocityMultiplier;
        }

        public override void Initialize()
        {
            base.Initialize();

            rotationVelocity = random.Next(0, 10);
            xVel = random.NextFloat(-200, -50);
            yVel = random.NextFloat(10, 50);
            //Scale = new Vector2(yVel / 75);
        }
        public override void Load(ContentManager cm)
        {
            Texture = cm.Load<Texture2D>("Assets/Effects/Particles/dust");

            Source = random.NextObject(new Rectangle(15, 8, 17, 16), new Rectangle(32, 5, 21, 19), new Rectangle(53, 0, 26, 24));
            Origin = Source.Size.ToVector2() / 2;//new Vector2(random.Next(1, 30), random.Next(1, 30));

            Color = Color.Lerp(new Color(230, 170, 125, 255), new Color(180, 125, 90, 255), random.NextFloat(0f, 1));
            SetTime(random.Next(1500, 3000));

            Velocity = new Vector2(random.NextFloat(-100, -5) * velocityMultiplier, random.NextFloat(-100, -5) * velocityMultiplier);
        }

        public override void Update(GameTime gt)
        {
            Angle += rotationVelocity * (float)gt.ElapsedGameTime.TotalSeconds;
            rotationVelocity -= rotationVelocity * 1f * (float)gt.ElapsedGameTime.TotalSeconds;

            //if (CurrentTime <= 500)
            //    Scale -= Scale * 5f * (float)gt.ElapsedGameTime.TotalSeconds;
            //else               
            if (CurrentTime > 1100 && CurrentTime < 1150)
                Scale += new Vector2(.15f) * (float)gt.ElapsedGameTime.TotalSeconds;
            if (CurrentTime > 1000 && CurrentTime < 1100)
                Scale += new Vector2(.75f) * (float)gt.ElapsedGameTime.TotalSeconds;
            if (CurrentTime <= 1000)
                Scale -= new Vector2(3f) * (float)gt.ElapsedGameTime.TotalSeconds;
            if (Scale.X <= 0)
                SetTime(0);

            Velocity -= Velocity * 5f * (float)gt.ElapsedGameTime.TotalSeconds;
            velocity.Y += yVel * (float)gt.ElapsedGameTime.TotalSeconds;
            velocity.X += xVel * (float)gt.ElapsedGameTime.TotalSeconds;

            base.Update(gt);
        }
    }
}
