using ExtensionsLibrary.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Meta.Goals
{
    public class WanderPath : ObjectGoal
    {
        public Point Offset { get; set; }
        public Point AreaSize { get; set; }
        public int MinStopTime { get; set; }
        public int MaxStopTime { get; set; }
        public float TileDistance { get; set; }
        public Point Target { get; set; }
        public bool IgnoreTileCost { get; set; }

        private int timer = 0, failCount = 0;
        private void SelectRandom()
        {
            timer = Object.TrueRandom.Next(MinStopTime, MaxStopTime);
            failCount = 0;

            while (Target == Point.Zero)
            {
                Point origin = Offset + Object.Path.ToPoint(Object.Position);

                int x = Object.TrueRandom.Next(origin.X - (AreaSize.X / 2), origin.X + (AreaSize.X / 2));
                int y = Object.TrueRandom.Next(origin.Y - (AreaSize.Y / 2), origin.Y + (AreaSize.Y / 2));

                if (GameManager.References().World.NavGrid.IsExists(new Point(x, y)))// && (origin.X != x && origin.Y != y))
                {
                    Target = new Point(x, y);
                    Object.Path.Find(origin, Target, IgnoreTileCost);
                }
                else
                    failCount++;

                if (failCount > 5)
                    break;
            }
        }

        public WanderPath(Point Offset, Point AreaSize, int MinStopTime, int MaxStopTime, float TileDistance, bool IgnoreTileCost, float Priority)
        {
            this.Offset = Offset;
            this.AreaSize = AreaSize;
            this.MinStopTime = MinStopTime;
            this.MaxStopTime = MaxStopTime;
            this.TileDistance = TileDistance;

            BasePriority = Priority;
        }

        public override void Initialize() { SelectRandom(); }
        public override void Update(GameTime gt)
        {
            if (timer <= 0)
            {
                if (Object.Path.IsComplete() == false)
                {
                    Object.Physics.MoveTo(gt, Object.Path.Next(), Object.Stats.Speed, TileDistance - 1);

                    if (Vector2.Distance(Object.Path.Next(), Object.Position) <= TileDistance)
                    {
                        Object.Path.RemoveFirst();
                        if (Object.Path.IsComplete())
                            Success();
                    }
                }
                else if (failCount > 5)
                    Success();
            }
            else
                timer -= gt.ElapsedGameTime.Milliseconds;

            base.Update(gt);
        }
        public override void Success()
        {
            Target = Point.Zero;
            SelectRandom();

            base.Success();
        }
    }
}
