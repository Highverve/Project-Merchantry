using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Merchantry.World.Pathfinding.Tiles
{
    public class DirectionalTile : PathTile
    {
        public float NorthCost { get; set; }
        public float SouthCost { get; set; }
        public float EastCost { get; set; }
        public float WestCost { get; set; }
        public override Color Color { get; set; } = Color.OrangeRed;

        public override float Cost(PathTile last)
        {
            //Moving north ...
            if (IsNorth(last.Position))
                return BaseCost + NorthCost;
            if (IsSouth(last.Position))
                return BaseCost + SouthCost;
            if (IsEast(last.Position))
                return BaseCost + EastCost;
            if (IsWest(last.Position))
                return BaseCost + WestCost;

            return base.Cost(last);
        }

        public DirectionalTile(Point Position, float BaseCost,
               float NorthCost, float SouthCost, float EastCost,
               float WestCost) : base(Position, BaseCost)
        {
            this.NorthCost = NorthCost;
            this.SouthCost = SouthCost;
            this.EastCost = EastCost;
            this.WestCost = WestCost;
        }
    }
}
