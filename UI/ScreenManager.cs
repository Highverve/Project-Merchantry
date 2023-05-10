using AetaLibrary;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Merchantry.UI.Developer.TileEditor;
using Merchantry.UI.Developer;
using Merchantry.UI.Developer.General;
using Merchantry.UI.Developer.Toolbag;

namespace Merchantry.UI
{
    public class ScreenManager : InterfaceManager
    {
        public BackpackUI Backpack { get; private set; }
        public MessageUI Message { get; private set; }

        public TooltipUI Tooltip { get; private set; }
        public ContextUI Context { get; private set; }
        public DragUI Drag { get; private set; }
        public HudUI HUD { get; private set; }
        public TransitionUI Transitions { get; private set; }

        #region Developer UIs

        public DeveloperHUD Developer { get; private set; }
        public LayersUI Layers { get; private set; }
        public TilesUI Tiles { get; private set; }
        public PropertiesUI Properties { get; private set; }
        public ToolsUI Tools { get; private set; }

        #endregion

        public ScreenManager(GraphicsDevice Graphics) : base(Graphics) { }

        public override void Initialize()
        {
            controls = GameManager.References().Controls;

            base.Initialize();
        }
        protected override void Load() { base.Load(); }
        protected override void PostInitialize()
        {
            base.PostInitialize();

            UI_Add("Tooltip", Tooltip = new TooltipUI(graphics, new Vector2(200, 200)));
            UI_Add("Context", Context = new ContextUI(graphics, new Vector2(200, 220)));

            UI_Add("Backpack", Backpack = new BackpackUI(graphics, new Vector2(742, 110)));
            UI_Add("Message", Message = new MessageUI(graphics, new Vector2(600, 400)));
            UI_Add("Drag", Drag = new DragUI(graphics, new Vector2(100, 100)));
            UI_Add("HUD", HUD = new HudUI(graphics, GameManager.References().Settings.Resolution));
            UI_Add("Transitions", Transitions = new TransitionUI(graphics, GameManager.References().Settings.Resolution));

            UI_Add("Layers", Layers = new LayersUI(graphics, new Vector2(300, 400)));
            UI_Add("Tiles", Tiles = new TilesUI(graphics, new Vector2(600, 400)));
            UI_Add("Properties", Properties = new PropertiesUI(graphics, new Vector2(400, 550)));
            UI_Add("Tools", Tools = new ToolsUI(graphics, new Vector2(300, 300)));
            UI_Add("Developer", Developer = new DeveloperHUD(graphics, GameManager.References().Settings.Resolution));

            UI_SetActive(Properties);
            Properties.Maximize();
        }

        public override void Update(GameTime gt)
        {
            UpdateSelected(gt);
            UpdatePriority(gt);
            UpdateAlways(gt);

            UpdateClickDetection(gt);
        }
        public override void UpdateAlways(GameTime gt)
        {
            base.UpdateAlways(gt);
        }

        public override void Prerender(SpriteBatch sb)
        {
            base.Prerender(sb);

            Developer.Prerender(sb);
            HUD.Prerender(sb);
            Backpack.Prerender(sb);
            Context.Prerender(sb);
            Tooltip.Prerender(sb);
            Drag.Prerender(sb);
            Transitions.Prerender(sb);

            graphics.SetRenderTarget(null);
        }
        public override void Draw(SpriteBatch sb)
        {
            HUD.DrawTargets(sb);
            Backpack.DrawTargets(sb);

            base.Draw(sb);

            Developer.DrawTargets(sb);
            Context.DrawTargets(sb);
            Tooltip.DrawTargets(sb);
            Drag.DrawTargets(sb);
            Transitions.DrawTargets(sb);
        }
    }
}
