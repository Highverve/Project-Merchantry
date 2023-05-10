using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Merchantry.World.Meta.Goals;
using Merchantry.World.Objects.UIs;
using Merchantry.World.Objects.UIs.Elements;
using Merchantry.UI.Items;
using ExtensionsLibrary.Extensions;

namespace Merchantry.World.Objects.Characters.Sentient
{
    public class Customer : Adult
    {
        public ItemObject[] LookingFor { get; set; }
        public float PurchaseChance { get; set; }
        public int MinPurchaseDelay { get; set; }
        public int MaxPurchaseDelay { get; set; }
        public SpriteFont Font { get; set; }

        /* Farmer. Common bait, stone axe, stone spade.
         * Builder. Stone hammer, iron hammer, stick, stone.
         * Woodsman. Iron axe, wooden bow, ironn bow, stone arrow, iron arrow.
         * Traveller. Stone cudgel, iron dagger, iron longsword, stone arrow, iron arrow.
         * Fighter. Iron dagger, iron longsword, iron bow, iron arrow.
         * Artisan. Stick, stone, string, feather, iron ore, iron ingot.
         * Miner. Stone axe, stone pickaxe, stone spade, iron pickaxe, iron spade.
         * Fisher. Fishig rod, common bait, string, feather.
         */
        public ItemObject[] FarmerItems()
        {
            DisplayName = "Farmer";
            Storage.AddItem("Currency", TrueRandom.NextObject(7, 15, 20, 25));

            return new ItemObject[]
            {
                References.Assets.Items.Copy("GorgerChow", (i) => i.Quantity = 5),
                References.Assets.Items.Copy("StoneAxe", (i) => i.Quantity = 1),
                References.Assets.Items.Copy("StoneSpade", (i) => i.Quantity = 1),
                References.Assets.Items.Copy("ChurchwardenPipe", (i) => i.Quantity = 1),
            };
        }
        public ItemObject[] ConstructorItems()
        {
            DisplayName = "Builder";
            Storage.AddItem("Currency", TrueRandom.NextObject(25, 60, 85));
            return new ItemObject[]
            {
                References.Assets.Items.Copy("StoneHammer", (i) => i.Quantity = 1),
                References.Assets.Items.Copy("IronHammer", (i) => i.Quantity = 1),
                References.Assets.Items.Copy("Stick", (i) => i.Quantity = 5),
                References.Assets.Items.Copy("Stone", (i) => i.Quantity = 5),
            };
        }
        public ItemObject[] WoodsmanItems()
        {
            DisplayName = "Woodsman";
            Storage.AddItem("Currency", TrueRandom.NextObject(15, 25, 40, 50));

            return new ItemObject[]
            {
                References.Assets.Items.Copy("IronAxe", (i) => i.Quantity = 1),
                References.Assets.Items.Copy("WoodenBow", (i) => i.Quantity = 1),
                References.Assets.Items.Copy("IronBow", (i) => i.Quantity = 1),
                References.Assets.Items.Copy("StoneArrow", (i) => i.Quantity = 10),
                References.Assets.Items.Copy("IronArrow", (i) => i.Quantity = 5),
                References.Assets.Items.Copy("ChurchwardenPipe", (i) => i.Quantity = 1),
            };
        }
        public ItemObject[] TravellerItems()
        {
            DisplayName = "Traveller";
            Storage.AddItem("Currency", TrueRandom.NextObject(15, 25, 45));

            return new ItemObject[]
            {
                References.Assets.Items.Copy("StoneCudgel", (i) => i.Quantity = 1),
                References.Assets.Items.Copy("IronDagger", (i) => i.Quantity = 1),
                References.Assets.Items.Copy("IronLongsword", (i) => i.Quantity = 1),
                References.Assets.Items.Copy("StoneArrow", (i) => i.Quantity = 10),
                References.Assets.Items.Copy("IronArrow", (i) => i.Quantity = 5),
                References.Assets.Items.Copy("ChurchwardenPipe", (i) => i.Quantity = 1),
            };
        }
        public ItemObject[] KnightItems()
        {
            DisplayName = "Fighter";
            Storage.AddItem("Currency", TrueRandom.NextObject(25, 40, 65, 80, 100));

            return new ItemObject[]
            {
                References.Assets.Items.Copy("IronDagger", (i) => i.Quantity = 1),
                References.Assets.Items.Copy("IronLongsword", (i) => i.Quantity = 1),
                References.Assets.Items.Copy("IronBow", (i) => i.Quantity = 1),
                References.Assets.Items.Copy("IronArrow", (i) => i.Quantity = 5),
            };
        }
        public ItemObject[] ArtisanItems()
        {
            DisplayName = "Artisan";
            Storage.AddItem("Currency", TrueRandom.NextObject(20, 40, 60));
            return new ItemObject[]
            {
                References.Assets.Items.Copy("Stick", (i) => i.Quantity = 10),
                References.Assets.Items.Copy("Stone", (i) => i.Quantity = 10),
                References.Assets.Items.Copy("String", (i) => i.Quantity = 5),
                References.Assets.Items.Copy("Feather", (i) => i.Quantity = 5),
                References.Assets.Items.Copy("IronOre", (i) => i.Quantity = 5),
                References.Assets.Items.Copy("IronIngot", (i) => i.Quantity = 3),
            };
        }
        public ItemObject[] MinerItems()
        {
            DisplayName = "Miner";
            Storage.AddItem("Currency", TrueRandom.NextObject(25, 40, 65, 75));

            return new ItemObject[]
            {
                References.Assets.Items.Copy("StoneAxe", (i) => i.Quantity = 1),
                References.Assets.Items.Copy("StonePickaxe", (i) => i.Quantity = 1),
                References.Assets.Items.Copy("StoneSpade", (i) => i.Quantity = 1),
                References.Assets.Items.Copy("IronPickaxe", (i) => i.Quantity = 1),
                References.Assets.Items.Copy("IronSpade", (i) => i.Quantity = 1),
            };
        }
        public ItemObject[] FisherItems()
        {
            DisplayName = "Fisher";
            Storage.AddItem("Currency", TrueRandom.NextObject(25, 40, 65, 75));

            return new ItemObject[]
            {
                References.Assets.Items.Copy("FishingRod", (i) => i.Quantity = 1),
                References.Assets.Items.Copy("GorgerChow", (i) => i.Quantity = 10),
                References.Assets.Items.Copy("String", (i) => i.Quantity = 3),
                References.Assets.Items.Copy("Feather", (i) => i.Quantity = 1),
                References.Assets.Items.Copy("ChurchwardenPipe", (i) => i.Quantity = 1),
            };
        }

