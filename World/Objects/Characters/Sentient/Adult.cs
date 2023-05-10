using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Merchantry.World.Meta.Shapes;
using Merchantry.World.Meta.Goals;
using Microsoft.Xna.Framework.Content;
using Merchantry.World.Meta;
using Merchantry.UI.Items;

namespace Merchantry.World.Objects.Characters.Sentient
{
    public class Adult : CharacterObject
    {
        public Adult(string ID, Vector2 Position, Texture2D Texture, string DisplayName) : base(ID, Position, Texture, DisplayName)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            FocusMultiplier = .1f;
            FocusOffset = new Vector2(0, -60);

            ROTATE_Speed = 10;
            IsAcceptingItems = false;

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

            Origin = new Vector2(35, 108);
            Shape.Add("Circle", new Circle(Vector2.Zero, 15) { Group = SHAPE_Collision });
            Stats.Mass = 75;
            Stats.MaxSpeed = 1000;
            Stats.Speed = 750;

            Chat.Add("Greet", () => { return "Hello, merchant."; });
        }

        public override void Load(ContentManager cm)
        {
            base.Load(cm);

            SetChatBox(cm, -130);
            SetExpressions(cm, -110);
        }

        public override void LeftClick(WorldObject user)
        {
            base.LeftClick(user);
        }
    }
}
