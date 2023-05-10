using AetaLibrary.Elements.Images;
using AetaLibrary.Elements.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.UI.Elements
{
    public class ButtonOption
    {
        public string Text { get; set; }
        public string Tooltip { get; set; }

        public Action LeftClick { get; set; }
        public Action RightClick { get; set; }

        public StretchBoxElement BoxElement { get; set; }
        public TextElement TextElement { get; set; }

        public ButtonOption(string Text, string Tooltip, Action LeftClick, Action RightClick)
        {
            this.Text = Text;
            this.Tooltip = Tooltip;

            this.LeftClick = LeftClick;
            this.RightClick = RightClick;
        }
    }
}
