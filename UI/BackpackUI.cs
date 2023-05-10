using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Merchantry.UI.Items;
using Merchantry.World.Meta;
using Microsoft.Xna.Framework.Content;
using AetaLibrary.Elements;
using AetaLibrary.Elements.Images;
using Microsoft.Xna.Framework.Input;
using AetaLibrary.Elements.Text;

namespace Merchantry.UI
{
    public class BackpackUI : BaseUI
    {
        private Texture2D itemBackground, itemBackgroundHover, wellBlack, backpackTabs;
        private StretchBoxElement tabBackground, moneyBackground;
        private ImageElement money;
        private TextElement moneyText, buttonOne, buttonTwo, buttonThree;
        private ImageElement buttonOneShortcut, buttonTwoShortcut, buttonThreeShortcut;

        public ObjectStorage Storage { get; private set; }
        public void SetStorage(ObjectStorage storage)
        {
            if (Storage != null)
                RemoveEvents();
            Storage = storage;

            AddEmptySlots();
            AddEvents();
            FillTabs();

            RefreshPositions(true);
            RefreshButtons(Tab.Selected());
        }

        public BackpackTab Tab { get; private set; }
        private Dictionary<ItemObject.ItemTabs, BackpackTab> Tabs = new Dictionary<ItemObject.ItemTabs, BackpackTab>();
        private Dictionary<int, ItemObject.ItemTabs> Ordered = new Dictionary<int, ItemObject.ItemTabs>();
        private void AddTabs()
        {
            AddTab(0, ItemObject.ItemTabs.Consumables);
            AddTab(1, ItemObject.ItemTabs.Tools);
            AddTab(2, ItemObject.ItemTabs.Apparel);
            AddTab(3, ItemObject.ItemTabs.Resources);
            AddTab(4, ItemObject.ItemTabs.Furniture);
            AddTab(5, ItemObject.ItemTabs.Various);

            SetTab(ItemObject.ItemTabs.Resources);
        }
        private void AddTab(int order, ItemObject.ItemTabs tab)
        {
            Tabs.Add(tab, new BackpackTab(order, this) { Tab = tab });
            Ordered.Add(order, tab);

            ELEMENTS_Add(tab + "_Icon", Tabs[tab].Icon = new ImageElement(1, TabIn(), backpackTabs, new Rectangle(order * 64, 0, 64, 64), Color.White,
                Vector2.Zero, Vector2.One, 0, SpriteEffects.None) { TargetRender = "EXTERNAL", POSITION_Speed = 15, SCALE_Speed = 10 });
            Tabs[tab].Icon.Origin = new Vector2(32, 32);
            Tabs[tab].OnSelect += (old, slot) =>
            {
                if (old != null)
                {
                    old.Background = itemBackground;
                    old.Buttons.InvokeHoverExit();
                }

                slot.Background = itemBackgroundHover;
                slot.SCALE_Loose(1.33f);
                slot.RenderOrder = 2;
                SetControlledElement(slot);

                RefreshButtons(slot);
            };
        }
        private void RefreshButtons(ItemElement slot)
        {
            if (slot.Item != null)
            {
                float multi = .25f;
                if (slot.Item.ButtonCount() == 1) multi = .5f;
                if (slot.Item.ButtonCount() == 2) multi = .33f;
                if (slot.Item.ButtonCount() == 3) multi = .25f;

                float count = multi;
                float oneX = 0, twoX = 0, threeX = 0;
                if (string.IsNullOrEmpty(slot.Item.ButtonOneText) == false)
                {
                    oneX = count;
                    count += multi;
                }
                if (string.IsNullOrEmpty(slot.Item.ButtonTwoText) == false)
                {
                    twoX = count;
                    count += multi;
                }
                if (string.IsNullOrEmpty(slot.Item.ButtonThreeText) == false)
                    threeX = count;

                SetButton(slot, buttonOne, buttonOneShortcut, slot.Item?.ButtonOneText, (obj) => slot.Item?.ButtonOne(obj), oneX);
                SetButton(slot, buttonTwo, buttonTwoShortcut, slot.Item?.ButtonTwoText, (obj) => slot.Item?.ButtonTwo(obj), twoX);
                SetButton(slot, buttonThree, buttonThreeShortcut, slot.Item?.ButtonThreeText, (obj) => slot.Item?.ButtonThree(obj), threeX);
            }
            else
            {
                buttonOne.SCALE_Loose(0);
                buttonTwo.SCALE_Loose(0);
                buttonThree.SCALE_Loose(0);

                buttonOneShortcut.SCALE_Loose(0);
                buttonTwoShortcut.SCALE_Loose(0);
                buttonThreeShortcut.SCALE_Loose(0);
            }
        }
        private void SetButton(ItemElement slot, TextElement text, ImageElement shortcut, string buttonText, Action<World.WorldObject> action, float offset)
        {
            text.Buttons.ClearLeftClick();
            if (string.IsNullOrEmpty(buttonText) == false)
            {
                text.Origin = text.Font.MeasureString(buttonText) / 2;
                text.Text = buttonText;
                text.Buttons.OnLeftClick += () => action(References.World.Controlled);

                text.SCALE_Loose(1);
                text.POSITION_Loose(TopLeft + new Vector2(Size.X * offset, -28));
                offset *= 2;

                shortcut.SCALE_Loose(1);
            }
            else
            {
                text.SCALE_Loose(0);
                shortcut.SCALE_Loose(0);
            }
        }

