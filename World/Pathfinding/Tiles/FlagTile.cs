using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Pathfinding.Tiles
{
    public class FlagTile : PathTile
    {
        #region Flags

        public const string FLAG_Wall = "WALL";
        public const string FLAG_FloorUp = "UP";
        public const string FLAG_FloorDown = "DOWN";

        #endregion
        public override Color Color { get; set; } = Color.Blue;

        public string Flag { get; private set; }

        public FlagTile(Point Position, int Cost, string Flag)
            : base(Position, Cost)
        {
            this.Flag = Flag;
        }
    }
}
