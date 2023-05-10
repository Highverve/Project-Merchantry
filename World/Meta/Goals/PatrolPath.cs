using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Meta.Goals
{
    public class PatrolPath : ObjectGoals
    {
        /// <summary>
        /// Must be larger than RemoveDistance, otherwise will not move.
        /// Math.Min applied automatically.
        /// </summary>
        public float StopDistance { get; set; }
        /// <summary>
        /// Lower causes sharper turns, higher causes smoother.
        /// </summary>
        public float RemoveDistance { get; set; }
        public bool IgnoreTileCost { get; set; } = false;

        public int Index { get; set; } = -1;
        public Point[] Destinations { get; private set; }

        /// <summary>
        /// Set to -1 for infinite laps (default value).
        /// </summary>
        public int PatrolCount { get; set; } = -1;

        public PatrolPath(float StopDistance, float RemoveDistance, float Priority, params Point[] Destinations)
        {
            this.StopDistance = Math.Min(StopDistance, RemoveDistance);
            this.RemoveDistance = RemoveDistance;
            BasePriority = Priority;

            this.Destinations = Destinations;
        }

        private void NextDestination()
        {
            Index++;
            if (Index >= Destinations.Length)
            {
                Index = 0;
                if (PatrolCount > 0)
                {
                    PatrolCount--;
                    if (PatrolCount == 0)
                        Success();
                }
            }

            if (PatrolCount > 0 || PatrolCount == -1)
            {
                AddGoal("Destination" + Index, new FollowPath(Destinations[Index], StopDistance, RemoveDistance, Index, IgnoreTileCost));
                Goals["Destination" + Index].OnSuccess += () => NextDestination();
            }
        }

        public override void Initialize()
        {
            NextDestination();

            base.Initialize();
        }
        public override void Update(GameTime gt)
        {
            base.Update(gt);
        }
        public override void Success() { IsDestruct = true; base.Success(); }
    }
}
