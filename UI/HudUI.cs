using AetaLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using AetaLibrary.Elements.Images;
using AetaLibrary.Elements.Text;
using Merchantry.World.Meta;
using AetaLibrary.Extensions;

namespace Merchantry.UI
{
    public class HudUI : UserInterface
    {
        private Texture2D sunIcon, button, well, noteIcon;
        private SpriteFont smallFont, font;

        private ImageElement sunTimer, sunFade;
        StretchBoxElement titleBackground = null, iconBackground = null;
        ImageElement icon = null;
        TextElement title = null;

        private int menuTime = 0;
        public bool IsMenuInteract { get; set; } = true;

        private ObjectStorage storage;
        public void SetStorage(ObjectStorage storage)
        {
            this.storage = storage;
        }

        private List<Tuple<Texture2D, string>> queue = new List<Tuple<Texture2D, string>>();
        private int queueTimer = 0;
        public void SendNotification(Texture2D icon, string title)
        {
            queue.Add(new Tuple<Texture2D, string>(icon, title));
        }
        private void UpdateNotifications(GameTime gt)
        {
            queueTimer -= gt.ElapsedGameTime.Milliseconds;
            if (queueTimer < 0)
            {
                if (queue.Count > 0)
                {
                    icon.Texture = queue.First().Item1;
                    icon.SetOrigin(.5f, .5f);
                    title.Text = queue.First().Item2;

                    titleBackground.Size = new Point((int)font.MeasureString(title.Text).X + 112, well.Height);
                    titleBackground.POSITION_Speed = 10;
                    titleBackground.POSITION_Loose(new Vector2(20, 20));
                    titleBackground.InvokePositionMove();

                    queue.Remove(queue.First());
                    queueTimer = 2000 + (title.Text.Length * 50);
                }
                else
                {
                    titleBackground.POSITION_Speed = 5;
                    titleBackground.POSITION_Loose(new Vector2(-(titleBackground.Size.X) - 20, 20));
                }
            }
        }

        public HudUI(GraphicsDevice Graphics, Vector2 Size)
            : base(Graphics, Size, false, Vector2.Zero)
        {
        }

