using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Merchantry.World.Tiles;
using AetaLibrary.Elements.Text;
using AetaLibrary.Elements.Images;
using AetaLibrary.Elements;
using ExtensionsLibrary.Extensions;

namespace Merchantry.UI.Developer.TileEditor
{
    public class LayersUI : BaseUI
    {
        private Texture2D developerIcons;
        private int layerCount = 0;

        private StretchBoxElement buttonWell;
        private ImageElement addLayer, removeLayer, renameLayer, showHideLayer, goToLayer;

        private TileLayer selected;
        public TileLayer Selected
        {
            get { return selected; }
            set
            {
                if (selected != null)
                {
                    string box = Selected.Name + "_Button";
                    if (ELEMENTS_Contains(box))
                        ELEMENTS_Select<StretchBoxElement>(box).Texture = button;
                }

                selected = value;
                onLayerChange?.Invoke(selected);

                if (selected != null)
                {
                    RefreshSelected();
                    layerIndex = layers.IndexOf(Selected);
                }
            }
        }
        private List<TileLayer> layers = new List<TileLayer>();
        private event Action<TileLayer> onLayerChange;
        public event Action<TileLayer> OnLayerChange { add { onLayerChange += value; } remove { onLayerChange -= value; } }

        /// <summary>
        /// Called on map load, before any tile layers have been added.
        /// </summary>
        public void ClearLayers()
        {
            layers.Clear();
            Selected = null;
        }

        public void AddLayer(TileLayer layer)
        {
            layers.Add(layer);

            StretchBoxElement button = CreateLayerButton();
            TextElement name = CreateLayerName(layer.Name);
            StretchBoxElement propertyButton = CreatePropertyButton(button);
            ImageElement propertyIcon = CreatePropertyIcon(button);

            button.Buttons.OnHoverEnter += () =>
            {
                button.Texture = buttonHover;
                Screens.Tooltip.LAYOUT_SetText(((layer.IsVisible) ? "Shown" : "Hidden") + "\nDepth: " + layer.Depth + "\nTiles: " + layer.Tiles.Count);
            };
            button.Buttons.OnHoverExit += () =>
            {
                if (layer != Selected)
                    button.Texture = base.button;

                Screens.Tooltip.Minimize();
            };
            button.Buttons.OnLeftClick += () =>
            {
                Selected = layer;
            };

            propertyButton.Buttons.OnHoverEnter += () =>
            {
                propertyButton.Texture = buttonHover;
                Screens.Tooltip.LAYOUT_SetText("Properties");
            };
            propertyButton.Buttons.OnHoverExit += () =>
            {
                propertyButton.Texture = base.button;
                Screens.Tooltip.Minimize();
            };
            propertyButton.Buttons.OnLeftClick += () => Screens.Properties.SetSelected(layer);

            ELEMENTS_Add(layer.Name + "_Button", button);
            ELEMENTS_Add(layer.Name + "_Name", name);
            ELEMENTS_Add(layer.Name + "_PropertyButton", propertyButton);
            ELEMENTS_Add(layer.Name + "_PropertyIcon", propertyIcon);

            RefreshPositions(true);

            if (Selected == null)
                Selected = layer;
        }
        public void RemoveLayer(TileLayer layer)
        {
            ELEMENTS_Remove(layer.Name + "_Button");
            ELEMENTS_Remove(layer.Name + "_Name");

            if (layer == Selected)
            {
                if (layers.Count > 1)
                {
                    int index = layers.IndexOf(layer);
                    Selected = layers[Math.Min(index + 1, layers.Count - 1)];
                }
                else
                    Selected = null;
            }
            else
                Selected = null;

            layers.Remove(layer);
            RefreshPositions();
        }

        private TextElement CreateLayerName(string name)
        {
            TextElement text = new TextElement(2, TopLeft + new Vector2(50, Size.Y), smallFont, name, ColorExt.CharcoalGray, Vector2.Zero, Vector2.One, 0, SpriteEffects.None);
            text.POSITION_Speed = 10;
            return text;
        }
        private StretchBoxElement CreateLayerButton()
        {
            StretchBoxElement box = new StretchBoxElement(1, TopLeft + new Vector2(50, Size.Y), button, Color.White, new Point((int)Size.X - 70, 30), 16);
            box.POSITION_Speed = 10;
            return box;
        }
        private StretchBoxElement CreatePropertyButton(Element attach)
        {
            StretchBoxElement element = new StretchBoxElement(1, TopLeft + new Vector2(254, Size.Y), button, Color.White, new Point(30, 30), 16);
            element.POSITION_Speed = 10;
            attach.OnPositionMove += () => element.Y = attach.Y;

            return element;
        }
        private ImageElement CreatePropertyIcon(Element attach)
        {
            ImageElement element = new ImageElement(2, TopLeft + new Vector2(256, Size.Y), developerIcons, new Rectangle(48, 96, 24, 24),
                ColorExt.CharcoalGray, Vector2.Zero, Vector2.One, 0, SpriteEffects.None);
            element.POSITION_Speed = 10;
            attach.OnPositionMove += () => element.Y = attach.Y + 2;

            return element;
        }

