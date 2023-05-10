using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Pathfinding.Tiles
{
    public class PathTile
    {
        public Point Position { get; private set; }
        public float BaseCost { get; set; } = 1;
        public bool IsActive { get; set; } = true;
        public virtual Color Color { get; set; } = Color.White;

        public virtual float Cost(PathTile last)
        {
            return BaseCost + DiagonalCost(last);
        }

        /// <summary>
        /// Returns 1 if tile is diagonal from last. Only use with neighbours.
        /// </summary>
        /// <param name="last"></param>
        /// <returns></returns>
        public float DiagonalCost(PathTile last)
        {
            if (last.Position.X != Position.X && last.Position.Y != Position.Y)
                return .4f;
            else
                return 0;
        }

        public bool IsSouth(Point position) { return position.X == Position.X && position.Y == Position.Y + 1; }
        public bool IsNorth(Point position) { return position.X == Position.X && position.Y == Position.Y - 1; }
        public bool IsEast(Point position) { return position.X == Position.X + 1 && position.Y == Position.Y; }
        public bool IsWest(Point position) { return position.X == Position.X - 1 && position.Y == Position.Y; }

        public PathTile(Point Position, float BaseCost)
        {
            this.Position = Position;
            this.BaseCost = BaseCost;
        }
    }
}
