using AetaLibrary.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Objects
{
    public class ChatBubble : RenderObject
    {
        public SpriteFont Font { get; set; }
        public WorldObject Object { get; set; }

        public ChatBubble(string ID, Vector2 Position, Texture2D Texture, SpriteFont Font, WorldObject Object) : base(ID, Position, Texture)
        {
            this.Font = Font;
            this.Object = Object;

            IsSectoring = false;
            IsAcceptingItems = false;
            IsSelectable = false;
        }
        public override void Update(GameTime gt)
        {
            base.Update(gt);
        }

        public override void Draw(SpriteBatch sb)
        {
            if (Object != null)
            {
                if (Object.Chat.HasMessage())
                {
                    Depth = Object.Depth;
                    Vector2 size = Font.MeasureString(Object.Chat.Message);

                    sb.DrawTexturedBox(Texture, new Rectangle((int)(Position.X - size.X) - 10, (int)Position.Y - 10, (int)size.X + 10, (int)size.Y + 10), Vector2.Zero, 10, Color.White, Depth + World.PixelDepth());
                    sb.DrawString(Font, Object.Chat.Message, Position + new Vector2(-5, 15), Color.Lerp(Color.Black, Color.White, .25f), 0, size, 1, SpriteEffects.None, Depth + World.PixelDepth() * 2);
                }
            }
        }
    }
}
