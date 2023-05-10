using Merchantry.Assets.Meta;
using Merchantry.UI.Items;
using Merchantry.World.Objects.UIs.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Content;
using Merchantry.UI;
using Merchantry.UI.Elements;

namespace Merchantry.World.Objects.UIs
{
    public class CraftingObject : UIObject
    {
        public Vector2 GridOffset { get; set; } = new Vector2(-6f, -45);

        public Recipe CurrentRecipe { get; set; }
        public Recipe GetRecipe()
        {
            foreach (Recipe r in GameManager.References().Assets.Recipes.Crafting.Values)
            {
                bool isResult = true;
                foreach (Ingredient i in r.Input)
                {
                    ItemSlot s = Slots[i.SlotName];
                    if (s.Item != null && s.Item.IsDestroyed == true)
                    {
                        isResult = false;
                        break;
                    }

                    if (i.Requirement(s.Item) == false)
                    {
                        isResult = false;
                        break;
                    }
                }

                if (isResult == true)
                    return r;
            }
            return null;
        }

        private Color backgroundColor = new Color(204, 172, 153);
        private Color hoverColor = new Color(153, 128, 114);
        private Color gray = Color.Lerp(Color.Black, Color.White, .25f);

        public ItemSlot OutputLeft { get; set; }
        public ItemSlot OutputMiddle { get; set; }
        public ItemSlot OutputRight { get; set; }
        public ButtonElement Craft { get; set; }

        private Texture2D craft, noCraft;
        private int craftTimer = 0, craftCount = 0;
        public ImageElement Timer { get; set; }

        public CraftingObject(string ID, Vector2 Position, Texture2D Texture, Vector2 GridOffset) : base(ID, Position, Texture)
        {
            this.GridOffset = GridOffset;
        }

        public override void Initialize()
        {
            DisplayName = "Crafting";
            FocusMultiplier = .1f;
            FocusScale = 16;
            FocusOffset = new Vector2(-15, -32);

            onInputChange += () =>
            {
                CurrentRecipe = GetRecipe();
                Craft.CallHoverExit();

                if (CurrentRecipe != null)
                    Craft.Icon = craft;
                else
                    Craft.Icon = noCraft;
            };

            base.Initialize();
        }