        public Customer(string ID, Vector2 Position, Texture2D Texture, float PurchaseChance, int MinPurchaseDelay, int MaxPurchaseDelay) : base(ID, Position, Texture, "Customer")
        {
            this.PurchaseChance = PurchaseChance;
            this.MinPurchaseDelay = MinPurchaseDelay;
            this.MaxPurchaseDelay = MaxPurchaseDelay;
        }

        public override void Initialize()
        {
            base.Initialize();

            GoToStall();
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);

            if (World.Selected == this)
            {
                sb.DrawString(Font, DisplayName, Physics.AltitudePosition() - new Vector2(0, SourceRect.Height - 16),
                    Color.White, 0, DisplayName.LineCenter(Font), 1f, SpriteEffects.None, Depth + World.PixelDepth());
            }
        }

        public void GoToStall()
        {
            Point[] destinations = { new Point(37, 29), new Point(37, 30), new Point(38, 30), new Point(39, 30), new Point(40, 30), new Point(40, 29), new Point(40, 28) };
            AI.AddGoal("ApproachStall", new FollowPath(destinations[TrueRandom.Next(0, destinations.Length)], 32, 24, 1));
            AI.Goals["ApproachStall"].OnSuccess += PurchaseItem;
        }
        public void PurchaseItem()
        {
            AI.AddGoal("Purchase", new PurchaseItem((MarketObject)World.Objects["MarketStall"], PurchaseChance, MinPurchaseDelay, MaxPurchaseDelay, LookingFor));
            AI.Goals["Purchase"].OnFail += Leave;
        }
        public void Leave()
        {
            Point[] destination = new Point[] { new Point(21, 38), new Point(21, 15), new Point(57, 14), new Point(57, 37) };

            AI.AddGoal("Leave", new FollowPath(destination[TrueRandom.Next(0, destination.Length)], 24, 32, 1));
            AI.Goals["Leave"].OnSuccess += Destroy;
            AI.Goals["Leave"].OnFail += () => Queue.Add(1000, () => Leave());
        }
    }
}
