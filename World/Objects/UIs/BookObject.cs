using AetaLibrary.Extensions;
using ExtensionsLibrary.Extensions;
using Merchantry.World.Objects.UIs.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Objects.UIs
{
    public class BookObject : UIObject
    {
        public BookData Data { get; private set; }
        public int Page { get; private set; } = 0;

        public void SetData(BookData data)
        {
            if (data != null)
            {
                Data = data;
                SetPage(0);

                lastPage.Offset = Data.LeftArrowOffset;
                nextPage.Offset = Data.RightArrowOffset;
                leftPage.Offset = Data.LeftNumberOffset;
                rightPage.Offset = Data.RightNumberOffset;
            }
        }
        public void SetPage(int index)
        {
            if (index < Data.Pages.Count)
            {
                Page = index;
                Elements.Clear();
                AddButtons();

                SetPageContent(index);
                leftPage.Text = Data.Pages[index].Name;

                if (index + 1 < Data.Pages.Count)
                {
                    SetPageContent(index + 1);
                    rightPage.Text = Data.Pages[index + 1].Name;
                }
                else
                    rightPage.Text = string.Empty;
            }
        }
        private void SetPageContent(int index)
        {
            for (int i = 0; i < Data.Pages[index].Content.Count; i++)
                AddElement(index + "." + i, Data.Pages[index].Content[i]);
        }

        private ButtonElement lastPage, nextPage;
        private TextElement leftPage, rightPage;
        private void AddButtons()
        {
            if (Page > 0)
            {
                AddElement("lastPage", lastPage);
                lastPage.SetBoxContains(-2.5f, 0, new Vector2(ToScale(lastPage.Background.Width), ToScale(lastPage.Background.Height)));
            }
            if (Page < Data.Pages.Count - 2)
            {
                AddElement("nextPage", nextPage);
                nextPage.SetBoxContains(0, -2.5f, new Vector2(ToScale(nextPage.Background.Width), ToScale(nextPage.Background.Height)));
            }

            AddElement("leftPage", leftPage);
            AddElement("rightPage", rightPage);
        }

        public BookObject(string ID, Vector2 Position, Texture2D Texture, float ScaleLevel, Vector2 FocusOffset) : base(ID, Position, Texture)
        {
            this.FocusScale = ScaleLevel;
            this.FocusOffset = FocusOffset;
        }

        public override void Initialize()
        {
            base.Initialize();

            DisplayName = "Recipe Book";

            IsAcceptingItems = false;
            FocusMultiplier = .1f;

            SCALE_Speed = 10;
            ROTATE_Speed = 5;
        }

        public override void Load(ContentManager cm)
        {
            base.Load(cm);

            lastPage = new ButtonElement(new Vector2(-25, 3), cm.Load<Texture2D>("UI/World/arrow"), null);
            lastPage.Rotation = MathHelper.PiOver2;
            lastPage.OnLeftClick += () => Queue.Add(0, () => SetPage(Page - 2));
            lastPage.OnHoverEnter += () => lastPage.Color = Color.Gray;
            lastPage.OnHoverExit += () => lastPage.Color = new Color(32, 32, 32, 255);
            lastPage.CallHoverExit();

            nextPage = new ButtonElement(new Vector2(5, 6), cm.Load<Texture2D>("UI/World/arrow"), null);
            nextPage.Rotation = -MathHelper.PiOver2;
            nextPage.OnLeftClick += () => Queue.Add(0, () => SetPage(Page + 2));
            nextPage.OnHoverEnter += () => nextPage.Color = Color.Gray;
            nextPage.OnHoverExit += () => nextPage.Color = new Color(32, 32, 32, 255);
            nextPage.CallHoverExit();

            leftPage = new TextElement(new Vector2(-24, 4), cm.Load<SpriteFont>("UI/Fonts/ArchwayKnights_12px"), string.Empty, Color.Black);
            rightPage = new TextElement(new Vector2(4, 4), cm.Load<SpriteFont>("UI/Fonts/ArchwayKnights_12px"), string.Empty, Color.Black);
        }

        public override void Draw(SpriteBatch sb)
        {
            if (Animation.FrameState == "Open")
                DrawElements(sb);

            base.Draw(sb);
        }

        public bool IsDusty { get; set; } = true;
        public override void LeftClick(WorldObject user)
        {
            if (World.Focused != this)
            {
                Queue.Add(500, () =>
                {
                    Animation.FrameState = "Open";
                    ROTATE_Wobble(1f, .1f, 5);
                    //SCALE_Pulse(1.15f, 1, 1, 1000, 1000, 1, 1);
                    SCALE_Loose(new Vector2(1.25f, 1));
                    Queue.Add(100, () => SCALE_Loose(new Vector2(1, 1)));

                    if (IsDusty == true)
                    {
                        AddDust(50, 2);
                        IsDusty = false;
                    }
                    else
                        AddDust(10, 1);
                });
            }

            base.LeftClick(user);
        }
        public override void RightClick(WorldObject user)
        {
            if (IsContainsMouse() == false)
                Animation.FrameState = "Closed";

            base.RightClick(user);
        }

        private float dustCount = 0;
        private void AddDust(int quantity, float velocityMultiplier)
        {
            References.Particles.AddBox("World", quantity, Position - new Vector2(15, 10), new Vector2(20, 15), () =>
            {
                dustCount += World.PixelDepth();
                return new Particles.Types.BookDust(new Vector2(trueRandom.NextFloat(Position.X - 20, Position.X + 5), trueRandom.NextFloat(Position.Y - 7, Position.Y + 7)), velocityMultiplier)
                { Depth = Depth + (World.PixelDepth() * 10) + dustCount, Scale = new Vector2(ToScale(8)) };
            });
            dustCount = 0;
            /*References.Particles.Add("World", quantity, () =>
            {
                return new Particles.Types.BookDust(new Vector2(trueRandom.NextFloat(Position.X - 20, Position.X + 5), trueRandom.NextFloat(Position.Y - 7, Position.Y + 7)), velocityMultiplier)
                    { Depth = Depth + World.PixelDepth() * 5 };
            });*/
        }
    }
    public class BookData
    {
        public string Name { get; set; }
        public List<BookPage> Pages { get; private set; } = new List<BookPage>();

        public Vector2 LeftArrowOffset { get; set; }
        public Vector2 RightArrowOffset { get; set; }
        public Vector2 LeftNumberOffset { get; set; }
        public Vector2 RightNumberOffset { get; set; }

        public void AddPage(int index, Action<BookPage> content)
        {
            BookPage page = new BookPage(index);
            content?.Invoke(page);

            Pages.Add(page);
        }
        public void AddPage(int index, string name, Action<BookPage> content)
        {
            BookPage page = new BookPage(index);
            page.Name = name;
            content?.Invoke(page);

            Pages.Add(page);
        }

        public BookData(string Name)
        {
            this.Name = Name;
        }
    }
    public class BookPage
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public List<Elements.Element> Content { get; set; }

        public BookPage(int Index) : this(Index, new Elements.Element[0]) { }
        public BookPage(int Index, params Elements.Element[] Content) : this(Index, (Index + 1).ToString(), Content: Content) { }
        public BookPage(int Index, string Name, params Elements.Element[] Content)
        {
            this.Index = Index;
            this.Name = Name;
            this.Content = Content.ToList();
        }
    }
}
