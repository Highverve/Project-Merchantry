using AetaLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using AetaLibrary.Extensions;
using AetaLibrary.Elements.Images;
using AetaLibrary.Elements.Text;
using Merchantry.Assets;

namespace Merchantry.UI
{
    public class BaseUI : UserInterface
    {
        private Texture2D background, border, mask;
        protected Texture2D pixel, wellBlack, wellBlackHover, button, buttonHover, divider;
        protected Texture2D ps3Icons, xboxIcons;
        protected SpriteFont smallFont, font;

        public StretchBoxElement TitleBar;
        protected TextElement TitlebarText { get; private set; }
        protected string Title
        {
            get { return TitlebarText.Text; }
            set { TitlebarText.Text = value; TitlebarText.SetOrigin(.5f, .5f); }
        }
        private bool isTitlebarEnabled = true;
        public bool IsTitlebarEnabled
        {
            get { return isTitlebarEnabled; }
            set
            {
                isTitlebarEnabled = value;

                if (isTitlebarEnabled == true)
                {
                    CLICKBOX_Add("TitlebarBox", Rectangle.Empty);
                }
                else
                {
                    CLICKBOX_Remove("TitlebarBox");
                }
            }
        }
        public int TitlebarWidth
        {
            get { return TitleBar.Size.X; }
            set
            {
                TitleBar.Size = new Point(value, wellBlack.Height);
                UpdateTitlebarBox();
                TitleBar.SetOrigin(.5f, .5f);
            }
        }
        private void UpdateTitlebarBox()
        {
            if (clickBoxes.ContainsKey("TitlebarBox"))
                CLICKBOX_Set("TitlebarBox", new Rectangle((int)(TitleBar.Position.X - TitleBar.Size.X / 2), (int)(TitleBar.Position.Y - TitleBar.Size.Y / 2), TitleBar.Size.X, TitleBar.Size.Y));
        }
        public override void SetSize(Vector2 size)
        {
            base.SetSize(size);

            if (TitleBar != null && TitlebarText != null)
            {
                TitleBar.Position = new Vector2(Center.X + 1, (Center.Y - SizeCenter.Y));
                TitlebarText.Position = new Vector2(Center.X + 1, (Center.Y - SizeCenter.Y));

                UpdateTitlebarBox();
            }
            if (clickBoxes.ContainsKey("MainBox"))
                CLICKBOX_Set("MainBox", new Rectangle((int)(Center.X - Size.X / 2), (int)(Center.Y - Size.Y / 2), (int)Size.X, (int)Size.Y));

            Camera.Origin = Camera.ToScreen(Center);
        }

        public float MoveRotationMultiplier { get; set; }
        public float MoveVerticalMultiplier { get; set; }
        public float MoveHorizontalMultiplier { get; set; }
        public bool IsMoveEffect { get; set; }

        public References References { get; set; }
        public ScreenManager Screens { get; set; }
        public SymbolAssets Symbols { get; set; }
        public Random Random { get; private set; }

        public BaseUI(GraphicsDevice Graphics, Vector2 Size)
            : base(Graphics, Size, false, Vector2.Zero) { }

        public override void PreInitialize()
        {
            IsAutoDefaultControls = true;
            Sampler = SamplerState.PointClamp;
            Blending = BlendState.AlphaBlend;

            References = GameManager.References();
            Symbols = References.Assets.Symbols;
            Screens = (ScreenManager)Interfaces;
            Random = new Random(Guid.NewGuid().GetHashCode());

            base.PreInitialize();
        }
        public override void Load(ContentManager cm)
        {
            base.Load(cm);

            pixel = cm.Load<Texture2D>("Debug/pixel");
            background = cm.Load<Texture2D>("UI/Elements/backgroundBlack");
            border = cm.Load<Texture2D>("UI/Elements/boxTransparent");
            mask = cm.Load<Texture2D>("UI/Borders/mask");

            wellBlack = cm.Load<Texture2D>("UI/Elements/wellBlack");
            wellBlackHover = cm.Load<Texture2D>("UI/Elements/wellBlackHover");
            button = cm.Load<Texture2D>("UI/Elements/buttonLarge");
            buttonHover = cm.Load<Texture2D>("UI/Elements/buttonLargeHover");
            divider = cm.Load<Texture2D>("UI/Elements/divider");

            ps3Icons = cm.Load<Texture2D>("UI/Icons/gamepadPlaystation");
            xboxIcons = cm.Load<Texture2D>("UI/Icons/gamepadXbox");

            smallFont = cm.Load<SpriteFont>("UI/Fonts/ArchwayKnights_12px");
            font = cm.Load<SpriteFont>("UI/Fonts/ArchwayKnights_19px");
        }
        public override void PostInitialize()
        {

            Camera.RotationSpeed = 15f;
            Camera.MaxMoveSpeed = 5000;
            Camera.MaxScaleSpeed = 1000;
            Camera.MaxRotationSpeed = 1000;

            MoveRotationMultiplier = .02f;
            MoveVerticalMultiplier = .002f;
            MoveHorizontalMultiplier = .002f;
            IsMoveEffect = true;

            CLICKBOX_Add("MainBox", (int)(Center.X - Size.X / 2), (int)(Center.Y - Size.Y / 2), (int)Size.X, (int)Size.Y);

            TitleBar = new StretchBoxElement(0, new Vector2(Center.X + 1, (Center.Y - SizeCenter.Y)), wellBlack, Color.White, new Point(128, wellBlack.Height), 14);
            TitleBar.PreInitialize();
            TitleBar.Load(Content);
            TitleBar.PostInitialize();
            TitleBar.UI = this;
            TitleBar.Camera = Camera;
            TitleBar.Controls = Controls;
            TitleBar.AddDefaultControls();
            TitleBar.SetOrigin(.5f, .5f);

            TitleBar.HoldTime = 16;
            TitleBar.Buttons.OnHover += () =>
            {
                if (Controls.IsMouseDown(Controls.CurrentMS.LeftButton) && IsTitlebarEnabled)
                    isDragging = true;
            };
            TitlebarWidth = 128;
            IsTitlebarEnabled = true;

            //TitleBar.SetHoverBoxCheck(TitleBar.Size);

            ELEMENTS_Add("TitlebarText", TitlebarText = new TextElement(0, new Vector2(Center.X + 1, Center.Y - SizeCenter.Y), Content.Load<SpriteFont>("UI/Fonts/ArchwayKnights_19px"), string.Empty, new Color(242, 224, 218, 255),
                Vector2.Zero, Vector2.One, 0, SpriteEffects.None));

            base.PostInitialize();
        }

