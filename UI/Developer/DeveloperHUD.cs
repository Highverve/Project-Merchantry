using AetaLibrary;
using AetaLibrary.Elements;
using AetaLibrary.Elements.Images;
using AetaLibrary.Elements.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EmberiumLibrary.Extensions;

namespace Merchantry.UI.Developer
{
    public class DeveloperHUD : UserInterface
    {
        private Texture2D button, wellBlack, wellBlackHover, developerIcons;
        private SpriteFont smallFont, font;

        private StretchBoxElement layerButton, tilesetButton, tileButton, cameraButton, gridButton;
        private TextElement layerName;
        private ImageElement tilesetIcon, tileIcon, cameraIcon, gridIcon;

        private StretchBoxElement menuButton, tileEditorButton, objectEditorButton;

        private References References;
        private ScreenManager Screens;

        public Layouts Layout { get; private set; } = Layouts.TileEditor;
        public enum Layouts { Menu, TileEditor, ObjectEditor }
        public void SetLayout(Layouts layout)
        {
            LayoutOutro(Layout);

            Layout = layout;
            LayoutIntro(Layout);

            if (Layout == Layouts.Menu) menuButton.Texture = wellBlackHover;
            else menuButton.Texture = wellBlack;
            if (Layout == Layouts.ObjectEditor) objectEditorButton.Texture = wellBlackHover;
            else objectEditorButton.Texture = wellBlack;
            if (Layout == Layouts.TileEditor) tileEditorButton.Texture = wellBlackHover;
            else tileEditorButton.Texture = wellBlack;
        }
        private void LayoutOutro(Layouts layout)
        {
            foreach (Element element in layoutElements[layout])
            {
                element.POSITION_Speed = 15;
                element.SCALE_Speed = 15;

                element.POSITION_Loose(new Vector2(-element.SnapSize.X, element.BasePosition.Y));
                //element.SCALE_Loose(0);
            }
        }
        private void LayoutIntro(Layouts layout)
        {
            foreach (Element element in layoutElements[layout])
            {
                element.POSITION_Speed = 15;
                element.SCALE_Speed = 15;

                element.POSITION_Loose(new Vector2(element.BasePosition.X, element.BasePosition.Y));
                //element.SCALE_Loose(1);
            }
        }
        private Dictionary<Layouts, List<Element>> layoutElements = new Dictionary<Layouts, List<Element>>();

        public DeveloperHUD(GraphicsDevice Graphics, Vector2 Size)
            : base(Graphics, Size, false, Vector2.Zero)
        {
        }

