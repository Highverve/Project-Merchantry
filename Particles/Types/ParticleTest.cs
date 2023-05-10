using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Merchantry.Particles.Types
{
    public class ParticleTest : ParticleEngine.Particle
    {
        float rotationVelocity;
        public ParticleTest(Vector2 Position) : base()
        {
            position = Position;
            FrameSize = new Point(1);
        }

        public override void Initialize()
        {
            base.Initialize();

            rotationVelocity = random.Next(0, 50);
        }
        public override void Load(ContentManager cm)
        {
            Texture = cm.Load<Texture2D>("Debug/pixel");
        }

        public override void Update(GameTime gt)
        {
            Angle += rotationVelocity * (float)gt.ElapsedGameTime.TotalSeconds;
            rotationVelocity -= rotationVelocity * 5 * (float)gt.ElapsedGameTime.TotalSeconds;

            if (CurrentTime >= 500 && CurrentTime <= 750)
                Scale += new Vector2(15f) * (float)gt.ElapsedGameTime.TotalSeconds;
            if (CurrentTime <= 500)
                Scale -= Scale * 5f * (float)gt.ElapsedGameTime.TotalSeconds;

            Velocity -= Velocity * 5f * (float)gt.ElapsedGameTime.TotalSeconds;
            velocity.Y += 500f * (float)gt.ElapsedGameTime.TotalSeconds;

            base.Update(gt);
        }
    }
}
