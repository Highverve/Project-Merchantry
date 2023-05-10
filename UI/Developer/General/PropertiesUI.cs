using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using AetaLibrary.Elements;
using AetaLibrary.Elements.Text;
using AetaLibrary.Elements.Images;
using Merchantry.UI.Elements;
using Microsoft.Xna.Framework.Content;
using ExtensionsLibrary.Extensions;

namespace Merchantry.UI.Developer.General
{
    public class PropertiesUI : BaseUI
    {
        private Texture2D developerIcons;
        private StretchBoxElement background, buttonWell;
        private ImageElement backButton, refreshButton, homeButton;

        private int section = 0, refreshTimer = 0;
        private List<Element> properties = new List<Element>();
        private List<Element> sideProperties = new List<Element>();
        private List<IProperties> navigation = new List<IProperties>();
        private IProperties selected;
        public IProperties Selected { get { return selected; } }

        public void SetSelected(IProperties properties, bool isSetParent = true)
        {
            Queue.Add(0, () =>
            {
                ClearProperties();
                selected?.NullifyProperties();

                if (properties != null)
                {
                    selected = properties;
                    if (isSetParent == true)
                        navigation.Add(selected);
                }
                else
                {
                    selected = info;
                    navigation.Clear();
                }

                selected.SetProperties(this);
                selected.RefreshProperties();
                refreshTimer = 0;
                background.Y = TopLeft.Y + 40;
                CheckButtonActivity();
            });
        }
        public void GoBack()
        {
            if (navigation.Count > 1)
            {
                SetSelected(navigation[navigation.Count - 2], false);
                navigation.RemoveAt(navigation.Count - 1);
            }
        }
        private void CheckButtonActivity()
        {
            if (Selected != null && navigation.Count > 1)
            {
                backButton.Fade(ColorExt.Raspberry, 15);
                backButton.IsInteract = true;
            }
            else
            {
                backButton.Fade(Color.Gray, 15);
                backButton.IsInteract = false;
                backButton.Buttons.InvokeHoverExit();
            }
        }

        public PropertiesUI(GraphicsDevice Graphics, Vector2 Size) : base(Graphics, Size)
        {
        }

