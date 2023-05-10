using Merchantry.UI.Items;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.Assets
{
    public class EnchantAssets
    {
        public Dictionary<string, ItemEnchant> Enchantments { get; private set; } = new Dictionary<string, ItemEnchant>(StringComparer.OrdinalIgnoreCase);

        public void Load(ContentManager cm)
        {
            Add("SlightlyDamaged", new ItemEnchant("Slightly Damaged", -15, ItemEnchant.Types.Negative));
            Add("Damaged", new ItemEnchant("Damaged", -30, ItemEnchant.Types.Negative));
            Add("VeryDamaged", new ItemEnchant("Very Damaged", -50, ItemEnchant.Types.Negative));
            Add("Broken", new ItemEnchant("Broken", -90, ItemEnchant.Types.Negative));

            Add("Cursed", new ItemEnchant("Cursed", -50, ItemEnchant.Types.Negative));
            Add("Dirty", new ItemEnchant("Dirty", -10, ItemEnchant.Types.Negative));

            Add("Sharpened", new ItemEnchant("Sharpened", 15, ItemEnchant.Types.Positive));
            Add("Durable", new ItemEnchant("Durable", 25, ItemEnchant.Types.Positive));
            Add("Flame", new ItemEnchant("Flame", 35, ItemEnchant.Types.Positive));
            Add("Ice", new ItemEnchant("Ice", 30, ItemEnchant.Types.Positive));
        }

        public void Add(string id, ItemEnchant enchant)
        {
            enchant.ID = id;
            Enchantments.Add(id, enchant);
        }
        public ItemEnchant Index(int index)
        {
            return Enchantments.Values.ToList()[index];
        }
    }
}
