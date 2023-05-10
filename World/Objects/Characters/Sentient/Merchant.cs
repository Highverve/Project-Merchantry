using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Merchantry.World.Objects.UIs;
using Microsoft.Xna.Framework.Content;
using Merchantry.UI.Items;
using Merchantry.World.Meta.Goals;

namespace Merchantry.World.Objects.Characters.Sentient
{
    public class Merchant : CharacterObject
    {
        public MarketObject Shop { get; set; }
        public List<ItemObject> Stock { get; set; } = new List<ItemObject>();

        public Vector2 LeftShopOffset { get; set; } = new Vector2(-15, -24);
        public Vector2 RightShopOffset { get; set; } = new Vector2(-24, -24);

        public Merchant(string ID, Vector2 Position, Texture2D Texture, string DisplayName, Color ItemIdle, Color ItemHover,
                             Color ButtonIdle, Color ButtonHover) : base(ID, Position, Texture, DisplayName)
        {
            Shop = new MarketObject(ID + "_Shop", Position, null, new Point(3, 2), new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), ItemIdle, ItemHover, ButtonIdle, ButtonHover, null);
            Shop.Merchant = this;
        }

        public override void Initialize()
        {
            base.Initialize();

            Animation.FrameSize = new Point(96, 96);
            Animation.AddState("Idle",
            () =>
            {
                Animation.CurrentFrame = new Point(0, 0);
                Shop.IsVisible = false;

                Animation.FrameSpeed = 600;
                SCALE_Speed = 6f;
            },
            () =>
            {
                if (Scale.Y >= .99f)
                    SmoothScale.Y.SetLoose(.95f, .0001f);
                if (Scale.Y <= .96f)
                    SmoothScale.Y.SetLoose(1f, .0001f);
            });
            Animation.AddState("Selling", () =>
            {
                Animation.CurrentFrame = new Point(1, 0);
                Shop.IsVisible = true;

                Animation.FrameSpeed = 1200;
                SCALE_Speed = 3f;
            }, () =>
            {
                if (Scale.Y >= .99f)
                    SmoothScale.Y.SetLoose(.95f, .0001f);
                if (Scale.Y <= .96f)
                    SmoothScale.Y.SetLoose(1f, .0001f);
            });
            Animation.AddState("Walk",
            () =>
            {
                Animation.CurrentFrame = new Point(0, 0);
                Shop.IsVisible = false;

                Animation.FrameSpeed = 150;
                ROTATE_Speed = 3;
                SCALE_Speed = 10;
            },
            () =>
            {
                if (Rotation > 0) ROTATE_Loose(-.1f);
                else ROTATE_Loose(.1f);
            });
            Animation.FrameState = "Idle";

            OnPositionMove += (gt) =>
            {
                TRANSITION_FlipByVelocity();

                if (Scale.X < 0f)
                    Shop.Attach(this, LeftShopOffset);
                if (Scale.X > 0f)
                    Shop.Attach(this, RightShopOffset);

                if (Animation.FrameState != "Selling")
                {
                    if (Physics.Velocity.Length() > 10)
                        Animation.FrameState = "Walk";
                    else
                        Animation.FrameState = "Idle";
                }
                else
                    TRANSITION_LeanByVelocity();
            };
            OnPositionStop += () =>
            {
                ROTATE_Speed = 5;
                ROTATE_Loose(0);
            };

            Shape.Add("Main", new Meta.Shapes.Circle(Vector2.Zero, 16) { Group = SHAPE_Collision });
            Stats.MaxSpeed = 1500;
            Stats.Speed = 1000;
            Stats.Mass = 50;

            IsAcceptingItems = false;
        }
        public override void Load(ContentManager cm)
        {
            base.Load(cm);

            World.AddObject(Shop);
            Shop.Attach(this, new Vector2(-24, -24));
            Shop.IsAcceptingItems = false;
            Shop.SetSeller();
        }

        public override void Update(GameTime gt)
        {
            base.Update(gt);

            Shop.Depth = Depth + World.PixelDepth();
        }
        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
        }

        public override void LeftClick(WorldObject user)
        {
            base.LeftClick(user);

            Animation.FrameState = "Selling";
            Shop.LeftClick(user);
            HoverExit(user);
        }
        public override void RightClick(WorldObject user)
        {
            base.RightClick(user);

            Animation.FrameState = "Idle";
            Shop.RightClick(user);
        }
        public override void HoverEnter(WorldObject user)
        {
            if (Shop.IsActive == false)
                base.HoverEnter(user);
        }

        public int MinRestockQuantity { get; set; } = 2;
        public int MaxRestockQuantity { get; set; } = 5;
        public int RestockTolerance { get; set; } = 1;

        public void AddStock(ItemObject item, int quantity)
        {
            item.Quantity = quantity;
            Stock.Add(item);
        }
        public void RefreshStock()
        {
            if (Stock.Count > 0)
            {
                onRefreshStock?.Invoke();

                int count = TrueRandom.Next(MinRestockQuantity, MaxRestockQuantity);
                foreach (UIs.Elements.ItemSlot slot in Shop.Slots.Values)
                {
                    if (slot.Item == null)
                    {
                        int index = TrueRandom.Next(0, Stock.Count);
                        ItemObject item = Stock[index].Copy();
                        item.Quantity = TrueRandom.Next(1, Stock[index].Quantity);

                        slot.Item = item;
                    }

                    count--;
                    if (count <= 0)
                        break;
                }
            }
        }
        protected event Action onRefreshStock;
        public void AddItem(ItemObject item)
        {
            foreach (UIs.Elements.ItemSlot slot in Shop.Slots.Values)
            {
                if (slot.Item == null)
                {
                    slot.Item = item;
                    break;
                }
            }
        }
        public void Escape(params Point[] destination)
        {
            AI.AddGoal("Escape", new FollowPath(destination[TrueRandom.Next(0, destination.Length)], 24, 32, 1));
            AI.Goals["Escape"].OnSuccess += () =>
            {
                RefreshStock();
                Queue.Add((20 + (Shop.SLOTS_FullCount() * 5)) * 1000, () => GoToStall(new Point(46, 29)));
            };
            AI.Goals["Escape"].OnFail += () => Queue.Add(1000, () => Escape(destination));
        }
        public virtual void GoToStall(Point tile)
        {
            AI.AddGoal("Return", new FollowPath(new Point(46, 29), 24, 32, 1));
            AI.Goals["Return"].OnSuccess += () => IsSelectable = true;
        }
    }
}
