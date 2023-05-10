using AetaLibrary.Extensions;
using ExtensionsLibrary.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Meta.Goals
{
    public class WanderVector : ObjectGoal
    {
        public float MinDistance { get; set; }
        public float MaxDistance { get; set; }
        public int MinTime { get; set; }
        public int MaxTime { get; set; }

        private int timer = 0;
        Vector2 target;
        private void SelectRandom()
        {
            float rotation = Object.TrueRandom.NextFloat(-MathHelper.Pi, MathHelper.Pi);
            float distance = Object.TrueRandom.NextFloat(MinDistance, MaxDistance);
            Vector2 offset = new Vector2((float)Math.Cos(rotation), (float)Math.Sin(rotation));

            target = Object.Position + (offset * distance);
        }

        public WanderVector(float MinDistance, float MaxDistance, int MinTime, int MaxTime, float Priority)
        {
            this.MinDistance = MinDistance;
            this.MaxDistance = MaxDistance;
            this.MinTime = MinTime;
            this.MaxTime = MaxTime;

            BasePriority = Priority;
        }

        public override void Initialize() { SelectRandom(); }
        public override void Update(GameTime gt)
        {
            if (timer <= 0)
            {
                Object.Physics.MoveTo(gt, target, 800, 40);
                if (Vector2.Distance(Object.Position, target) <= 40)
                    Success();
            }
            else
                timer -= gt.ElapsedGameTime.Milliseconds;

            base.Update(gt);
        }
        public override void Success()
        {
            timer = Object.TrueRandom.Next(MinTime, MaxTime);
            SelectRandom();

            base.Success();
        }
    }
}
