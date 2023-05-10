using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Objects.UIs.Elements
{
    public class ButtonElement : Element
    {
        public Texture2D Background { get; set; }
        public Texture2D Icon { get; set; }

        public bool IsShadowed { get; set; } = true;
        public Vector2 ShadowOffset { get; set; }
        public Color ShadowColor { get; set; }
        public Color IconColor { get; set; }

        public ButtonElement(Vector2 Offset, Texture2D Background, Texture2D Icon) : base(Offset)
        {
            this.Background = Background;
            this.Icon = Icon;
        }

        public override void Initialize()
        {
            Size = new Vector2(UI.ToScale(1));
            ShadowOffset = new Vector2(UI.ToScale(4));
            ShadowColor = Color.Lerp(Color.Black, Color.Transparent, .75f);

            SetBoxContains(new Vector2(UI.ToScale(Background.Width), UI.ToScale(Background.Height)));

            base.Initialize();
        }

        public override void Draw(SpriteBatch sb)
        {
            if (Background != null)
            {
                if (IsShadowed)
                    sb.Draw(Background, CombinedPosition() + ShadowOffset, Background.Bounds, ShadowColor, CombinedRotation(), Vector2.Zero,
                            Size, SpriteEffects.None, UI.Depth + DepthOffset);

                sb.Draw(Background, CombinedPosition(), Background.Bounds, Color, CombinedRotation(), Vector2.Zero,
                        Size, SpriteEffects.None, UI.Depth + (DepthOffset + UI.World.PixelDepth()));
            }
            if (Icon != null)
            {
                sb.Draw(Icon, CombinedPosition() + new Vector2(UI.ToScale(Background.Width) / 2, UI.ToScale(Background.Height) / 2),
                        Icon.Bounds, IconColor, CombinedRotation(), Icon.Bounds.Center.ToVector2(),
                        Size, SpriteEffects.None, UI.Depth + (DepthOffset + UI.World.PixelDepth() * 2));
            }

            base.Draw(sb);
        }
    }
}
