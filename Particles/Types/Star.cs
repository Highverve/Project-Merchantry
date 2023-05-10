using ExtensionsLibrary.Extensions;
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
    public class Firework : ParticleEngine.Particle
    {
        float rotationVelocity;

        public Firework(Vector2 Position) : base()
        {
            position = Position;
        }

        public override void Initialize()
        {
            base.Initialize();

            rotationVelocity = random.Next(0, 10);
        }
        public override void Load(ContentManager cm)
        {
            Texture = cm.Load<Texture2D>("Assets/Effects/Particles/stars");
            Source = random.NextObject(new Rectangle(0, 0, 10, 10), new Rectangle(10, 0, 10, 10), new Rectangle(20, 0, 10, 10),
                                      new Rectangle(0, 10, 18, 18), new Rectangle(18, 10, 18, 18), new Rectangle(36, 10, 18, 18));
            Origin = Source.Size.ToVector2() / 2;

            Color = Color.Lerp(Color.Beige, Color.Gold, (float)random.NextDouble());
            Scale = new Vector2(1);
            SetTime(700);
        }

        public override void Update(GameTime gt)
        {
            Angle += rotationVelocity * (float)gt.ElapsedGameTime.TotalSeconds;
            rotationVelocity -= rotationVelocity * 5 * (float)gt.ElapsedGameTime.TotalSeconds;

            //if (CurrentTime >= 500 && CurrentTime <= 750)
            //    Scale += new Vector2(15f) * (float)gt.ElapsedGameTime.TotalSeconds;
            if (CurrentTime <= 500)
                Scale -= Scale * 5f * (float)gt.ElapsedGameTime.TotalSeconds;

            if (CurrentTime <= 500)
                Velocity -= Velocity * 5f * (float)gt.ElapsedGameTime.TotalSeconds;
            //velocity.Y += 500f * (float)gt.ElapsedGameTime.TotalSeconds;

            base.Update(gt);
        }
    }
}
