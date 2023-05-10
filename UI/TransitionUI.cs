using AetaLibrary;
using AetaLibrary.Elements.Images;
using AetaLibrary.Elements.Text;
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
    public class TransitionUI : UserInterface
    {
        private Texture2D pixel;
        private SpriteFont font, smallFont;

        public ImageElement TintOne { get; set; }
        public ImageElement TintTwo { get; set; }

        //End of Day Layout
        public TextElement CurrentDay { get; set; }

        public TransitionUI(GraphicsDevice Graphics, Vector2 Size)
            : base(Graphics, Size, false, Vector2.Zero)
        {
        }

        public override void PreInitialize()
        {
            base.PreInitialize();
        }
        public override void Load(ContentManager cm)
        {
            pixel = cm.Load<Texture2D>("Debug/pixel");
            font = cm.Load<SpriteFont>("UI/Fonts/ArchwayKnights_19px");
            smallFont = cm.Load<SpriteFont>("UI/Fonts/ArchwayKnights_12px");

            base.Load(cm);

            /*
            References r = GameManager.References();

            r.Calendar.OnHourChange += (h) =>
            {
                if (h >= 0 && h < 18)
                    Fade(Color.Lerp(Color.Black, Color.Transparent, 1), 1f);
            };
            r.Calendar.OnTime(r.Calendar.Year, r.Calendar.Quarter, r.Calendar.Day, 18, () => Fade(Color.Lerp(Color.Black, Color.Transparent, .75f), .1f));
            r.Calendar.OnTime(r.Calendar.Year, r.Calendar.Quarter, r.Calendar.Day, 20, () => Fade(Color.Lerp(Color.Black, Color.Transparent, .5f), .1f));
            r.Calendar.OnMinuteChange += (m) =>
            {
                if (r.Calendar.Hour >= 23 && m > 55)
                    Fade(Color.Lerp(Color.Black, Color.Transparent, 1), 1f);
            };
            r.Calendar.OnTime(r.Calendar.Year, r.Calendar.Quarter, r.Calendar.Day, 23, 55, () => Fade(Color.Lerp(Color.Black, Color.Transparent, 0), 2.5f));

            GameManager.References().Calendar.OnDayChange += (d) =>
            {
                //CurrentDay.SetScaleTarget(Vector2.One, new Vector2(2.5f));
                CurrentDay.Scale = Vector2.One;
                CurrentDay.Text = "Day " + GameManager.References().Calendar.Day;
                CurrentDay.AlignCenter();

                //Queue.Add(2500, () => CurrentDay.SetScaleTarget(Vector2.Zero, new Vector2(5)));
            };*/
        }
        public override void PostInitialize()
        {
            base.PostInitialize();

            IsPriority = true;
            IsAutoDefaultControls = true;
            Sampler = SamplerState.PointClamp;
            Blending = BlendState.AlphaBlend;

            ELEMENTS_Add("TintOne", TintOne = new ImageElement(0, Vector2.Zero, pixel, new Rectangle(0, 0, (int)Size.X, (int)Size.Y / 2), new Color(0, 0, 0, 255), Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            ELEMENTS_Add("TintTwo", TintTwo = new ImageElement(0, new Vector2(0, Size.Y / 2), pixel, new Rectangle(0, 0, (int)Size.X, (int)Size.Y / 2), new Color(0, 0, 0, 255), Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            //ELEMENTS_Add("Day", CurrentDay = new TextElement(1, Center, font, "Day 1", Color.White, font.MeasureString("Day 1") / 2, Vector2.One, 0, SpriteEffects.None));
            //CurrentDay.Scale = Vector2.Zero;

            Queue.Add(750, () =>
            {
                //Fade(Color.Transparent, 1f);
                BoxVertical(2, 1);
                GameManager.References().World.Camera.Scale = new Vector2(1.5f);
                GameManager.References().World.Camera.TargetScale = new Vector2(2);
            });

            Maximize();
        }

        public override void Update(GameTime gt)
        {
            base.Update(gt);
        }
        public override void UpdateAlways(GameTime gt)
        {
            base.UpdateAlways(gt);
        }

        protected override void Draw(SpriteBatch sb)
        {
            sb.Begin(Sorting, Blending, Sampler, null, null, null, Camera.View());
            DrawElements(sb);
            sb.End();

            base.Draw(sb);
        }

        //Transitions
        public void BoxVertical(float speed, float positionMultiplier)
        {
            TintOne.POSITION_SmoothStep(new Vector2(0, (-Size.Y / 2) * positionMultiplier));
            TintTwo.POSITION_SmoothStep(new Vector2(0, (Size.Y / 2) + ((Size.Y / 2) * positionMultiplier)));

            TintOne.POSITION_Speed = speed;
            TintTwo.POSITION_Speed = speed;
        }
    }
}
