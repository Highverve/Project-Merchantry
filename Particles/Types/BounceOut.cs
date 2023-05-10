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
    public class BounceOut : ParticleEngine.Particle
    {
        public BounceOut(Vector2 Position) : base()
        {
            position = Position;
        }

        public override void Initialize() { base.Initialize(); }
        public override void Load(ContentManager cm) { base.Load(cm); }

        public override void Update(GameTime gt)
        {
            if (CurrentTime > 1100 && CurrentTime < 1150)
                Scale += new Vector2(.15f) * (float)gt.ElapsedGameTime.TotalSeconds;
            if (CurrentTime > 1000 && CurrentTime < 1100)
                Scale += new Vector2(.75f) * (float)gt.ElapsedGameTime.TotalSeconds;
            if (CurrentTime <= 1000)
                Scale -= new Vector2(3f) * (float)gt.ElapsedGameTime.TotalSeconds;
            if (Scale.X <= 0)
                SetTime(0);

            base.Update(gt);
        }
    }
}
