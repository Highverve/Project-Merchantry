using Merchantry.UI.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.Assets.Meta
{
    public class Ingredient
    {
        public string SlotName { get; set; }
        public Func<ItemObject, bool> Requirement { get; set; }
        public Action<ItemObject> Crafted { get; set; }

        public Ingredient(string SlotName, Func<ItemObject, bool> Requirement, Action<ItemObject> Crafted)
        {
            this.SlotName = SlotName;
            this.Requirement = Requirement;
            this.Crafted = Crafted;
        }
    }
}
