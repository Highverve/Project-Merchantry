using Merchantry.Assets.Meta;
using Merchantry.UI.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Objects.UIs.Elements
{
    public class ItemSlot : Element
    {
        public bool IsLocked { get; set; } = false;
        public bool IsOpenDefault { get; set; } = true;

        public ItemObject Item { get; set; }
        public Texture2D Background { get; set; }
        public SpriteFont Font { get; set; }
        public Color BackgroundColor { get; set; } = Color.White;
        public Color ShadowColor { get; set; } = Color.Lerp(Color.Black, Color.Transparent, .75f);

        public ItemSlot(Vector2 Offset, Texture2D Background, SpriteFont Font) : base(Offset)
        {
            this.Background = Background;
            this.Font = Font;
        }

        public override void Initialize()
        {
            base.Initialize();

            SetBoxContains(new Vector2(UI.ToScale(64), UI.ToScale(64)));
        }
        public override void Update(GameTime gt)
        {
            if (Item != null && Item.IsDestroyed == true)
                Item = null;

            base.Update(gt);
        }
        public override void Draw(SpriteBatch sb)
        {
            DrawBackground(sb);
            DrawItem(sb);

            base.Draw(sb);
        }
        public void DrawBackground(SpriteBatch sb)
        {
            sb.Draw(Background, CombinedPosition() + new Vector2(UI.ToScale(4), UI.ToScale(4)), Background.Bounds, ShadowColor,
                0, Vector2.Zero, Size, SpriteEffects.None, UI.Depth + (UI.World.PixelDepth() / 2));
            sb.Draw(Background, CombinedPosition(), Background.Bounds, BackgroundColor,
                    0, Vector2.Zero, Size, SpriteEffects.None, UI.Depth + UI.World.PixelDepth());
        }
        public void DrawItem(SpriteBatch sb)
        {
            if (Item != null)
            {
                Item.Draw(sb, CombinedPosition() + (Size * .5f), Color.White, 0,
                                      new Vector2(UI.ToScale(1)), SpriteEffects.None, UI.Depth + (UI.World.PixelDepth() * 2));
                Item.DrawQuantity(sb, Font, CombinedPosition() + new Vector2(UI.ToScale(64), UI.ToScale(72)), Color.White, 0, new Vector2(UI.ToScale(1)), UI.Depth + (UI.World.PixelDepth() * 3));

            }
        }
    }
}