        private void AddEmptySlots()
        {
            AddSlot(ItemObject.ItemTabs.Consumables);
            AddSlot(ItemObject.ItemTabs.Tools);
            AddSlot(ItemObject.ItemTabs.Apparel);
            AddSlot(ItemObject.ItemTabs.Resources);
            AddSlot(ItemObject.ItemTabs.Furniture);
            AddSlot(ItemObject.ItemTabs.Various);
        }
        private void AddSlot(ItemObject.ItemTabs tab)
        {
            for (int i = 0; i < Storage.MaxTabCount(tab); i++)
            {
                ItemElement element = CreateSlot();
                ELEMENTS_Add(tab.ToString().ToUpper() + i, element);
                Tabs[tab].Slots.Add(element);
            }
            Tabs[tab].ScrollIndex = 4;
        }
        private ItemElement CreateSlot()
        {
            ItemElement element = new ItemElement(1, Vector2.Zero, itemBackground, font, Color.White, Vector2.One);

            element.IsInteract = false;
            element.SCALE_Speed = 10;
            element.POSITION_Speed = 10;
            element.Buttons.OnHoverEnter += () =>
            {
                if (Tab.Selected() != element)
                {
                    element.SCALE_Loose(1.3f);
                    element.RenderOrder = 3;
                }

                if (element.Item != null)
                    Screens.Tooltip.LAYOUT_SetText(element.Item.Name);
            };
            element.Buttons.OnHoverExit += () =>
            {
                if (Tab.Selected() != element)
                {
                    element.SCALE_Loose(1f);
                    element.RenderOrder = 1;
                }

                Screens.Tooltip.Minimize();
            };

            return element;
        }

        private void FillTabs()
        {
            FillSlots(ItemObject.ItemTabs.Consumables);
            FillSlots(ItemObject.ItemTabs.Tools);
            FillSlots(ItemObject.ItemTabs.Apparel);
            FillSlots(ItemObject.ItemTabs.Resources);
            FillSlots(ItemObject.ItemTabs.Furniture);
            FillSlots(ItemObject.ItemTabs.Various);
        }
        private void FillSlots(ItemObject.ItemTabs tab)
        {
            List<ItemObject> data = Storage.GetTab(tab);
            for (int i = 0; i < data.Count; i++)
                Tabs[tab].Slots[i].Item = data[i];
        }

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
            Tabs[item.Tab].NextOpen().Item = item;
        }
        private void RemoveTabItem(ItemObject item)
        {
            Tabs[item.Tab].RemoveItem(item);
        }

        private int tabIndex;
        public int TabIndex
        {
            get { return tabIndex; }
            private set
            {
                tabIndex = MathHelper.Clamp(value, -Tabs.Count + 1, 0);
                RefreshPositions();
            }
        }
        public void ScrollTabs(int direction)
        {
            TabIndex += direction;
            SetTab(Ordered[-TabIndex]);

            RefreshButtons(Tab.Selected());
        }
        public void RefreshPositions(bool isForcing = false)
        {
            foreach (BackpackTab tab in Tabs.Values)
                tab.RefreshPositions(isForcing);
        }
        public Vector2 FromIndex(int index, int order)
        {
            return TopLeft + new Vector2((77 * index) + 65, (100 * order) + 56);
        }
        private Vector2 TabOut() { return TopLeft + new Vector2(-48, Size.Y / 2); }
        private Vector2 TabIn() { return TopLeft + new Vector2(64, Size.Y / 2); }

