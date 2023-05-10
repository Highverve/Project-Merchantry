using AetaLibrary;
using Merchantry.UI.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.UI
{
    public class DragUI : UserInterface
    {
        /// <summary>
        /// Consider letting this UI handles the controls and shortcuts.
        /// Add another itemElement called "Hover", which will be set by other UIs.
        /// Add event to OnLeftClick, checking for CTRL and Shift. See Minecraft for
        /// good shortcuts (Try --- LMB: Stack, LMB + CTRL: One, LMB + SHIFT: Half).
        /// </summary>

        //Assets
        private SpriteFont font;
        private ItemElement itemElement, outroItemElement;

        //Classes
        ScreenManager screens;

        public ItemObject Item() { return itemElement.Item; }
        public ItemObject HoverItem { get; set; }
        public bool IsHolding() { return itemElement.Item != null; }
        public bool IsHovering() { return HoverItem != null; }
        private bool isOutroing { get; set; }

        private void SetItem(ItemObject item)
        {
            itemElement.Item = item;
            outroItemElement.Item = item;

            //Reset visual effects
            itemElement.Intro();
            //outroItemElement.Scale = Vector2.One;

            //Close tooltip
            screens.Tooltip.Minimize();
        }
        public void AddItem(ItemObject item, int quantity)
        {
            if (IsHolding() == true && item != null)
            {
                if (item.StackID() == Item().StackID())
                {
                    if (item.Quantity > 0)
                    {
                        Item().Quantity += quantity;
                        item.Quantity -= quantity;

                        //itemElement.Bounce(.25f, Vector2.One);
                    }
                }
            }
            else
            {
                if (quantity <= item.Quantity)
                    SetItem(item.Split(quantity));
            }
        }
        public void AddItem(ItemObject item) { AddItem(item, item.Quantity); }

        public ItemObject TakeItem()
        {
            if (IsHolding())
            {
                ItemObject temp = itemElement.Item;
                itemElement.Item = null;

                outroItemElement.Outro(() => outroItemElement.Item = null);

                return temp;
            }
            else
                return null;
        }
        public ItemObject TakeItem(int quantity)
        {
            if (IsHolding())
            {
                if (itemElement.Item.Quantity > quantity)
                {
                    ItemObject copy = itemElement.Item.Copy();
                    copy.Quantity = quantity;
                    itemElement.Item.Quantity -= quantity;

                    outroItemElement.Outro(() => outroItemElement.Item = null);
                    return copy;
                }
                else
                {
                    ItemObject result = itemElement.Item;
                    itemElement.Item = null;

                    outroItemElement.Outro(() => outroItemElement.Item = null);
                    return result;
                }
            }
            else
                return null;
        }

        public DragUI(GraphicsDevice Graphics, Vector2 Size) : base(Graphics, Size, false, Vector2.Zero) { }

        public override void Load(ContentManager cm)
        {
            font = cm.Load<SpriteFont>("UI/Fonts/ArchwayKnights_19px");

            base.Load(cm);
        }
        public override void PostInitialize()
        {
            base.PostInitialize();

            IsAutoDefaultControls = true;
            IsPriority = true;
            Sampler = SamplerState.PointClamp;
            Blending = BlendState.AlphaBlend;
            screens = (ScreenManager)Interfaces;

            Camera.MoveSpeed = 15f;
            Camera.MaxMoveSpeed = 5000;
            Camera.MaxScaleSpeed = 1000;
            Camera.MaxRotationSpeed = 1000;

            Maximize();

            ELEMENTS_Add("ItemElement", itemElement = new ItemElement(5, Center, Content.Load<Texture2D>("UI/Elements/itemBackground"), font, Color.White, Vector2.One));
            ELEMENTS_Add("OutroItemElement", outroItemElement = new ItemElement(5, Center, Content.Load<Texture2D>("UI/Elements/itemBackground"), font, Color.White, Vector2.One));

            itemElement.IsDrawBackground = false;
            outroItemElement.IsDrawBackground = false;
        }

        Vector2 lastPosition;
        public override void Update(GameTime gt)
        {
            lastPosition = Camera.Position;
            UI_SetPosition(Controls.MouseVector() - new Vector2((Center.X - SizeCenter.X) + 50, (Center.Y - SizeCenter.Y) + 50));

            float distance = (Camera.Position.X - Camera.TargetPosition.X) * .005f;
            itemElement.Rotation = distance;

            if (IsHolding())
                Camera.TargetScale = new Vector2(Camera.TargetScale.X, 1 + (Camera.TargetPosition.Y - Camera.Position.Y) * .002f);

            base.Update(gt);
        }
        public override void UpdateAlways(GameTime gt)
        {
            base.UpdateAlways(gt);
        }

        protected override void Draw(SpriteBatch sb)
        {
            sb.Begin(Sorting, Blending, Sampler, null, null, null, Camera.View());

            if (itemElement.Item == null)
                outroItemElement.Draw(sb);
            itemElement.Draw(sb);

            sb.End();

            base.Draw(sb);
        }

        protected override void MaximizeTransition()
        {
            Fade(Color.White, 15f);
            Scale(1f, 10f);
        }
        protected override void MinimizeTransition()
        {
            Fade(Color.Transparent, 7.5f);
            Scale(.75f, 5f);
        }

        protected new void InactivityTransition() { }
        protected new void ActivityTransition() { }
    }
}
