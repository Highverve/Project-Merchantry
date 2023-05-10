using Merchantry.World.Objects;
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
using Merchantry.World.Objects.Characters;
using Merchantry.World.Meta;
using Merchantry.World.Meta.Goals;
using Merchantry.UI.Developer.General;

namespace Merchantry.World
{
    public class PlayerObject : CharacterObject
    {
        public PlayerObject(string ObjectID, Vector2 Position, Texture2D Texture)
            : base(ObjectID, Position, Texture, "Player") { }

        public override void Initialize()
        {
            base.Initialize();

            FocusMultiplier = .1f;
            FocusOffset = new Vector2(0, -60);

            ROTATE_Speed = 10;
            Stats.MaxSpeed = 1000;
            Stats.Speed = 750;

            Animation.FrameSize = new Point(64, 128);
            Animation.AddState("Idle",
            () =>
            {
                Animation.CurrentFrame = Point.Zero;
                Animation.FrameSpeed = 800;
                SCALE_Speed = 5f;
            },
            () =>
            {
                if (Scale.Y >= .99f)
                    SmoothScale.Y.SetLoose(.95f, .0001f);
                if (Scale.Y <= .96f)
                    SmoothScale.Y.SetLoose(1f, .0001f);
            });
            Animation.AddState("Walk",
            () =>
            {
                Animation.FrameSpeed = 200;
                Animation.CurrentFrame = new Point(1, 0);
                SCALE_Speed = 10f;
            },
            () =>
            {
                Animation.FrameSpeed = 275 - (int)Physics.Velocity.Length();
                Animation.X++;
                if (Animation.X > 1)
                    Animation.X = 0;
            });

            OnPositionMove += (gt) =>
            {
                if (Physics.IsAir() == true)
                {
                    Animation.CurrentFrame = new Point(1, 0);
                    Animation.Pause(50);
                }

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

                Animation.FrameState = "Idle";
            };

            //AI.Goals["Pathfinding"].OnSuccess += (g) => g.IsDestruct = false;
            //((FollowPath)AI.Goals[""]).End =
        }
        public override void Load(ContentManager cm)
        {
            base.Load(cm);

            SetChatBox(cm, -130);
            SetExpressions(cm, -110);

            Storage.OnTabMaximum += (i) =>
            {
                World.SpawnItem(i, Position + new Vector2(0, 32));
                References.Screens.HUD.SendNotification(References.Screens.Message.ICON_Note, "Too many items!");
            };
        }

        public override void UpdateControls(GameTime gt)
        {
        }

        public override void LeftClick(WorldObject user)
        {
            World.SetControlled(this);
            World.Focused = this;
            Camera.TargetScale = new Vector2(2);

            base.LeftClick(user);
        }
        public override void ItemClick(ItemObject item, WorldObject user)
        {
            Storage.AddItem(item);

            base.ItemClick(item, user);
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
        }

        public override void SetProperties(PropertiesUI ui)
        {
            base.SetProperties(ui);
        }
    }
}
