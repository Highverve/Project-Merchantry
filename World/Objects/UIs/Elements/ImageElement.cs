using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Objects.UIs.Elements
{
    public class ImageElement : Element
    {
        public Texture2D Texture { get; set; }
        public Rectangle Source { get; set; }

        public bool IsShadowed { get; set; } = true;
        public Vector2 ShadowOffset { get; set; }
        public Color ShadowColor { get; set; }

        public ImageElement(Vector2 Offset, Texture2D Texture) : base(Offset)
        {
            this.Texture = Texture;
            Source = Texture.Bounds;
        }

        public override void Initialize()
        {
            Size = new Vector2(UI.ToScale(1));
            ShadowOffset = new Vector2(UI.ToScale(4));
            ShadowColor = Color.Lerp(Color.Black, Color.Transparent, .75f);

            SetBoxContains(new Vector2(UI.ToScale(Source.Width), UI.ToScale(Source.Height)));

            base.Initialize();
        }

        public override void Draw(SpriteBatch sb)
        {
            if (Texture != null)
            {
                if (IsShadowed)
                    sb.Draw(Texture, CombinedPosition() + ShadowOffset, Source, ShadowColor, 0, Vector2.Zero,
                            Size, SpriteEffects.None, UI.Depth + DepthOffset);

                sb.Draw(Texture, CombinedPosition(), Source, Color, 0, Vector2.Zero,
                        Size, SpriteEffects.None, UI.Depth + (DepthOffset + UI.World.PixelDepth()));
            }

            base.Draw(sb);
        }
    }
}
