using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ExtensionsLibrary.Extensions;

namespace Merchantry.World.Objects.Flora
{
    public class SpringFlower : Flower
    {
        public float Springiness { get; set; }
        private int springTimer = 0;
        private float springTarget = 0, speedOffset;

        public SpringFlower(string ID, Vector2 Position, Texture2D Texture, float Springiness) : base(ID, Position, Texture)
        {
            this.Springiness = Springiness;

            Origin = new Vector2(17, 53);
            IsSelectable = false;

            SmoothScale.Y.Speed = trueRandom.NextFloat(2.5f, 7.5f);
            /*SmoothScale.Y.OnComplete += () =>
            {
                if (SmoothScale.Y.Result <= 1)
                    SmoothScale.Y.SetSmoothStep(springTarget = trueRandom.NextFloat(1, 1 + Springiness));
                else if (SmoothScale.Y.Result >= springTarget)
                    SmoothScale.Y.SetSmoothStep(1);
            };
            SmoothScale.Y.InvokeCompleteEvent();*/
        }

        public override void Initialize()
        {
            base.Initialize();
            multi = .05f;
            speedOffset = trueRandom.NextFloat(.002f, .0075f);
        }

        public override void Update(GameTime gt)
        {
            springTimer += gt.ElapsedGameTime.Milliseconds;
            SmoothScale.Y.SetLoose(1 + (float)Math.Sin(SmoothRotation.Result + springTimer * speedOffset) * Springiness);

            base.Update(gt);
        }
    }
}
