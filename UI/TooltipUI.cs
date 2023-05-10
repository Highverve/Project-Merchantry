using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Merchantry.UI.Items;
using AetaLibrary.Elements.Text;
using AetaLibrary.Elements.Images;

namespace Merchantry.UI
{
    public class TooltipUI : BaseUI
    {
        //Assets
        private SpriteFont font, smallFont;

        //Layout methods
        private ItemElement item;
        private TextElement itemName, itemSubtitle, itemWorth, itemDescription, itemEnchants;
        private StretchBoxElement itemDivider;
        public void LAYOUT_SetItem(ItemObject item)
        {
            ELEMENTS_Clear();

            //Data
            this.item.Item = item;
            itemName.Text = this.item.Item.Name;
            itemSubtitle.Text = this.item.Item.Subtitle;
            itemWorth.Text = "$ " + this.item.Item.BaseWorth;
            itemDescription.Text = this.item.Item.Description;
            itemEnchants.Text = string.Empty;
            foreach (ItemEnchant e in item.Enchants.Values)
                itemEnchants.Text += "- " + e.DisplayName + " (" + ((e.WorthModifier > 0) ? "+" : "") + e.WorthModifier + "%)\n";
            //itemEnchants.Text = itemEnchants.Text.Remove(itemEnchants.Text.Length - 2, itemEnchants.Text.Length);

            //Size
            float descHeight = font.MeasureString(item.Description).Y;
            float enchantHeight = font.MeasureString(itemEnchants.Text).Y;
            SetSize(new Vector2(500, 130 + descHeight + enchantHeight + 10));

            //Spatial
            this.item.Position = TopLeft + new Vector2(64, 64);
            itemName.Position = TopLeft + new Vector2(120, 25);
            itemSubtitle.Position = TopLeft + new Vector2(120, 60);
            itemSubtitle.Fade(item.SubtitleColor, 50f);
            itemWorth.Position = new Vector2(BottomRight.X - 32, TopLeft.Y + 32);
            itemWorth.SetOrigin(1, 0);
            itemDivider.Position = TopLeft + new Vector2(20, 110);
            itemDivider.Size = new Point((int)Size.X - 40, 12);
            itemDescription.Position = TopLeft + new Vector2(30, 130);
            itemEnchants.Position = TopLeft + new Vector2(30, 130 + descHeight);

            ELEMENTS_Add("Item", this.item);
            ELEMENTS_Add("ItemName", itemName);
            ELEMENTS_Add("ItemSubtitle", itemSubtitle);
            ELEMENTS_Add("ItemWorth", itemWorth);
            ELEMENTS_Add("ItemDivider", itemDivider);
            ELEMENTS_Add("ItemDescription", itemDescription);
            ELEMENTS_Add("ItemEnchants", itemEnchants);
        }
        public bool IsSelectedItem(ItemObject item)
        {
            if (item != null && this.item.Item != null)
                return item == this.item.Item;
            return false;
        }

        private TextElement simpleText;
        public void LAYOUT_SetText(string text)
        {
            ELEMENTS_Clear();

            simpleText.Text = text;
            SetSize(simpleText.Font.MeasureString(simpleText.Text) + new Vector2(40, 30));
            simpleText.Position = TopLeft + new Vector2(20, 15);

            ELEMENTS_Add("SimpleText", simpleText);
            Maximize();
        }

        public TooltipUI(GraphicsDevice Graphics, Vector2 Size) : base(Graphics, Size) { }

        public override void Load(ContentManager cm)
        {
            font = cm.Load<SpriteFont>("UI/Fonts/ArchwayKnights_19px");
            smallFont = cm.Load<SpriteFont>("UI/Fonts/ArchwayKnights_12px");

            base.Load(cm);
        }
        public override void PostInitialize()
        {
            base.PostInitialize();

            IsPriority = true;
            IsTitlebarEnabled = false;
            Maximize();
            Minimize();
            CLICKBOX_Remove("MainBox");

            InitializeItemLayout();
            simpleText = new TextElement(1, Vector2.Zero, smallFont, string.Empty, Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None);
        }
        private void InitializeItemLayout()
        {
            item = new ItemElement(5, Vector2.Zero, Content.Load<Texture2D>("UI/Elements/wellBlack"), font, Color.White, Vector2.One);
            item.IsInteract = false;
            itemName = new TextElement(5, Vector2.Zero, font, string.Empty, Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None);
            itemSubtitle = new TextElement(5, Vector2.Zero, smallFont, string.Empty, Color.LightGray, Vector2.Zero, Vector2.One, 0, SpriteEffects.None);
            itemWorth = new TextElement(5, Vector2.Zero, smallFont, string.Empty, Color.Lerp(Color.Gold, Color.White, .25f), Vector2.Zero, Vector2.One, 0, SpriteEffects.None);
            itemDivider = new StretchBoxElement(5, Vector2.Zero, Content.Load<Texture2D>("UI/Elements/divider"), Color.White, new Point(100, 12), 12);

            itemDescription = new TextElement(5, Vector2.Zero, smallFont, string.Empty, Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None);
            itemEnchants = new TextElement(5, Vector2.Zero, font, string.Empty, Color.Lerp(Color.White, Color.Yellow, .5f), Vector2.Zero, Vector2.One, 0, SpriteEffects.None);

            item.IsDrawBackground = true;
        }

        public override void Update(GameTime gt)
        {
            IsMoveEffect = IsMaximized;
            UI_SetPosition(Controls.MouseVector() - new Vector2(Center.X - (SizeCenter.X + 48), Center.Y));

            /*
            Vector2 tlScreen = Camera.ToScreen(TopLeft);
            Vector2 brScreen = Camera.ToScreen(BottomRight);

            Camera.Position = new Vector2(MathHelper.Clamp(Camera.Position.X, TopLeft.X + Center.X, BottomRight.X - Center.X),
                                          MathHelper.Clamp(Camera.Position.Y, TopLeft.Y + Center.Y, BottomRight.Y - Center.Y));
            */
            base.Update(gt);
        }

        protected override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
        }
        protected override void DrawInterior(SpriteBatch sb)
        {
            DrawElements(sb);
        }

        public override void Maximize()
        {
            base.Maximize();
        }
        public override void Minimize()
        {
            base.Minimize();
        }
        protected new void InactivityTransition() { }
        protected new void ActivityTransition() { }
    }
}
