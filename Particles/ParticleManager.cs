using ExtensionsLibrary.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.Particles
{
    public class ParticleManager : ParticleEngine.ParticleManager
    {
        public ParticleManager() { }

        public void AddCircle(string groupID, int quantity, float directionShift, float minDistance, float maxDistance, Func<ParticleEngine.Particle> particle)
        {
            float direction = 0;
            float step = (float)(Math.PI * 2) / quantity;

            Add(groupID, quantity, particle,
            (p) =>
            {
                direction += step + random.NextFloat(-directionShift, directionShift);
                p.Position += new Vector2((float)Math.Cos(direction), (float)Math.Sin(direction)) * random.NextFloat(minDistance, maxDistance);
            });
        }
        public void AddBox(string groupID, int quantity, Vector2 position, Vector2 size, Func<ParticleEngine.Particle> particle)
        {
            Add(groupID, quantity, particle,
            (p) =>
            {
                p.Position = new Vector2(random.NextFloat(position.X, position.X + size.X),
                                         random.NextFloat(position.Y, position.Y + size.Y));
            });
        }

        public void AddCircleExpand(string groupID, int quantity, float directionShift, float minDistance, float maxDistance, Func<ParticleEngine.Particle> particle)
        {
            float direction = 0;
            float step = (float)(Math.PI * 2) / quantity;

            Add(groupID, quantity, particle,
            (p) =>
            {
                direction += step + random.NextFloat(-directionShift, directionShift);
                p.Velocity += new Vector2((float)Math.Cos(direction), (float)Math.Sin(direction)) * random.NextFloat(minDistance, maxDistance);
            });
        }
    }
}