        public override void Load(ContentManager cm)
        {
            developerIcons = cm.Load<Texture2D>("UI/Icons/developerIcons");
            base.Load(cm);
        }
        PropertyInfo info = new PropertyInfo();
        public override void PostInitialize()
        {
            base.PostInitialize();

            ELEMENTS_Add("Background", background = new StretchBoxElement(1, TopLeft + new Vector2(20, 40), wellBlack, Color.White, new Point((int)Size.X - 40, 120), 16));
            background.POSITION_Speed = 15;

            //Black well to contain buttons
            ELEMENTS_Add("ButtonWell", buttonWell = new StretchBoxElement(1, TopLeft + new Vector2(-38, 16), wellBlack, Color.White, new Point(48, 48 * 3), 16));
            CLICKBOX_Add("ButtonWell", buttonWell.Position.ToPoint(), buttonWell.Size);
            buttonWell.TargetRender = "EXTERNAL";

            ELEMENTS_Add("BackButton", backButton = new ImageElement(1, buttonWell.Position + new Vector2(24, 24), developerIcons, new Rectangle(0, 48, 48, 48),
                Color.Lerp(Color.Green, Color.White, .4f), Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            backButton.SetOrigin(.5f, .5f);
            backButton.SCALE_Speed = 10;
            backButton.TargetRender = "EXTERNAL";
            backButton.Buttons.OnHoverEnter += () =>
            {
                backButton.SCALE_Loose(1.25f);
                Screens.Tooltip.LAYOUT_SetText("Back");
            };
            backButton.Buttons.OnHoverExit += () =>
            {
                backButton.SCALE_Loose(1);
                Screens.Tooltip.Minimize();
            };
            backButton.Buttons.OnLeftClick += GoBack;

            ELEMENTS_Add("RefreshButton", refreshButton = new ImageElement(1, buttonWell.Position + new Vector2(24, 72), developerIcons, new Rectangle(48, 48, 48, 48),
                Color.Lerp(Color.Orange, Color.White, .25f), Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            refreshButton.SetOrigin(.5f, .5f);
            refreshButton.SCALE_Speed = 10;
            refreshButton.TargetRender = "EXTERNAL";
            refreshButton.Buttons.OnHoverEnter += () =>
            {
                refreshButton.SCALE_Loose(1.25f);
                Screens.Tooltip.LAYOUT_SetText("Refresh");
            };
            refreshButton.Buttons.OnHoverExit += () =>
            {
                refreshButton.SCALE_Loose(1);
                Screens.Tooltip.Minimize();
            };
            refreshButton.Buttons.OnLeftClick += () =>
            {
                Queue.Add(0, () =>
                {
                    ClearProperties();
                    Selected.SetProperties(this);
                    Selected.RefreshProperties();
                    background.Y = TopLeft.Y + 40;
                });
            };

            ELEMENTS_Add("HomeButton", homeButton = new ImageElement(1, buttonWell.Position + new Vector2(24, 120), developerIcons, new Rectangle(96, 48, 48, 48),
                Color.Lerp(Color.White, Color.White, .25f), Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            homeButton.SetOrigin(.5f, .5f);
            homeButton.SCALE_Speed = 10;
            homeButton.TargetRender = "EXTERNAL";
            homeButton.Buttons.OnHoverEnter += () =>
            {
                homeButton.SCALE_Loose(1.25f);
                Screens.Tooltip.LAYOUT_SetText("Home");
            };
            homeButton.Buttons.OnHoverExit += () =>
            {
                homeButton.SCALE_Loose(1);
                Screens.Tooltip.Minimize();
            };
            homeButton.Buttons.OnLeftClick += () => SetSelected(null);

            IsTitlebarEnabled = true;
            Title = "Properties";
            TitlebarWidth = 160;

            UI_SetOrigin(Center);
            Minimize();

            SetSelected(info);
        }

        public TextElement PROPERTY_AddHeader(string text)
        {
            TextElement element = new TextElement(2, new Vector2(Center.X, TopLeft.Y + 70), font, text,
                Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None);
            element.POSITION_Speed = 15;
            element.SnapMargins = new Vector2(10, 5);
            element.SetOrigin(.5f, 0f);

            AddProperty(section + "Header", element);
            return element;
        }
        public TextElement PROPERTY_AddText(string text, int indent = 0)
        {
            TextElement element = new TextElement(2, new Vector2(TopLeft.X + Indent(indent), 0), smallFont, text,
                Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None);
            element.POSITION_Speed = 15;
            element.SnapMargins = new Vector2(5, 2);

            AddProperty(section + "Label", element);
            return element;
        }
        public NumberElement PROPERTY_AddNumber(string prefixLabel, float number, string format, Action onIncrement, Action onDecrement, int indent = 0, Action onNumberClick = null)
        {
            NumberElement element = new NumberElement(2, new Vector2(TopLeft.X + Indent(indent), 0), smallFont, number, prefixLabel, Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None);
            element.Buttons.OnLeftClick += onNumberClick;
            element.POSITION_Speed = 15;
            element.SnapMargins = new Vector2(5, 2);
            element.Format = format;

            if (onIncrement != null)
            {
                StretchBoxElement increment = new StretchBoxElement(2, new Vector2(TopLeft.X + Size.X - 60, 0), button, Color.White, new Point(26, 26), 16);
                increment.Buttons.OnLeftClick += onIncrement;
                increment.Buttons.OnHoverEnter += () => increment.Texture = buttonHover;
                increment.Buttons.OnHoverExit += () => increment.Texture = button;
                element.OnPositionMove += () => increment.Y = element.Position.Y;
                AddSideProperty(section + "Number_Increment", increment);
            }

            if (onDecrement != null)
            {
                StretchBoxElement decrement = new StretchBoxElement(2, new Vector2(TopLeft.X + Size.X - 90, 0), button, Color.White, new Point(26, 26), 16);
                decrement.Buttons.OnLeftClick += onDecrement;
                decrement.Buttons.OnHoverEnter += () => decrement.Texture = buttonHover;
                decrement.Buttons.OnHoverExit += () => decrement.Texture = button;
                element.OnPositionMove += () => decrement.Y = element.Position.Y;
                AddSideProperty(section + "Number_Decrement", decrement);
            }

            AddProperty(section + "Number", element);
            return element;
        }
        public TextElement PROPERTY_AddButton(string text, Action onClick)
        {
            StretchBoxElement element = new StretchBoxElement(2, new Vector2(TopLeft.X + 30, 0), button, Color.White, new Point((int)Size.X - 60, 30), 16);
            element.Buttons.OnLeftClick += onClick;
            element.Buttons.OnHoverEnter += () => element.Texture = buttonHover;
            element.Buttons.OnHoverExit += () => element.Texture = button;
            element.POSITION_Speed = 15;
            element.SnapMargins = new Vector2(5, 2);

            TextElement label = new TextElement(3, new Vector2(Center.X, 0), smallFont, text,
                new Color(32, 32, 32, 255), Vector2.Zero, Vector2.One, 0, SpriteEffects.None);
            label.Buttons.OnLeftClick += onClick;
            label.POSITION_Speed = 15;
            element.OnPositionMove += () => label.Position = element.Position + (element.Size.ToVector2() / 2);
            label.OnTextChange += () => label.SetOrigin(.5f, .5f);
            label.SetOrigin(.5f, .5f);
            label.IsInteract = false;
            element.InvokePositionMove();

            AddSideProperty(section + "Button_Label", label);
            AddProperty(section + "Button", element);
            return label;
        }
        public TextElement PROPERTY_AddLabelButton(string text, params Action[] onButtonClick)
        {
            TextElement element = new TextElement(2, new Vector2(TopLeft.X + 30, 0), smallFont, text,
                Color.White, Vector2.Zero, Vector2.One, 0, SpriteEffects.None);
            element.POSITION_Speed = 15;
            element.SnapMargins = new Vector2(5, 2);

            for (int i = 0; i < onButtonClick.Length; i++)
            {
                StretchBoxElement button = new StretchBoxElement(2, new Vector2(TopLeft.X + Size.X - (60 + (i * 30)), 0), this.button, Color.White, new Point(26, 26), 16);
                button.Buttons.OnLeftClick += onButtonClick[i];
                button.Buttons.OnHoverEnter += () => button.Texture = buttonHover;
                button.Buttons.OnHoverExit += () => button.Texture = this.button;
                element.OnPositionMove += () => button.Y = element.Position.Y;
                AddSideProperty(section + "Label_Button" + i, button);
            }

            AddProperty(section + "Label", element);
            return element;
        }
        public void PROPERTY_AddButtonRow(params Tuple<Texture2D, string, Action>[] buttons)
        {

        }
        public ImageElement PROPERTY_AddImage(Texture2D texture)
        {
            ImageElement element = new ImageElement(2, new Vector2(Center.X, 0), texture, texture.Bounds, Color.White,
                Vector2.Zero, Vector2.One, 0, SpriteEffects.None);
            element.POSITION_Speed = 15;
            element.SnapMargins = new Vector2(10, 5);
            element.SetOrigin(.5f, 0f);

            AddProperty(section + "Image", element);
            return element;
        }
        public DividerElement PROPERTY_AddDivider(int heightMargin)
        {
            DividerElement element = new DividerElement(2, new Vector2(Center.X - 118, 0), divider, Color.White, new Vector2(48, 6), 4, 28, 32, 200);
            element.POSITION_Speed = 15;
            element.SnapMargins = new Vector2(5, heightMargin);
            element.SnapSize = new Vector2(element.SnapSize.X, divider.Height);
            //element.RefreshSnapSize();

            AddProperty(section + "Divider", element);
            return element;
        }
        public ImageElement PROPERTY_AddSpacer(int spacing)
        {
            ImageElement element = new ImageElement(2, new Vector2(TopLeft.X + 20, 0), pixel, pixel.Bounds,
                Color.Transparent, Vector2.Zero, new Vector2(Size.X - 40, spacing), 0, SpriteEffects.None);
            element.POSITION_Speed = 15;
            element.SnapMargins = Vector2.Zero;
            element.RefreshSnapSize();

            AddProperty(section + "Spacer", element);
            return element;
        }

        public int PROPERTIES_IndexOf(Element element)
        {
            return properties.IndexOf(element);
        }
        public void PROPERTIES_Remove(int index, int length)
        {
            for (int i = index; i < index + length; i++)
                PROPERTIES_Remove(i);
        }
        public void PROPERTIES_Remove(int index)
        {
            Element element = properties[index];
            PROPERTIES_Remove(element);
        }
        public void PROPERTIES_Remove(Element element)
        {
            properties.Remove(element);
            ELEMENTS_Remove(element);
        }

        private void AddProperty(string name, Element element)
        {
            properties.Add(element);
            ELEMENTS_Add(name, element);
            section++;

            RefreshPositions();
            RefreshSize();
        }
        private void AddSideProperty(string name, Element element)
        {
            sideProperties.Add(element);
            ELEMENTS_Add(name, element);
            section++;
        }
        public void ClearProperties()
        {
            for (int i = 0; i < properties.Count; i++)
                ELEMENTS_Remove(properties[i]);
            properties.Clear();

            for (int i = 0; i < sideProperties.Count; i++)
                ELEMENTS_Remove(sideProperties[i]);
            sideProperties.Clear();

            section = 0;
        }

        private void RefreshSize()
        {
            if (properties.Count > 0)
            {
                Element last = properties.Last();
                background.Size = new Point((int)Size.X - 40, (int)((last.Y + last.SnapSize.Y + last.SnapMargins.Y) - properties.First().Y) + 20);
            }
        }
        public void RefreshPositions()
        {
            for (int i = 0; i < properties.Count; i++)
                properties[i].Y = IndexPosition(properties[i]);
        }
        private float IndexPosition(Element element)
        {
            int index = properties.IndexOf(element);
            if (index > 0)
            {
                Element last = properties[index - 1];
                return last.ExteriorBottomVector(element).Y;
            }
            else
                return background.Y + 10;
        }
        private float Indent(int count) { return 30 + (count * 10); }
        public void Scroll(int direction)
        {
            if (properties.Count > 0)
                background.Y = background.Y + (direction * 50);
            RefreshPositions();
        }
        public void MoveTo(Element element, int index)
        {
            properties.Remove(element);
            properties.Insert(index, element);

            RefreshPositions();
            RefreshSize();
        }
        /// <summary>
        /// Positions the element below the target
        /// </summary>
        /// <param name="element"></param>
        /// <param name="target"></param>
        public void MoveTo(Element element, Element target)
        {
            MoveTo(element, properties.IndexOf(target));
        }

        public override void Update(GameTime gt)
        {
            base.Update(gt);

            if (IsMaximized == true)
            {
                if (CLICKBOX_Contains(Controls.MouseVector()))
                {
                    if (Controls.ScrollDirection() > 0)
                        Scroll(1);
                    if (Controls.ScrollDirection() < 0)
                        Scroll(-1);
                }

                refreshTimer += gt.ElapsedGameTime.Milliseconds;
                if (refreshTimer >= 250)
                {
                    Selected.RefreshProperties();
                    refreshTimer = 0;
                }
            }
        }
        protected override void DrawInterior(SpriteBatch sb)
        {
            DrawElements(sb);
        }
        protected override void DrawExterior(SpriteBatch sb)
        {
            buttonWell.Draw(sb);
            backButton.Draw(sb);
            refreshButton.Draw(sb);
            homeButton.Draw(sb);
        }
    }
    public interface IProperties
    {
        void SetProperties(PropertiesUI ui);
        void RefreshProperties();
        void NullifyProperties();
    }
    class PropertyInfo : IProperties
    {
        public virtual void SetProperties(PropertiesUI ui)
        {
            ui.PROPERTY_AddHeader("How To");
            ui.PROPERTY_AddText("1. Select Element\n2. Configure\n3. Done!", 2);
        }
        public void RefreshProperties() { }
        public void NullifyProperties() { }
    }
}
