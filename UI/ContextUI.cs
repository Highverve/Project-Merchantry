using AetaLibrary;
using AetaLibrary.Elements.Images;
using Merchantry.UI.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using AetaLibrary.Elements.Text;
using Merchantry.UI.Elements;

namespace Merchantry.UI
{
    public class ContextUI : BaseUI
    {
        //Assets
        private Texture2D button, buttonSelect;
        private SpriteFont font;

        public UserInterface ParentUI { get; set; }

        //Elements
        private StretchBoxElement itemButtonOne, itemButtonTwo, itemButtonThree, itemDestroy;
        private TextElement itemTextOne, itemTextTwo, itemTextThree, itemTextDestroy;
        public void LAYOUT_Item(ItemObject item)
        {
            LAYOUT_Custom(new ButtonOption(item.ButtonGrabText(), null, () => item.ButtonGrab(GameManager.References().World.Controlled), null),
                new ButtonOption(item.ButtonOneText, null, () => item.ButtonOne(GameManager.References().World.Controlled), null),
                new ButtonOption(item.ButtonTwoText, null, () => item.ButtonTwo(GameManager.References().World.Controlled), null),
                new ButtonOption(item.ButtonThreeText, null, () => item.ButtonThree(GameManager.References().World.Controlled), null));
        }

        /// <summary>
        /// EXPERIMENTAL!
        /// </summary>
        /// <param name="options"></param>
        public void LAYOUT_Custom(params ButtonOption[] options)
        {
            ELEMENTS_Clear();

            //Assign
            int validCount = 0;
            for (int i = 0; i < options.Length; i++)
            {
                if (options[i] != null && string.IsNullOrEmpty(options[i].Text) == false)
                {
                    options[i].BoxElement = StretchBox();
                    options[i].TextElement = TextElement();

                    validCount++;
                }
                else
                    options[i] = null;
            }

            //Set size/clickbox
            SetSize(new Vector2(200, 35 + (button.Height + 4) * validCount));
            //CLICKBOX_Set("MainBox", new Rectangle((int)(Center.X - Size.X / 2), (int)(Center.Y - Size.Y / 2), (int)Size.X, (int)Size.Y));

            //Set the elements' variables for the options
            for (int i = 0; i < options.Length; i++)
            {
                ButtonOption o = options[i];
                if (o != null)
                {
                    options[i].BoxElement.Position = TopLeft + new Vector2(20, 20 + (button.Height + 4) * i);
                    options[i].BoxElement.Buttons.OnLeftClick += () => o.LeftClick?.Invoke();
                    options[i].BoxElement.Buttons.OnRightClick += () => o.RightClick?.Invoke();
                    options[i].BoxElement.Size = new Point((int)Size.X - 40, button.Height);

                    options[i].TextElement.Text = options[i].Text;
                    options[i].TextElement.Position = options[i].BoxElement.Position + (options[i].BoxElement.Size.ToVector2() / 2);
                    options[i].TextElement.SetOrigin(.5f, .5f);

                    //Add to elements
                    ELEMENTS_Add("Box" + i, options[i].BoxElement);
                    ELEMENTS_Add("Text" + i, options[i].TextElement);
                }
            }

            //Move to mouse position
            UI_SetPosition(Controls.MouseVector() - Center + SizeCenter + new Vector2(16), true);
        }

        private int minimizeTimer = 0;

        public ContextUI(GraphicsDevice Graphics, Vector2 Size) : base(Graphics, Size)
        {
        }

