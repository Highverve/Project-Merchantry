using AetaLibrary.Extensions;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Meta.Goals
{
    public class FlyTo : ObjectGoal
    {
        public Vector2 Position { get; set; }
        public float Altitude { get; set; }

        public FlyTo(Vector2 Position, float Altitude, float Priority)
        {
            this.Position = Position;
            this.Altitude = Altitude;

            BasePriority = Priority;
        }

        public override void Initialize() { base.Initialize(); }
        public override void Update(GameTime gt)
        {
            Object.Physics.MoveTo(gt, Position, 1000, 40);
            Object.Physics.FlyTo(gt, Altitude, 100, 10);

            if (Vector2.Distance(Object.Position, Position) <= 50 &&
               (Object.Physics.Altitude >= Altitude - 10 || Object.Physics.Altitude <= Altitude + 10))
            {
                Success();
            }

            base.Update(gt);
        }
        public override void Success() { IsDestruct = true; base.Success(); }
    }
}