        public override void Update(GameTime gt)
        {
            if (IsTitlebarEnabled == true)
            {
                UI_SetOrigin(Center - new Vector2(0, SizeCenter.Y));

                TitleBar.Update(gt);

                if (isDragging)
                {
                    UI_SetPosition((Controls.MouseVector() - Center) + new Vector2(0, SizeCenter.Y));

                    if (Controls.IsMouseUp(Controls.CurrentMS.LeftButton))
                    {
                        Camera.TargetRotation = 0;
                        Scale(1, 10);
                        isDragging = false;
                    }
                }
            }

            base.Update(gt);
        }

        public override void UpdateAlways(GameTime gt)
        {
            if (IsTitlebarEnabled == true)
                TitleBar.UpdateAlways(gt);
            
            if (IsMoveEffect == true && IsMaximized == true)
            {
                if (Camera.PositionChange > 0)
                {
                    Camera.TargetRotation = (Camera.Position.X - Camera.TargetPosition.X) * MoveRotationMultiplier;
                    Scale(1 + -Math.Abs(Camera.TargetPosition.X - Camera.Position.X) * MoveHorizontalMultiplier,
                          1 + (Camera.TargetPosition.Y - Camera.Position.Y) * MoveVerticalMultiplier, 10);
                }
                else
                {
                    Camera.TargetRotation = 0;
                    Scale(1, 10);
                }
            }

            base.UpdateAlways(gt);
        }

        protected override void Draw(SpriteBatch sb)
        {
            DrawStencil(sb, () =>
            {
                sb.Draw(pixel, new Rectangle((int)(Center.X - (SizeCenter.X - 8)), (int)(Center.Y - (SizeCenter.Y - 5)), (int)Size.X - 12, (int)Size.Y - 8), Color.White);
            }, () =>
            {
                sb.DrawTexturedBox(background, new Rectangle((int)(Center.X - (SizeCenter.X - 10)), (int)(Center.Y - (SizeCenter.Y - 5)), (int)Size.X - 20, (int)Size.Y - 9), 24, Color.White);
                DrawInterior(sb);
            });

            sb.Begin(Sorting, Blending, Sampler, null, null, null, Camera.View());

            sb.DrawTexturedBox(border, new Rectangle((int)(Center.X - SizeCenter.X) - 5, (int)(Center.Y - SizeCenter.Y) - 5, (int)Size.X + 16, (int)Size.Y + 16), 32, Color.Lerp(Color.Black, Color.Transparent, .75f));
            sb.DrawTexturedBox(border, new Rectangle((int)(Center.X - SizeCenter.X) - 8, (int)(Center.Y - SizeCenter.Y) - 8, (int)Size.X + 16, (int)Size.Y + 16), 32, Color.White);

            if (IsTitlebarEnabled == true)
            {
                TitleBar.Draw(sb);
                TitlebarText.Draw(sb);
            }
            DrawExterior(sb);

            //DrawClickboxes(sb);

            sb.End();

            base.Draw(sb);
        }
        protected virtual void DrawInterior(SpriteBatch sb) { }
        protected virtual void DrawExterior(SpriteBatch sb) { }

        public override void Minimize()
        {
            base.Minimize();

            if (this != Screens.Tooltip && Screens.Tooltip.IsMaximized == true && Interfaces.UI_IsActive(this))
                Screens.Tooltip.Minimize();
        }

        //Transitions
        protected override void MaximizeTransition()
        {
            Scale(1, 40);
        }
        protected override void MinimizeTransition()
        {
            Scale(new Vector2(.75f, 0), 20);
        }

        protected override void ActivityTransition()
        {
            Scale(1f, 20f);
            Fade(Color.White, 10f);
        }
        protected override void InactivityTransition()
        {
            Scale(.8f, 12f);
            Fade(Color.Lerp(Color.Black, Color.White, .65f), 5f);
        }
    }
}
