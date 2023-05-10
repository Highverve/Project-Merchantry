using Merchantry.UI.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Merchantry.Particles.Types;

namespace Merchantry.World.Meta.Goals
{
    public class DigItem : ObjectGoal
    {
        public ItemObject[] Pool { get; set; }
        public Vector2 ItemOffset { get; set; }

        public int DigSpeed { get; set; }
        public int MinDiggedItems { get; set; }
        public int MaxDiggedItems { get; set; }

        private int items = 0, digTimer = 0, dustTimer = 0, particleCount = 0;

        public DigItem(float Priority, int DigSpeed, int MinDiggedItems, int MaxDiggedItems, Vector2 ItemOffset, params ItemObject[] ItemPool)
        {
            BasePriority = Priority;
            this.DigSpeed = DigSpeed;

            this.MinDiggedItems = MinDiggedItems;
            this.MaxDiggedItems = MaxDiggedItems;
            this.ItemOffset = ItemOffset;

            Pool = ItemPool;
            if (ItemPool == null || ItemPool.Length == 0)
                Fail();
        }

        public override void Initialize()
        {
            items = Object.TrueRandom.Next(MinDiggedItems, MaxDiggedItems);
            digTimer = DigSpeed;
            base.Initialize();
        }

        public override void Update(GameTime gt)
        {
            if (digTimer >= DigSpeed)
                Dig();
            else
                digTimer += gt.ElapsedGameTime.Milliseconds;

            if (dustTimer <= 0)
            {
                GameManager.References().Particles.AddCircleExpand("WORLD", Object.TrueRandom.Next(3, 5), .2f, 50, 125, () =>
                {
                    particleCount++;
                    return new DirtDust(Object.Position - new Vector2(0, 15)) { Depth = Object.Depth + (Object.World.PixelDepth() * (10 + particleCount)) };
                });
                dustTimer = 250;
            }
            else
                dustTimer -= gt.ElapsedGameTime.Milliseconds;
            //Spawn dirt particles

            base.Update(gt);
        }

        public void Dig()
        {
            Object.Physics.Jump(200);
            GameManager.References().World.SpawnItem(Pool[Object.TrueRandom.Next(0, Pool.Length)].Copy(), Object.Position + ItemOffset);
            items--;
            digTimer = 0;

            if (items <= 0)
                Success();
        }

        public override void Success()
        {
            IsDestruct = true;
            base.Success();
        }
        public override void Fail()
        {
            IsDestruct = true;
            base.Fail();
        }
    }
}
