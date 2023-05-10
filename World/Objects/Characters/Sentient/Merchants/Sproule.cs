using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Merchantry.World.Objects.UIs.Elements;
using ExtensionsLibrary.Extensions;
using Merchantry.UI.Items;
using Merchantry.World.Meta.Goals;
using Merchantry.World.Objects.UIs;

namespace Merchantry.World.Objects.Characters.Sentient.Merchants
{
    public class Sproule : Merchant
    {
        public int Happiness { get; set; }
        public int LastHappiness { get; set; }

        int restockTimer = 0;

        public Sproule(string ID, Vector2 Position, Texture2D Texture, string DisplayName, Color ItemIdle, Color ItemHover, Color ButtonIdle, Color ButtonHover)
            : base(ID, Position, Texture, DisplayName, ItemIdle, ItemHover, ButtonIdle, ButtonHover)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            FocusMultiplier = .1f;
            FocusOffset = new Vector2(0, -15);

            Chat.Add("Greet", "Look.");
            Chat.Add("Greet", "Sproule sell.");
            Chat.Add("Greet", () =>
            {
                if (Shop.SLOTS_FullCount() == 1)
                    return "One left.";
                if (Shop.SLOTS_FullCount() == 2)
                    return "Two left.";
                if (Shop.SLOTS_FullCount() == 3 || Shop.SLOTS_FullCount() == 4)
                    return "Some wares left.";
                return "Many wares left.";
            });
            Chat.Add("Greet", () =>
            {
                if (Happiness == 0)
                    return "Please buy wares.";
                if (Happiness > 0 && Happiness < 20)
                    return "Tokens for wares.";
                if (Happiness >= 20 && Happiness < 50)
                    return "You buy wares again?";
                if (Happiness >= 50)
                    return "Welcome, friend.";

                return "Welcome.";
            });
            Chat.Add("Buy", "Good wares.");
            Chat.Add("Buy", "Take care.");
            Chat.Add("Buy", "Sproule thanks.");
            Chat.Add("Gift", "Sproule loves gift.");
            Chat.Add("Gift", "Sproule appreciates.");
            Chat.Add("Gift", "Thanks, friend!");
        }
        public override void Load(ContentManager cm)
        {
            base.Load(cm);

            noteIcon = cm.Load<Texture2D>("UI/Icons/note");

            Queue.Add(60 * 1000, () => GoToStall(new Point(46, 29)));
            SetChatBox(cm, -110);

            Shop.OnPurchase += (obj, i, q) =>
            {
                LastHappiness = Happiness;
                Happiness += i.QuantityWorth(q);

                if (LastHappiness < 10 && Happiness >= 10)
                {
                    AddStock(References.Assets.Items.Items["GorgerChow"].Copy(), 3);
                    MinRestockQuantity = 2;
                    MaxRestockQuantity = 4;
                }
                if (LastHappiness < 20 && Happiness >= 20)
                    AddStock(References.Assets.Items.Items["String"].Copy(), 5);
                if (LastHappiness < 30 && Happiness >= 30)
                {
                    AddStock(References.Assets.Items.Items["Feather"].Copy(), 3);
                    MinRestockQuantity = 3;
                    MaxRestockQuantity = 5;
                    RestockTolerance = 0;
                }
                if (LastHappiness < 50 && Happiness >= 50)
                {
                    AddStock(References.Assets.Items.Items["IronIngot"].Copy(), 5);
                    MinRestockQuantity = 4;
                    MaxRestockQuantity = 6;
                }
            };

            RestockTolerance = 1;
            MinRestockQuantity = 1;
            MaxRestockQuantity = 3;

            AddStock(References.Assets.Items.Items["Stick"].Copy(), 10);
            AddStock(References.Assets.Items.Items["Stone"].Copy(), 10);
            onRefreshStock += () =>
            {
                int c = 0;
                foreach (ItemSlot s in Shop.Slots.Values)
                {
                    if (s.Item == null)
                    {
                        ItemObject item = Stock[c].Copy();
                        item.Quantity = TrueRandom.Next(1, Stock[c].Quantity);

                        s.Item = item;
                        c++;
                        if (c > 1)
                            break;
                    }
                }
            };

            RefreshStock();
        }

