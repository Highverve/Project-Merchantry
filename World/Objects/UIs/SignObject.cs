using Merchantry.World.Objects.UIs.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Objects.UIs
{
    public class SignObject : UIObject
    {
        private TextElement[] text;

        public SignObject(string ID, Vector2 Position, Texture2D Texture, float ScaleLevel, Vector2 FocusOffset, params TextElement[] Text) : base(ID, Position, Texture)
        {
            this.FocusScale = ScaleLevel;
            this.FocusOffset = FocusOffset;
            text = Text;
        }

        public override void Initialize()
        {
            base.Initialize();

            IsAcceptingItems = false;
            FocusMultiplier = .1f;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] != null)
                {
                    if (string.IsNullOrEmpty(text[i].Name))
                        text[i].Name = "Text" + i;
                    AddElement(text[i].Name, text[i]);
                }
            }
            DisplayName = "Sign";
        }

        public override void Draw(SpriteBatch sb)
        {
            DrawElements(sb);

            base.Draw(sb);
        }
    }
}
