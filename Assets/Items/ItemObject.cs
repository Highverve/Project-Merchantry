using ExtensionsLibrary.Extensions;
using Merchantry.Extensions;
using Merchantry.UI.Developer.General;
using Merchantry.World.Meta;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.UI.Items
{
    public class ItemObject : IProperties
    {
        public string ID { get; private set; }
        public string Name { get; private set; }
        public string Description { get; set; }
        public Texture2D Icon { get; private set; }

        //Values that tell how many are in this stack.
        private int quantity;
        public int Quantity
        {
            get { return quantity; }
            set
            {
                quantity = MathHelper.Clamp(value, 0, MaxQuantity);
                if (quantity <= 0)
                    Destroy();
            }
        }
        public int MaxQuantity { get; set; }
        public bool IsMaxQuantity() { return Quantity >= MaxQuantity; }

        //Value/methods used for the item's worth.
        /// <summary>
        /// The base currency value of the item.
        /// </summary>
        public int BaseWorth { get; set; }
        public int QuantityWorth(int quantity) { return BaseWorth * quantity; }
        public int StackWorth() { return BaseWorth * Quantity; }
        public int MaxStackWorth() { return BaseWorth * MaxQuantity; }

        /// <summary>
        /// Low quality gets no worth boost, middle quality gets a decent worth boost, and high gets the biggest worth boost.
        /// </summary>
        public enum ItemQuality { None, Low, Mid, High }
        public ItemQuality Quality { get; set; } = ItemQuality.None;

        /// <summary>
        /// Used by characters to make special requests.
        /// E.g, a character says "I need a [sharp] [weapon]".
        /// Offering an iron dagger would succeed, whereas an iron hammer would not.
        /// </summary>
        public List<string> Attributes { get; set; } = new List<string>();
        /// <summary>
        /// The text that displays below the item's name in the tooltip.
        /// </summary>
        public string Subtitle { get; set; }
        public Color SubtitleColor { get; set; } = Color.LightGray;

        #region Enchantments

        private Dictionary<string, ItemEnchant> enchants;
        /// <summary>
        /// Used for various attributes such as enchanting. 
        /// </summary>
        public Dictionary<string, ItemEnchant> Enchants
        {
            get
            {
                if (enchants == null)
                    enchants = new Dictionary<string, ItemEnchant>(StringComparer.OrdinalIgnoreCase);
                return enchants;
            }
            set { enchants = value; }
        }

        public void ENCHANTS_Add(ItemEnchant enchant)
        {
            if (Enchants.ContainsKey(enchant.ID) == false)
            {
                if (ENCHANTS_IsConflicts(enchant) == false)
                    Enchants.Add(enchant.ID, enchant);
            }
        }
        public void ENCHANTS_Remove(string enchantID)
        {
            if (Enchants.ContainsKey(enchantID) == true)
                Enchants.Remove(enchantID);
        }
        public void ENCHANTS_Clear()
        {
            if (Enchants != null)
                Enchants.Clear();
        }

        public bool ENCHANTS_IsConflicts(ItemEnchant enchant)
        {
            foreach (ItemEnchant e in Enchants.Values)
            {
                if (e.Conflicts(enchant))
                    return true;
            }

            return false;
        }
        public bool ENCHANTS_IsConflicts(string enchantID)
        {
            foreach (ItemEnchant e in Enchants.Values)
            {
                if (e.Conflicts(enchantID))
                    return true;
            }

            return false;
        }

        #endregion

        #region Events

        //Quantity
        private event Action onQuantityGain, onQuantityLoss, onQuantityZero, onQuantityMax;
        public event Action OnQuantityGain { add { onQuantityGain += value; } remove { onQuantityGain -= value; } }
        public event Action OnQuantityLoss { add { onQuantityLoss += value; } remove { onQuantityLoss -= value; } }
        public event Action OnQuantityZero { add { onQuantityZero += value; } remove { onQuantityZero -= value; } }
        public event Action OnQuantityMax { add { onQuantityMax += value; } remove { onQuantityMax -= value; } }

        #endregion

        protected Random seedRandom, trueRandom;
        public string RandomID { get; set; }

        protected ObjectAnimation animation;
        public ObjectAnimation Animation
        {
            get
            {
                if (animation == null)
                {
                    animation = new ObjectAnimation();
                    animation.FrameSize = new Point(64);
                    animation.FrameSpeed = 100;
                }
                return animation;
            }
            set { animation = value; }
        }

        public string StackID()
        {
            string result = ID;
            if (Quality != ItemQuality.None)
                result += "," + Quality + ",";

            if (enchants != null && enchants.Count > 0)
            {
                List<ItemEnchant> attributeList = enchants.Values.ToList();
                attributeList.Sort((a, b) => a.DisplayName.CompareTo(b.DisplayName));

                result += ",";
                foreach (ItemEnchant a in attributeList)
                    result += a.ID + ",";
            }

            if (MaxQuantity == 1)
                result += RandomID;

            return result;
        }

        public enum ItemTabs { Consumables, Tools, Apparel, Resources, Furniture, Various }
        public ItemTabs Tab { get; set; }

        public ItemObject(string ID, string Name, Texture2D Icon, string Description, int BaseWorth, int MaxQuantity, ItemTabs Tab)
        {
            this.ID = ID;
            this.Name = Name;
            this.Icon = Icon;
            this.Description = Description;
            this.BaseWorth = BaseWorth;
            this.MaxQuantity = MaxQuantity;
            this.Tab = Tab;

            Quantity = 1;

            BaseWorth = 0;
            Subtitle = string.Empty;

            seedRandom = new Random((ID + Name + Description).AddChars());
            trueRandom = new Random(Guid.NewGuid().GetHashCode());
        }

        #region Standard Methods

        public virtual void Initialize() { }
        public virtual void Load(ContentManager cm) { }

        public virtual void Update(GameTime gt) { }
        public virtual void Draw(SpriteBatch sb, Vector2 position, Color color, float rotation, Vector2 scale, SpriteEffects effects, float depth = 0)
        {
            if (Icon != null)
            {
                Rectangle source = Icon.Bounds;
                if (animation != null)
                    source = animation.Source();

                sb.Draw(Icon, position, source, color, rotation, Icon.Bounds.Center.ToVector2(), scale, effects, depth);
            }
            //Add extra draw options as different methods. E.g., quantity, worth, etc.
        }
        public virtual void DrawQuantity(SpriteBatch sb, SpriteFont font, Vector2 position, Color color, float rotation, Vector2 scale, float depth = 0)
        {
            if (font != null)
            {
                if (MaxQuantity > 1)
                {
                    if (Quantity < 1000)
                        sb.DrawString(font, quantity.ToString(), position, color, rotation, font.MeasureString(quantity.ToString()), scale, SpriteEffects.None, depth);
                    else if (Quantity >= 1000)
                    {
                        string text = ((float)Quantity / 1000).ToString("0.0") + "k";
                        sb.DrawString(font, text, position, Color.Lerp(Color.Green, Color.White, .35f), rotation, font.MeasureString(text), scale, SpriteEffects.None, depth);
                    }
                }
            }
        }
        
        #endregion

        #region Buttons

        public string ButtonGrabText()
        {
            if (true) //if not holding this item
                return "Hold";
            else
                return "Put Away";
        }
        public string ButtonOneText { get; set; } = "Equip";
        public string ButtonTwoText { get; set; } = "Drop";
        public string ButtonThreeText { get; set; } = "Examine";

        public virtual void ButtonGrab(World.WorldObject user) { onGrab?.Invoke(this, user); }
        public virtual void ButtonOne(World.WorldObject user) { onButtonOne?.Invoke(this, user); }
        public virtual void ButtonTwo(World.WorldObject user) { onButtonTwo?.Invoke(this, user); }
        public virtual void ButtonThree(World.WorldObject user) { onButtonThree?.Invoke(this, user); Console.WriteLine("Examined"); }

        private event Action<ItemObject, World.WorldObject> onGrab, onButtonOne, onButtonTwo, onButtonThree;
        public event Action<ItemObject, World.WorldObject> OnGrab { add { onGrab += value; } remove { onGrab -= value; } }
        public event Action<ItemObject, World.WorldObject> OnButtonOne { add { onButtonOne += value; } remove { onButtonOne -= value; } }
        public event Action<ItemObject, World.WorldObject> OnButtonTwo { add { onButtonTwo += value; } remove { onButtonTwo -= value; } }
        public event Action<ItemObject, World.WorldObject> OnButtonThree { add { onButtonThree += value; } remove { onButtonThree -= value; } }

        public int ButtonCount()
        {
            int result = 0;
            if (string.IsNullOrEmpty(ButtonOneText) == false)
                result++;
            if (string.IsNullOrEmpty(ButtonTwoText) == false)
                result++;
            if (string.IsNullOrEmpty(ButtonThreeText) == false)
                result++;

            return result;
        }

        #endregion

        //Object destruction
        public bool IsDestroyed { get; set; }
        public virtual void Destroy() { IsDestroyed = true; }

        //Add a "UseOnItem" method?

        public ItemObject Copy()
        {
            ItemObject copy = (ItemObject)MemberwiseClone();
            copy.RandomID = trueRandom.NextString("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890", 8);

            return copy;
        }
        public int ToMax(ItemObject item)
        {
            if (MaxQuantity == 1)
                return 0;
            if (item != null)
                return MaxQuantity - (quantity + item.Quantity);
            return MaxQuantity - quantity;
        }
        public void Merge(ItemObject item)
        {
            //If the maximum quantity is 1, then no merging will be possible.
            //Max-1 items are tools, apparel, etc.
            if (MaxQuantity > 1)
            {
                //Compare ids
                if (ID == item.ID)
                {
                    //To merge successfully, the stack names have to match
                    if (StackID() == item.StackID())
                    {
                        //How much the stack will go over by
                        int overload = Math.Max(0, (Quantity + item.Quantity) - MaxQuantity);
                        int tranfer = item.Quantity - overload;

                        Quantity += tranfer;
                        item.Quantity -= tranfer;
                    }
                }
            }
        }
        public ItemObject Split(int take)
        {
            if (take <= Quantity)
            {
                ItemObject item = Copy();

                //Transfer quantities
                item.Quantity = take;
                Quantity -= take;

                return item;
            }
            else return this;
        }

        private Elements.NumberElement quantityNumber;
        public virtual void SetProperties(PropertiesUI ui)
        {
            ui.PROPERTY_AddImage(Icon);
            ui.PROPERTY_AddHeader(Name);
            AetaLibrary.Elements.Text.TextElement subtitleLabel = ui.PROPERTY_AddText(Subtitle);
            subtitleLabel.X = ui.Center.X;
            subtitleLabel.SetOrigin(.5f, 0);
            subtitleLabel.Fade(SubtitleColor, 50);

            ui.PROPERTY_AddText("ID: " + ID).Fade(Color.Gray, 20);
            ui.PROPERTY_AddText("Stack ID: " + StackID()).Fade(Color.Gray, 20);
            quantityNumber = ui.PROPERTY_AddNumber("Quantity: ", Quantity, null, () => Quantity++, () => Quantity--);
            quantityNumber.Fade(Color.Gray, 20);
            ui.PROPERTY_AddText("Max Quantity: " + MaxQuantity).Fade(Color.Gray, 20);
            ui.PROPERTY_AddText("Worth: " + BaseWorth).Fade(Color.Gray, 20);

            ui.PROPERTY_AddDivider(10);
            ui.PROPERTY_AddSpacer(10);
            ui.PROPERTY_AddHeader("Buttons");
            ui.PROPERTY_AddText("One: " + (string.IsNullOrEmpty(ButtonOneText) == false ? ButtonOneText : "None")).Fade(Color.Gray, 20);
            ui.PROPERTY_AddText("Two: " + (string.IsNullOrEmpty(ButtonTwoText) == false ? ButtonTwoText : "None")).Fade(Color.Gray, 20);
            ui.PROPERTY_AddText("Three: " + (string.IsNullOrEmpty(ButtonThreeText) == false ? ButtonThreeText : "None")).Fade(Color.Gray, 20);
        }
        public void RefreshProperties()
        {
            quantityNumber.Number = Quantity;
        }
        public virtual void NullifyProperties()
        {
            quantityNumber = null;
        }
    }
    public struct ItemEnchant
    {
        /// <summary>
        /// These will appear below the item's name (tooltip and other UIs).
        /// Usage examples:
        /// 
        /// Slightly Damaged [-15%, Negative]
        /// Very Damaged [-75%, Negative]
        /// Broken [-90%, Negative]
        /// Cursed [-50%, Negative]
        /// 
        /// Flaming [+25%, Positive]
        /// Regen [+30%, Positive]
        /// Durable [+40%, Positive]
        /// 
        /// Average [+0%, Neutral]
        /// </summary>

        public string ID { get; set; }
        public string DisplayName { get; set; }
        public float WorthModifier { get; set; }

        public List<ItemEnchant> Conflicting { get; set; }
        public bool Conflicts(ItemEnchant enchant) { return Conflicts(enchant.ID); }
        public bool Conflicts(string enchantID)
        {
            for (int i = 0; i < Conflicting.Count; i++)
            {
                if (Conflicting[i].ID == enchantID)
                    return true;
            }
            return false;
        }

        public enum Types { Neutral, Negative, Positive }
        public Types Type { get; set; }

        public ItemEnchant(string DisplayName, float WorthModifier, Types Type)
        {
            ID = string.Empty;
            this.DisplayName = DisplayName;
            this.WorthModifier = WorthModifier;
            this.Type = Type;

            Conflicting = new List<ItemEnchant>();
        }
    }
}