        public override void Update(GameTime gt)
        {
            IsAcceptingItems = (World.Focused != this || World.Focused != Shop);
            if (World.NavGrid.IsExists(Path.ToPoint(Position)) == false)
                Physics.AddVelocity(-Stats.Speed * (float)gt.ElapsedGameTime.TotalSeconds, 0);

            if (World.Focused != this || World.Focused != Shop)
            {
                if (AI.CurrentGoal == null)
                {
                    restockTimer += gt.ElapsedGameTime.Milliseconds;

                    if (restockTimer >= 90000)
                    {
                        if (Shop.SLOTS_FullCount() >= 5)
                        {
                            foreach (ItemSlot s in Shop.Slots.Values)
                                s.Item = null;
                        }

                        LeaveRestock();
                    }
                }
            }

            base.Update(gt);
        }

        Texture2D noteIcon;
        UI.MessageData main = null;
        public override void LeftClick(WorldObject user)
        {
            //Close by, you see a peculiar fellow dressed in a hat and cloak.
            //He notices your glance and makes his way toward you.
            //With narrowed eyes, he inquires: "What business have you in a calm hamlet such as this?"

            // • "I am just a traveller."
            // 
            // • "I am a great artisan."
            // • Say nothing.

            //[Not introducted]
            //The odd fellow approaches. "You buy wares?"
            //[Good customer]
            //"My friend! Sproule hopes you are well. Please, come look at wares."
            //"Friend! Sproule hopes you are well. Please, come look at wares."

            // ----- ALTERNATE -----

            //[Not introducted]
            //The odd fellow approaches with narrow eyes. "You buy wares?" he speaks with a hushed tone.

            //[Introducted]
            //"You again. Sproule sell?"
            //"Sproule welcomes you. You buy wares?"
            //"Sproule has wares for your tokens."
            if (World.Focused != Shop)
            {
                restockTimer = 0;

                if (buy == null)
                    buy = new UI.Elements.ButtonOption("Buy Wares", "", () =>
                    {
                        Chat.SetPacket("Greet");
                        Chat.SetCalculatedTime();

                        References.Screens.Message.Minimize();
                        References.Screens.Transitions.BoxVertical(2, 1);
                        References.Screens.HUD.Fade(Color.White, 5f);
                        World.IsAllowSelecting = true;

                        Queue.Add(500, () =>
                        {
                            base.LeftClick(user);
                            References.Screens.Backpack.Maximize();
                        });
                    }, null);
                if (decline == null)
                    decline = new UI.Elements.ButtonOption("Leave", "", () => { EndCutscene(); }, null);
                rumors = new UI.Elements.ButtonOption("Talk", "", () =>
                {
                    SetRumorDialogue();
                    References.Screens.Message.LAYOUT_SetMessage(new UI.MessageData(DisplayName, "Rumors", data[TrueRandom.Next(0, data.Count)], noteIcon,
                        new UI.Elements.ButtonOption("Continue", null, () => { References.Screens.Message.LAYOUT_SetMessage(main); }, null)));
                }, null);

                if (Happiness == 0)
                {
                    StartCutscene();
                    References.Screens.Message.LAYOUT_SetMessage(main = new UI.MessageData(DisplayName, "Introductions", "The odd fellow approaches with narrow\neyes. \"You buy wares? Sproule sell.\", he\ncroaks with anticipation.", noteIcon,
                        buy, rumors, decline) { SubtitleColor = ColorExt.Raspberry });

                    DisplayName = "Sproule of Oro";
                }
                if (Happiness > 0 && Happiness <= 50)
                {
                    StartCutscene();
                    string message = TrueRandom.NextString("\"You again. Sproule sells wares.\"", "\"Sproule welcomes again. You buy wares?\"", "\"Sproule has wares for your tokens.\"");

                    References.Screens.Message.LAYOUT_SetMessage(main = new UI.MessageData(DisplayName, "Merchant", message + "\nHe waits patiently with unblinking eyes.", noteIcon,
                        buy, rumors, decline) { SubtitleColor = ColorExt.Raspberry });
                }
                if (Happiness > 50)
                {
                    StartCutscene();
                    string message = TrueRandom.NextString("\"Friend! Please look at wares.\"", "\"Sproule greets friend. Many wares for you.\"", "\"Sproule is here for friend.\"");

                    References.Screens.Message.LAYOUT_SetMessage(main = new UI.MessageData(DisplayName, "Merchant", message + "\n\nHis eagerness shines through his\nwide eyes and smile.", noteIcon,
                        buy, rumors, decline) { SubtitleColor = ColorExt.Raspberry });
                }
            }
        }
        public override void RightClick(WorldObject user)
        {
            base.RightClick(user);

            if (Shop.SLOTS_FullCount() <= RestockTolerance)
                LeaveRestock();
        }
        public void LeaveRestock()
        {
            restockTimer = 0;
            IsSelectable = false;

            Point[] ends = new Point[] { new Point(21, 38), new Point(21, 15), new Point(57, 14), new Point(57, 37) };
            if (TrueRandom.NextDouble() > ((float)MathHelper.Min(Happiness, 100) / 500) + .1f)
                Escape(new Point(21, 38), new Point(21, 15), new Point(57, 14), new Point(57, 37));
            else
            {
                Point[] destinations = { new Point(37, 29), new Point(37, 30), new Point(38, 30), new Point(39, 30), new Point(40, 30), new Point(40, 29), new Point(40, 28) };
                AI.AddGoal("ApproachStall", new FollowPath(destinations[TrueRandom.Next(0, destinations.Length)], 32, 24, 1));
                AI.Goals["ApproachStall"].OnSuccess += () =>
                {
                    AI.AddGoal("Purchase", new PurchaseItem((MarketObject)World.Objects["MarketStall"], .70f, 500, 1500,
                        References.Assets.Items.Copy("StoneAxe", (i) => i.Quantity = 1),
                        References.Assets.Items.Copy("StonePickaxe", (i) => i.Quantity = 1),
                        References.Assets.Items.Copy("StoneHammer", (i) => i.Quantity = 1),
                        References.Assets.Items.Copy("StoneSpade", (i) => i.Quantity = 1),
                        References.Assets.Items.Copy("StoneCudgel", (i) => i.Quantity = 1),
                        References.Assets.Items.Copy("StoneArrow", (i) => i.Quantity = 5),
                        References.Assets.Items.Copy("WoodenBow", (i) => i.Quantity = 1),
                        References.Assets.Items.Copy("FishingRod", (i) => i.Quantity = 1)));
                    AI.Goals["Purchase"].OnFail += () =>
                    {
                        PurchaseItem p = ((PurchaseItem)AI.Goals["Purchase"]);
                        if (p.Market.IsSelling == true && p.Market.SLOTS_FullCount() > 0)
                        {
                            Chat.SetPacket("Buy");
                            Chat.SetCalculatedTime();
                        }

                        Escape(new Point(21, 38), new Point(21, 15), new Point(57, 14), new Point(57, 37));
                    };
                };
            }
        }

