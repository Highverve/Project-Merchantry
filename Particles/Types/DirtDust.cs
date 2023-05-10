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
    public class DirtDust : ParticleEngine.Particle
    {
        float rotationVelocity;
        public DirtDust(Vector2 Position) : base() { position = Position; }

        public override void Initialize()
        {
            base.Initialize();
            rotationVelocity = random.Next(0, 10);
        }
        public override void Load(ContentManager cm)
        {
            Texture = cm.Load<Texture2D>("Assets/Effects/Particles/dust");

            Source = random.NextObject(new Rectangle(15, 8, 17, 16), new Rectangle(32, 5, 21, 19), new Rectangle(53, 0, 26, 24));
            Origin = Source.Size.ToVector2() / 2;
            Color = Color.Lerp(new Color(230, 170, 125, 255), new Color(180, 125, 90, 255), random.NextFloat(0f, 1));
            SetTime(random.Next(1500, 3000));
            Scale = Vector2.One;
        }

        public override void Update(GameTime gt)
        {
            Angle += rotationVelocity * (float)gt.ElapsedGameTime.TotalSeconds;
            rotationVelocity -= rotationVelocity * 1f * (float)gt.ElapsedGameTime.TotalSeconds;

            if (CurrentTime > 1100 && CurrentTime < 1150)
                Scale += new Vector2(.15f) * (float)gt.ElapsedGameTime.TotalSeconds;
            if (CurrentTime > 1000 && CurrentTime < 1100)
                Scale += new Vector2(.75f) * (float)gt.ElapsedGameTime.TotalSeconds;
            if (CurrentTime <= 1000)
                Scale -= new Vector2(3f) * (float)gt.ElapsedGameTime.TotalSeconds;
            if (Scale.X <= 0)
                SetTime(0);

            Velocity -= Velocity * 5f * (float)gt.ElapsedGameTime.TotalSeconds;

            base.Update(gt);
        }
    }
}
