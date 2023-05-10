using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Merchantry.World.Objects.UIs.Elements
{
    public class SaleSlot : ItemSlot
    {
        Color worthColor = Color.Lerp(Color.Gold, Color.White, .25f);
        public SaleSlot(Vector2 Offset, Texture2D Background, SpriteFont Font) : base(Offset, Background, Font)
        {
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);

            if (Item != null)
                sb.DrawString(Font, "$" + Item.BaseWorth, CombinedPosition() + new Vector2(UI.ToScale(-6), UI.ToScale(-13)), worthColor,
                    0, Vector2.Zero, new Vector2(UI.ToScale(1)), SpriteEffects.None, UI.Depth + (UI.World.PixelDepth() * 3));
        }
    }
}
