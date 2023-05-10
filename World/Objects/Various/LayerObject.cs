using Merchantry.World.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Merchantry.World.Objects.Various
{
    public class LayerObject : RenderObject
    {
        public TileLayer Layer { get; private set; }

        public LayerObject(string ID, Vector2 Position, TileLayer Layer) : base(ID, Position, null)
        {
            this.Layer = Layer;
            this.Layer.Attached = this;
        }

        public override void Update(GameTime gt)
        {
            if (Layer != null)
            {
                Layer.Position = Physics.AltitudePosition() - Layer.Origin;
                Layer.Scale = Scale;
                Layer.Rotation = Rotation;
                Layer.Depth = Depth;
            }

            base.Update(gt);
        }
        public override void Draw(SpriteBatch sb)
        {
            //GameManager.References().Debugging.DrawWireframe(sb, (int)Position.X, (int)Position.Y, 0, 0, "\n\n\n" + Layer.Position.X + "," + Layer.Position.Y + "\n" + Layer.Origin.X + "," + Layer.Origin.Y + "\n" + Layer.Rotation, "");

            base.Draw(sb);
        }
    }
}
