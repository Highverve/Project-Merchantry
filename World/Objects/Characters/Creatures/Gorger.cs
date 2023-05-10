using ExtensionsLibrary.Extensions;
using Merchantry.Particles.Types;
using Merchantry.UI.Items;
using Merchantry.World.Meta.Goals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Objects.Characters.Creatures
{
    public class Gorger : CharacterObject
    {
        public Gorger(string ID, Vector2 Position, Texture2D Texture, string DisplayName) : base(ID, Position, Texture, DisplayName)
        {
        }

        public override void Initialize()
        {
            Shape.Add("Circle", new Meta.Shapes.Circle(Vector2.Zero, 12) { Group = SHAPE_Collision });
            Stats.Mass = 15;
            Stats.MaxSpeed = 800;
            Stats.Speed = 500;
            FocusMultiplier = .1f;
            FocusScale = 4;

            Origin = new Vector2(24, 33);
            Animation.FrameSize = new Point(48, 48);
            Animation.AddState("Idle",
                () =>
                {
                    Animation.FrameSpeed = 800;
                    Animation.CurrentFrame = Point.Zero;

                    SCALE_Speed = 5;
                    ROTATE_Speed = 5;
                    ROTATE_Loose(0);
                },
                () =>
                {
                    if (Scale.Y >= 1) SmoothScale.Y.SetLoose(.95f, .0001f);
                    else SmoothScale.Y.SetLoose(1.05f, .0001f);
                });
            Animation.AddState("Walk",
                () =>
                {
                    Animation.FrameSpeed = 150;
                    Animation.CurrentFrame = Point.Zero;
                    ROTATE_Speed = 3;
                    SCALE_Speed = 10;
                },
                () =>
                {
                    if (Rotation > 0) ROTATE_Loose(-.2f);
                    else ROTATE_Loose(.2f);
                });
            Animation.FrameState = "Idle";

            OnPositionMove += (gt) =>
            {
                TRANSITION_FlipByVelocity(-1);
                SmoothScale.Y.SetLoose(1 + (Physics.Velocity.Y * .001f));

                if (Physics.Velocity.Length() > 10)
                    Animation.FrameState = "Walk";
                else
                    Animation.FrameState = "Idle";
            };

            Physics.BounceMultiplier = .45f;
            Physics.OnImpact += () =>
            {
                SmoothScale.Y.Speed = 15f;
                SmoothScale.Y.SetLoose(.5f);
                Queue.Add(100, () => SmoothScale.Y.SetLoose(1));
            };

            //Goals.AddGoal("Follow", new MoveToObject(World.Controlled));
            //Goals.AddGoal("Wander", new Wander(50, 125, 1500, 5000, .1f));

            Chat.Add("Speak", "Oink!");
            Chat.Add("Speak", "Oink!");
            Chat.Add("Speak", "Oink...");
            Chat.Add("Speak", "Oink.");
            Chat.Add("Speak", "Oink?");
            Chat.Add("Speak", "Oink!!!");
            Chat.Add("Speak", "Oink oink!");
            Chat.Add("Speak", "Oink! Oink!");
            chatTimer = trueRandom.Next(15000, 20000);

            base.Initialize();
        }
        public override void Load(ContentManager cm)
        {
            base.Load(cm);

            SetChatBox(cm, -50);
            SetExpressions(cm, -40);
        }

        private int chatTimer, dustTimer, dropTimer;
        public override void Update(GameTime gt)
        {
            if (chatTimer <= 0)
            {
                Chat.SetPacket("Speak");
                Chat.SetCalculatedTime();

                chatTimer = trueRandom.Next(15000, 20000);
            }
            else
                chatTimer -= gt.ElapsedGameTime.Milliseconds;

            if (isStampedeBehaviour && Animation.FrameState == "Walk")
            {
                if (dustTimer <= 0)
                {
                    GameManager.References().Particles.AddCircleExpand("WORLD", TrueRandom.Next(1, 4), .5f, 100, 200, () =>
                    {
                        return new DirtDust(Position - new Vector2(0, 15)) { Depth = Depth };
                    });
                    dustTimer = 175;
                }
                else
                    dustTimer -= gt.ElapsedGameTime.Milliseconds;

                if (dropTimer <= 0 && Storage.Items.Count > 0)
                {
                    ItemObject item = Storage.Items.First().Value;
                    Storage.RemoveItem(item);
                    World.SpawnItem(item, Position);
                    dropTimer = TrueRandom.Next(3000, 5000);
                }
                else
                    dropTimer -= gt.ElapsedGameTime.Milliseconds;
            }

            base.Update(gt);
        }

        public override void HoverEnter(WorldObject user)
        {
            if (References.Screens.Drag.IsHolding())
            {
                base.HoverEnter(user);

                if (References.Screens.Tooltip.IsMaximized == false)
                {
                    References.Screens.Tooltip.LAYOUT_SetText("Feed");
                    References.Screens.Tooltip.Maximize();
                }
            }
        }
        public override void HoverExit(WorldObject user)
        {
            if (References.Screens.Tooltip.IsMaximized == true)
                References.Screens.Tooltip.Minimize();

            base.HoverExit(user);
        }

        public override void LeftClick(WorldObject user)
        {
            base.LeftClick(user);
        }
        public override void RightClick(WorldObject user)
        {
            base.RightClick(user);
        }
        public override void ItemClick(ItemObject item, WorldObject user)
        {
            //Storage.AddItem(item);
            if (item.ID == "GorgerChow" && Memory.Contains("Friend") == false)
            {
                item.Quantity -= 1;
                if (item.Quantity > 0)
                    World.SpitItem(new WorldItem(item.StackID() + item.RandomID, Position + new Vector2(0, 24), item), 50, 50, 200);

                Chat.SetMessage("*munching*", 500);
                Queue.Add(500, () =>
                {
                    References.Particles.AddCircleExpand("WORLD", TrueRandom.Next(2, 7), .2f, 20, 50,
                    () =>
                    {
                        BounceOut p = new BounceOut(Physics.AltitudePosition() - new Vector2(0, 30));
                        p.OnLoad += (cm) =>
                        {
                            p.Texture = cm.Load<Texture2D>("Assets/Effects/Particles/hearts");
                            p.Source = trueRandom.NextObject(new Rectangle(0, 0, 17, 19), new Rectangle(17, 0, 16, 20), new Rectangle(33, 0, 13, 17), new Rectangle(46, 0, 11, 12), new Rectangle(57, 0, 9, 11));
                            p.Origin = p.Source.Size.ToVector2() / 2;

                            p.Depth = Depth + World.PixelDepth() * 51;
                            p.SetTime(1500);
                            p.Scale = Vector2.One;
                            p.Color = Color.Lerp(Color.Red, Color.White, trueRandom.NextFloat(0, .5f));
                            p.Velocity -= new Vector2(0, trueRandom.NextFloat(10, 50));
                        };
                        return p;
                    });
                    References.Particles.AddCircleExpand("WORLD", 20, .2f, 50, 120, () =>
                    {
                        return new Firework(Physics.AltitudePosition() - new Vector2(0, 24)) { Depth = Depth + World.PixelDepth() * 50 };
                    });

                    AI_AddDig();
                    Memory.AddMemory(new Meta.MemoryTags("Friend"), user, TrueRandom.Next(90, 120));
                });
                //  make friends
            }
            else World.SpitItem(new WorldItem(item.StackID() + item.RandomID, Position + new Vector2(0, 24), item), 50, 50, 200);

            if (References.Screens.Tooltip.IsMaximized == true)
                References.Screens.Tooltip.Minimize();

            base.ItemClick(item, user);
        }

        private void AI_AddDig()
        {
            AI.AddGoal("Dig", new DigItem(2, 1000, 1, 4, new Vector2(0, 40), References.Assets.Items.Copy("Stone"), References.Assets.Items.Copy("Stone"), References.Assets.Items.Copy("Stone"), References.Assets.Items.Copy("IronOre")));
        }
        public void SetNormalBehaviour()
        {
            AI.AddGoal("Wander", new WanderPath(Point.Zero, new Point(8, 5), 3000, 7000, 16, true, 1));
            AI.Goals["Wander"].OnSuccess += () =>
            {
                if (TrueRandom.NextDouble() < .01f)
                    AI_AddDig();
            };
            AI.AddGoal("Return", new FollowPath(Path.ToPoint(Position), 15, 16, 0));
            Vector2 home = Position;
            AI.Goals["Return"].PriorityFormula = () => { return Vector2.Distance(Position, home) / 200; };
        }
        private bool isStampedeBehaviour = false;
        public void SetStampedeBehaviour(Point destination)
        {
            isStampedeBehaviour = true;
            IsSelectable = false;
            dropTimer = TrueRandom.Next(3000, 5000);
            chatTimer = trueRandom.Next(1000, 8000);

            if (TrueRandom.NextDouble() > .5f)
                Expressions.Animation.FrameState = "Exclamation";
            if (TrueRandom.NextDouble() < .1f)
            {
                if (TrueRandom.NextDouble() > .5f)
                    Storage.AddItem("Stone", TrueRandom.Next(1, 2));
                if (TrueRandom.NextDouble() < .1f)
                    Storage.AddItem("IronOre", 1);
            }

            Stats.MaxSpeed = TrueRandom.Next(800, 900, 1000, 1100, 1200, 1300);
            Stats.Speed = Stats.MaxSpeed;
            AI.AddGoal("GoTo", new FollowPath(destination, 15, 16, 0, true));
            AI.Goals["GoTo"].OnSuccess += Destroy;
        }
        public void SetFollowBehaviour(WorldObject target)
        {
            AI.AddGoal("FollowObject", new FollowObject(target, 150, 32));
            /*AI.Goals["FollowObject"].OnFail += () =>
            {
                AI.AddGoal("FollowPath", new FollowPath(target.Path.Coords(), 32, 24, 1, true));
            };*/
        }

        public override void UpdateControls(GameTime gt)
        {
            if (Controls.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W))
                Physics.AddVelocity(0, -Stats.MaxSpeed * (float)gt.ElapsedGameTime.TotalSeconds);
            if (Controls.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
                Physics.AddVelocity(0, Stats.MaxSpeed * (float)gt.ElapsedGameTime.TotalSeconds);
            if (Controls.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A))
                Physics.AddVelocity(-Stats.MaxSpeed * (float)gt.ElapsedGameTime.TotalSeconds, 0);
            if (Controls.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D))
                Physics.AddVelocity(Stats.MaxSpeed * (float)gt.ElapsedGameTime.TotalSeconds, 0);

            base.UpdateControls(gt);
        }
    }
}
