using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Tiles
{
    public class DepthTile : Tile
    {
        public float DepthPosition { get; set; }

        public DepthTile(Texture2D Texture, float DepthPosition) : base(Texture)
        {
            this.DepthPosition = DepthPosition;
        }
        public DepthTile(Texture2D Texture, Point Frame, Point Coords, float DepthPosition) : base(Texture, Frame, Coords)
        {
            this.DepthPosition = DepthPosition;
        }
        public DepthTile(Texture2D Texture, int FrameX, int FrameY, int CoordsX, int CoordsY, float DepthPosition) : base(Texture, new Point(FrameX, FrameY), new Point(CoordsX, CoordsY))
        {
            this.DepthPosition = DepthPosition;
        }

        public override void Initialize()
        {
            base.Initialize();

            onPositionChange += RefreshDepth;
            RefreshDepth();
        }
        private void RefreshDepth()
        {
            DepthOffset = GameManager.References().World.CalculateDepth(Position.Y + DepthPosition);
        }

        public override void Update(GameTime gt)
        {
            base.Update(gt);
        }
    }
}
