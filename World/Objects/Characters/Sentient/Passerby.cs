using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Merchantry.World.Meta.Goals;

namespace Merchantry.World.Objects.Characters.Sentient
{
    public class Passerby : Adult
    {
        Point destination;
        public Passerby(string ID, Vector2 Position, Texture2D Texture, string DisplayName, Point Destination) : base(ID, Position, Texture, DisplayName)
        {
            destination = Destination;
        }

        public override void Initialize()
        {
            base.Initialize();

            AI.AddGoal("GoTo", new FollowPath(destination, 32, 24, 1));
            AI.Goals["GoTo"].OnSuccess += Destroy;
        }
    }
}
