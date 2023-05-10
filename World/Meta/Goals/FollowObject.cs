using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Merchantry.World.Meta.Goals
{
    public class FollowObject : ObjectGoal
    {
        public WorldObject Target { get; set; }
        public float PriorityDistance { get; set; }
        public float ArrivalDistance { get; set; }

        public FollowObject(WorldObject Target, float PriorityDistance, float ArrivalDistance)
        {
            this.Target = Target;
            this.PriorityDistance = PriorityDistance;
            this.ArrivalDistance = ArrivalDistance;

            PriorityFormula = () =>
            {
                float distance = Vector2.Distance(Object.Position, Target.Position);
                if (distance >= this.PriorityDistance)
                    return 1 - (distance / (this.PriorityDistance * 2));
                else
                {
                    Fail();
                    return 0;
                }
            };
        }

        public override void Update(GameTime gt)
        {
            Object.Physics.MoveTo(gt, Target.Position, Object.Stats.Speed, ArrivalDistance);

            if (Vector2.Distance(Object.Position, Target.Position) <= ArrivalDistance)
                Success();

            base.Update(gt);
        }
    }
}
