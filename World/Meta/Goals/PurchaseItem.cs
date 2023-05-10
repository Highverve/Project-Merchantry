using Merchantry.UI.Items;
using Merchantry.World.Objects.UIs;
using Merchantry.World.Objects.UIs.Elements;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Meta.Goals
{
    public class PurchaseItem : ObjectGoal
    {
        public MarketObject Market { get; set; }
        public List<ItemObject> LookingFor { get; private set; }

        public float PurchaseChance { get; set; }
        public int MinDelay { get; set; }
        public int MaxDelay { get; set; }
        public int Timer { get; set; }

        public PurchaseItem(MarketObject Market, float PurchaseChance, int MinDelay, int MaxDelay, params ItemObject[] LookingFor)
        {
            this.Market = Market;
            this.PurchaseChance = PurchaseChance;
            this.MinDelay = MinDelay;
            this.MaxDelay = MaxDelay;

            this.LookingFor = LookingFor.ToList();

            BasePriority = 1;
            //PriorityFormula = () => { return (float)Object.Storage.Currency() / 100; };
        }

        public override void Initialize()
        {
            base.Initialize();
            Timer = Object.TrueRandom.Next(MinDelay, MaxDelay);
        }
        public override void Update(GameTime gt)
        {
            if (Timer < 0)
                Purchase();
            else
                Timer -= gt.ElapsedGameTime.Milliseconds;

            base.Update(gt);
        }
        public override void Success()
        {
            base.Success();
            Initialize();
        }
        public override void Fail()
        {
            IsDestruct = true;
            base.Fail();
        }

        public void Purchase()
        {
            if (LookingFor.Count > 0 && Market.IsSelling == true)
            {
                ItemObject item = LookingFor[Object.TrueRandom.Next(0, LookingFor.Count)];
                LookingFor.Remove(item);

                foreach (ItemSlot slot in Market.Slots.Values)
                {
                    //Check if slot contains specified item
                    if (slot.Item != null && slot.Item.ID == item.ID)
                    {
                        //Buy up to max random, or within budget.
                        int quantity = MathHelper.Min(Object.TrueRandom.Next(1, MathHelper.Min(slot.Item.Quantity, item.Quantity)), Object.Storage.Currency() / slot.Item.BaseWorth);
                        if (quantity > 0)
                        {
                            int quantityWorth = (int)(slot.Item.QuantityWorth(quantity) * 1.25f);
                            Market.CallOnPurchase(Object, slot.Item, quantity);

                            Object.Storage.RemoveItem("Currency", quantityWorth);
                            Object.Storage.AddItem(slot.Item.Split(quantity));
                            Market.Merchant.Storage.AddItem("Currency", quantityWorth);
                            break;
                        }
                    }
                }
            }
            else
                Fail();

            if (Object.TrueRandom.NextDouble() > PurchaseChance)
                Fail();
            else
                Success();
        }
    }
}
