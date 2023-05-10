using Merchantry.World.Meta.Goals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Merchantry.UI.Items;
using Merchantry.UI.Elements;
using Microsoft.Xna.Framework.Content;

namespace Merchantry.World.Objects.Characters.Creatures
{
    public class Doggo : CharacterObject
    {
        public Doggo(string ID, Vector2 Position, Texture2D Texture, string DisplayName) : base(ID, Position, Texture, DisplayName)
        {
        }

        public override void Initialize()
        {
            Shape.Add("Circle", new Meta.Shapes.Circle(Vector2.Zero, 16) { Group = WorldObject.SHAPE_Collision });
            Stats.Mass = 25;

            Origin = new Vector2(32, 54);
            Animation.FrameSize = new Point(64, 64);
            Animation.AddState("Idle",
                () =>
                {
                    Animation.CurrentFrame = Point.Zero;
                    Animation.FrameSpeed = 800;
                    SCALE_Speed = 5;
                },
                () =>
                {

                });
            Animation.AddState("Walk",
                () =>
                {
                    Animation.FrameSpeed = 200;
                    Animation.CurrentFrame = new Point(1, 0);
                    SCALE_Speed = 10;
                },
                () =>
                {
                    Animation.FrameSpeed = 275 - (int)Physics.Velocity.Length();
                    Animation.X++;
                    if (Animation.X > 2)
                        Animation.X = 1;
                });
            Animation.FrameState = "Idle";

            OnPositionMove += (gt) =>
            {
                TRANSITION_FlipByVelocity();
                TRANSITION_LeanByVelocity();

                if (Physics.Velocity.Length() > 40)
                    Animation.FrameState = "Walk";
                else
                    Animation.FrameState = "Idle";
            };
            OnPositionStop += () =>
            {
                ROTATE_Speed = 5;
                ROTATE_Loose(0);
            };

            //AI.AddGoal("Follow", new FollowObject(World.Controlled));
            AI.AddGoal("Wander", new WanderVector(50, 125, 1500, 5000, .1f));

            Chat.Add("Bark", "Arf!");
            Chat.Add("Bark", "Arrf!");
            Chat.Add("Bark", "Arf...");
            Chat.Add("Bark", "Arf.");
            Chat.Add("Bark", "Aarf?");
            Chat.Add("Bark", "Arf!!!");
            Chat.Add("Bark", "Aarf arf!");
            Chat.Add("Bark", "Arf! Aarf!");

            base.Initialize();
        }
        public override void Load(ContentManager cm)
        {
            base.Load(cm);

            SetChatBox(cm, -68);
        }

        private int chatTimer = 0;
        public override void Update(GameTime gt)
        {
            if (chatTimer <= 0)
            {
                Chat.SetPacket("Bark");
                Chat.SetCalculatedTime();

                chatTimer = trueRandom.Next(5000, 10000);
            }
            else
                chatTimer -= gt.ElapsedGameTime.Milliseconds;

            base.Update(gt);
        }

        public override void HoverEnter(WorldObject user)
        {
            Color = Color.Lerp(Color.White, Color.Black, .25f);
            base.HoverEnter(user);
        }
        public override void HoverExit(WorldObject user)
        {
            Color = Color.White;
            base.HoverExit(user);
        }

        public override void LeftClick(WorldObject user)
        {
            //Goals.Goals["Follow"].BasePriority = 1;
            SetTalk();

            base.LeftClick(user);
        }
        public override void RightClick(WorldObject user)
        {
            if (World.Focused != this)
            {
                ButtonOption call = new ButtonOption("Call", "", () => AI.Goals["Follow"].BasePriority = 1, null);
                ButtonOption talk = new ButtonOption("Talk", "", () =>
                {
                    SetTalk();
                }, null);

                if (Vector2.Distance(Position, user.Position) <= 50f)
                    References.Screens.Context.LAYOUT_Custom(talk);
                else
                    References.Screens.Context.LAYOUT_Custom(call);
                References.Screens.Context.Maximize();
            }

            base.RightClick(user);
        }
        public override void ItemClick(ItemObject item, WorldObject user)
        {
            Chat.SetMessage("Aarf?!?!!!", 2500);
            Storage.AddItem(item);

            base.ItemClick(item, user);
        }

        private void SetTalk()
        {
            Queue.Add(0, () =>
            {
                References.Settings.IsPaused = true;
                References.Screens.Message.LAYOUT_SetMessage(new UI.MessageData(DisplayName, "Doggo", "The dog looks up at you patiently.\n\n\"Arf?\" it says.", References.Screens.Message.ICON_Craft,
                    new ButtonOption("Pet", "", () =>
                    {
                        References.Screens.Message.LAYOUT_SetMessage(new UI.MessageData(DisplayName, "Doggo", "He liked that.", References.Screens.Message.ICON_Craft,
                            new ButtonOption("OK", "", () =>
                            {
                                References.Screens.Message.Minimize();
                                References.Settings.IsPaused = false;
                            }, null)));
                    }, null),
                    new ButtonOption("Shoo", "", () =>
                    {
                        References.Screens.Message.Minimize();
                        AI.Goals["Wander"].BasePriority = 1;
                        Queue.Add(5000, () => AI.Goals["Wander"].BasePriority = .1f);
                        References.Settings.IsPaused = false;
                    }, null)) { SubtitleColor = Color.SkyBlue });
            });
        }
    }
}
