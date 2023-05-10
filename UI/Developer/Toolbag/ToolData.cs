using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Merchantry.UI.Developer.General;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Merchantry.UI.Developer.Toolbag
{
    public abstract class ToolData : IProperties
    {
        public References References { get; set; }
        public bool IsRender { get; set; } = false;

        public ToolData()
        {
            References = GameManager.References();
        }

        public abstract void SetProperties(PropertiesUI ui);
        public abstract void RefreshProperties();
        public abstract void NullifyProperties();

        public abstract void LeftClick();
        public abstract void RightClick();
        public abstract void MiddleClick();

        public virtual void Update(GameTime gt)
        {
            if (References.Screens.UI_IsMouseInsideUI() == false)
            {
                if (References.Controls.IsMouseClicked(References.Controls.CurrentMS.LeftButton))
                    LeftClick();
                if (References.Controls.IsMouseClicked(References.Controls.CurrentMS.RightButton))
                    RightClick();
                if (References.Controls.IsMouseClicked(References.Controls.CurrentMS.MiddleButton))
                    MiddleClick();
            }
        }
        public abstract void Draw(SpriteBatch sb);
    }
}
