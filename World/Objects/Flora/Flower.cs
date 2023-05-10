using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AetaLibrary.Extensions;
using ExtensionsLibrary.Extensions;

namespace Merchantry.World.Objects.Flora
{
    public class Flower : RenderObject
    {
        protected float offset = 0;
        protected float speed = 1;
        protected float multi = .1f;

        public Flower(string ID, Vector2 Position, Texture2D Texture) : base(ID, Position, Texture)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            ROTATE_Speed = 7.5f;
            offset = trueRandom.NextFloat(-.5f, .5f);
            speed = 2 + trueRandom.NextFloat(-.5f, .5f);
            multi = trueRandom.NextFloat(0.075f, .1f);
        }

        private float timer = 0;
        public override void Update(GameTime gt)
        {
            timer += speed * (float)gt.ElapsedGameTime.TotalSeconds;
            ROTATE_Loose((float)(Math.Sin(timer + Position.X) * multi) + offset);
            
            base.Update(gt);
        }
    }
}
