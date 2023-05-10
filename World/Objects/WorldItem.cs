using Merchantry.UI.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;

namespace Merchantry.World.Objects
{
    public class WorldItem : RenderObject
    {
        private ItemObject item;
        public ItemObject Item
        {
            get { return item; }
            set
            {
                if (item != value || item != null)
                    Texture = value.Icon;

                item = value;
            }
        }

        private SpriteFont font;

        public WorldItem(string ObjectID, Vector2 Position, ItemObject Item) : base(ObjectID, Position, Item.Icon)
        {
            item = Item;
            IsAcceptingItems = false;
        }

        public override void Initialize()
        {
            base.Initialize();

            Scale = Vector2.Zero;

            Shape.Add("Main", new Meta.Shapes.Circle(new Vector2(0, 0), 8) { Group = SHAPE_Collision });
            Origin = new Vector2(32, 48);
            Stats.Mass = 15;
            Stats.MaxSpeed = 5000;
            Stats.Speed = 5000;
            Physics.BounceMultiplier = .5f;

            OnPositionMove += (gt) =>
            {
                ROTATE_Speed = 10;
                ROTATE_Loose(Physics.Velocity.X * -.0015f);
            };
            OnPositionStop += () =>
            {
                ROTATE_Speed = 10;
                ROTATE_Loose(0);
                Position = Position.ToPoint().ToVector2();
            };
        }
        public override void Load(ContentManager cm)
        {
            font = cm.Load<SpriteFont>("UI/Fonts/ArchwayKnights_19px");

            base.Load(cm);
        }

        private int destroyTimer = 0;
        public override void Update(GameTime gt)
        {
            if (item != null)
            {
                item.Update(gt);

                if (item.Quantity <= 0)
                    item = null;
            }
            else
            {
                Stats.IsCollidable = false;
                Physics.MoveTo(gt, target, 7500, 8);
                if (Vector2.Distance(Position, target) <= 128)
                {
                    SCALE_Speed = 15;
                    SCALE_Loose(0, 0);

                    destroyTimer += gt.ElapsedGameTime.Milliseconds;
                    if (destroyTimer >= 500)
                    {
                        Scale = Vector2.Zero;
                        Destroy();
                    }
                }
            }

            base.Update(gt);
        }

        public override void Draw(SpriteBatch sb)
        {
            if (item != null && item.Quantity > 1)
                item.DrawQuantity(sb, font, Position + new Vector2(28 * Scale.X, (24 * Scale.Y) - Physics.Altitude), Color.White, Rotation, Scale, Depth + World.PixelDepth());
            base.Draw(sb);
        }

        public override void LeftClick(WorldObject user)
        {
            if (item != null)
            {
                if (References.Screens.Backpack.IsMaximized == true && user == World.Controlled)
                {
                    References.Screens.Drag.AddItem(item);
                    target = Position;
                }
                else
                {
                    user.Storage.AddItem(item);
                    target = user.Position - new Vector2(0, 32);
                    item = null;
                }
            }
        }
        private Vector2 target;
    }
}
