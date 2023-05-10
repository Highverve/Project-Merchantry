using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Merchantry.UI.Developer.Toolbag
{
    public class ToolsUI : BaseUI
    {
        public ToolData SelectedTool { get; set; }

        public ToolsUI(GraphicsDevice Graphics, Vector2 Size) : base(Graphics, Size)
        {
        }

        public override void Load(ContentManager cm)
        {
            base.Load(cm);
        }
        public override void PostInitialize()
        {
            base.PostInitialize();

            IsAutoDefaultControls = true;
            IsTitlebarEnabled = true;
            Title = "Layers";
            TitlebarWidth = 120;

            UI_SetOrigin(Center);
            Minimize();

            SelectedTool = new Tools.TileTool();
            SelectedTool.References = References;
        }

        public override void UpdateAlways(GameTime gt)
        {
            base.UpdateAlways(gt);

            if (SelectedTool != null)
                SelectedTool.Update(gt);
        }

        protected override void DrawInterior(SpriteBatch sb)
        {
            DrawElements(sb);
        }
        protected override void DrawExterior(SpriteBatch sb)
        {
        }
    }
}