        public override void Load(ContentManager cm)
        {
            button = cm.Load<Texture2D>("UI/Elements/buttonLarge");
            buttonSelect = cm.Load<Texture2D>("UI/Elements/buttonLargeHover");

            font = cm.Load<SpriteFont>("UI/Fonts/ArchwayKnights_19px");

            base.Load(cm);
        }
        public override void PostInitialize()
        {
            base.PostInitialize();

            IsAutoDefaultControls = false;
            IsPriority = true;

            IsTitlebarEnabled = false;
            Maximize();
            Minimize();

            InitializeItemLayout();
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
            TextElement text = new TextElement(2, Vector2.Zero, font, string.Empty, new Color(32, 32, 32, 255), Vector2.Zero, Vector2.One, 0, SpriteEffects.None);
            return text;
        }
        private void InitializeItemLayout()
        {
            //Assigning
            itemButtonOne = new StretchBoxElement(1, Vector2.Zero, button, Color.White, button.Bounds.Size, 14);
            itemButtonTwo = new StretchBoxElement(1, Vector2.Zero, button, Color.White, button.Bounds.Size, 14);
            itemButtonThree = new StretchBoxElement(1, Vector2.Zero, button, Color.White, button.Bounds.Size, 14);
            itemDestroy = new StretchBoxElement(1, Vector2.Zero, button, Color.White, button.Bounds.Size, 14);

            itemTextOne = new TextElement(2, Vector2.Zero, font, string.Empty, new Color(32, 32, 32, 255), Vector2.Zero, Vector2.One, 0, SpriteEffects.None);
            itemTextTwo = new TextElement(2, Vector2.Zero, font, string.Empty, new Color(32, 32, 32, 255), Vector2.Zero, Vector2.One, 0, SpriteEffects.None);
            itemTextThree = new TextElement(2, Vector2.Zero, font, string.Empty, new Color(32, 32, 32, 255), Vector2.Zero, Vector2.One, 0, SpriteEffects.None);
            itemTextDestroy = new TextElement(2, Vector2.Zero, font, "Dispose", new Color(32, 32, 32, 255), Vector2.Zero, Vector2.One, 0, SpriteEffects.None);
            itemTextDestroy.SetOrigin(.5f, .5f);

            //Add controls
            itemButtonOne.AddDefaultControls();
            itemButtonTwo.AddDefaultControls();
            itemButtonThree.AddDefaultControls();
            itemDestroy.AddDefaultControls();

            //Events
            itemButtonOne.Buttons.OnHoverEnter += () => itemButtonOne.Texture = buttonSelect;
            itemButtonTwo.Buttons.OnHoverEnter += () => itemButtonTwo.Texture = buttonSelect;
            itemButtonThree.Buttons.OnHoverEnter += () => itemButtonThree.Texture = buttonSelect;
            itemDestroy.Buttons.OnHoverEnter += () => itemDestroy.Texture = buttonSelect;

            itemButtonOne.Buttons.OnHoverExit += () => itemButtonOne.Texture = button;
            itemButtonTwo.Buttons.OnHoverExit += () => itemButtonTwo.Texture = button;
            itemButtonThree.Buttons.OnHoverExit += () => itemButtonThree.Texture = button;
            itemDestroy.Buttons.OnHoverExit += () => itemDestroy.Texture = button;
        }

        public override void Update(GameTime gt)
        {
            if (IsMaximized == true)
            {
                if (CLICKBOX_Contains(Controls.MouseVector()) == false)
                {
                    if (Camera.PositionChange == 0)
                    {
                        minimizeTimer += gt.ElapsedGameTime.Milliseconds;
                        if (minimizeTimer >= 500)
                            Minimize();
                    }
                }
            }

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
            if (IsMaximized == false)
            {
                minimizeTimer = 0;
                Screens.Tooltip.Minimize();
            }

            base.Maximize();
        }
        public override void Minimize()
        {
            if (IsMaximized == true && ParentUI != null)
                Interfaces.UI_SetActive(ParentUI);

            base.Minimize();
        }

        protected override void MaximizeTransition()
        {
            Fade(Color.White, 20f);
            Scale(1f, 20f);
        }
        protected override void MinimizeTransition()
        {
            Fade(Color.Transparent, 20f);
            Scale(.75f, .25f, 20f);
        }
    }
}
