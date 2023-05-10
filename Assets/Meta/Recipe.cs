using Merchantry.UI.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.Assets.Meta
{
    public class Recipe
    {
        //Meta
        public string Name { get; set; }
        public string UI { get; set; }
        public int CraftTime { get; set; } = 1000;
        //public bool IsDiscovered { get; set; }

        public Ingredient[] Input { get; set; }
        public ItemObject[] Output { get; set; }

        public bool IsOutputEquals(params ItemObject[] items)
        {
            int count = 0;
            for (int i = 0; i < items.Length; i++)
                if (items[i] != null)
                    count++;
            if (count == items.Length)
                return true;

            for (int i = 0; i < Math.Min(Output.Length, items.Length); i++)
            {
                //Compare to same index position. If not same stack, return false.
                if (Output[i].StackID() != items[i].StackID())
                    return false;
            }
            return true;
        }

        public void SetInput(params Ingredient[] input) { Input = input; }
        public void SetOutput(params ItemObject[] output) { Output = output; }

        public Recipe(string Name, Ingredient[] Input, params ItemObject[] Output)
        {
            this.Name = Name;

            this.Input = Input;
            this.Output = Output;
        }
    }
}
