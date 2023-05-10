using Merchantry.UI.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.Assets
{
    public class ItemAssets
    {
        public Dictionary<string, ItemObject> Items { get; private set; }
        private References references;

        public ItemAssets()
        {
            Items = new Dictionary<string, ItemObject>(StringComparer.OrdinalIgnoreCase);
            references = GameManager.References();
        }

        public void Load(ContentManager cm)
        {
            //AddItem(new ItemObject("", "", cm.Load<Texture2D>("Assets/Items/"), ""));
            AddItem(new ItemObject("Currency", "Monarch Token", cm.Load<Texture2D>("Assets/Items/currency"), "Currency for buying and selling\nthroughout the kingdoms.", 1, 999999, ItemObject.ItemTabs.Various));
            Items["Currency"].Subtitle = "Currency";
            Items["Currency"].SubtitleColor = Color.Lerp(Color.Gold, Color.White, .25f);

            AddItem(new ItemObject("Stick", "Stick", cm.Load<Texture2D>("Assets/Items/stick"), "A small branch of common wood.", 1, 99, ItemObject.ItemTabs.Resources));
            Items["Stick"].ButtonOneText = null;
            AddItem(new ItemObject("Stone", "Stone", cm.Load<Texture2D>("Assets/Items/stone"), "A crudely-formed stone.", 2, 99, ItemObject.ItemTabs.Resources));
            AddItem(new ItemObject("String", "String", cm.Load<Texture2D>("Assets/Items/string"), "A cord made from springflower fibers.", 3, 99, ItemObject.ItemTabs.Resources));
            AddItem(new ItemObject("Feather", "Feather", cm.Load<Texture2D>("Assets/Items/feather"), "A feather used for making arrows.", 3, 99, ItemObject.ItemTabs.Resources));
            AddItem(new ItemObject("IronOre", "Iron Ore", cm.Load<Texture2D>("Assets/Items/ironOre"), "An unrefined chunk of iron. Had you a\nfurnace, this could be smelted.", 6, 99, ItemObject.ItemTabs.Resources));
            AddItem(new ItemObject("IronIngot", "Iron Ingot", cm.Load<Texture2D>("Assets/Items/ironIngot"), "Iron ore which has been smelted into an ingot.", 10, 99, ItemObject.ItemTabs.Resources));

            AddItem(new ItemObject("StonePickaxe", "Stone Pickaxe", cm.Load<Texture2D>("Assets/Items/stonePickaxe"), "A primitive tool made from stone and wood.", 12, 1, ItemObject.ItemTabs.Tools));
            AddItem(new ItemObject("StoneAxe", "Stone Axe", cm.Load<Texture2D>("Assets/Items/stoneAxe"), "A primitive tool made from stone and wood.", 12, 1, ItemObject.ItemTabs.Tools));
            AddItem(new ItemObject("StoneHammer", "Stone Hammer", cm.Load<Texture2D>("Assets/Items/stoneHammer"), "A primitive tool made from stone and wood.", 15, 1, ItemObject.ItemTabs.Tools));
            AddItem(new ItemObject("StoneSpade", "Stone Spade", cm.Load<Texture2D>("Assets/Items/stoneSpade"), "A primitive tool made from stone and wood.", 7, 1, ItemObject.ItemTabs.Tools));
            AddItem(new ItemObject("StoneCudgel", "Stone Cudgel", cm.Load<Texture2D>("Assets/Items/stoneClub"), "A primitive weapon made from stone and wood.", 10, 1, ItemObject.ItemTabs.Tools));
            AddItem(new ItemObject("WoodenBow", "Wooden Bow", cm.Load<Texture2D>("Assets/Items/woodenBow"), "A carved bow for hunting.", 18, 1, ItemObject.ItemTabs.Tools));
            AddItem(new ItemObject("StoneArrow", "Stone Arrow", cm.Load<Texture2D>("Assets/Items/stoneArrow"), "A primitive arrow made from stone and wood.", 2, 99, ItemObject.ItemTabs.Tools));

            AddItem(new ItemObject("IronAxe", "Iron Axe", cm.Load<Texture2D>("Assets/Items/ironAxe"), "A sharpened tool wielded by woodsmen.", 45, 1, ItemObject.ItemTabs.Tools));
            AddItem(new ItemObject("IronPickaxe", "Iron Pickaxe", cm.Load<Texture2D>("Assets/Items/ironPickaxe"), "A reliable tool valued by miners.", 45, 1, ItemObject.ItemTabs.Tools));
            AddItem(new ItemObject("IronHammer", "Iron Hammer", cm.Load<Texture2D>("Assets/Items/ironHammer"), "A durable tool used by builders.", 55, 1, ItemObject.ItemTabs.Tools));
            AddItem(new ItemObject("IronSpade", "Iron Spade", cm.Load<Texture2D>("Assets/Items/ironSpade"), "A well-crafted tool intended for digging.", 20, 1, ItemObject.ItemTabs.Tools));

            //AddItem(new ItemObject("IronShield", "Iron Shield", cm.Load<Texture2D>("Assets/Items/ironShield"), "", 1, ItemObject.ItemTabs.Tools));
            AddItem(new ItemObject("IronBow", "Iron Bow", cm.Load<Texture2D>("Assets/Items/ironBow"), "A wooden bow reinforced with iron.", 30, 1, ItemObject.ItemTabs.Tools));
            AddItem(new ItemObject("IronLongsword", "Iron Longsword", cm.Load<Texture2D>("Assets/Items/ironLongsword"), "A double-edged longsword for killing.", 30, 1, ItemObject.ItemTabs.Tools));
            AddItem(new ItemObject("IronDagger", "Iron Dagger", cm.Load<Texture2D>("Assets/Items/ironDagger"), "A wooden bow, reinforced with iron.", 18, 1, ItemObject.ItemTabs.Tools));
            AddItem(new ItemObject("IronArrow", "Iron Arrow", cm.Load<Texture2D>("Assets/Items/ironArrow"), "A durable arrow made from iron and wood.", 4, 99, ItemObject.ItemTabs.Tools));

            AddItem(new ItemObject("GorgerChow", "Common Bait", cm.Load<Texture2D>("Assets/Items/commonBait"), "Food for animals and fish.", 5, 99, ItemObject.ItemTabs.Consumables));
            Items["GorgerChow"].Subtitle = "Food";
            Items["GorgerChow"].ButtonOneText = "Eat";
            Items["GorgerChow"].OnButtonOne += (i, u) =>
            {
                if (i.Quantity > 0)
                {
                    references.Screens.Context.Minimize();

                    if (u.Memory.Contains("HasEaten", "GorgerChow") == false)
                    {
                        references.Screens.Message.LAYOUT_SetMessage(new UI.MessageData("Common Bait", "Food", "You take the nibblets of bait out of your\nbackpack, staring at it for a moment.\n\nYou really don't need to eat this.\nYou probably shouldn't.", cm.Load<Texture2D>("UI/Icons/Note"),
                            new UI.Elements.ButtonOption("Eat Anyway", "", () =>
                            {
                                u.Memory.AddMemory(true, "HasEaten", "GorgerChow");
                                i.Quantity--;

                                references.Screens.Message.LAYOUT_SetMessage(new UI.MessageData("Common Bait", "Food", "With poor judgment, you consume the food.\nYou can barely stomach it.", cm.Load<Texture2D>("UI/Icons/Note"),
                                    new UI.Elements.ButtonOption("Continue", "", () =>
                                    {
                                        references.Screens.Message.Minimize();
                                    }, null)));
                            }, null),
                            new UI.Elements.ButtonOption("Put Away", "", () =>
                            {
                                references.Screens.Message.Minimize();
                            }, null)));
                    }
                    //else
                    //    message = new UI.MessageData("Gorger Chow", "Bait", "", cm.Load<Texture2D>("UI/Icons/Note"), new UI.Elements.ButtonOption("Eat Again", "", null, null), new UI.Elements.ButtonOption("Put Away", "", null, null));
                }
            };
            AddItem(new ItemObject("FishingRod", "Fishing Rod", cm.Load<Texture2D>("Assets/Items/fishingRod"), "An effective tool for catching fish.", 22, 1, ItemObject.ItemTabs.Tools));
            AddItem(new ItemObject("ChurchwardenPipe", "Churchwarden Pipe", cm.Load<Texture2D>("Assets/Items/churchwardenPipe"), "A smoking pipe for tobacco and herbs.", 10, 1, ItemObject.ItemTabs.Tools));
        }

        public void AddItem(ItemObject item)
        {
            if (Items.ContainsKey(item.ID))
                throw new Exception("Item with ID \"" + item.ID + "\" already exists.");
            else
            {
                Items.Add(item.ID, item);
            }
        }
        public ItemObject Copy(string id)
        {
            if (Items.ContainsKey(id))
                return Items[id].Copy();
            return null;
        }
        public ItemObject Copy(string id, Action<ItemObject> apply)
        {
            ItemObject copy = Copy(id);
            apply?.Invoke(copy);
            return copy;
        }
        public ItemObject CopyIndex(int index) { return Items.Values.ToList()[index].Copy(); }
    }
}
