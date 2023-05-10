using AetaLibrary.Elements;
using AetaLibrary.Extensions;
using Merchantry.UI.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.UI
{
    public class ItemElement : Element
    {
        private Texture2D background;
        public ItemObject Item { get; set; }
        public bool IsOutro { get; set; }
        public Texture2D Background
        {
            get { return background; }
            set { background = value; RefreshSnapSize(); }
        }
        public SpriteFont QuantityFont { get; set; }
        public Texture2D NewIcon { get; set; }

        public override void SetOrigin(float x, float y, Vector2 offset)
        {
            if (Background != null)
                Origin = new Vector2(Background.Width * x, Background.Height * y) + offset;
        }
        public override void RefreshSnapSize()
        {
            SnapSize = Background.Bounds.Size.ToVector2();
        }

        public ItemElement(int RenderOrder, Vector2 Offset, Texture2D Background, SpriteFont QuantityFont, Color Color, Vector2 Scale)
            : base(RenderOrder, Offset, Color, Vector2.Zero, Scale, 0, SpriteEffects.None)
        {
            this.Background = Background;
            this.QuantityFont = QuantityFont;

            if (Background != null)
            {
                SetOrigin(.5f, .5f);
                SetHoverBoxCheck(Background.Bounds.Size);
            }
            IsDrawBackground = true;
        }

        public bool IsDrawBackground { get; set; }
        public override void Draw(SpriteBatch sb)
        {
            if (IsDrawBackground == true && Background != null)
                sb.Draw(Background, Position, Background.Bounds, RenderColor.CurrentColor, Rotation, Origin, Scale, Effects, 0);

            if (Item != null)
            {
                Item.Draw(sb, Position, RenderColor.CurrentColor, Rotation, Scale, Effects);

                if (Item.MaxQuantity > 1)
                    Item.DrawQuantity(sb, QuantityFont, Position + new Vector2((30) * Scale.X, (40) * Scale.Y), RenderColor.CurrentColor, Rotation, Scale);
            }

            base.Draw(sb);
        }

        public void Intro()
        {
            Queue.Add(0, () => Scale = Vector2.Zero);
            Queue.Add(16, () =>
            {
                SCALE_SmoothStep(new Vector2(1.25f));
                SCALE_Speed = 7.5f;
            });
            Queue.Add(130, () =>
            {
                SCALE_SmoothStep(new Vector2(1));
                SCALE_Speed = 7.5f;
            });
        }
        public void Outro(Action onFinish)
        {
            Queue.Add(0, () => Scale = Vector2.One);
            Queue.Add(16, () =>
            {
                SCALE_SmoothStep(new Vector2(1.35f));
                SCALE_Speed = 7.5f;
            });
            Queue.Add(100, () =>
            {
                SCALE_SmoothStep(Vector2.Zero);
                SCALE_Speed = 7.5f;
            });
            Queue.Add(180, () => onFinish?.Invoke());
        }

        public void Bounce(float difference)
        {
            Vector2 home = Scale;
            Queue.Add(16, () =>
            {
                SCALE_SmoothStep(new Vector2(Scale.X + difference, Scale.Y + difference));
                SCALE_Speed = 7.5f;
            });
            Queue.Add(130, () =>
            {
                SCALE_SmoothStep(home);
                SCALE_Speed = 7.5f;
            });
        }
        public void Bounce(float difference, Vector2 returnTo)
        {
            Queue.Add(16, () =>
            {
                SCALE_SmoothStep(new Vector2(Scale.X + difference, Scale.Y + difference));
                SCALE_Speed = 7.5f;
            });
            Queue.Add(130, () =>
            {
                SCALE_SmoothStep(returnTo);
                SCALE_Speed = 7.5f;
            });
        }
    }
}
