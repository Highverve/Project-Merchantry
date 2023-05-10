using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Merchantry.World.Objects.UIs.Elements;
using Merchantry.UI.Items;

namespace Merchantry.World.Objects.UIs
{
    public class MarketObject : StorageObject
    {
        public WorldObject Merchant { get; set; }
        public ButtonElement SellButton { get; set; }
        public bool IsSelling { get; set; } = false;

        public Texture2D SellingIcon { get; set; }
        public Texture2D NotSellingIcon { get; set; }

        public MarketObject(string ID, Vector2 Position, Texture2D Texture, Point GridSize, Vector2 GridOffset, Vector2 FocusOffset,
                            Vector2 EmptyOffset, Color ItemIdle, Color ItemHover, Color ButtonIdle, Color ButtonHover, SpriteFont Font)
                            : base(ID, Position, Texture, GridSize, GridOffset, FocusOffset, EmptyOffset, ItemIdle,
                                   ItemHover, ButtonIdle, ButtonHover, Font)
        {
        }

        public override void Load(ContentManager cm)
        {
            base.Load(cm);

            DisplayName = "Market Stall";

            foreach (ItemSlot s in Slots.Values)
                Elements.Remove(s.Name);
            Slots.Clear();
            
            AddSlots((x, y) => new SaleSlot(new Vector2(x * ToScale(72), y * ToScale(72)) + GridOffset, pixel, font));

            SellingIcon = cm.Load<Texture2D>("UI/World/sellingIcon");
            NotSellingIcon = cm.Load<Texture2D>("UI/World/sellingNotIcon");

            AddElement("MakeMarket", SellButton = new ButtonElement(new Vector2(-ToScale(72), ToScale(72)) + GridOffset, cm.Load<Texture2D>("UI/World/button"), NotSellingIcon));
            SellButton.OnLeftClick += () =>
            {
                IsSelling = !IsSelling;

                if (IsSelling == true)
                    SellButton.Icon = SellingIcon;
                else
                    SellButton.Icon = NotSellingIcon;

                SellButton.CallHoverEnter();
            };
            SellButton.OnHoverEnter += () =>
            {
                SellButton.Color = ButtonHover;
                References.Screens.Tooltip.LAYOUT_SetText(IsSelling ? "Currently Selling" : "Currently Not Selling");
                References.Screens.Tooltip.Maximize();
            };
            SellButton.OnHoverExit += () =>
            {
                SellButton.Color = ButtonIdle;
                References.Screens.Tooltip.Minimize();
            };
            SellButton.CallHoverExit();
            SellButton.IconColor = Color.White;
        }

        public override void Update(GameTime gt)
        {
            bool isControlled = Merchant == World.Controlled;
            TakeAll.IsVisible = isControlled;
            TakeAll.IsEnabled = isControlled;
            SellButton.IsVisible = isControlled;
            SellButton.IsEnabled = isControlled;

            base.Update(gt);
        }

        public void SetSeller()
        {
            foreach (ItemSlot slot in Slots.Values)
            {
                slot.ClearLeftClick();
                SetPurchaseClick(slot);
            }
        }
        public void SetPurchaseClick(ItemSlot slot)
        {
            slot.OnLeftClick += () =>
            {
                if (slot.IsLocked == false && slot.Item != null)
                {
                    if (Controls.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
                        PurchaseItem(World.Controlled, slot, slot.Item.Quantity);
                    else
                        PurchaseItem(World.Controlled, slot, 1);
                }
            };
        }
        public void PurchaseItem(WorldObject obj, ItemSlot slot, int quantity)
        {
            quantity = MathHelper.Clamp(quantity, 1, slot.Item.Quantity);
            int quantityWorth = slot.Item.QuantityWorth(quantity);

            if (obj.Storage.ContainsItem("Currency") && obj.Storage.Items["Currency"].Quantity >= quantityWorth)
            {
                obj.Storage.RemoveItem("Currency", quantityWorth);
                obj.Storage.AddItem(slot.Item.Split(quantity));
                Merchant.Storage.AddItem("Currency", quantityWorth);
                onPurchase?.Invoke(obj, slot.Item, quantity);
            }
        }

        private event Action<WorldObject, ItemObject, int> onPurchase;
        public event Action<WorldObject, ItemObject, int> OnPurchase { add { onPurchase += value; } remove { onPurchase -= value; } }
        public void CallOnPurchase(WorldObject obj, ItemObject item, int quantity) { onPurchase?.Invoke(obj, item, quantity); }
    }
}