        public BackpackUI(GraphicsDevice Graphics, Vector2 Size) : base(Graphics, Size)
        {
        }

        public override void Load(ContentManager cm)
        {
            base.Load(cm);

            itemBackground = cm.Load<Texture2D>("UI/Elements/itemBackground");
            itemBackgroundHover = cm.Load<Texture2D>("UI/Elements/itemBackgroundHover");
            wellBlack = cm.Load<Texture2D>("UI/Elements/wellBlack");
            backpackTabs = cm.Load<Texture2D>("UI/Icons/backpackTabs");

            ELEMENTS_Add("TabBackground", tabBackground = new StretchBoxElement(0, TabOut() - new Vector2(48, 0), wellBlack, Color.White, new Point(128, 64), 16));
            tabBackground.SetOrigin(0, .5f);
            tabBackground.TargetRender = "EXTERNAL";

            ELEMENTS_Add("TabBackground", moneyBackground = new StretchBoxElement(0, new Vector2(BottomRight.X -32, TopLeft.Y + Size.Y / 2), wellBlack, Color.White, new Point(128, 64), 16));
            moneyBackground.SetOrigin(0, .5f);
            moneyBackground.TargetRender = "EXTERNAL";
            Texture2D currencyIcon = cm.Load<Texture2D>("UI/Icons/currencyIcon");
            ELEMENTS_Add("Currency", money = new ImageElement(0, moneyBackground.Position + new Vector2(32, 0), currencyIcon,
                currencyIcon.Bounds, Color.White, Vector2.Zero, Vector2.Zero, 0, SpriteEffects.None));
            money.SetOrigin(.5f, .5f);
            money.TargetRender = "EXTERNAL";

            ELEMENTS_Add("CurrencyText", moneyText = new TextElement(1, moneyBackground.Position + new Vector2(56, 0), font,
                "$0", Color.Lerp(Color.Gold, Color.White, .25f), Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            moneyText.SetOrigin(0, .5f);
            moneyText.TargetRender = "EXTERNAL";

            ELEMENTS_Add("ButtonOne", buttonOne = new TextElement(1, TopLeft + new Vector2(Size.X * .25f, -24), font,
                "empty", Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            buttonOne.TargetRender = "EXTERNAL";
            buttonOne.SCALE_Speed = 10;
            buttonOne.POSITION_Speed = 10;
            buttonOne.Buttons.OnHoverEnter += () => buttonOne.Fade(Color.Gray, 10);
            buttonOne.Buttons.OnHoverExit += () => buttonOne.Fade(Color.White, 10);

            ELEMENTS_Add("ButtonTwo", buttonTwo = new TextElement(1, TopLeft + new Vector2(Size.X * .5f, -24), font,
                string.Empty, Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            buttonTwo.TargetRender = "EXTERNAL";
            buttonTwo.SCALE_Speed = 10;
            buttonTwo.POSITION_Speed = 10;
            buttonTwo.Buttons.OnHoverEnter += () => buttonTwo.Fade(Color.Gray, 10);
            buttonTwo.Buttons.OnHoverExit += () => buttonTwo.Fade(Color.White, 10);

            ELEMENTS_Add("ButtonThree", buttonThree = new TextElement(1, TopLeft + new Vector2(Size.X * .75f, -24), font,
                string.Empty, Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            buttonThree.TargetRender = "EXTERNAL";
            buttonThree.SCALE_Speed = 10;
            buttonThree.POSITION_Speed = 10;
            buttonThree.Buttons.OnHoverEnter += () => buttonThree.Fade(Color.Gray, 10);
            buttonThree.Buttons.OnHoverExit += () => buttonThree.Fade(Color.White, 10);

            ELEMENTS_Add("ButtonOneShortcut", buttonOneShortcut = CreateShortcut(Symbols.Gamepad.Source("SquareReleased"), buttonOne));
            ELEMENTS_Add("ButtonTwoShortcut", buttonTwoShortcut = CreateShortcut(Symbols.Gamepad.Source("CircleReleased"), buttonTwo));
            ELEMENTS_Add("ButtonThreeShortcut", buttonThreeShortcut = CreateShortcut(Symbols.Gamepad.Source("TriangleReleased"), buttonThree));

            AddTabs();

            /*ELEMENTS_Add("CurrencyText", buttonTwo = new TextElement(1, moneyBackground.Position + new Vector2(56, 0), font,
                "$0", Color.Lerp(Color.Gold, Color.White, .25f), Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            ELEMENTS_Add("CurrencyText", buttonThree = new TextElement(1, moneyBackground.Position + new Vector2(56, 0), font,
                "$0", Color.Lerp(Color.Gold, Color.White, .25f), Vector2.Zero, Vector2.One, 0, SpriteEffects.None));*/

        }

        private ImageElement CreateShortcut(Rectangle source, TextElement button)
        {
            ImageElement shortcut = new ImageElement(1, Vector2.Zero, Symbols.Gamepad.Texture, source,
                Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None);

            shortcut.SetOrigin(.5f, .5f);
            shortcut.SnapMargins = Vector2.Zero;
            shortcut.SCALE_Speed = 10;
            button.OnPositionMove += () => button.SnapExterior(Element.SnapLocation.Left, shortcut);
            button.OnTextChange += () => button.SnapExterior(Element.SnapLocation.Left, shortcut);
            shortcut.TargetRender = "EXTERNAL";

            return shortcut;
        }
        public override void PostInitialize()
        {
            base.PostInitialize();

            IsPriority = true;
            IsTitlebarEnabled = false;
            UI_SetPosition(new Vector2(0, (Resolution.Y / 2) - 75), true);
            UI_SetOrigin(Center);
            Maximize();
        }

        public override void Update(GameTime gt)
        {
            base.Update(gt);

            if (IsMaximized == true)
            {
                if (Storage != null)
                {
                    moneyText.Text = "$" + Storage.Currency();
                    moneyText.SetOrigin(0, .5f);
                }
                else
                {
                    moneyText.Text = "$0";
                    moneyText.SetOrigin(0, .5f);
                }

                //Mouse navigation
                if (Controls.IsKeyDown(Keys.LeftControl))
                {
                    if (Controls.ScrollDirection() > 0)
                        ScrollTabs(1);
                    if (Controls.ScrollDirection() < 0)
                        ScrollTabs(-1);
                }
                else
                {
                    if (Controls.ScrollDirection() > 0)
                        Tab.Scroll(1);
                    if (Controls.ScrollDirection() < 0)
                        Tab.Scroll(-1);
                }

                //Keyboard navigation
                if (Controls.IsKeyPressed(Keys.Up) || Controls.IsKeyPressed(Keys.W))
                    ScrollTabs(1);
                if (Controls.IsKeyPressed(Keys.Down) || Controls.IsKeyPressed(Keys.S))
                    ScrollTabs(-1);
                if (Controls.IsKeyPressed(Keys.Left) || Controls.IsKeyPressed(Keys.A))
                    Tab.Scroll(1);
                if (Controls.IsKeyPressed(Keys.Right) || Controls.IsKeyPressed(Keys.D))
                    Tab.Scroll(-1);

                //Keyboard shortcuts
                if (Controls.IsKeyPressed(Keys.Home))
                    Tab.ScrollIndex = 4;
                if (Controls.IsKeyPressed(Keys.End))
                    Tab.ScrollIndex = -Tab.Slots.Count;
            }

            if (Controls.IsKeyPressed(Keys.E) || Controls.IsMouseClicked(Controls.CurrentMS.MiddleButton))
            {
                //Not holding, item in slot.
                if (Screens.Drag.IsHolding() == false && Tab.Selected().Item != null && IsMaximized == true)
                {
                    Tab.Selected().Item.ButtonGrab(GameManager.References().World.Controlled);
                    Screens.Drag.AddItem(Tab.Selected().Item);
                }
                //Holding, no item in slot.
                else if (Screens.Drag.IsHolding() == true)
                {
                    ItemObject item = Screens.Drag.TakeItem();
                    ItemElement openSlot = Tab.NextOpen();
                    Storage.AddItem(item);

                    //Move stored item from first open slot to selected index,
                    //but only if same tab and selected tab is not full.
                    if (item.Tab == Tab.Tab && Storage.IsTabMaximum(item.Tab) == false)
                        Tab.SwapItems(Tab.Selected(), openSlot);
                }
            }
        }
        public override void UpdateAlways(GameTime gt)
        {
            if (Controls.IsKeyPressed(Keys.Tab))
            {
                if (IsMaximized == false)
                    Maximize();
                else
                    Minimize();
            }

            base.UpdateAlways(gt);
        }

        protected override void Draw(SpriteBatch sb)
        {
            sb.Begin(Sorting, Blending, Sampler, null, null, null, Camera.View());

            moneyBackground.Draw(sb);
            money.Draw(sb);
            moneyText.Draw(sb);

            buttonOne.Draw(sb);
            buttonTwo.Draw(sb);
            buttonThree.Draw(sb);

            buttonOneShortcut.Draw(sb);
            buttonTwoShortcut.Draw(sb);
            buttonThreeShortcut.Draw(sb);

            tabBackground.Draw(sb);
            foreach (BackpackTab tab in Tabs.Values)
                tab.Icon.Draw(sb);

            sb.End();

            base.Draw(sb);
        }
        protected override void DrawInterior(SpriteBatch sb)
        {
            DrawElements(sb, Element.DefaultTarget);
        }

        public void SetTab(ItemObject.ItemTabs tab)
        {
            //Prevent unnecessary method calls.
            if (Tab != null && tab == Tab.Tab)
                return;

            //Transition out previous tab's icon and set to non-interactable.
            if (Tab != null)
            {
                Tab.Icon.POSITION_Loose(TabIn());
                Tab.Icon.SCALE_Loose(.5f);

                Tab.SetInteract(false);
            }

            //Set tab, order, and icon transition
            Tab = Tabs[tab];
            TabIndex = -Tab.Order;
            Tab.SetInteract(true);
            Tab.Icon.POSITION_Loose(TabOut());
            Tab.Icon.SCALE_Loose(1.5f);
        }
        public void ScrollToItem(ItemObject item)
        {
            SetTab(item.Tab);
            Tab.ScrollToItem(item);   
        }
    }
    public class BackpackTab
    {
        public BackpackUI Backpack { get; set; }
        public ItemObject.ItemTabs Tab { get; set; }
        public int Order { get; private set; }

        public ImageElement Icon { get; set; }
        public List<ItemElement> Slots { get; private set; } = new List<ItemElement>();

        private event Action<ItemElement, ItemElement> onSelect;
        public event Action<ItemElement, ItemElement> OnSelect { add { onSelect += value; } remove { onSelect -= value; } }

        public void SetInteract(bool isInteract)
        {
            for (int i = 0; i < Slots.Count; i++)
                Slots[i].IsInteract = isInteract;
        }

        private int scrollIndex;
        public int ScrollIndex
        {
            get { return scrollIndex; }
            set
            {
                ItemElement old = Selected();
                scrollIndex = MathHelper.Clamp(value, -(Slots.Count - 5), 4);
                RefreshPositions();
                onSelect?.Invoke(old, Selected());
            }
        }
        public void RefreshPositions(bool isForcing = false)
        {
            for (int i = 0; i < Slots.Count; i++)
            {
                if (isForcing == false)
                    Slots[i].POSITION_Loose(Backpack.FromIndex(i + ScrollIndex, Order + Backpack.TabIndex));
                else
                    Slots[i].Position = Backpack.FromIndex(i + ScrollIndex, Order + Backpack.TabIndex);
            }
        }
        public void Scroll(int direction)
        {
            ScrollIndex += direction;
            RefreshPositions();
        }
        public void ScrollToItem(ItemObject item)
        {
            ItemElement element = FindSlot(item);
            if (element != null)
                ScrollIndex = -(Slots.IndexOf(element)) + 4;
        }
        public int SlotIndex() { return -scrollIndex + 4; }
        public ItemElement Selected()
        {
            if (Slots.Count > 0)
                return Slots[SlotIndex()];
            else
                return null;
        }

        public BackpackTab(int Order, BackpackUI Backpack)
        {
            this.Order = Order;
            this.Backpack = Backpack;
        }

        public ItemElement FindSlot(ItemObject item)
        {
            return Slots.First((slot) => slot.Item == item);
        }
        public ItemElement NextOpen()
        {
            for (int i = 0; i < Slots.Count; i++)
            {
                if (Slots[i].Item == null)
                    return Slots[i];
            }
            return null;
        }
        public void SwapItems(ItemElement slot1, ItemElement slot2)
        {
            ItemObject item = slot2.Item;
            slot2.Item = slot1.Item;
            slot1.Item = item;
        }
        public void RemoveItem(ItemObject item)
        {
            ItemElement element = Slots.FirstOrDefault(s => s.Item == item);
            element.Item = null;
        }
        public void ClearItems()
        {
            for (int i = 0; i < Slots.Count; i++)
                Slots[i].Item = null;
        }
    }
}