        private UI.Elements.ButtonOption buy, rumors, decline;
        List<string> data = new List<string>();
        private void SetRumorDialogue()
        {
            data.Clear();
            if (Shop.SLOTS_FullCount() >= 5)
            {
                data.Add("\"Not talk now. Many wares to sell.\"");
                data.Add("\"You buy wares? I have plenty.\"");
            }
            else
            {
                if (Happiness == 0)
                {
                    data.Add("\"Townsfolk not trust Sproule. Feels bad.\"");
                    data.Add("\"No tokens today. Wares not sell.\"");
                    data.Add("\"Long walk from home make Sproule tired.\"");
                }
                if (Happiness > 0 && Happiness <= 50)
                {
                    data.Add("\"Life as trader not bad. Oro hamlet\nis pleasant, and lake is best place.\"");
                    data.Add("\"Home far away, but Sproule find\nmany wares along the path.\"");
                    data.Add("\"Maybe someday Sproule learn\nto make wares like you.\"");
                    data.Add("\"Gorgers love bait. You feed\nthem, something happens.\"");
                    //data.Add("Life as trader not so bad. Oro province\nis pleasant, and lake is best place.");
                }
                if (Happiness > 50)
                {
                    data.Add("\"Sproule dwells by lake.\" he speaks, gesturing\nnorth. \"Friend welcome anytime.\"\n\nHe pauses. \"... but not night time.\"");
                    data.Add("\"Many tokens from you. Thanks.\"");
                    data.Add("\"Sproule has renewed purpose.\"");
                }
            }
            //[If stock > 75%]
            //"Not talk today. Many wares to sell."
            //[If happiness == 0]
            //"Townsfolk not trust Sproule. Wares not sell."
            //

            //[If happiness > 0]
            //"Oro province is pleasant. Lake is best place."
            //"Gorgers love chow. You feed them, good things happen." 
            //""

            //"Sproule dwells by lake." He gestures north.\n"Visit soon."
        }
        private void StartCutscene()
        {
            World.Focused = this;
            Camera.TargetScale = new Vector2(3);
            World.IsAllowSelecting = false;
            References.Screens.Transitions.BoxVertical(2, .85f);
            References.Screens.HUD.Fade(Color.Transparent, 7.5f);
            References.Screens.Backpack.Minimize();
        }
        private void EndCutscene()
        {
            References.Screens.Message.Minimize();
            References.Screens.Transitions.BoxVertical(2, 1);
            References.Screens.HUD.Fade(Color.White, 5f);

            World.Focused = World.Controlled;
            Camera.TargetScale = new Vector2(2);
            World.IsAllowSelecting = true;
        }