        public override void Load(ContentManager cm)
        {
            base.Load(cm);

            Point grid = new Point(0, 0);
            for (int i = 0; i < 9; i++)
            {
                ItemSlot s = new ItemSlot(new Vector2(grid.X * ToScale(72), grid.Y * ToScale(72)) + GridOffset, pixel, font);
                s.Size = new Vector2(ToScale(64));

                SetSlotClick(s);
                SetInputHover(s);
                s.CallHoverExit();

                AddSlot((grid.X + 1) + "," + (grid.Y + 1), s);

                grid.X++;
                if (grid.X > 2)
                {
                    grid.Y++;
                    grid.X = 0;
                    if (grid.Y > 2)
                        grid.Y = 0;
                }
            }

            OutputLeft = new ItemSlot(new Vector2(0, ToScale(72) * 4) + GridOffset, pixel, font);
            OutputMiddle = new ItemSlot(new Vector2(ToScale(72), ToScale(72) * 4) + GridOffset, pixel, font);
            OutputRight = new ItemSlot(new Vector2(ToScale(72) * 2, ToScale(72) * 4) + GridOffset, pixel, font);

            OutputLeft.Size = new Vector2(ToScale(64));
            OutputMiddle.Size = new Vector2(ToScale(64));
            OutputRight.Size = new Vector2(ToScale(64));

            OutputLeft.IsOpenDefault = false;
            OutputMiddle.IsOpenDefault = false;
            OutputRight.IsOpenDefault = false;

            SetOutputClick(OutputLeft);
            SetOutputClick(OutputMiddle);
            SetOutputClick(OutputRight);

            SetOutputHover(OutputLeft);
            SetOutputHover(OutputMiddle);
            SetOutputHover(OutputRight);

            OutputLeft.CallHoverExit();
            OutputMiddle.CallHoverExit();
            OutputRight.CallHoverExit();

            AddSlot("OutputLeft", OutputLeft);
            AddSlot("OutputMiddle", OutputMiddle);
            AddSlot("OutputRight", OutputRight);

            craft = cm.Load<Texture2D>("UI/World/craftIcon");
            noCraft = cm.Load<Texture2D>("UI/World/exitIcon");
            Craft = new ButtonElement(new Vector2(ToScale(72) * 4, ToScale(72)) + GridOffset, cm.Load<Texture2D>("UI/World/button"), noCraft);
            Craft.OnLeftClick += () =>
            {
                if (CurrentRecipe != null)
                    craftCount++;
            };
            Craft.OnRightClick += () =>
            {
                if (CurrentRecipe != null && craftCount > 0)
                    craftCount--;

                if (craftCount == 0)
                {
                    Timer.Source = Rectangle.Empty;
                    craftTimer = 0;
                }
            };
            Craft.OnHoverEnter += () =>
            {
                Craft.Color = hoverColor;

                if (CurrentRecipe != null)
                {
                    if (World.Controlled.Memory.Contains(Meta.ObjectMemory.Crafted, CurrentRecipe.Name))
                    {
                        References.Screens.Tooltip.LAYOUT_SetText(CurrentRecipe.Name);
                        References.Screens.Tooltip.Maximize();
                    }
                    else
                    {
                        References.Screens.Tooltip.LAYOUT_SetText("???");
                        References.Screens.Tooltip.Maximize();
                    }
                }
            };
            Craft.OnHoverExit += () =>
            {
                Craft.Color = backgroundColor;
                References.Screens.Tooltip.Minimize();
            };
            Craft.CallHoverExit();
            Craft.IconColor = Color.White;

            AddElement("Craft", Craft);

            AddElement("Arrow", new ImageElement(new Vector2(ToScale(72), ToScale(72) * 3) + GridOffset, cm.Load<Texture2D>("UI/World/arrow")));
            ((ImageElement)Elements["Arrow"]).Color = gray;
            Queue.Add(0, () => ((ImageElement)Elements["Arrow"]).DepthOffset = World.PixelDepth() * 2);

            AddElement("ArrowTimer", Timer = new ImageElement(new Vector2(ToScale(72), ToScale(72) * 3) + GridOffset, cm.Load<Texture2D>("UI/World/arrow")));
            Timer.IsShadowed = false;
            Timer.Color = Color.Lerp(Color.Green, Color.White, .25f);
            Timer.Source = Rectangle.Empty;
            Timer.DepthOffset = World.PixelDepth() * 3;
        }

