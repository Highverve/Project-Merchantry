using Merchantry.UI.Developer.General;
using Merchantry.UI.Items;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Meta
{
    public class ObjectStorage : IProperties
    {
        public Dictionary<string, ItemObject> Items { get; private set; } = new Dictionary<string, ItemObject>();
        public WorldObject Object { get; set; }
        public int Currency()
        {
            if (ContainsItem("Currency"))
                return Items["Currency"].Quantity;
            return 0;
        }

        private event Action<ItemObject.ItemTabs, int> onNewMaximum;
        private event Action<ItemObject> onAddItem, onMergeItem, onNewStack, onRemoveItem, onTabMaximum;
        public event Action<ItemObject> OnAddItem { add { onAddItem += value; } remove { onAddItem -= value; } }
        public event Action<ItemObject> OnMergeItem { add { onMergeItem += value; } remove { onMergeItem -= value; } }
        public event Action<ItemObject> OnNewStack { add { onNewStack += value; } remove { onNewStack -= value; } }
        public event Action<ItemObject> OnRemoveItem { add { onRemoveItem += value; } remove { onRemoveItem -= value; } }
        public event Action<ItemObject> OnTabMaximum { add { onTabMaximum += value; } remove { onTabMaximum -= value; } }
        public event Action<ItemObject.ItemTabs, int> OnNewMaximum { add { onNewMaximum += value; } remove { onNewMaximum -= value; } }

        public int MaximumConsumables { get; private set; } = 5;
        public int MaximumTools { get; private set; } = 5;
        public int MaximumApparel { get; private set; } = 5;
        public int MaximumResources { get; private set; } = 5;
        public int MaximumFurniture { get; private set; } = 5;
        public int MaximumVarious { get; private set; } = 5;
        public void SetMaximum(ItemObject.ItemTabs tab, int value)
        {
            if (tab == ItemObject.ItemTabs.Consumables)
                MaximumConsumables = value;
            else if (tab == ItemObject.ItemTabs.Tools)
                MaximumTools = value;
            else if (tab == ItemObject.ItemTabs.Apparel)
                MaximumApparel = value;
            else if (tab == ItemObject.ItemTabs.Resources)
                MaximumResources = value;
            else if (tab == ItemObject.ItemTabs.Furniture)
                MaximumFurniture = value;
            else if (tab == ItemObject.ItemTabs.Various)
                MaximumVarious = value;

            onNewMaximum?.Invoke(tab, value);
        }
        public int TabCount(ItemObject.ItemTabs tab)
        {
            return Items.Count((o) => o.Value.Tab == tab);
        }
        public int MaxTabCount(ItemObject.ItemTabs tab)
        {
            if (tab == ItemObject.ItemTabs.Consumables)
                return MaximumConsumables;
            else if (tab == ItemObject.ItemTabs.Tools)
                return MaximumTools;
            else if (tab == ItemObject.ItemTabs.Apparel)
                return MaximumApparel;
            else if (tab == ItemObject.ItemTabs.Resources)
                return MaximumResources;
            else if (tab == ItemObject.ItemTabs.Furniture)
                return MaximumFurniture;
            else if (tab == ItemObject.ItemTabs.Various)
                return MaximumVarious;
            return 0;
        }
        public bool IsTabMaximum(ItemObject.ItemTabs tab)
        {
            return TabCount(tab) >= MaxTabCount(tab);
        }

        public ObjectStorage() { }

        public void Update(GameTime gt)
        {
            foreach (KeyValuePair<string, ItemObject> item in Items.ToArray())
            {
                item.Value.Update(gt);

                if (item.Value.IsDestroyed == true)
                {
                    Items.Remove(item.Key);
                    onRemoveItem?.Invoke(item.Value);
                }
            }
        }

        public void AddItem(string id) { AddItem(id, 1); }
        public void AddItem(string id, int quantity)
        {
            ItemObject item = GameManager.References().Assets.Items.Copy(id);
            item.Quantity = quantity;

            AddItem(item);
        }
        public void AddItem(ItemObject item)
        {
            if (item != null)
            {
                if (item.IsDestroyed == true)
                    item.IsDestroyed = false;

                string key = item.StackID();
                if (Items.ContainsKey(key))
                {
                    Items[key].Merge(item);
                    onMergeItem?.Invoke(item);
                    onAddItem?.Invoke(item);
                }
                else if (IsTabMaximum(item.Tab) == false)
                {
                    Items.Add(key, item);
                    onNewStack?.Invoke(item);
                    onAddItem?.Invoke(item);
                }
                else
                    onTabMaximum?.Invoke(item);
            }
            else
                throw new NullReferenceException("Parameter 'item' cannot be null.");
        }

        public ItemObject First(string id) { return All(id).FirstOrDefault(); }
        public ItemObject Last(string id) { return All(id).LastOrDefault(); }

        public List<ItemObject> All(string id)
        {
            List<ItemObject> result = new List<ItemObject>();

            foreach (ItemObject item in Items.Values)
            {
                if (item.ID == id)
                    result.Add(item);
            }

            return result;
        }
        public List<ItemObject> WithQuality(string id, ItemObject.ItemQuality quality)
        {
            List<ItemObject> result = All(id);
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i].Quality != quality)
                {
                    result.Remove(result[i]);
                    i--;
                }
            }

            return result;
        }
        public List<ItemObject> WithEnchant(string id, ItemObject.ItemQuality quality)
        {
            List<ItemObject> result = All(id);
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i].Quality != quality)
                {
                    result.Remove(result[i]);
                    i--;
                }
            }

            return result;
        }

        /// <summary>
        /// Removes an entire stack of an item by StackID().
        /// </summary>
        /// <param name="item"></param>
        public void RemoveItem(ItemObject item)
        {
            string key = item.StackID();
            if (Items.ContainsKey(key))
                item.Destroy();
        }
        /// <summary>
        /// Removes a specific quantity from an item by StackID().
        /// </summary>
        /// <param name="item"></param>
        /// <param name="quantity"></param>
        public void RemoveItem(ItemObject item, int quantity)
        {
            string key = item.StackID();
            if (Items.ContainsKey(key))
            {
                Items[key].Quantity -= quantity;
                if (Items[key].Quantity == 0)
                    Items.Remove(key);
                onRemoveItem?.Invoke(item);
            }
        }
        /// <summary>
        /// Removes all stacks of an item by ID.
        /// </summary>
        /// <param name="id"></param>
        public void RemoveItem(string id)
        {
            foreach (KeyValuePair<string, ItemObject> item in Items)
            {
                if (item.Value.ID == id)
                    item.Value.Destroy();
            }
        }
        /// <summary>
        /// Removes from stacks until the quantity is used up.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="quantity"></param>
        public void RemoveItem(string id, int quantity)
        {
            foreach (KeyValuePair<string, ItemObject> item in Items)
            {
                if (item.Value.ID == id)
                {
                    int difference = Math.Max(0, quantity - item.Value.Quantity);
                    item.Value.Quantity -= quantity;
                    quantity -= difference;

                    if (quantity <= 0)
                        break;
                }
            }
        }
        //Iterates through the item list, removing all items after invoking events.
        public void Clear()
        {
            List<ItemObject> items = Items.Values.ToList();
            for (int i = 0; i < items.Count; i++)
                RemoveItem(items[i]);
        }

        public bool ContainsItem(ItemObject item)
        {
            return Items.ContainsKey(item.StackID());
        }
        public bool ContainsItem(string name)
        {
            return Items.ContainsKey(name);
        }
        public bool ContainsID(string id)
        {
            foreach (KeyValuePair<string, ItemObject> item in Items)
            {
                if (item.Value.ID == id)
                    return true;
            }
            return false;
        }

        public List<ItemObject> GetTab(ItemObject.ItemTabs tab)
        {
            return Items.Values.Where(i => i.Tab == tab).ToList();
        }

        private AetaLibrary.Elements.Text.TextElement itemLabel;
        private UI.Elements.NumberElement currencyNumber;
        public void SetProperties(PropertiesUI ui)
        {
            ui.PROPERTY_AddHeader(Object.ID + "'s Storage");
            itemLabel = ui.PROPERTY_AddLabelButton("Items: " + Items.Count, Clear);
            currencyNumber = ui.PROPERTY_AddNumber("Currency: ", Currency(), null, () => AddItem("Currency", 1), () => RemoveItem("Currency", 1));

            ui.PROPERTY_AddDivider(10);
            ui.PROPERTY_AddSpacer(10);

            ui.PROPERTY_AddHeader("Items");
            foreach (ItemObject item in Items.Values)
            {
                AetaLibrary.Elements.Text.TextElement itemText = null;
                itemText = ui.PROPERTY_AddLabelButton(item.StackID(), () =>
                {
                    if (ContainsItem(item))
                        ui.SetSelected(item);
                }, () =>
                {
                    if (ContainsItem(item))
                    {
                        RemoveItem(item);
                        itemText.Fade(ExtensionsLibrary.Extensions.ColorExt.Raspberry, 15);
                    }
                    else
                    {
                        AddItem(item);
                        itemText.Fade(Color.Gray, 15);
                    }
                });
                itemText.Fade(Color.Gray, 50);
            }
        }
        public void RefreshProperties()
        {
            itemLabel.Text = "Items: " + Items.Count;
            currencyNumber.Number = Currency();
        }
        public void NullifyProperties()
        {
            itemLabel = null;
            currencyNumber = null;
        }
    }
}