        public override void GoToStall(Point tile)
        {
            base.GoToStall(tile);

            AI.Goals["Return"].OnSuccess += () =>
            {
                if (Happiness > 0)
                    References.Screens.HUD.SendNotification(References.Screens.Message.ICON_Craft, "Sproule has returned.");
            };
        }

        public override void HoverEnter(WorldObject user)
        {
            base.HoverEnter(user);

            if (References.Screens.Drag.IsHolding())
            {
                References.Screens.Tooltip.LAYOUT_SetText("Gift");
                References.Screens.Tooltip.Maximize();
            }
        }
        public override void HoverExit(WorldObject user)
        {
            base.HoverExit(user);

            References.Screens.Tooltip.Minimize();
        }
        public override void ItemClick(ItemObject item, WorldObject user)
        {
            if (item.ID == "Currency")
            {
                Chat.SetMessage("No donations.");
                Chat.SetCalculatedTime();
                World.SpawnItem(item, Position);
            }
            else if (item.ID == "ChurchwardenPipe")
            {
                if (Storage.ContainsID(item.ID) == false)
                {
                    Chat.SetMessage("Perfect gift.");
                    Chat.SetCalculatedTime();
                    Storage.AddItem(item);
                }
                else
                {
                    Chat.SetMessage("Enough pipes.");
                    Chat.SetCalculatedTime();
                    World.SpawnItem(item, Position);
                }
            }
            else
            {
                Chat.SetPacket("Gift");
                Chat.SetCalculatedTime();
            }

            base.ItemClick(item, user);
        }
    }
}
