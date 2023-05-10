using Merchantry.World.Meta;
using Merchantry.World.Objects.Various;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Objects.Characters
{
    public class CharacterObject : RenderObject
    {
        protected ChatBubble chatBox;
        protected void SetChatBox(ContentManager cm, float height)
        {
            chatBox = new ChatBubble(ID + "_Chat", Position, cm.Load<Texture2D>("UI/HUD/chatboxWhite"),
                                     cm.Load<SpriteFont>("UI/Fonts/ArchwayKnights_12px"), this);
            World.AddObject(chatBox);

            chatBox.Attach(this, new Vector2(0, height));
        }
        public Expression Expressions { get; private set; }
        protected void SetExpressions(ContentManager cm, float height)
        {
            Expressions = new Expression(ID + "_Expressions", Position, this);
            World.AddObject(Expressions);
            Expressions.Attach(this, new Vector2(0, height));
        }

        public CharacterObject(string ID, Vector2 Position, Texture2D Texture, string DisplayName) : base(ID, Position, Texture)
        {
            this.DisplayName = DisplayName;
        }

        public override void Initialize()
        {
            base.Initialize();

            Storage.OnAddItem += (i) => Memory.AddMemory(i.Name, ObjectMemory.HasItem, i.ID);
        }
        public override void Load(ContentManager cm)
        {
            base.Load(cm);
        }
        public override void Update(GameTime gt)
        {
            base.Update(gt);
        }
        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
        }

        public void FaceToward(int direction)
        {
            SmoothScale.X.SetLoose(direction, .0001f);
        }
        public void TRANSITION_FlipByVelocity(int multi = 1)
        {
            if (Physics.Velocity.X > 0) FaceToward(-1 * multi);// SmoothScale.X.SetLoose(-1 * multi, .0001f);
            if (Physics.Velocity.X < 0) FaceToward(1 * multi);//SmoothScale.X.SetLoose(1 * multi, .0001f);
        }
        public void TRANSITION_LeanByVelocity()
        {
            ROTATE_Loose(Physics.Velocity.X * -.001f);
            SmoothScale.Y.SetLoose(1 + (Physics.Velocity.Y * .001f));
        }
        public void TRANSITION_WobbleByVelocity()
        {
            //ROTATE_Loose(Math.Cos(Position.X));
            //SmoothScale.Y.SetLoose(1 + (Physics.Velocity.Y * .001f));
        }

        protected override void DestroyObjects()
        {
            base.DestroyObjects();

            if (Expressions != null)
                World.RemoveObject(Expressions);
            if (chatBox != null)
                World.RemoveObject(chatBox);
        }
    }
}