        private void RefreshPositions(bool isForcing = false)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                ELEMENTS_Select(layers[i].Name + "_Button").POSITION_Loose(FromIndex(i));
                ELEMENTS_Select(layers[i].Name + "_Name").POSITION_Loose(FromIndex(i) + new Vector2(12, 2));
            }
        }
        private void RefreshSelected()
        {
            if (Selected.IsVisible == true)
                showHideLayer.Source = new Rectangle(144, 0, 48, 48);
            else
                showHideLayer.Source = new Rectangle(192, 0, 48, 48);

            StretchBoxElement button = ELEMENTS_Select<StretchBoxElement>(Selected.Name + "_Button");
            TextElement text = ELEMENTS_Select<TextElement>(Selected.Name + "_Text");

            button.Texture = buttonHover;
        }

        private Vector2 FromIndex(int index)
        {
            return TopLeft + new Vector2(20, ((index * 34)) + (scrollIndex * 34) + 40);
        }
        private int scrollIndex;
        private void Scroll(int direction)
        {
            scrollIndex += direction;
            scrollIndex = MathHelper.Clamp(scrollIndex, Math.Min(-(layers.Count - 10), 0), 0);
            RefreshPositions();
        }
        private void SwapLayerOrder(TileLayer last, TileLayer next)
        {
            int lastIndex = layers.IndexOf(last);
            int nextIndex = layers.IndexOf(next);
            layers[nextIndex] = last;
            layers[lastIndex] = next;
        }

        private int layerIndex = 0;
        public void ScrollIndex(int direction)
        {
            if (layers.Count > 0)
            {
                layerIndex += direction;

                if (layerIndex > layers.Count - 1)
                    layerIndex = 0;
                else if (layerIndex < 0)
                    layerIndex = layers.Count - 1;

                Selected = layers[layerIndex];
            }
        }

        public LayersUI(GraphicsDevice Graphics, Vector2 Size)
            : base(Graphics, Size) { }

        public override void Load(ContentManager cm)
        {
            developerIcons = cm.Load<Texture2D>("UI/Icons/developerIcons");

            base.Load(cm);
        }
        public override void PostInitialize()
        {
            base.PostInitialize();

            IsAutoDefaultControls = true;
            IsTitlebarEnabled = true;
            Title = "Layers";
            TitlebarWidth = 120;

            AddWellButtons();
            UI_SetOrigin(Center);
            Minimize();
        }
        private void AddWellButtons()
        {
            //Black well to contain buttons
            ELEMENTS_Add("ButtonWell", buttonWell = new StretchBoxElement(1, TopLeft + new Vector2(-38, 16), wellBlack, Color.White, new Point(48, 48 * 5), 16));
            CLICKBOX_Add("ButtonWell", buttonWell.Position.ToPoint(), buttonWell.Size);
            buttonWell.TargetRender = "EXTERNAL";

            ELEMENTS_Add("AddLayerButton", addLayer = new ImageElement(1, buttonWell.Position + new Vector2(24, 24), developerIcons, new Rectangle(0, 0, 48, 48),
                Color.Lerp(Color.Green, Color.White, .4f), Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            addLayer.SetOrigin(.5f, .5f);
            addLayer.SCALE_Speed = 10;
            addLayer.TargetRender = "EXTERNAL";
            addLayer.Buttons.OnHoverEnter += () =>
            {
                addLayer.SCALE_Loose(1.25f);
                Screens.Tooltip.LAYOUT_SetText("Add Layer");
            };
            addLayer.Buttons.OnHoverExit += () =>
            {
                addLayer.SCALE_Loose(1);
                Screens.Tooltip.Minimize();
            };
            addLayer.Buttons.OnLeftClick += () =>
            {
                Queue.Add(0, () =>
                {
                    References.World.AddLayer("Layer_" + layerCount, new TileLayer());
                    layerCount++;
                });
            };

            ELEMENTS_Add("RemoveLayerButton", removeLayer = new ImageElement(1, buttonWell.Position + new Vector2(24, 24 + 48), developerIcons, new Rectangle(48, 0, 48, 48),
                ColorExt.Raspberry, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            removeLayer.SetOrigin(.5f, .5f);
            removeLayer.SCALE_Speed = 10;
            removeLayer.TargetRender = "EXTERNAL";
            removeLayer.Buttons.OnHoverEnter += () =>
            {
                removeLayer.SCALE_Loose(1.25f);
                Screens.Tooltip.LAYOUT_SetText("Delete Layer");
            };
            removeLayer.Buttons.OnHoverExit += () =>
            {
                removeLayer.SCALE_Loose(1);
                Screens.Tooltip.Minimize();
            };
            removeLayer.Buttons.OnLeftClick += () => Queue.Add(0, () => References.World.RemoveLayer(Selected));

            ELEMENTS_Add("RenameButton", renameLayer = new ImageElement(1, buttonWell.Position + new Vector2(24, 24 + 96), developerIcons, new Rectangle(96, 0, 48, 48),
                Color.Lerp(Color.Gray, Color.Orange, .75f), Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            renameLayer.SetOrigin(.5f, .5f);
            renameLayer.SCALE_Speed = 10;
            renameLayer.TargetRender = "EXTERNAL";
            renameLayer.Buttons.OnHoverEnter += () =>
            {
                renameLayer.SCALE_Loose(1.25f);
                Screens.Tooltip.LAYOUT_SetText("Rename Layer");
            };
            renameLayer.Buttons.OnHoverExit += () =>
            {
                renameLayer.SCALE_Loose(1);
                Screens.Tooltip.Minimize();
            };
            renameLayer.Buttons.OnLeftClick += () => { };

            ELEMENTS_Add("ShowHideButton", showHideLayer = new ImageElement(1, buttonWell.Position + new Vector2(24, 24 + 144), developerIcons, new Rectangle(144, 0, 48, 48),
                Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            showHideLayer.SetOrigin(.5f, .5f);
            showHideLayer.SCALE_Speed = 10;
            showHideLayer.TargetRender = "EXTERNAL";
            showHideLayer.Buttons.OnHoverEnter += () =>
            {
                showHideLayer.SCALE_Loose(1.25f);
                Screens.Tooltip.LAYOUT_SetText((Selected.IsVisible == true) ? "Hide Layer" : "Show Layer");
            };
            showHideLayer.Buttons.OnHoverExit += () =>
            {
                showHideLayer.SCALE_Loose(1);
                Screens.Tooltip.Minimize();
            };
            showHideLayer.Buttons.OnLeftClick += () =>
            {
                Selected.IsVisible = !Selected.IsVisible;

                RefreshSelected();
                showHideLayer.Buttons.InvokeHoverEnter();
            };

            ELEMENTS_Add("GoToButton", goToLayer = new ImageElement(1, buttonWell.Position + new Vector2(24, 24 + 192), developerIcons, new Rectangle(240, 0, 48, 48),
                Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            goToLayer.SetOrigin(.5f, .5f);
            goToLayer.SCALE_Speed = 10;
            goToLayer.TargetRender = "EXTERNAL";
            goToLayer.Buttons.OnHoverEnter += () =>
            {
                goToLayer.SCALE_Loose(1.25f);
                Screens.Tooltip.LAYOUT_SetText("Go To Layer");
            };
            goToLayer.Buttons.OnHoverExit += () =>
            {
                goToLayer.SCALE_Loose(1);
                Screens.Tooltip.Minimize();
            };
            goToLayer.Buttons.OnLeftClick += () =>
            {
                if (Selected != null)
                {
                    References.World.Focused = null;
                    References.World.Camera.ForceFocus(Selected.Position);
                }
            };
        }

        public override void Update(GameTime gt)
        {
            base.Update(gt);

            if (IsMaximized == true && CLICKBOX_Contains(Controls.MouseVector()))
            {
                if (Controls.ScrollDirection() > 0)
                    Scroll(1);
                if (Controls.ScrollDirection() < 0)
                    Scroll(-1);
            }
        }
        public override void UpdateAlways(GameTime gt)
        {
            base.UpdateAlways(gt);

            References.Debugging.IsTileMouseRender = IsMaximized;
            /*if (Screens.UI_IsMouseInsideUI() == false)
            {
                if (Selected != null)
                {
                    if (Screens.Tiles.SelectedTileset != null && Screens.Tiles.SelectedTile != null)
                    {
                        if (Controls.IsMouseClicked(Controls.CurrentMS.LeftButton))
                            Selected.AddTile(new Tile(Screens.Tiles.SelectedTileset.Tileset, Screens.Tiles.SelectedTile.Coords, Selected.MouseCoords()));
                        if (Controls.IsMouseClicked(Controls.CurrentMS.RightButton))
                            Selected.RemoveTile(Selected.MousePosition());
                    }
                }
            }*/
        }

        protected override void DrawInterior(SpriteBatch sb)
        {
            base.DrawInterior(sb);

            DrawElements(sb, Element.DefaultTarget);
        }
        protected override void DrawExterior(SpriteBatch sb)
        {
            buttonWell.Draw(sb);
            addLayer.Draw(sb);
            removeLayer.Draw(sb);
            renameLayer.Draw(sb);
            showHideLayer.Draw(sb);
            goToLayer.Draw(sb);
        }
    }
}
