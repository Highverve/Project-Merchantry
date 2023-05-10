using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.Assets
{
    public class AssetManager
    {
        public SymbolAssets Symbols { get; private set; }

        public EnchantAssets Enchantments { get; private set; }
        public ItemAssets Items { get; private set; }
        public RecipeAssets Recipes { get; private set; }

        //Honestly, think of a replacement for this class.
        public EventAssets Events { get; private set; }

        public void Load(ContentManager cm)
        {
            Symbols = new SymbolAssets();
            Enchantments = new EnchantAssets();
            Items = new ItemAssets();
            Recipes = new RecipeAssets();
            Events = new EventAssets();

            Symbols.Load(cm);
            Enchantments.Load(cm);
            Items.Load(cm);
            Recipes.Load(cm);
            Events.Load(cm);
        }
    }
}
