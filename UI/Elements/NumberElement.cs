using AetaLibrary.Elements.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Merchantry.UI.Elements
{
    public class NumberElement : TextElement
    {
        private float number;
        public float Number
        {
            get { return number; }
            set
            {
                number = value;

                if (string.IsNullOrEmpty(Format) == false)
                    Text = Label + number.ToString(Format);
                else
                    Text = Label + number.ToString();
            }
        }
        public string Label { get; set; }
        public string Format { get; set; }

        public NumberElement(int RenderOrder, Vector2 Offset, SpriteFont Font, float Number, string Label,
            Color InitialColor, Vector2 Origin, Vector2 Scale, float Rotation, SpriteEffects Effects)
            : base(RenderOrder, Offset, Font, Number.ToString(), InitialColor, Origin, Scale, Rotation, Effects)
        {
            this.Label = Label;
            this.Number = Number;
        }
    }
}
