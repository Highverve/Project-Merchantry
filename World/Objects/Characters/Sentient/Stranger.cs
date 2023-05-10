using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ExtensionsLibrary.Extensions;
using Merchantry.World.Meta.Goals;
using Microsoft.Xna.Framework.Content;

namespace Merchantry.World.Objects.Characters.Sentient
{
    public class Stranger : Adult
    {
        int ignoredTimer = 0;

        public Stranger(string ID, Vector2 Position, Texture2D Texture, string DisplayName)
            : base(ID, Position, Texture, DisplayName) { }

        public override void Load(ContentManager cm)
        {
            base.Load(cm);

            FaceToward(-1);
            Expressions.Animation.FrameState = "Question";

            Memory.AddMemory(true, "NextQuest");
        }

        public override void Update(GameTime gt)
        {
            if (ignoredTimer >= 0 && World.Focused != this)
                ignoredTimer += gt.ElapsedGameTime.Milliseconds;
            if (ignoredTimer >= 45000)
            {
                WalkAway(null);
                ignoredTimer = -1;
            }

            base.Update(gt);
        }

        public override void LeftClick(WorldObject user)
        {
            if (((bool)Memory.All("NextQuest").Memory) == true)
            {
                if (Memory.ContainsAny("Quest", "Introductions") == false)
                    IntroductionsDialogue(user);
            }
            base.LeftClick(user);
        }

        private void IntroductionsDialogue(WorldObject user)
        {
            World.IsAllowSelecting = false;
            References.Screens.Transitions.BoxVertical(2.5f, .85f);
            References.Screens.HUD.Fade(Color.Transparent, 5f);
            World.Focused = this;
            Camera.TargetScale = new Vector2(3);
            References.Screens.Backpack.Minimize();

            Queue.Add(250, () =>
            {
                Texture2D icon = World.Content.Load<Texture2D>("UI/Icons/note");
                Color subtitle = Color.LightBlue;
                Expressions.Outro();
                Memory.AddMemory(false, "NextQuest");

                References.Screens.Message.LAYOUT_SetMessage(new UI.MessageData("Stranger", "Introductions", "As you're setting up, you spot a\nfigure peering from behind the\nhouse's corner.\n\n", icon,
                new UI.Elements.ButtonOption("Greet The Stranger", "", () =>
                {
                    user.Chat.SetMessage("Welcome!", 1500);
                    World.Focused = user;
                    References.Screens.Message.LAYOUT_SetMessage(new UI.MessageData("Stranger", "Introductions", "\"Welcome!\", you exclaim. The stranger\noffers a small nod in return, and\ndisappears behind the corner.", icon,
                        new UI.Elements.ButtonOption("Continue", "", () =>
                        {
                            Stats.Speed = 1000;
                            WalkAway(null);
                            Memory.AddMemory("Welcome", "Quest", "Introductions");

                            World.ExitCutscene();
                        }, null))
                    { SubtitleColor = subtitle });
                }, null),
                new UI.Elements.ButtonOption("Beckon Over", "", () =>
                {
                    References.Screens.Message.Minimize();
                    Stats.Speed = 600;
                    AI.AddGoal("ApproachMerchant", new FollowPath(new Point(37, 28), 32, 24, 1));
                    AI.Goals["ApproachMerchant"].OnSuccess += () =>
                    {
                        References.Screens.Message.LAYOUT_SetMessage(new UI.MessageData("Stranger", "Introductions", "He cautiously approaches you.\n\"Merchant?\", the man speaks.", icon,
                        new UI.Elements.ButtonOption("Yes", "", () =>
                        {
                            References.Screens.Message.LAYOUT_SetMessage(new UI.MessageData("Stranger", "Introductions", "The man pauses, glancing at your belongings.\nWith a slight bow, he departs.", icon,
                            new UI.Elements.ButtonOption("Continue", "", () =>
                            {
                                Memory.AddMemory("Truth", "Quest", "Introductions");
                                WalkAway(null);

                                World.ExitCutscene();
                            }, null)) { SubtitleColor = subtitle });
                        }, null),
                        new UI.Elements.ButtonOption("No", "", () =>
                        {
                            References.Screens.Message.LAYOUT_SetMessage(new UI.MessageData("Stranger", "Introductions", "With squinted eyes, he studies you and your\nbelongings carefully. He leaves immediately\nwithout another word.", icon,
                            new UI.Elements.ButtonOption("Continue", "", () =>
                            {
                                Stats.Speed = 1000;
                                Memory.AddMemory("Lie", "Quest", "Introductions");
                                WalkAway(null);

                                World.ExitCutscene();
                            }, null))
                            { SubtitleColor = subtitle });
                        }, null)) { SubtitleColor = subtitle });
                    };
                }, null),
                new UI.Elements.ButtonOption("Ignore", "", () =>
                {
                    Memory.AddMemory("Ignored", "Quest", "Introductions");
                    World.ExitCutscene();
                    World.OnFocusedChange += WalkAway;
                }, null)) { SubtitleColor = subtitle });
            });
        }
        private void WalkAway(WorldObject obj)
        {
            IsSelectable = false;
            Expressions.Outro();

            AI.AddGoal("WalkAway", new FollowPath(new Point(19, 37), 32, 24, 1));
            AI.Goals["WalkAway"].OnSuccess += () => Memory.AddMemory(true, "NextQuest");
            World.OnFocusedChange -= WalkAway;
        }
    }
}
