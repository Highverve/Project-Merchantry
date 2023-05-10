using Merchantry.World.Meta.Goals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Merchantry.UI.Items;
using ExtensionsLibrary.Extensions;
using Merchantry.Particles.Types;

namespace Merchantry.World.Objects.Characters.Creatures
{
    public class Berd : CharacterObject
    {
        public Berd(string ID, Vector2 Position, Texture2D Texture, string DisplayName) : base(ID, Position, Texture, DisplayName)
        {
        }

        public override void Initialize()
        {
            //Shape.Add("Main", new Meta.Shapes.Circle(Vector2.Zero, 16));
            Stats.Mass = 15;
            IsAcceptingItems = false;

            Origin = new Vector2(33, 51);
            Animation.FrameSize = new Point(64, 64);
            SCALE_Speed = 5;

            Animation.AddState("Stand",
                () =>
                {
                    Animation.CurrentFrame = Point.Zero;
                    SCALE_Speed = 5;
                }, null);
            Animation.AddState("Sit",
                () =>
                {
                    Animation.CurrentFrame = new Point(1, 0);
                    SCALE_Speed = 5;
                }, null);
            Animation.AddState("Fly",
                () =>
                {
                    Animation.CurrentFrame = new Point(0, 1);
                    Animation.FrameSpeed = 125;
                },
                () =>
                {
                    if (Physics.Altitude > 100)
                        Animation.FrameSpeed = 400 - (int)Physics.Velocity.Length();
                    else
                        Animation.FrameSpeed = 100;

                    Animation.X++;
                    if (Animation.X > 1)
                        Animation.X = 0;
                });

            OnPositionMove += (gt) =>
            {
                TRANSITION_FlipByVelocity(-1);

                if (Physics.IsGravityEnabled == true && Physics.IsAir() == false)
                    Physics.Jump(trueRandom.Next(150, 200));

                if (Scale.X > 0)
                    ROTATE_Loose(Physics.AltitudeVelocity * -.001f);
                else
                    ROTATE_Loose(Physics.AltitudeVelocity * .001f);
            };
            OnPositionStop += () =>
            {
                ROTATE_Speed = 5;
                ROTATE_Loose(0);
            };
            Physics.OnImpact += () =>
            {
                Physics.Velocity = Vector2.Zero;
                Physics.AltitudeVelocity = 0;

                Animation.FrameState = "Stand";
                Physics.IsGravityEnabled = true;
            };

            //Goals.AddGoal("Wander", new Wander(50, 100, 1500, 5000, .1f));
            /*Goals.AddGoal("Fly", new FlyTo(Position + new Vector2(1000, 0), 500, 1));
            Goals.Goals["Fly"].OnInitialize += (g) =>
            {
                Stats.IsGravityApplied = false;
                Animation.FrameState = "Fly";
            };*/
            //Goals.AddGoal("FollowPath", new FollowPath(new Point(38, 18), 16, 32, 1));

            Chat.Add("Speak", "Chirp!");
            Chat.Add("Speak", "Chirp! Chirp!");
            Chat.Add("Speak", "Chirp.");
            Chat.Add("Speak", "Chirp?");
            Chat.Add("Speak", "Chirp, chirp.");

            Stats.MaxSpeed = 1500;
            Stats.Speed = 1500;

            FocusMultiplier = .25f;
            Fly(0);
            Physics.Altitude = TrueRandom.Next(75, 150);
            AltitudeTarget = TrueRandom.Next(90, 150);
            chatTimer = TrueRandom.Next(500, 3000);

            SetBirdPath();

            base.Initialize();
            Physics.BounceMultiplier = 0;
        }
        public override void Load(ContentManager cm)
        {
            base.Load(cm);

            SetChatBox(cm, -64);
        }

        private int chatTimer = 1500;
        public override void Update(GameTime gt)
        {
            if (chatTimer <= 0)
            {
                Chat.SetPacket("Speak");
                Chat.SetCalculatedTime();

                chatTimer = trueRandom.Next(5000, 10000);
            }
            else
                chatTimer -= gt.ElapsedGameTime.Milliseconds;

            Physics.FlyTo(gt, AltitudeTarget, 100, 0);

            base.Update(gt);
        }

        public void Fly(float direction)
        {
            Physics.IsGravityEnabled = false;
            Physics.AltitudeVelocity += direction;

            Animation.FrameState = "Fly";
        }

        public override void UpdateControls(GameTime gt)
        {
            base.UpdateControls(gt);
        }
        public override void LeftClick(WorldObject user)
        {
            if (IsResourceBird == true && Storage.Items.Count > 0)
            {
                References.Particles.AddCircleExpand("WORLD", 20, .5f, 75, 150, () =>
                {
                    return new Firework(Physics.AltitudePosition() - new Vector2(0, 24))
                    { Depth = Depth + World.PixelDepth() * 50 };
                });

                ItemObject item = Storage.Items.ElementAt(TrueRandom.Next(0, Storage.Items.Count)).Value.Copy();
                Storage.RemoveItem(item, 1);
                item.Quantity = 1;

                WorldItem obj = new WorldItem(item.StackID() + item.RandomID, Position, item);
                obj.Physics.Altitude = Physics.Altitude;
                obj.Physics.Velocity = Physics.Velocity;
                obj.Physics.AirFriction = 0;

                World.AddObject(obj);
            }

            base.LeftClick(user);
        }
        public override void ItemClick(ItemObject item, WorldObject user)
        {
            base.ItemClick(item, user);
        }

        public override void Draw(SpriteBatch sb)
        {
            sb.Draw(Texture, Physics.AltitudePosition(-.5f) + new Vector2(0, 32), Animation.Source(), Color.Lerp(Color.Black, Color.Transparent, .7f), -Rotation, Origin, Scale, SpriteEffects.FlipVertically, Depth - World.PixelDepth());

            base.Draw(sb);
        }

        public float AltitudeTarget { get; set; }
        public bool IsResourceBird { get; set; }
        public void SetBirdPath(int pathIndex = 0)
        {
            Point start = Point.Zero, end = Point.Zero;

            float stopDistance = 64, removeDistance = 48;

            if (pathIndex == 0)
            {
                start = new Point(20, 30);
                Position = Path.ToVector(start);
                AI.AddGoal("FollowPath", new PatrolPath(stopDistance, removeDistance, 1, new Point(40, 31), new Point(56, 32)) { IgnoreTileCost = true, PatrolCount = 1 });
            }
            if (pathIndex == 1)
            {
                start = new Point(56, 32);
                Position = Path.ToVector(start);
                AI.AddGoal("FollowPath", new PatrolPath(stopDistance, removeDistance, 1, new Point(40, 31), new Point(20, 30)) { IgnoreTileCost = true, PatrolCount = 1 });
            }

            AI.Goals["FollowPath"].OnSuccess += () => Queue.Add(() =>
            {
                World.RemoveObject(this);
                if (chatBox != null) World.RemoveObject(chatBox);
                if (Expressions != null) World.RemoveObject(Expressions);
            });
        }
    }
}
