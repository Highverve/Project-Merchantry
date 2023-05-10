using Merchantry.UI.Items;
using Merchantry.World.Meta;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using AetaLibrary;
using AetaLibrary.Elements.Images;
using static Merchantry.UI.Items.ItemObject;
using Merchantry.Extensions;
using AetaLibrary.Elements;
using AetaLibrary.Elements.Text;
using ExtensionsLibrary.Extensions;

namespace Merchantry.UI
{
    public class InventoryUI : BaseUI
    {
        //Textures
        public Texture2D ItemBackground { get; private set; }
        public SpriteFont Font { get; set; }
        private Texture2D itemBackgroundSelect, tabBackground, tabBackgroundSelect;
        private Texture2D iconConsumables, iconTools, iconApparel, iconResources, iconVarious;

        public TextElement EmptyText { get; set; }

        //Classes
        public ObjectStorage Storage { get; private set; }
        public void SetStorage(ObjectStorage storage)
        {
            if (Storage != null)
                RemoveEvents();

            Storage = storage;
            AddEvents();

            ClearTabs();
            FillTabs();
        }
        public void ClearTabs()
        {
            consumables.ClearItems();
            tools.ClearItems();
            apparel.ClearItems();
            resources.ClearItems();
            various.ClearItems();
        }

        public ItemElement SelectedElement { get; set; }
        public ItemObject SelectedItem()
        {
            if (SelectedElement != null)
                return SelectedElement.Item;
            return null;
        }
        public void SetSelected(ItemElement element)
        {
            //Visual effect applied to previously selected element, if any (outro).
            if (SelectedElement != null)
            {
                SelectedElement.Background = ItemBackground;
                SelectedElement.SCALE_SmoothStep(Vector2.One);
                SelectedElement.SCALE_Speed = 5;
            }
            
            //Visual effect applied to newly selected element (intro).
            if (element != null)
            {
                SelectedElement = element;
                SelectedElement.Background = itemBackgroundSelect;
                Screens.Drag.HoverItem = element.Item;

                ResetSelectAnimation();
            }
            else
            {
                SelectedElement = null;

                if (Screens.Drag != null)
                    Screens.Drag.HoverItem = null;
            }
        }

        public InventoryTab SelectedTab { get; set; }
        private InventoryTab consumables, tools, apparel, resources, various;
        private void InitializeTabs()
        {
            consumables = new InventoryTab("Foods", ItemTabs.Consumables, this);
            tools = new InventoryTab("Tools", ItemTabs.Tools, this);
            apparel = new InventoryTab("Apparel", ItemTabs.Apparel, this);
            resources = new InventoryTab("Resources", ItemTabs.Resources, this);
            various = new InventoryTab("Various", ItemTabs.Various, this);
        }
        private void FillTabs()
        {
            consumables.FillItems(Storage.Items.Values.ToList());
            tools.FillItems(Storage.Items.Values.ToList());
            apparel.FillItems(Storage.Items.Values.ToList());
            resources.FillItems(Storage.Items.Values.ToList());
            various.FillItems(Storage.Items.Values.ToList());
        }
        public void SetTab(ItemTabs tab)
        {
            SetSelected(null);

            //Previous tab
            if (SelectedTab != null)
                SelectedTab.SetInteraction(false);

            //Set new tab
            if (tab == ItemTabs.Consumables) SelectedTab = consumables;
            if (tab == ItemTabs.Tools) SelectedTab = tools;
            if (tab == ItemTabs.Apparel) SelectedTab = apparel;
            if (tab == ItemTabs.Resources) SelectedTab = resources;
            if (tab == ItemTabs.Various) SelectedTab = various;

            consumables.LeftTab.Texture = tabBackground;
            tools.LeftTab.Texture = tabBackground;
            apparel.LeftTab.Texture = tabBackground;
            resources.LeftTab.Texture = tabBackground;
            various.LeftTab.Texture = tabBackground;

            SelectedTab.LeftTab.Texture = tabBackgroundSelect;
            Title = SelectedTab.Name;
            TitlebarWidth = MathHelper.Max(50, (int)TitlebarText.Font.MeasureString(TitlebarText.Text).X + 60);
            SelectedTab.SetInteraction(true);
            SelectedTab.ValidateElements();
        }

