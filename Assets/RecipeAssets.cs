using Merchantry.Assets.Meta;
using Merchantry.UI.Items;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.Assets
{
    public class RecipeAssets
    {
        private References references;
        private ItemAssets items;
        public Dictionary<string, Recipe> Crafting { get; private set; } = new Dictionary<string, Recipe>(StringComparer.OrdinalIgnoreCase);

        public void Load(ContentManager cm)
        {
            references = GameManager.References();
            items = references.Assets.Items;

            AddCrafting();
        }

        private void AddCrafting()
        {
            Crafting.Add("", new Recipe("", Fill(
                new Ingredient("1,1", (i) => { return CheckID(i, ""); }, (i) => { i.Quantity--; }),
                Empty("2,1"),
                Empty("3,1"),
                Empty("1,2"),
                Empty("2,2"),
                Empty("3,2"),
                Empty("1,3"),
                Empty("2,3"),
                Empty("3,3")),
                items.Copy("")));

            #region Stone Tools

            Crafting.Add("StonePickaxe", new Recipe("Stone Pickaxe", Fill(
                new Ingredient("1,1", (i) => { return CheckID(i, "Stone"); }, (i) => { i.Quantity--; }),
                new Ingredient("2,1", (i) => { return CheckID(i, "Stone"); }, (i) => { i.Quantity--; }),
                new Ingredient("3,1", (i) => { return CheckID(i, "Stone"); }, (i) => { i.Quantity--; }),
                Empty("1,2"),
                new Ingredient("2,2", (i) => { return CheckID(i, "Stick"); }, (i) => { i.Quantity--; }),
                Empty("3,2"),
                Empty("1,3"),
                new Ingredient("2,3", (i) => { return CheckID(i, "Stick"); }, (i) => { i.Quantity--; }),
                Empty("3,3")),
                items.Copy("StonePickaxe")) { CraftTime = 500 });
            Crafting.Add("StoneAxe", new Recipe("Stone Axe", Fill(
                new Ingredient("1,1", (i) => { return CheckID(i, "Stone"); }, (i) => { i.Quantity--; }),
                new Ingredient("2,1", (i) => { return CheckID(i, "Stone"); }, (i) => { i.Quantity--; }),
                Empty("3,1"),
                new Ingredient("1,2", (i) => { return CheckID(i, "Stone"); }, (i) => { i.Quantity--; }),
                new Ingredient("2,2", (i) => { return CheckID(i, "Stick"); }, (i) => { i.Quantity--; }),
                Empty("3,2"),
                Empty("1,3"),
                new Ingredient("2,3", (i) => { return CheckID(i, "Stick"); }, (i) => { i.Quantity--; }),
                Empty("3,3")),
                items.Copy("StoneAxe")) { CraftTime = 500 });
            Crafting.Add("StoneHammer", new Recipe("Stone Hammer", Fill(
                new Ingredient("1,1", (i) => { return CheckID(i, "Stone"); }, (i) => { i.Quantity--; }),
                new Ingredient("2,1", (i) => { return CheckID(i, "Stick"); }, (i) => { i.Quantity--; }),
                new Ingredient("3,1", (i) => { return CheckID(i, "Stone"); }, (i) => { i.Quantity--; }),
                new Ingredient("1,2", (i) => { return CheckID(i, "Stone"); }, (i) => { i.Quantity--; }),
                new Ingredient("2,2", (i) => { return CheckID(i, "Stick"); }, (i) => { i.Quantity--; }),
                new Ingredient("3,2", (i) => { return CheckID(i, "Stone"); }, (i) => { i.Quantity--; }),
                Empty("1,3"),
                new Ingredient("2,3", (i) => { return CheckID(i, "Stick"); }, (i) => { i.Quantity--; }),
                Empty("3,3")),
                items.Copy("StoneHammer")) { CraftTime = 500 });
            Crafting.Add("StoneSpade", new Recipe("Stone Spade", Fill(
                Empty("1,1"),
                new Ingredient("2,1", (i) => { return CheckID(i, "Stone"); }, (i) => { i.Quantity--; }),
                Empty("3,1"),
                Empty("1,2"),
                new Ingredient("2,2", (i) => { return CheckID(i, "Stick"); }, (i) => { i.Quantity--; }),
                Empty("3,2"),
                Empty("1,3"),
                new Ingredient("2,3", (i) => { return CheckID(i, "Stick"); }, (i) => { i.Quantity--; }),
                Empty("3,3")),
                items.Copy("StoneSpade")) { CraftTime = 500 });
            Crafting.Add("StoneCudgel", new Recipe("Stone Cudgel", Fill(
                new Ingredient("1,1", (i) => { return CheckID(i, "Stone"); }, (i) => { i.Quantity--; }),
                new Ingredient("2,1", (i) => { return CheckID(i, "Stick"); }, (i) => { i.Quantity--; }),
                Empty("3,1"),
                new Ingredient("1,2", (i) => { return CheckID(i, "Stone"); }, (i) => { i.Quantity--; }),
                new Ingredient("2,2", (i) => { return CheckID(i, "Stick"); }, (i) => { i.Quantity--; }),
                Empty("3,2"),
                Empty("1,3"),
                new Ingredient("2,3", (i) => { return CheckID(i, "Stick"); }, (i) => { i.Quantity--; }),
                Empty("3,3")),
                items.Copy("StoneCudgel")) { CraftTime = 500 });

            #endregion

            #region Iron Tools

            Crafting.Add("IronPickaxe", new Recipe("Iron Pickaxe", Fill(
                new Ingredient("1,1", (i) => { return CheckID(i, "IronIngot"); }, (i) => { i.Quantity--; }),
                new Ingredient("2,1", (i) => { return CheckID(i, "IronIngot"); }, (i) => { i.Quantity--; }),
                new Ingredient("3,1", (i) => { return CheckID(i, "IronIngot"); }, (i) => { i.Quantity--; }),
                Empty("1,2"),
                new Ingredient("2,2", (i) => { return CheckID(i, "Stick"); }, (i) => { i.Quantity--; }),
                Empty("3,2"),
                Empty("1,3"),
                new Ingredient("2,3", (i) => { return CheckID(i, "Stick"); }, (i) => { i.Quantity--; }),
                Empty("3,3")),
                items.Copy("IronPickaxe")) { CraftTime = 1000 });
            Crafting.Add("IronAxe", new Recipe("Iron Axe", Fill(
                new Ingredient("1,1", (i) => { return CheckID(i, "IronIngot"); }, (i) => { i.Quantity--; }),
                new Ingredient("2,1", (i) => { return CheckID(i, "IronIngot"); }, (i) => { i.Quantity--; }),
                Empty("3,1"),
                new Ingredient("1,2", (i) => { return CheckID(i, "IronIngot"); }, (i) => { i.Quantity--; }),
                new Ingredient("2,2", (i) => { return CheckID(i, "Stick"); }, (i) => { i.Quantity--; }),
                Empty("3,2"),
                Empty("1,3"),
                new Ingredient("2,3", (i) => { return CheckID(i, "Stick"); }, (i) => { i.Quantity--; }),
                Empty("3,3")),
                items.Copy("IronAxe")) { CraftTime = 1000 });
            Crafting.Add("IronHammer", new Recipe("Iron Hammer", Fill(
                new Ingredient("1,1", (i) => { return CheckID(i, "IronIngot"); }, (i) => { i.Quantity--; }),
                new Ingredient("2,1", (i) => { return CheckID(i, "Stick"); }, (i) => { i.Quantity--; }),
                new Ingredient("3,1", (i) => { return CheckID(i, "IronIngot"); }, (i) => { i.Quantity--; }),
                new Ingredient("1,2", (i) => { return CheckID(i, "IronIngot"); }, (i) => { i.Quantity--; }),
                new Ingredient("2,2", (i) => { return CheckID(i, "Stick"); }, (i) => { i.Quantity--; }),
                new Ingredient("3,2", (i) => { return CheckID(i, "IronIngot"); }, (i) => { i.Quantity--; }),
                Empty("1,3"),
                new Ingredient("2,3", (i) => { return CheckID(i, "Stick"); }, (i) => { i.Quantity--; }),
                Empty("3,3")),
                items.Copy("IronHammer")) { CraftTime = 1000 });
            Crafting.Add("IronSpade", new Recipe("Iron Spade", Fill(
                Empty("1,1"),
                new Ingredient("2,1", (i) => CheckID(i, "IronIngot"), (i) => i.Quantity--),
                Empty("3,1"),
                Empty("1,2"),
                new Ingredient("2,2", (i) => CheckID(i, "Stick"), (i) => i.Quantity--),
                Empty("3,2"),
                Empty("1,3"),
                new Ingredient("2,3", (i) => CheckID(i, "Stick"), (i) => i.Quantity--),
                Empty("3,3")),
                items.Copy("IronSpade")) { CraftTime = 1000 });
            Crafting.Add("IronLongsword", new Recipe("Iron Longsword", Fill(
                Empty("1,1"),
                new Ingredient("2,1", (i) => CheckID(i, "IronIngot"), (i) => i.Quantity--),
                Empty("3,1"),
                Empty("1,2"),
                new Ingredient("2,2", (i) => CheckID(i, "IronIngot"), (i) => i.Quantity--),
                Empty("3,2"),
                Empty("1,3"),
                new Ingredient("2,3", (i) => CheckID(i, "Stick"), (i) => i.Quantity--),
                Empty("3,3")),
                items.Copy("IronLongsword")) { CraftTime = 1000 });
            Crafting.Add("IronDagger", new Recipe("Iron Dagger", Fill(
                Empty("1,1"),
                Empty("2,1"),
                Empty("3,1"),
                Empty("1,2"),
                new Ingredient("2,2", (i) => CheckID(i, "IronIngot"), (i) => i.Quantity--),
                Empty("3,2"),
                Empty("1,3"),
                new Ingredient("2,3", (i) => CheckID(i, "Stick"), (i) => i.Quantity--),
                Empty("3,3")),
                items.Copy("IronDagger")) { CraftTime = 1000 });

            #endregion

            #region Bows

            Crafting.Add("WoodenBow", new Recipe("WoodenBow", Fill(
                Empty("1,1"),
                new Ingredient("2,1", (i) => { return CheckID(i, "Stick"); }, (i) => { i.Quantity--; }),
                new Ingredient("3,1", (i) => { return CheckID(i, "String"); }, (i) => { i.Quantity--; }),
                new Ingredient("1,2", (i) => { return CheckID(i, "Stick"); }, (i) => { i.Quantity--; }),
                Empty("2,2"),
                new Ingredient("3,2", (i) => { return CheckID(i, "String"); }, (i) => { i.Quantity--; }),
                Empty("1,3"),
                new Ingredient("2,3", (i) => { return CheckID(i, "Stick"); }, (i) => { i.Quantity--; }),
                new Ingredient("3,3", (i) => { return CheckID(i, "String"); }, (i) => { i.Quantity--; })),
                items.Copy("WoodenBow")) { CraftTime = 500 });

            Crafting.Add("IronBow", new Recipe("IronBow", Fill(
                Empty("1,1"),
                new Ingredient("2,1", (i) => { return CheckID(i, "Stick"); }, (i) => { i.Quantity--; }),
                new Ingredient("3,1", (i) => { return CheckID(i, "String"); }, (i) => { i.Quantity--; }),
                new Ingredient("1,2", (i) => { return CheckID(i, "Stick"); }, (i) => { i.Quantity--; }),
                new Ingredient("2,2", (i) => { return CheckID(i, "IronIngot"); }, (i) => { i.Quantity--; }),
                new Ingredient("3,2", (i) => { return CheckID(i, "String"); }, (i) => { i.Quantity--; }),
                Empty("1,3"),
                new Ingredient("2,3", (i) => { return CheckID(i, "Stick"); }, (i) => { i.Quantity--; }),
                new Ingredient("3,3", (i) => { return CheckID(i, "String"); }, (i) => { i.Quantity--; })),
                items.Copy("IronBow")) { CraftTime = 1000 });

            #endregion

            #region Arrows

            Crafting.Add("StoneArrow", new Recipe("Stone Arrow", Fill(
                Empty("1,1"),
                new Ingredient("2,1", (i) => CheckID(i, "Stone"), (i) => i.Quantity--),
                Empty("3,1"),
                Empty("1,2"),
                new Ingredient("2,2", (i) => CheckID(i, "Stick"), (i) => i.Quantity--),
                Empty("3,2"),
                Empty("1,3"),
                new Ingredient("2,3", (i) => CheckID(i, "Feather"), (i) => i.Quantity--),
                Empty("3,3")),
                items.Copy("StoneArrow", (i) => i.Quantity = 5)) { CraftTime = 1000 });

            Crafting.Add("IronArrow", new Recipe("Iron Arrow", Fill(
                Empty("1,1"),
                new Ingredient("2,1", (i) => CheckID(i, "IronIngot"), (i) => i.Quantity--),
                Empty("3,1"),
                Empty("1,2"),
                new Ingredient("2,2", (i) => CheckID(i, "Stick"), (i) => i.Quantity--),
                Empty("3,2"),
                Empty("1,3"),
                new Ingredient("2,3", (i) => CheckID(i, "Feather"), (i) => i.Quantity--),
                Empty("3,3")),
                items.Copy("IronArrow", (i) => i.Quantity = 5)) { CraftTime = 2000 });

            #endregion

            Crafting.Add("FishingRod", new Recipe("Fishing Rod", Fill(
                new Ingredient("1,1", (i) => CheckID(i, "Stick"), (i) => i.Quantity--),
                Empty("2,1"),
                Empty("3,1"),
                new Ingredient("1,2", (i) => CheckID(i, "String"), (i) => i.Quantity--),
                new Ingredient("2,2", (i) => CheckID(i, "Stick"), (i) => i.Quantity--),
                Empty("3,2"),
                new Ingredient("1,3", (i) => CheckID(i, "IronIngot"), (i) => i.Quantity--),
                Empty("2,3"),
                new Ingredient("3,3", (i) => CheckID(i, "Stick"), (i) => i.Quantity--)),
                items.Copy("FishingRod")) { CraftTime = 1500 });

            Crafting.Add("ChurchwardenPipe", new Recipe("Churchwarden Pipe", Fill(
                Empty("1,1"),
                Empty("2,1"),
                new Ingredient("3,1", (i) => CheckID(i, "Stick"), (i) => i.Quantity--),
                Empty("1,2"),
                new Ingredient("2,2", (i) => CheckID(i, "Stick"), (i) => i.Quantity--),
                Empty("3,2"),
                new Ingredient("1,3", (i) => CheckID(i, "Stick"), (i) => i.Quantity--),
                Empty("2,3"),
                Empty("3,3")),
                items.Copy("ChurchwardenPipe")) { CraftTime = 3000 });
        }

        //Helping
        private Ingredient[] Fill(params Ingredient[] input)
        {
            return input;
        }
        private bool CheckID(ItemObject i, string id)
        {
            if (i != null)
                return i.ID.ToUpper() == id.ToUpper();
            return false;
        }
        private Ingredient Empty(string slot)
        {
            return new Ingredient(slot, (i) => { return i == null; }, null);
        }
        private enum Grid { TopLeft, Top, TopRight, Left, Center, Right, BottomLeft, Bottom, BottomRight }
        private string ToGrid(Grid grid)
        {
            if (grid == Grid.TopLeft) return "1,1";
            if (grid == Grid.Top) return "2,1";
            if (grid == Grid.TopRight) return "3,1";

            if (grid == Grid.Left) return "1,2";
            if (grid == Grid.Center) return "2,2";
            if (grid == Grid.Right) return "3,2";

            if (grid == Grid.BottomLeft) return "1,3";
            if (grid == Grid.Bottom) return "2,3";
            if (grid == Grid.BottomRight) return "3,3";
            return string.Empty;
        }
    }
}