        public override void PreInitialize()
        {
            base.PreInitialize();
        }
        public override void Load(ContentManager cm)
        {
            smallFont = cm.Load<SpriteFont>("UI/Fonts/ArchwayKnights_12px");
            font = cm.Load<SpriteFont>("UI/Fonts/ArchwayKnights_19px");

            sunIcon = cm.Load<Texture2D>("UI/Icons/sunClock");
            noteIcon = cm.Load<Texture2D>("UI/Icons/note");

            well = cm.Load<Texture2D>("UI/Elements/wellBlack");
            button = cm.Load<Texture2D>("UI/Elements/button");

            base.Load(cm);
        }
        public override void PostInitialize()
        {
            base.PostInitialize();

            IsPriority = true;
            IsAutoDefaultControls = true;
            Sampler = SamplerState.PointClamp;
            Blending = BlendState.AlphaBlend;

            Maximize();

            /*
            ELEMENTS_Add("SunFade", sunFade = new ImageElement(0, new Vector2(80, 80), sunIcon, sunIcon.Bounds, Color.Gray, sunIcon.Bounds.Center.ToVector2(), Vector2.One, 0, SpriteEffects.None));
            ELEMENTS_Add("SunTimer", sunTimer = new ImageElement(1, new Vector2(80, 80), sunIcon, sunIcon.Bounds, Color.White, sunIcon.Bounds.Center.ToVector2(), Vector2.One, 0, SpriteEffects.None));
            ELEMENTS_Add("DayCount", new TextElement(2, new Vector2(80, 140), smallFont, "Day 1", new Color(255, 255, 255, 255), smallFont.MeasureString("Day 1") / 2, Vector2.One, 0, SpriteEffects.None));

            GameManager.References().Calendar.OnMinuteChange += (m) =>
            {
                float pct = GameManager.References().Calendar.DayPercentage();
                //sunTimer.Offset = new Vector2(sunTimer.Offset.X, 80);
                sunTimer.SourceRect = new Rectangle(0, 0, sunIcon.Width, sunIcon.Height - (int)(sunIcon.Height * pct));
            };
            GameManager.References().Calendar.OnHourChange += (h) =>
            {
                if (h == 22)
                {
                    sunTimer.SetScaleTarget(new Vector2(1.35f), new Vector2(7.5f));
                    Queue.Add(150, () => sunTimer.SetScaleTarget(new Vector2(1), new Vector2(5f)));
                    Queue.Add(300, () => sunTimer.SetScaleTarget(new Vector2(1.5f), new Vector2(7.5f)));
                    Queue.Add(450, () => sunTimer.SetScaleTarget(new Vector2(1), new Vector2(2.5f)));
                }
            };
            GameManager.References().Calendar.OnDayChange += (d) =>
            {
                ELEMENTS_Select<TextElement>("DayCount").Text = "Day " + d;
                ELEMENTS_Select<TextElement>("DayCount").AlignCenter();
            };*/

            //Notifications
            ELEMENTS_Add("TitleBackground", titleBackground = new StretchBoxElement(1, new Vector2(-200, Size.Y - (well.Height + 20)), well, Color.White, new Point(250, well.Height), 32));
            ELEMENTS_Add("IconBackground", iconBackground = new StretchBoxElement(2, Vector2.Zero, button, Color.White, new Point(76, 72), 14));
            ELEMENTS_Add("Icon", icon = new ImageElement(3, Vector2.Zero, noteIcon, noteIcon.Bounds, Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            ELEMENTS_Add("TitleText", title = new TextElement(2, Vector2.Zero, font, "Null", Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            title.SetOrigin(.5f, .5f);

            titleBackground.OnPositionMove += () =>
            {
                iconBackground.Position = titleBackground.Position - new Vector2(4, 4);
                icon.Position = iconBackground.Position + (iconBackground.Size.ToVector2() / 2);// new Vector2(4, 0);
                title.Position = titleBackground.Position + new Vector2(112, (well.Height / 2));
            };

            References r = GameManager.References();
            ScreenManager sm = (ScreenManager)Interfaces;

            TextElement infoText, controlsText, exitText;
            Color fadeIn = Color.Lerp(Color.IndianRed, Color.DarkRed, .25f);
            Color fadeOut = Color.Lerp(Color.White, Color.Black, 0f);
            ELEMENTS_Add("InfoText", infoText = new TextElement(1, new Vector2(Size.X - 20, 20), font, "INFO", fadeOut, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            infoText.SetOrigin(1, 0);
            infoText.POSITION_Speed = 10;
            infoText.Buttons.OnHoverEnter += () => infoText.Fade(fadeIn, 10f);
            infoText.Buttons.OnHoverExit += () => infoText.Fade(fadeOut, 5f);

            MessageData infoStart, infoNext = null, infoToDo = null;
            infoStart = new MessageData("Info", "", "Thank you for playing. You can contact\nme through my website or social media.\nI appreciate suggestions, criticism, or bug\nreports.\n\nWEBSITE: Emberium.com\nPATREON: Emberium.com/patreon\nCOMMUNITY: Emberium.com/community\nSOCIAL: @Soestae\n\nVERSION: Alpha " + r.Debugging.Version, Content.Load<Texture2D>("UI/Icons/note"),
                new Elements.ButtonOption("What's Next?", "", () => { sm.Message.LAYOUT_SetMessage(infoNext, false); }, null),
                new Elements.ButtonOption("Back", "", () => { GameManager.References().Settings.IsPaused = false; sm.Message.Minimize(); }, null));
            infoStart.SubtitleColor = Color.LightBlue;

            //

            infoNext = new MessageData("What's Next?", "", "Depending on the early alpha's reception,\nI will either:\n\n- Keep updating the game. See to-do list.\n- Move on to something else.\n\nLet me know what you think.", Content.Load<Texture2D>("UI/Icons/note"),
                new Elements.ButtonOption("To-Do List", "", () => { sm.Message.LAYOUT_SetMessage(infoToDo, false); }, null),
                new Elements.ButtonOption("Back", "", () => { sm.Message.LAYOUT_SetMessage(infoStart, false); }, null));
            infoNext.SubtitleColor = Color.LightBlue;

            infoToDo = new MessageData("To-Do List", "", "ATMOSPHERE. Weather, wind, temperature,\n  dynamic shadows, day and night cycle,\n  sounds, and music.\n\nGAMEPLAY. Energy, improved trading, more\n  dialogue, examine items.\n\nUIs. Tile editor, object editor, main menu,\n  overhauled inventory.\n\nWORLDBUILDING. More characters, events,\n  locations, items, and an economy.\n\nPROFESSIONS. Apothecary, blacksmith, sage,\n  baker, clothier, wordsmith. Most will\n  have a specific crafting UI.\n\nCUSTOMIZE. Player appearance, starting\n  profession, etc.", Content.Load<Texture2D>("UI/Icons/note"),
                new Elements.ButtonOption("Back", "", () => { sm.Message.LAYOUT_SetMessage(infoNext, false); }, null));
            infoToDo.SubtitleColor = Color.LightGreen;

            infoText.Buttons.OnLeftClick += () => { GameManager.References().Settings.IsPaused = true; sm.Message.LAYOUT_SetMessage(infoStart, false); };
            //infoText.SetHoverBoxCheck(

            ELEMENTS_Add("ControlsText", controlsText = new TextElement(1, new Vector2(Size.X - 20, 50), font, "CONTROLS", fadeOut, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            controlsText.SetOrigin(1, 0);
            controlsText.POSITION_Speed = 10;
            controlsText.Buttons.OnHoverEnter += () => controlsText.Fade(fadeIn, 10f);
            controlsText.Buttons.OnHoverExit += () => controlsText.Fade(fadeOut, 5f);

            MessageData controlsMessage = new MessageData("Controls", "", "Press `Tab` to open your bag. Click an item while holding\nleft control to split it. Mouse buttons to use an object.\n\nObjects may behave differently when left or right-clicked.\nClicking the crafting table will zoom in or out for use,\nwhereas clicking a person might start dialogue.\n\nTry clicking objects while dragging an item.\n\nHover over top-right corner or press `Escape` to show menu.",
                Content.Load<Texture2D>("UI/Icons/note"), new Elements.ButtonOption("OK", "", () => { GameManager.References().Settings.IsPaused = false; sm.Message.Minimize(); }, null));
            controlsText.Buttons.OnLeftClick += () => { GameManager.References().Settings.IsPaused = true; sm.Message.LAYOUT_SetMessage(controlsMessage, false); };

            ELEMENTS_Add("ExitText", exitText = new TextElement(1, new Vector2(Size.X - 20, 80), font, "EXIT", fadeOut, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            exitText.SetOrigin(1, 0);
            exitText.POSITION_Speed = 10;
            exitText.Buttons.OnHoverEnter += () => exitText.Fade(fadeIn, 10f);
            exitText.Buttons.OnHoverExit += () => exitText.Fade(fadeOut, 5f);

            MessageData exitConfirm = new MessageData("Exit", "Are you sure?", "NOTE: Saving has not been implemented.\nExiting will mean you will have to\nstart over.", Content.Load<Texture2D>("UI/Icons/note"),
                new Elements.ButtonOption("Yes, exit.", "", () => { r.Game.Exit(); }, null),
                new Elements.ButtonOption("No, I'll stay.", "", () => { GameManager.References().Settings.IsPaused = false; sm.Message.Minimize(); }, null));
            exitConfirm.SubtitleColor = new Color(240, 100, 100, 255);
            exitText.Buttons.OnLeftClick += () => { GameManager.References().Settings.IsPaused = true; sm.Message.LAYOUT_SetMessage(exitConfirm, false); };

            //GameManager.References().Calendar.OnTime(776, 1, 1, 11, 10, () => controlsText.Buttons.InvokeLeftClick());
        }

        public override void Update(GameTime gt)
        {
            UpdateNotifications(gt);
            //sunFade.Scale = sunTimer.Scale;

            if (Controls.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Space))
                SendNotification(noteIcon, "Temporary Test");

            if (IsMenuInteract == true)
            {
                menuTime += gt.ElapsedGameTime.Milliseconds;
                if (Controls.MouseVector().X >= Size.X - 150 && Controls.MouseVector().Y <= 150)
                    menuTime = -100;
                if (GameManager.References().Settings.IsPaused == true)
                    menuTime = 0;

                if (Controls.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Escape))
                    menuTime = -5000;
            }
            else
                menuTime = 1000;

            if (menuTime < 0)
            {
                ELEMENTS_Select<TextElement>("InfoText").POSITION_SmoothStep(new Vector2(Size.X - 20, 20));
                ELEMENTS_Select<TextElement>("ControlsText").POSITION_SmoothStep(new Vector2(Size.X - 20, 50));
                ELEMENTS_Select<TextElement>("ExitText").POSITION_SmoothStep(new Vector2(Size.X - 20, 80));
            }
            else
            {
                ELEMENTS_Select<TextElement>("InfoText").POSITION_SmoothStep(new Vector2(Size.X + 125, 20));
                ELEMENTS_Select<TextElement>("ControlsText").POSITION_SmoothStep(new Vector2(Size.X + 125, 50));
                ELEMENTS_Select<TextElement>("ExitText").POSITION_SmoothStep(new Vector2(Size.X + 125, 80));
            }

            base.Update(gt);
        }
        public override void UpdateAlways(GameTime gt)
        {
            base.UpdateAlways(gt);
        }

        protected override void Draw(SpriteBatch sb)
        {
            sb.Begin(Sorting, Blending, Sampler, null, null, null, Camera.View());

            //sb.DrawTexturedBox(border, new Rectangle((int)(titleBackground.Position.X - 10), (int)(titleBackground.Position.Y - 8), titleBackground.Size.X + 18, titleBackground.Size.Y + 16), 32, Color.White);

            DrawElements(sb);

            sb.End();

            base.Draw(sb);
        }
    }
}