        //Data
        public int GridWidth { get; set; }
        public int GridHeight { get; set; }

        //Events
        public void AddEvents()
        {
            Storage.OnNewStack += AddTabItem;
            Storage.OnRemoveItem += RemoveTabItem;
        }
        public void RemoveEvents()
        {
            Storage.OnNewStack -= AddTabItem;
            Storage.OnRemoveItem -= RemoveTabItem;
        }

        private void AddTabItem(ItemObject item)
        {
            if (item.Tab == ItemTabs.Consumables)
                consumables.AddItem(item);
            if (item.Tab == ItemTabs.Tools)
                tools.AddItem(item);
            if (item.Tab == ItemTabs.Apparel)
                apparel.AddItem(item);
            if (item.Tab == ItemTabs.Resources)
                resources.AddItem(item);
            if (item.Tab == ItemTabs.Various)
                various.AddItem(item);
        }
        private void RemoveTabItem(ItemObject item)
        {
            if (item.Tab == ItemTabs.Consumables)
                consumables.RemoveItem(item);
            if (item.Tab == ItemTabs.Tools)
                tools.RemoveItem(item);
            if (item.Tab == ItemTabs.Apparel)
                apparel.RemoveItem(item);
            if (item.Tab == ItemTabs.Resources)
                resources.RemoveItem(item);
            if (item.Tab == ItemTabs.Various)
                various.RemoveItem(item);
        }

        public void SetGridSize(int x, int y)
        {
            GridWidth = x;
            GridHeight = y;
        }

        public InventoryUI(GraphicsDevice Graphics, Vector2 Size) : base(Graphics, Size) { }

