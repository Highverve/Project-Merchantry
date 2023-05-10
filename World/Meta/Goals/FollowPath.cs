using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Meta.Goals
{
    public class FollowPath : ObjectGoal
    {
        public Point End { get; set; }
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

        public FollowPath(Point End, float StopDistance, float RemoveDistance, float Priority, bool IgnoreTileCost = false)
        {
            this.End = End;
            this.StopDistance = Math.Min(StopDistance, RemoveDistance);
            this.RemoveDistance = RemoveDistance;
            this.IgnoreTileCost = IgnoreTileCost;
            BasePriority = Priority;
        }

        public override void Initialize()
        {
            Object.Path.Find(Object.Path.ToPoint(Object.Position), End, IgnoreTileCost);
            base.Initialize();

            if (Object.World.NavGrid.IsExists(Object.Path.ToPoint(Object.Position)) == false ||
                Object.World.NavGrid.IsExists(End) == false)
                Fail();
        }
        public override void Update(GameTime gt)
        {
            Object.Physics.MoveTo(gt, Object.Path.Next(), Object.Stats.Speed, StopDistance);

            if (Vector2.Distance(Object.Path.Next(), Object.Position) <= RemoveDistance)
            {
                Object.Path.RemoveFirst();
                if (Object.Path.IsComplete())
                    Success();
            }

            base.Update(gt);
        }
        public override void Success() { IsDestruct = true; base.Success(); }
        public override void Fail() { IsDestruct = true; base.Fail(); }
    }
}
