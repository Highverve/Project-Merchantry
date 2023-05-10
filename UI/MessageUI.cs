using AetaLibrary.Elements.Images;
using AetaLibrary.Elements.Text;
using Merchantry.UI.Elements;
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
    public class MessageUI : BaseUI
    {
        //Assets
        private Texture2D wellBrown, wellBlack, button, buttonSelect;
        private SpriteFont font, smallFont;

        public Texture2D ICON_Craft { get; private set; }
        public Texture2D ICON_Note { get; private set; }
        public Texture2D ICON_Gear { get; private set; }

        public MessageData Message { get; private set; }

        private StretchBoxElement titleBackground, messageBackground, iconBackground;
        public ImageElement Icon { get; set; }
        public TextElement TitleText { get; private set; }
        public TextElement SubtitleText { get; private set; }
        public TextElement MessageText { get; private set; }
        public bool IsTypingAnimation { get; set; }

        private int messageTimer = 0, charCount = 0;

        public void LAYOUT_SetMessage(MessageData message, bool isTypingAnimation = true)
        {
            ELEMENTS_Clear();

            float maxWidth = 0;
            maxWidth = CompareWidth(message.Title, maxWidth, 120);
            maxWidth = CompareWidth(message.Subtitle, maxWidth, 80);
            maxWidth = CompareWidth(message.Description, maxWidth, 12);

            for (int i = 0; i < message.Options.Count; i++)
            {
                if (message.Options[i] != null && string.IsNullOrEmpty(message.Options[i].Text) == false)
                {
                    maxWidth = CompareWidth(message.Options[i].Text, maxWidth);

                    if (message.Options[i].BoxElement == null) message.Options[i].BoxElement = StretchBox();
                    if (message.Options[i].TextElement == null) message.Options[i].TextElement = TextElement();
                }
                else
                    message.Options.Remove(message.Options[i]);
            }

            maxWidth += 16;
            //Set size/clickbox

            for (int i = 0; i < message.Options.Count; i++)
            {
                ButtonOption o = message.Options[i];
                if (o != null)
                {
                    o.BoxElement.Buttons.OnLeftClick += () => Queue.Add(0, () => o.LeftClick?.Invoke());
                    o.BoxElement.Buttons.OnRightClick += () => Queue.Add(0, () => o.RightClick?.Invoke());
                    o.TextElement.Text = o.Text;

                    //Add to elements
                    ELEMENTS_Add("Box" + i, o.BoxElement);
                    ELEMENTS_Add("Text" + i, o.TextElement);
                }
            }

            //Set elements to message's variables.
            Message = message;

            Icon.Texture = Message.Icon;
            if (Icon.Texture != null)
            {
                Icon.Source = Icon.Texture.Bounds;
                Icon.Origin = Icon.Texture.Bounds.Size.ToVector2() / 2;
            }

            TitleText.Text = Message.Title;
            SubtitleText.Text = Message.Subtitle;
            if (isTypingAnimation == true)
                MessageText.Text = string.Empty;
            else
                MessageText.Text = Message.Description;
            SubtitleText.Fade(Message.SubtitleColor, 50f);

            //Re-add elements to UI.
            ELEMENTS_Add("IconBackground", iconBackground);
            ELEMENTS_Add("TitleBackground", titleBackground);
            ELEMENTS_Add("MessageBackground", messageBackground);

            ELEMENTS_Add("Icon", Icon);
            ELEMENTS_Add("TitleText", TitleText);
            ELEMENTS_Add("SubtitleText", SubtitleText);
            ELEMENTS_Add("MessageText", MessageText);
            charCount = 0;
            messageTimer = 0;

            SetSize(new Vector2(Math.Max(maxWidth + 48, 300), 160 + smallFont.MeasureString(Message.Description).Y + ((button.Height + 8) * message.Options.Count)));

            if (Screens.Context.IsMaximized == true)
            {
                Screens.Context.ParentUI = this;
                Screens.Context.Minimize();
            }
            Controls.SetMouseDelay(50);

            Screens.UI_SetActive(this);
            Maximize();

            IsTypingAnimation = isTypingAnimation;
        }

        public override void SetSize(Vector2 size)
        {
            base.SetSize(size);

            if (titleBackground != null)
            {
                titleBackground.Position = TopLeft + new Vector2(24, 32);
                titleBackground.Size = new Point((int)Size.X - 48, 64);

                if (iconBackground != null)
                {
                    iconBackground.Position = titleBackground.Position - new Vector2(4, 4);
                    iconBackground.Size = new Point(76, 72);
                }
                if (Icon != null)
                    Icon.Position = titleBackground.Position + new Vector2(32);
                if (TitleText != null)
                    TitleText.Position = titleBackground.Position + new Vector2(80, 4);
                if (SubtitleText != null)
                    SubtitleText.Position = titleBackground.Position + new Vector2(80, 34);
            }
            if (messageBackground != null)
            {
                messageBackground.Position = TopLeft + new Vector2(24, 112);
                messageBackground.Size = new Point((int)Size.X - 48, (int)smallFont.MeasureString(Message.Description).Y + 16);

                if (MessageText != null)
                    MessageText.Position = messageBackground.Position + new Vector2(12, 8);
            }

            if (Message != null && Message.Options != null)
            {
                for (int i = 0; i < Message.Options.Count; i++)
                {
                    Message.Options[i].BoxElement.Position = messageBackground.Position + new Vector2(0, messageBackground.Size.Y + 16 + ((button.Height + 8) * i));
                    Message.Options[i].BoxElement.Size = new Point((int)Size.X - 48, button.Height);

                    Message.Options[i].TextElement.Position = Message.Options[i].BoxElement.Position + (Message.Options[i].BoxElement.Size.ToVector2() / 2);
                    Message.Options[i].TextElement.SetOrigin(.5f, .5f);
                }
            }

            CLICKBOX_Set("MainBox", new Rectangle((int)(Center.X - Size.X / 2), (int)(Center.Y - Size.Y / 2), (int)Size.X, (int)Size.Y));
        }
        private float CompareWidth(string text, float currentWidth, float offset = 0)
        {
            float newWidth = 0;
            if ((newWidth = smallFont.MeasureString(text).X + offset) > currentWidth)
                return newWidth;

            return currentWidth;
        }

        public MessageUI(GraphicsDevice Graphics, Vector2 Size) : base(Graphics, Size) { }

        public override void Load(ContentManager cm)
        {
            wellBrown = cm.Load<Texture2D>("UI/Elements/wellBrown");
            wellBlack = cm.Load<Texture2D>("UI/Elements/wellBlack");
            button = cm.Load<Texture2D>("UI/Elements/buttonLarge");
            buttonSelect = cm.Load<Texture2D>("UI/Elements/buttonLargeHover");

            smallFont = cm.Load<SpriteFont>("UI/Fonts/ArchwayKnights_12px");
            smallFont.LineSpacing += 5;
            font = cm.Load<SpriteFont>("UI/Fonts/ArchwayKnights_19px");

            ICON_Craft = cm.Load<Texture2D>("Assets/Items/ironHammer");
            ICON_Note = cm.Load<Texture2D>("UI/Icons/note");
            ICON_Gear = cm.Load<Texture2D>("UI/Icons/gear");

            base.Load(cm);
        }
        public override void PostInitialize()
        {
            base.PostInitialize();

            IsAutoDefaultControls = true;
            IsTitlebarEnabled = true;
            Title = "Event";
            TitlebarWidth = 120;

            ELEMENTS_Add("IconBackground", iconBackground = new StretchBoxElement(2, Vector2.Zero, wellBlack, Color.White, wellBlack.Bounds.Size, 16));
            ELEMENTS_Add("TitleBackground", titleBackground = new StretchBoxElement(1, Vector2.Zero, wellBlack, Color.White, wellBlack.Bounds.Size, 16));
            ELEMENTS_Add("MessageBackground", messageBackground = new StretchBoxElement(1, Vector2.Zero, wellBlack, Color.White, wellBlack.Bounds.Size, 16));

            ELEMENTS_Add("Icon", Icon = new ImageElement(3, Vector2.Zero, null, Rectangle.Empty, Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            ELEMENTS_Add("TitleText", TitleText = new TextElement(2, Vector2.Zero, font, "Nessa of Moira", Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            ELEMENTS_Add("SubtitleText", SubtitleText = new TextElement(2, Vector2.Zero, smallFont, "Item Quest", Color.SkyBlue, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            ELEMENTS_Add("MessageText", MessageText = new TextElement(2, Vector2.Zero, smallFont, "The armored woman quickly approaches your shop.\n\"My iron axe needs repairing.\" she speaks.\n\nWhat say you?", Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));

            Icon.Texture = ICON_Craft;
            Icon.Source = Icon.Texture.Bounds;
            Icon.Origin = Icon.Texture.Bounds.Size.ToVector2() / 2;

            IsAutoDefaultControls = false;
            CurrentControls = ControlTypes.Mouse;
            Minimize();
        }

        private StretchBoxElement StretchBox()
        {
            StretchBoxElement element = new StretchBoxElement(1, Vector2.Zero, button, Color.White, button.Bounds.Size, 14);
            element.AddDefaultControls();

            element.Buttons.OnHoverEnter += () => element.Texture = buttonSelect;
            element.Buttons.OnHoverExit += () => element.Texture = button;

            return element;
        }
        private TextElement TextElement()
        {
            TextElement text = new TextElement(2, Vector2.Zero, smallFont, string.Empty, new Color(32, 32, 32, 255), Vector2.Zero, Vector2.One, 0, SpriteEffects.None);
            return text;
        }

        public override void Update(GameTime gt)
        {
            UI_KeepInBounds(TopLeft - new Vector2(12, 24), BottomRight + new Vector2(12, 12));

            base.Update(gt);
        }
        public override void UpdateAlways(GameTime gt)
        {
            if (Message != null && IsTypingAnimation)
            {
                if (charCount < Message.Description.Length)
                {
                    messageTimer += gt.ElapsedGameTime.Milliseconds;
                    if (messageTimer > 0)
                    {
                        if (Message.Description.Length >= 50)
                            charCount += Math.Min(2, Message.Description.Length - charCount);
                        else
                            charCount++;

                        messageTimer = 0;
                        MessageText.Text = Message.Description.Substring(0, charCount);
                    }
                }
            }

            base.UpdateAlways(gt);
        }

        protected override void Draw(SpriteBatch sb)
        {
            Camera.Origin = Camera.ToScreen(Center);
            base.Draw(sb);
        }
        protected override void DrawInterior(SpriteBatch sb)
        {
            DrawElements(sb);
        }

        public override void Maximize()
        {
            Screens.Tooltip.Minimize();
            base.Maximize();
        }
        public override void Minimize()
        {
            base.Minimize();
        }
    }
    public class MessageData
    {
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Description { get; set; }
        public Texture2D Icon { get; set; }
        public Color SubtitleColor { get; set; } = Color.LightGray;

        public List<ButtonOption> Options { get; private set; } = new List<ButtonOption>();

        public MessageData(string Title, string Subtitle, string Description, Texture2D Icon, params ButtonOption[] Options)
        {
            this.Title = Title;
            this.Subtitle = Subtitle;
            this.Description = Description;
            this.Icon = Icon;

            this.Options = Options.ToList();
        }
    }
}