        public override void Load(ContentManager cm)
        {
            ItemBackground = cm.Load<Texture2D>("UI/Elements/itemBackground");
            itemBackgroundSelect = cm.Load<Texture2D>("UI/Elements/itemBackgroundSelect");

            tabBackground = cm.Load<Texture2D>("UI/Elements/tabBackground");
            tabBackgroundSelect = cm.Load<Texture2D>("UI/Elements/tabBackgroundSelect");
            Font = cm.Load<SpriteFont>("UI/Fonts/ArchwayKnights_19px");

            iconConsumables = cm.Load<Texture2D>("UI/Icons/tabFoods");
            iconTools = cm.Load<Texture2D>("UI/Icons/tabTools");
            iconApparel = cm.Load<Texture2D>("UI/Icons/tabApparel");
            iconResources = cm.Load<Texture2D>("UI/Icons/tabResources");
            iconVarious = cm.Load<Texture2D>("UI/Icons/tabVarious");

            base.Load(cm);
        }
        public override void PostInitialize()
        {
            base.PostInitialize();

            SetGridSize(5, 5);
            InitializeTabs();

            #region Tabs

            ELEMENTS_Add("ConsumablesTab", consumables.LeftTab = new ImageElement(0, Center - (new Vector2(SizeCenter.X + 53, SizeCenter.Y - 24)),
                tabBackground, tabBackground.Bounds, Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            ELEMENTS_Add("ToolsTab", tools.LeftTab = new ImageElement(0, Center - (new Vector2(SizeCenter.X + 53, (SizeCenter.Y - 24) - (tabBackground.Height + 2))),
                tabBackground, tabBackground.Bounds, Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            ELEMENTS_Add("ApparelTab", apparel.LeftTab = new ImageElement(0, Center - (new Vector2(SizeCenter.X + 53, (SizeCenter.Y - 24) - (tabBackground.Height + 2) * 2)),
                tabBackground, tabBackground.Bounds, Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            ELEMENTS_Add("ResourcesTab", resources.LeftTab = new ImageElement(0, Center - (new Vector2(SizeCenter.X + 53, (SizeCenter.Y - 24) - (tabBackground.Height + 2) * 3)),
                tabBackground, tabBackground.Bounds, Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            ELEMENTS_Add("VariousTab", various.LeftTab = new ImageElement(0, Center - (new Vector2(SizeCenter.X + 53, (SizeCenter.Y - 24) - (tabBackground.Height + 2) * 4)),
                tabBackground, tabBackground.Bounds, Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));

            ELEMENTS_Add("ConsumablesIcon", consumables.TabIcon = new ImageElement(1, Center - (new Vector2(SizeCenter.X + 53, (SizeCenter.Y - 24) - (tabBackground.Height + 2))),
                iconConsumables, iconConsumables.Bounds, Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            ELEMENTS_Add("ToolsIcon", tools.TabIcon = new ImageElement(1, Center - (new Vector2(SizeCenter.X + 53, (SizeCenter.Y - 24) - (tabBackground.Height + 2))),
                iconTools, iconTools.Bounds, Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            ELEMENTS_Add("ApparelIcon", apparel.TabIcon = new ImageElement(1, Center - (new Vector2(SizeCenter.X + 53, (SizeCenter.Y - 24) - (tabBackground.Height + 2))),
                iconApparel, iconApparel.Bounds, Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            ELEMENTS_Add("ResourcesIcon", resources.TabIcon = new ImageElement(1, Center - (new Vector2(SizeCenter.X + 53, (SizeCenter.Y - 24) - (tabBackground.Height + 2))),
                iconResources, iconResources.Bounds, Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            ELEMENTS_Add("VariousIcon", various.TabIcon = new ImageElement(1, Center - (new Vector2(SizeCenter.X + 53, (SizeCenter.Y - 24) - (tabBackground.Height + 2))),
                iconVarious, iconVarious.Bounds, Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));

            CLICKBOX_Add("ConsumablesBox", new Rectangle(0, 0, tabBackground.Width, tabBackground.Height));
            CLICKBOX_Add("ToolsBox", new Rectangle(0, 0, tabBackground.Width, tabBackground.Height));
            CLICKBOX_Add("ApparelBox", new Rectangle(0, 0, tabBackground.Width, tabBackground.Height));
            CLICKBOX_Add("ResourcesBox", new Rectangle(0, 0, tabBackground.Width, tabBackground.Height));
            CLICKBOX_Add("VariousBox", new Rectangle(0, 0, tabBackground.Width, tabBackground.Height));

            consumables.TabIcon.SetOrigin(.5f, .5f);
            tools.TabIcon.SetOrigin(.5f, .5f);
            apparel.TabIcon.SetOrigin(.5f, .5f);
            resources.TabIcon.SetOrigin(.5f, .5f);
            various.TabIcon.SetOrigin(.5f, .5f);

            consumables.SetTabEvents();
            tools.SetTabEvents();
            apparel.SetTabEvents();
            resources.SetTabEvents();
            various.SetTabEvents();

            icons.Add(consumables.TabIcon);
            icons.Add(tools.TabIcon);
            icons.Add(apparel.TabIcon);
            icons.Add(resources.TabIcon);
            icons.Add(various.TabIcon);

            #endregion

            ELEMENTS_Add("Empty", EmptyText = new TextElement(0, Center, Font, "EMPTY",
                Color.Lerp(Color.Black, Color.Transparent, .75f), Font.MeasureString("EMPTY") / 2,
                new Vector2(2), 0, SpriteEffects.None));

            SetTab(ItemTabs.Consumables);
        }

        public int Delay = 50;

        public override void Update(GameTime gt)
        {
            UpdateShortcuts();

            if (SelectedTab.ElementCount() == 0)
            {
                EmptyText.SCALE_SmoothStep(new Vector2(2));
                EmptyText.SCALE_Speed = 20f;
            }
            if (SelectedTab.ElementCount() > 0)
            {
                EmptyText.SCALE_SmoothStep(Vector2.Zero);
                EmptyText.SCALE_Speed = 15f;
            }

            if (Controls.IsMouseClicked(Controls.CurrentMS.LeftButton) && Delay <= 0)
            {
                if (Screens.Drag.IsHolding() &&
                    Screens.Drag.HoverItem != Screens.Drag.Item() &&
                    CLICKBOX_Contains(Controls.MouseVector()))
                {
                    Storage.AddItem(Screens.Drag.TakeItem());
                }
            }
            if (Delay > 0)
                Delay -= gt.ElapsedGameTime.Milliseconds;

            consumables.Update();
            tools.Update();
            apparel.Update();
            resources.Update();
            various.Update();

            AnimateSelected(gt);
            UI_KeepInBounds(TopLeft - new Vector2(64, 24), BottomRight + new Vector2(12, 12));

            IsInteract = true;

            base.Update(gt);
        }
        private void UpdateShortcuts()
        {
            if (CLICKBOX_Contains(Controls.MouseVector()))
            {
                int mouseWheel = Controls.ScrollDirection();
                if (mouseWheel > 0)
                    SelectedTab.MoveIndex(1);
                if (mouseWheel < 0)
                    SelectedTab.MoveIndex(-1);

                if (Controls.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.PageUp))
                    SelectedTab.GoPageUp();
                if (Controls.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.PageDown))
                    SelectedTab.GoPageDown();
                if (Controls.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.Home))
                    SelectedTab.GoToTop();
                if (Controls.IsKeyPressed(Microsoft.Xna.Framework.Input.Keys.End))
                    SelectedTab.GoToBottom();
            }
        }

        public override void UpdateAlways(GameTime gt)
        {
            base.UpdateAlways(gt);

            consumables.TabIcon.Position = consumables.LeftTab.Position + new Vector2(36, 27);
            tools.TabIcon.Position = tools.LeftTab.Position + new Vector2(36, 27);
            apparel.TabIcon.Position = apparel.LeftTab.Position + new Vector2(36, 27);
            resources.TabIcon.Position = resources.LeftTab.Position + new Vector2(32, 27);
            various.TabIcon.Position = various.LeftTab.Position + new Vector2(36, 27);

            consumables.UpdateAlways();
            tools.UpdateAlways();
            apparel.UpdateAlways();
            resources.UpdateAlways();
            various.UpdateAlways();

            if (GameManager.References().Settings.IsPaused == true && IsMaximized == true)
            {
                IsInteract = false;
                Minimize();
            }
        }

        private float scaleLerp;
        private bool isScaleIncrease = true;
        public void ResetSelectAnimation()
        {
            scaleLerp = 0;
            isScaleIncrease = true;
        }
        private void AnimateSelected(GameTime gt)
        {
            if (isScaleIncrease == true)
                scaleLerp += 4 * (float)gt.ElapsedGameTime.TotalSeconds;
            if (isScaleIncrease == false)
                scaleLerp -= 4 * (float)gt.ElapsedGameTime.TotalSeconds;

            if (scaleLerp > 1) isScaleIncrease = false;
            if (scaleLerp < 0) isScaleIncrease = true;
            scaleLerp = MathHelper.Clamp(scaleLerp, 0, 1);

            if (SelectedElement != null)
                SelectedElement.Scale = Vector2.SmoothStep(Vector2.One, new Vector2(1.25f), scaleLerp);
        }

        private List<Element> icons = new List<Element>();
        protected override void Draw(SpriteBatch sb)
        {
            sb.Begin(Sorting, Blending, Sampler, null, null, null, Camera.View());

            consumables.LeftTab.Draw(sb);
            tools.LeftTab.Draw(sb);
            apparel.LeftTab.Draw(sb);
            resources.LeftTab.Draw(sb);
            various.LeftTab.Draw(sb);

            /*consumables.TabIcon.Draw(sb);
            tools.TabIcon.Draw(sb);
            apparel.TabIcon.Draw(sb);
            resources.TabIcon.Draw(sb);
            various.TabIcon.Draw(sb);*/
            DrawElements(sb, icons);

            sb.End();

            base.Draw(sb);
        }
        protected override void DrawInterior(SpriteBatch sb)
        {
            //Might switch over to drawing all tabs, instead using fade/scale transitions

            EmptyText.Draw(sb);
            SelectedTab.Draw(sb);
            GameManager.References().Particles.DrawGroup(sb, "InventoryInterior");
        }
        protected override void DrawExterior(SpriteBatch sb) { }

        public override void Maximize()
        {
            if (GameManager.References().Settings.IsPaused == false)
                base.Maximize();
        }
    }
    public class InventoryTab
    {
        List<ItemElement> elements = new List<ItemElement>();
        public ItemTabs Tab { get; set; }
        public string Name { get; set; }

        public ImageElement LeftTab { get; set; }
        public ImageElement TabIcon { get; set; }
        public void SetTabEvents()
        {
            LeftTab.Buttons.OnHoverEnter += () =>
            {
                LeftTab.POSITION_SmoothStep(new Vector2(UI.Center.X - (UI.SizeCenter.X + 73), LeftTab.BasePosition.Y));
                LeftTab.POSITION_Speed = 5;
                TabIcon.SCALE_SmoothStep(new Vector2(2f));
                TabIcon.SCALE_Speed = 7.5f;

                TabIcon.RenderOrder = 2;
            };
            LeftTab.Buttons.OnHoverExit += () =>
            {
                LeftTab.POSITION_SmoothStep(LeftTab.BasePosition);
                LeftTab.POSITION_Speed = 5;
                TabIcon.SCALE_SmoothStep(new Vector2(1f));
                TabIcon.SCALE_Speed = 5f;

                TabIcon.RenderOrder = 1;
            };
            LeftTab.Buttons.OnLeftClick += () => UI.SetTab(Tab);
        }

        public InventoryUI UI { get; set; }
        public InventoryTab(string Name, ItemTabs Tab, InventoryUI UI)
        {
            this.Name = Name;
            this.Tab = Tab;
            this.UI = UI;
        }

        public void AddItem(ItemObject item)
        {
            string name = item.StackID();

            if (UI.Elements.ContainsKey(name))
                throw new Exception("Contains item key. This should not be hit.");

            ItemElement element = GetElement(name);

            element.Item = item;
            Point grid = ToGrid(elements.Count);
            element.Position = ToPosition(grid);
            element.BasePosition = element.Position;

            //Hook up button events.
            element.Buttons.OnHover += () =>
            {
                UI.Screens.Drag.HoverItem = element.Item;

                if (UI.SelectedElement != element)
                    UI.SetSelected(element);

                //if (UI.Screens.Tooltip.IsSelectedItem(item) == false)
                UI.Screens.Tooltip.LAYOUT_SetItem(item);
                if (UI.Screens.Tooltip.IsMaximized == false)
                    UI.Screens.Tooltip.Maximize();
            };
            element.Buttons.OnHoverExit += () =>
            {
                if (UI.SelectedElement != element)
                {
                    element.SCALE_SmoothStep(Vector2.One);
                    element.SCALE_Speed = 5f;
                }
                if (UI.SelectedElement == element)
                {
                    UI.Screens.Tooltip.Minimize();
                    UI.SetSelected(null);
                }
            };
            element.Buttons.OnLeftClick += () =>
            {
                if (UI.Screens.Drag.IsHolding() == false || element.Item.StackID() == UI.Screens.Drag.Item().StackID())
                {
                    UI.Delay = 50;

                    if (UI.Controls.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl))
                        UI.Screens.Drag.AddItem(item, 1);
                    else
                        UI.Screens.Drag.AddItem(item);
                }
            };
            element.Buttons.OnRightClick += () =>
            {
                UI.Screens.Context.ParentUI = UI;
                UI.Screens.Context.LAYOUT_Item(item);
                UI.Screens.Context.Maximize();

                UI.IsInteract = false;
            };

            //Interact if this is the current tab.
            if (UI.SelectedTab == this) element.IsInteract = true;
            else element.IsInteract = false;

            elements.Add(element);
            UI.ELEMENTS_Add(name, element);

            //Skip intro if grid position is off screen.
            if (grid.Y < UI.GridHeight) element.Intro();
            else
            {
                element.Scale = Vector2.Zero;
                SetGridElement(element, elements.Count);
            }

            //Set default events (use Method(ItemElement)). Maybe not, might be unnecessary.
            //Set position, set scale to 0, set scale target to 1.
        }
        public void RemoveItem(ItemElement item)
        {
            item.Item = null;

            elements.Remove(item);
            UI.ELEMENTS_Remove(item);

            ValidateIndex();
            ValidatePositions();
            ValidateElements();
        }
        public void RemoveItem(ItemObject item)
        {
            ItemElement element = elements.FirstOrDefault(s => s.Item == item);

            if (element != null)
                RemoveItem(element);
        }

        public ItemElement GetElement(string key)
        {
            if (UI.Elements.ContainsKey(key))
                return (ItemElement)UI.Elements[key];
            else
                return new ItemElement(5, Vector2.Zero, UI.ItemBackground, UI.Font, Color.White, Vector2.Zero);
        }
        public void FillItems(List<ItemObject> items)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].Tab == Tab)
                    AddItem(items[i]);
            }
        }
        public void ClearItems()
        {
            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].Item = null;
                UI.ELEMENTS_Remove(elements[i]);
                elements.Remove(elements[i]);

                i--;
            }
        }
        public int ElementCount() { return elements.Count; }

        public int ScrollIndex { get; set; }
        public Point ToGrid(int index)
        {
            return new Point(index % UI.GridWidth, (index / UI.GridHeight) + ScrollIndex);
        }
        public Vector2 ToPosition(Point grid)
        {
            return UI.TopLeft + new Vector2(65 + (grid.X * (UI.ItemBackground.Width + 4)), 72 + (grid.Y * (UI.ItemBackground.Height + 4)));
        }
        public Vector2 ToPosition(int index) { return ToPosition(ToGrid(index)); }

        public void ValidatePositions()
        {
            for (int i = 0; i < elements.Count; i++)
            {
                Point grid = ToGrid(i);
                elements[i].POSITION_SmoothStep(ToPosition(grid));
                elements[i].POSITION_Speed = 7.5f;
            }
        }
        public void ValidateIndex()
        {
            ScrollIndex = MathHelper.Clamp(ScrollIndex, Math.Min(-((elements.Count - 1) / UI.GridHeight) + (UI.GridHeight - 1), 0), 0);
        }

        public void SetInteraction(bool value)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                elements[i].IsInteract = value;
            }
        }
        public void ValidateElements()
        {
            for (int i = 0; i < elements.Count; i++)
                SetGridElement(elements[i], i);
        }
        private void SetGridElement(ItemElement element, int index)
        {
            if (IsInGrid(ToGrid(index).Y))
            {
                element.IsInteract = false;
                element.SCALE_SmoothStep(new Vector2(.85f));
                element.SCALE_Speed = 5f;
                element.Fade(Color.Lerp(Color.White, Color.Transparent, .5f), 5f);
            }
            else
            {
                element.IsInteract = true;
                element.SCALE_SmoothStep(Vector2.One);
                element.SCALE_Speed = 7.5f;
                element.Fade(Color.White, 7.5f);
            }
        }
        public bool IsInGrid(int gridY)
        {
            return gridY < 0 || gridY >= UI.GridHeight;
        }

        public void MoveIndex(int direction)
        {
            ScrollIndex += direction;

            ValidateIndex();
            ValidatePositions();
            ValidateElements();
        }
        public void GoToIndex(int index)
        {
            ScrollIndex = index;

            ValidateIndex();
            ValidatePositions();
            ValidateElements();
        }
        public void GoToBottom() { GoToIndex(-(elements.Count / UI.GridHeight)); }
        public void GoToTop() { GoToIndex(0); }
        public void GoPageUp() { GoToIndex(ScrollIndex + UI.GridHeight); }
        public void GoPageDown() { GoToIndex(ScrollIndex - UI.GridHeight); }

        public void Update()
        {
            LeftTab.SetHoverBoxCheck(new Point((int)((UI.TopLeft.X - LeftTab.Position.X) + 4), LeftTab.Texture.Height));
            UI.CLICKBOX_Position(Tab + "Box", LeftTab.Position.ToPoint());         
        }
        public void UpdateAlways()
        {
            /*
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].Item != null)
                {
                    if (elements[i].Item.IsDestroyed == true)
                    {
                        if (elements[i].IsOutro == false)
                        {
                            elements[i].IsOutro = true;
                            elements[i].IsInteract = false;

                            RemoveItem(elements[i]);
                        }
                    }
                }
                else
                {
                    RemoveItem(elements[i]);
                }
            }*/
        }
        public void Draw(SpriteBatch sb)
        {
            for (int i = 0; i < elements.Count; i++)
            {
                if (elements[i].IsRendered == true)
                    elements[i].Draw(sb);
            }
        }
    }
}