        public void SetInputClick(ItemSlot s)
        {
            s.OnLeftClick += () =>
            {
                if (s.IsLocked == false)
                {
                    if (s.Item == null && GameManager.References().Screens.Drag.IsHolding())
                    {
                        Queue.Add(16, () =>
                        {
                            s.Item = GameManager.References().Screens.Drag.TakeItem();
                            InvokeInputChange();
                        });
                    }
                    else if (s.Item != null && GameManager.References().Screens.Drag.IsHolding() == false)
                    {
                        Queue.Add(16, () =>
                        {
                            GameManager.References().Screens.Drag.AddItem(s.Item);
                            s.Item = null;

                            InvokeInputChange();
                        });
                    }
                    else if (s.Item != null && GameManager.References().Screens.Drag.IsHolding())
                    {
                        Queue.Add(16, () =>
                        {
                            ItemObject take = GameManager.References().Screens.Drag.TakeItem();
                            GameManager.References().Screens.Drag.AddItem(s.Item);
                            s.Item = take;

                            InvokeInputChange();
                        });
                    }
                }
            };
        }
        public void SetInputHover(ItemSlot s)
        {
            s.OnHoverEnter += () =>
            {
                s.BackgroundColor = hoverColor;
            };
            s.OnHoverExit += () =>
            {
                s.BackgroundColor = backgroundColor;
            };
        }
        public void SetOutputClick(ItemSlot s)
        {
            s.OnLeftClick += () =>
            {
                if (s.Item != null && GameManager.References().Screens.Drag.IsHolding() == false)
                {
                    Queue.Add(16, () =>
                    {
                        GameManager.References().Screens.Drag.AddItem(s.Item);
                        s.Item = null;

                        InvokeOutputChange();
                        s.CallHoverEnter();
                    });
                }
            };
        }
        public void SetOutputHover(ItemSlot s)
        {
            s.OnHoverEnter += () =>
            {
                if (s.Item != null)
                    s.BackgroundColor = hoverColor;
                else
                    s.BackgroundColor = Color.Lerp(gray, Color.White, .25f);
            };
            s.OnHoverExit += () =>
            {
                if (s.Item != null)
                    s.BackgroundColor = backgroundColor;
                else
                    s.BackgroundColor = gray;
            };
        }
        public void SetOutputItem(ItemSlot s, int index)
        {
            if (CurrentRecipe.Output.Length > index)
            {
                ItemObject copy = CurrentRecipe.Output[index].Copy();
                if (s.Item != null)
                {
                    //Same stack ...
                    if (s.Item.StackID() == copy.StackID())
                    {
                        s.Item.Merge(copy);
                        if (copy.Quantity > 0)
                            SpitItem(copy, new Vector2(0, 50));
                    }
                    else
                        SpitItem(copy, new Vector2(0, 50));
                }
                else
                    s.Item = copy;

                s.CallHoverExit();
            }
        }
        private void CraftItem()
        {
            if (CurrentRecipe != null)
            {
                SetOutputItem(OutputLeft, 0);
                SetOutputItem(OutputMiddle, 1);
                SetOutputItem(OutputRight, 2);

                if (CurrentRecipe.Output.Length > 3)
                {
                    for (int i = 3; i < CurrentRecipe.Output.Length; i++)
                        SpitItem(CurrentRecipe.Output[i], new Vector2(0, 50));
                }

                foreach (Ingredient i in CurrentRecipe.Input)
                {
                    ItemSlot s = Slots[i.SlotName];
                    i.Crafted?.Invoke(s.Item);
                }

                World.Controlled.Memory.AddMemory(true, Meta.ObjectMemory.Crafted, CurrentRecipe.Name);

                InvokeInputChange();
            }
        }

        public override void Update(GameTime gt)
        {
            if (craftCount > 0)
            {
                craftTimer += gt.ElapsedGameTime.Milliseconds;

                if (CurrentRecipe != null)
                {
                    Timer.Source = new Rectangle(0, 0, Timer.Texture.Width, (int)(Timer.Texture.Height * ((float)craftTimer / CurrentRecipe.CraftTime)));

                    if (craftTimer >= CurrentRecipe.CraftTime)
                    {
                        CraftItem();

                        craftCount--;
                        craftTimer = 0;
                    }
                }
                else
                {
                    Timer.Source = Rectangle.Empty;

                    craftCount = 0;
                    craftTimer = 0;
                }
            }

            base.Update(gt);
        }

        public override void Draw(SpriteBatch sb)
        {
            DrawElements(sb);

            if (craftCount > 0)
                sb.DrawString(font, craftCount.ToString(), Craft.CombinedPosition() + new Vector2(ToScale(60), ToScale(68)), Color.White, 0, font.MeasureString(craftCount.ToString()), ToScale(1), SpriteEffects.None, Depth + (World.PixelDepth() * 3));

            base.Draw(sb);
        }

        public override void RightClick(WorldObject user)
        {
            if (World.Focused != this)
            {
                if (SLOTS_FullCount() > 0)
                {
                    ButtonOption emptyOption = new ButtonOption("Empty", "Temp", () =>
                    {
                        EmptySlots(new Vector2(0, 50));
                    }, null);

                    References.Screens.Context.LAYOUT_Custom(emptyOption);
                    References.Screens.Context.Maximize();
                }
            }

            base.RightClick(user);
        }
    }
}
