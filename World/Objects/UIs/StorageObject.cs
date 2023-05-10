using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Merchantry.World.Objects.UIs.Elements;
using Microsoft.Xna.Framework.Graphics;
using Merchantry.UI.Items;
using Merchantry.UI;
using Merchantry.UI.Elements;

namespace Merchantry.World.Objects.UIs
{
    public class StorageObject : UIObject
    {
        public Point GridSize { get; set; }
        public Vector2 GridOffset { get; set; }
        public Vector2 SlotMargins { get; set; }
        public Vector2 EmptyOffset { get; set; }
        public ButtonElement TakeAll { get; set; }

        public Color ItemIdle { get; set; }
        public Color ItemHover { get; set; }
        public Color ButtonIdle { get; set; }
        public Color ButtonHover { get; set; }
        public SpriteFont Font { get; set; }
        public bool IsVisibleOnlyActive { get; set; } = false;

        public StorageObject(string ID, Vector2 Position, Texture2D Texture, Point GridSize, Vector2 GridOffset,
                             Vector2 FocusOffset, Vector2 EmptyOffset, Color ItemIdle, Color ItemHover,
                             Color ButtonIdle, Color ButtonHover, SpriteFont Font) : base(ID, Position, Texture)
        {
            this.GridSize = GridSize;
            this.GridOffset = GridOffset;

            FocusMultiplier = .1f;
            FocusScale = 16;
            this.FocusOffset = FocusOffset;
            this.EmptyOffset = EmptyOffset;

            this.ItemIdle = ItemIdle;
            this.ItemHover = ItemHover;
            this.ButtonIdle = ButtonIdle;
            this.ButtonHover = ButtonHover;

            this.Font = Font;
        }

        public override void Load(ContentManager cm)
        {
            base.Load(cm);

            /*Storage.OnRemoveItem += (item) =>
            {
                foreach (ItemSlot slot in Slots.Values)
                {
                    if (slot.Item == item)
                        slot.Item = null;
                }
            };*/

            AddSlots((x, y) => new ItemSlot(new Vector2(x * ToScale(72), y * ToScale(72)) + GridOffset, pixel, font));

            AddElement("TakeAll", TakeAll = new ButtonElement(new Vector2(-ToScale(72), 0) + GridOffset, cm.Load<Texture2D>("UI/World/button"), cm.Load<Texture2D>("UI/World/takeAllIcon")));
            TakeAll.OnLeftClick += () =>
            {
                foreach (ItemSlot slot in Slots.Values)
                {
                    if (slot.Item != null)
                    {
                        World.Controlled.Storage.AddItem(slot.Item);
                        slot.Item = null;
                    }
                }
            };
            TakeAll.OnHoverEnter += () =>
            {
                TakeAll.Color = ButtonHover;
                References.Screens.Tooltip.LAYOUT_SetText("Take All");
                References.Screens.Tooltip.Maximize();
            };
            TakeAll.OnHoverExit += () =>
            {
                TakeAll.Color = ButtonIdle;
                References.Screens.Tooltip.Minimize();
            };
            TakeAll.CallHoverExit();
            TakeAll.IconColor = Color.White;
        }

        protected void AddSlots(Func<int, int, ItemSlot> copy)
        {
            for (int y = 0; y < GridSize.Y; y++)
            {
                for (int x = 0; x < GridSize.X; x++)
                {
                    ItemSlot slot = copy(x, y);
                    slot.Size = new Vector2(ToScale(64));

                    //Add events
                    SetSlotClick(slot);
                    SetHover(slot);
                    slot.CallHoverExit();

                    AddSlot(x + "," + y, slot);
                }
            }
        }

        public void SetHover(ItemSlot s)
        {
            s.OnHoverEnter += () => s.BackgroundColor = ItemHover;
            s.OnHoverExit += () => s.BackgroundColor = ItemIdle;
        }

        public override void Draw(SpriteBatch sb)
        {
            //if (IsActive == true)
            if (IsVisibleOnlyActive == true)
            {
                if (IsActive == true)
                    DrawElements(sb);
            }
            else
                DrawElements(sb);

            if (Font != null && World.Selected == this)
            {
                string text = SLOTS_FullCount() + "-" + Slots.Count;
                sb.DrawString(Font, text, Physics.AltitudePosition() - new Vector2(0, SourceRect.Height), Color.White,
                                0, Font.MeasureString(text) / 2, 1, SpriteEffects.None, Depth + World.PixelDepth());
            }

            base.Draw(sb);
        }

        public override void RightClick(WorldObject user)
        {
            base.RightClick(user);
        }
    }
}