        public override void PreInitialize()
        {
            base.PreInitialize();
        }
        public override void Load(ContentManager cm)
        {
            smallFont = cm.Load<SpriteFont>("UI/Fonts/ArchwayKnights_12px");
            font = cm.Load<SpriteFont>("UI/Fonts/ArchwayKnights_19px");

            wellBlack = cm.Load<Texture2D>("UI/Elements/wellBlack");
            wellBlackHover = cm.Load<Texture2D>("UI/Elements/wellBlackHover");
            button = cm.Load<Texture2D>("UI/Elements/button");
            developerIcons = cm.Load<Texture2D>("UI/Icons/developerIcons");

            base.Load(cm);
        }
        public override void PostInitialize()
        {
            base.PostInitialize();

            IsPriority = true;
            IsAutoDefaultControls = true;
            Sampler = SamplerState.PointClamp;
            Blending = BlendState.AlphaBlend;
            References = GameManager.References();
            Screens = (ScreenManager)Interfaces;

            //Menu buttons

            ELEMENTS_Add("MenuButton", menuButton = new StretchBoxElement(1, TopLeft + new Vector2(20, 20), wellBlack, Color.White, new Point(80, 80), 16));
            ELEMENTS_Add("TileEditorButton", tileEditorButton = new StretchBoxElement(1, TopLeft + new Vector2(110, 20), wellBlack, Color.White, new Point(80, 80), 16));
            ELEMENTS_Add("ObjectEditorButton", objectEditorButton = new StretchBoxElement(1, TopLeft + new Vector2(200, 20), wellBlack, Color.White, new Point(80, 80), 16));
            menuButton.Buttons.OnLeftClick += () => SetLayout(Layouts.Menu);
            tileEditorButton.Buttons.OnLeftClick += () => SetLayout(Layouts.TileEditor);
            objectEditorButton.Buttons.OnLeftClick += () => SetLayout(Layouts.ObjectEditor);

            //Layers
            ELEMENTS_Add("LayerBackground", layerButton = new StretchBoxElement(1, TopLeft + new Vector2(20, 120), wellBlack, Color.White, new Point(128, 48), 16));
            ELEMENTS_Add("LayerName", layerName = new TextElement(2, layerButton.Position + new Vector2(10, 10), smallFont, string.Empty, Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            layerButton.Buttons.OnHover += () =>
            {
                if (Controls.ScrollDirection() > 0)
                    Screens.Layers.ScrollIndex(-1);
                if (Controls.ScrollDirection() < 0)
                    Screens.Layers.ScrollIndex(1);
            };
            layerButton.Buttons.OnLeftClick += () =>
            {
                if (Screens.Layers.IsMaximized == false)
                {
                    Screens.Layers.Maximize();
                    Queue.Add(20, () => Screens.UI_SetActive(Screens.Layers));
                }
                else if (Screens.Layers.IsMaximized == true)
                    Screens.Layers.Minimize();
            };
            Screens.Layers.OnLayerChange += (layer) =>
            {
                if (layer != null)
                    layerName.Text = layer.Name;
                else
                    layerName.Text = "Layers";

                layerButton.Size = smallFont.MeasureString(layerName.Text).ToPoint() + new Point(20, 20);
                CLICKBOX_Set("Layers", new Rectangle(0, 0, layerButton.Size.X, layerButton.Size.Y));
            };

            ELEMENTS_Add("TilesetBackground", tilesetButton = new StretchBoxElement(1, TopLeft + new Vector2(20, 175), wellBlack, Color.White, new Point(80, 80), 16));
            ELEMENTS_Add("TilesetIcon", tilesetIcon = new ImageElement(2, tilesetButton.Position + new Vector2(8), null, Rectangle.Empty, Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            tilesetButton.Buttons.OnHover += () =>
            {
                if (Controls.ScrollDirection() > 0)
                    Screens.Tiles.ScrollSetIndex(-1);
                if (Controls.ScrollDirection() < 0)
                    Screens.Tiles.ScrollSetIndex(1);
            };
            tilesetButton.Buttons.OnLeftClick += () =>
            {
                if (Screens.Tiles.IsMaximized == false)
                {
                    Screens.Tiles.Maximize();
                    Queue.Add(20, () => Screens.UI_SetActive(Screens.Tiles));
                }
                else if (Screens.Tiles.IsMaximized == true)
                    Screens.Tiles.Minimize();
            };
            Screens.Tiles.OnTilesetChange += (tileset) =>
            {
                tilesetIcon.Texture = tileset.Icon.Texture;
                tilesetIcon.Scale = tileset.Icon.Scale;
                tilesetIcon.Source = tileset.Icon.Source;
            };

            ELEMENTS_Add("TileBackground", tileButton = new StretchBoxElement(1, tilesetButton.Position + new Vector2(0, 90), wellBlack, Color.White, new Point(80, 80), 16));
            ELEMENTS_Add("TileIcon", tileIcon = new ImageElement(2, tileButton.Position + new Vector2(8), null, Rectangle.Empty, Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            tileButton.Buttons.OnHover += () =>
            {
                if (Controls.ScrollDirection() > 0)
                    Screens.Tiles.ScrollTileIndex(-1);
                if (Controls.ScrollDirection() < 0)
                    Screens.Tiles.ScrollTileIndex(1);
            };
            tileButton.Buttons.OnLeftClick += () =>
            {
            };
            Screens.Tiles.OnTileChange += (tile) =>
            {
                tileIcon.Texture = tile.Icon.Texture;
                tileIcon.Source = new Rectangle(1 + (tile.Coords.X * 66), 1 + (tile.Coords.Y * 66), 64, 64);
            };

            ELEMENTS_Add("CameraButton", cameraButton = new StretchBoxElement(1, new Vector2(BottomRight.X - 50, TopLeft.Y + 10), wellBlack, Color.White, new Point(40), 16));
            ELEMENTS_Add("CameraIcon", cameraIcon = new ImageElement(2, cameraButton.Position + new Vector2(8), developerIcons,
                new Rectangle(24, 96, 24, 24), Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            cameraButton.Buttons.OnLeftClick += () =>
            {
                References.Debugging.IsDebugCamera = !References.Debugging.IsDebugCamera;

                if (References.Debugging.IsDebugCamera == true)
                {
                    References.World.Camera.TargetScale = new Vector2(1.5f);
                    References.World.Camera.ScaleSpeed = 10;
                    References.World.Focused = null;
                }
                else
                    References.World.Focused = References.World.Controlled;
            };

            CLICKBOX_Add("Layers", 0, 0, 128, 48);
            CLICKBOX_Add("LeftMenu", 0, 0, 100, 300);
            CLICKBOX_Add("RightMenu", (int)BottomRight.X - 52, (int)TopLeft.Y, 52, 52);

            Minimize();

            List<Element> tileEditor = new List<Element>();
            tileEditor.Add(layerButton, layerName, tilesetButton, tilesetIcon, tileButton, tileIcon);

            layoutElements.Add(Layouts.Menu, new List<Element>());
            layoutElements.Add(Layouts.TileEditor, tileEditor);
            layoutElements.Add(Layouts.ObjectEditor, new List<Element>());

            Queue.Add(0, () =>
            {
                LayoutOutro(Layouts.TileEditor);
                LayoutOutro(Layouts.ObjectEditor);
            });
            SetLayout(Layouts.Menu);
        }

        public override void Update(GameTime gt)
        {
            base.Update(gt);
        }
        public override void UpdateAlways(GameTime gt)
        {
            if (Controls.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.F1))
            {
                if (IsMaximized == false)
                {
                    Maximize();
                    References.World.IsAllowSelecting = false;
                    Screens.HUD.IsMenuInteract = false;
                    Screens.Backpack.Minimize();
                }
                else
                {
                    Minimize();
                    References.World.IsAllowSelecting = true;
                    Screens.HUD.IsMenuInteract = true;
                }
            }

            base.UpdateAlways(gt);
        }

        protected override void Draw(SpriteBatch sb)
        {
            sb.Begin(Sorting, Blending, Sampler, null, null, null, Camera.View());
            DrawElements(sb);
            sb.End();

            base.Draw(sb);
        }

        protected override void MaximizeTransition()
        {
            mainColor.Result = Color.White;
            base.MaximizeTransition();
        }
        protected override void MinimizeTransition()
        {
            mainColor.Result = Color.Transparent;
            base.MinimizeTransition();
        }
    }
}
