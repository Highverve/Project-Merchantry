using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Merchantry.World.Objects.UIs.Elements;

namespace Merchantry.World.Objects.UIs
{
    public class LibraryObject : UIObject
    {
        public Vector2 LibraryFocus { get; set; }

        public BookObject CurrentBook { get; private set; }

        public LibraryObject(string ID, Vector2 Position, Texture2D Texture, float ScaleLevel, Vector2 FocusOffset) : base(ID, Position, Texture)
        {
            this.FocusScale = ScaleLevel;
            this.FocusOffset = FocusOffset;
        }

        public override void Initialize()
        {
            base.Initialize();

            IsAcceptingItems = false;
            FocusMultiplier = .1f;
        }

        public override void Load(ContentManager cm)
        {
            base.Load(cm);

            ButtonElement recipeBook = new ButtonElement(new Vector2(ToScale(-180), ToScale(-320)), cm.Load<Texture2D>("Assets/Objects/bookRed"), null);
            AddElement("Recipes", recipeBook);
            recipeBook.OnLeftClick += () =>
            {
                CurrentBook.SCALE_Loose(1, 1);
                Queue.Add(500, () => CurrentBook.LeftClick(this));
            };
            recipeBook.OnHoverEnter += () => recipeBook.Color = Color.White;
            recipeBook.OnHoverExit += () => recipeBook.Color = Color.Lerp(Color.White, Color.Gray, .25f);
            recipeBook.CallHoverExit();
            recipeBook.IconColor = Color.White;
        }

        public override void Draw(SpriteBatch sb)
        {
            DrawElements(sb);

            base.Draw(sb);
        }
    }
}
