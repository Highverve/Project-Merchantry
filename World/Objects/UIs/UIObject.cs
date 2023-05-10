using AetaLibrary;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Merchantry.UI.Items;
using Microsoft.Xna.Framework.Content;
using Merchantry.World.Objects.UIs.Elements;
using AetaLibrary.Extensions;

namespace Merchantry.World.Objects.UIs
{
    public class UIObject : RenderObject
    {
        protected Texture2D pixel;
        protected SpriteFont font, smallFont;

        public bool IsActive { get; set; }
        public float ToScale(float pixel) { return pixel / FocusScale; }

        #region Elements

        public Dictionary<string, Element> Elements { get; set; } = new Dictionary<string, Element>(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, ItemSlot> Slots { get; set; } = new Dictionary<string, ItemSlot>(StringComparer.OrdinalIgnoreCase);

        public void AddElement(string name, Element element)
        {
            if (!Elements.ContainsKey(name))
            {
                element.Name = name;
                element.UI = this;
                element.Initialize();

                Elements.Add(name, element);
            }
        }
        public void RemoveElement(string name)
        {
            if (Elements.ContainsKey(name))
                Elements.Remove(name);
        }

        public void AddSlot(string name, ItemSlot slot)
        {
            if (!Slots.ContainsKey(name) &&
                !Elements.ContainsKey(name))
            {
                Slots.Add(name, slot);
                AddElement(name, slot);
            }
        }
        public void RemoveSlot(string name)
        {
            if (Slots.ContainsKey(name))
                Slots.Remove(name);

            RemoveElement(name);
        }

        public int SLOTS_FullCount()
        {
            int result = 0;
            foreach (ItemSlot slot in Slots.Values)
                if (slot.Item != null)
                    result++;
            return result;
        }
        public bool SLOTS_HasItemID(string id)
        {
            foreach (ItemSlot slot in Slots.Values)
                if (slot.Item != null && slot.Item.ID == id)
                    return true;
            return false;
        }
        public bool IsContainsMouse()
        {
            foreach (Element e in Elements.Values)
                if ((e.IsLeftClickNull() == false || e.IsRightClickNull() == false) &&
                    e.IsHovering == true)
                    return true;

            return false;
        }

        #endregion

        #region Events

        protected event Action onInputChange, onOutputChange;
        public event Action OnInputChange { add { onInputChange += value; } remove { onInputChange -= value; } }
        public event Action OnOutputChange { add { onOutputChange += value; } remove { onOutputChange -= value; } }
        public void InvokeInputChange() { onInputChange?.Invoke(); }
        public void InvokeOutputChange() { onOutputChange?.Invoke(); }

        #endregion

        public UIObject(string ID, Vector2 Position, Texture2D Texture) : base(ID, Position, Texture) { }

        public override void Load(ContentManager cm)
        {
            //Temporary --- move lower soon
            pixel = cm.Load<Texture2D>("Debug/pixel");
            font = cm.Load<SpriteFont>("UI/Fonts/ArchwayKnights_19px");
            smallFont = cm.Load<SpriteFont>("UI/Fonts/ArchwayKnights_12px");

            base.Load(cm);
        }
        public override void Update(GameTime gt)
        {
            base.Update(gt);

            foreach (Element e in Elements.Values)
            {
                if (e.IsEnabled)
                {
                    e.Update(gt);

                    if (IsActive && References.Screens.UI_IsMouseInsideUI() == false)
                    {
                        if (e.IsContains(Camera.ToWorld(Controls.MouseVector())))
                        {
                            if (e.IsHovering == false)
                            {
                                e.CallHoverEnter();
                                e.IsHovering = true;
                            }

                            if (Controls.IsMouseClicked(Controls.CurrentMS.LeftButton))
                                e.CallLeftClick();
                            if (Controls.IsMouseClicked(Controls.CurrentMS.RightButton))
                                e.CallRightClick();
                        }
                        else if (e.IsHovering == true)
                        {
                            e.CallHoverExit();
                            e.IsHovering = false;
                        }
                    }
                }
            }
        }
        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
        }
        protected void DrawElements(SpriteBatch sb)
        {
            foreach (Element e in Elements.Values)
            {
                if (e.IsVisible)
                    e.Draw(sb);
            }
        }

        public void SetSlotClick(ItemSlot s)
        {
            s.OnLeftClick += () =>
            {
                if (s.IsLocked == false)
                {
                    //Slot- / Drag+
                    if (s.Item == null && GameManager.References().Screens.Drag.IsHolding())
                    {
                        Queue.Add(16, () =>
                        {
                            if (Controls.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl))
                            {
                                //Not complete!
                                s.Item = GameManager.References().Screens.Drag.TakeItem(1);
                                InvokeInputChange();
                            }
                            else
                            {
                                ItemObject item = GameManager.References().Screens.Drag.TakeItem();
                                //Storage.AddItem(item);
                                s.Item = item;

                                InvokeInputChange();
                            }
                        });
                    } //Slot+ / Drag-
                    else if (s.Item != null && GameManager.References().Screens.Drag.IsHolding() == false)
                    {
                        Queue.Add(16, () =>
                        {
                            if (Controls.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl))
                                GameManager.References().Screens.Drag.AddItem(s.Item, 1);
                            else
                            {
                                GameManager.References().Screens.Drag.AddItem(s.Item);
                                //Storage.RemoveItem(s.Item);
                                s.Item = null;
                            }

                            InvokeInputChange();
                        });
                    } //Both
                    else if (s.Item != null && GameManager.References().Screens.Drag.IsHolding())
                    {
                        Queue.Add(16, () =>
                        {
                            if (s.Item.StackID() == References.Screens.Drag.Item().StackID())
                            {
                                if (Controls.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftControl))
                                    s.Item.Merge(References.Screens.Drag.TakeItem(1));
                                else
                                {
                                    ItemObject taken = References.Screens.Drag.TakeItem();
                                    s.Item.Merge(taken);

                                    if (taken.Quantity > 0)
                                        References.Screens.Drag.AddItem(taken);
                                }
                            }
                            else
                            {
                                ItemObject take = GameManager.References().Screens.Drag.TakeItem();
                                GameManager.References().Screens.Drag.AddItem(s.Item);
                                s.Item = take;

                                InvokeInputChange();
                            }
                        });
                    }
                }
            };
        }

        public override void HoverEnter(WorldObject user)
        {
            if (IsActive == false)
            {
                base.HoverEnter(user);
                SmoothScale.Y.SetLoose(1.05f);
                SCALE_Speed = 10;

                References.Screens.Tooltip.LAYOUT_SetText(DisplayName);
                References.Screens.Tooltip.Maximize();
            }
        }
        public override void HoverExit(WorldObject user)
        {
            SCALE_Loose(1);
            References.Screens.Tooltip.Minimize();

            base.HoverExit(user);
        }
        public override void SetFocus()
        {
            Camera.MinMoveDistance = .01f;
            Camera.ScaleSpeed = 2f;
            Camera.MoveSpeed = 5f;

            base.SetFocus();
        }
        public override void LeftClick(WorldObject user)
        {
            if (World.Focused != this)
            {
                SetFocus();

                IsActive = true;
                IsAcceptingItems = false;
                HoverExit(this);
            }

            base.LeftClick(user);
        }
        public override void RightClick(WorldObject user)
        {
            if (IsContainsMouse() == false)
            {
                World.Focused = World.Controlled;
                Camera.TargetScale = new Vector2(2f);
                Camera.ScaleSpeed = 7.5f;
                Camera.MinMoveDistance = 1f;

                IsActive = false;
                IsAcceptingItems = true;

                base.RightClick(user);
            }
        }
        public override void ItemClick(ItemObject item, WorldObject user)
        {
            if (IsActive == false)
            {
                bool isTaken = false;
                foreach (ItemSlot slot in Slots.Values)
                {
                    if (slot.IsOpenDefault && slot.Item == null)
                    {
                        slot.Item = item;
                        isTaken = true;
                        break;
                    }
                }

                //No takers, spit it back out.
                if (isTaken == false)
                    SpitItem(item, new Vector2(0, 50));
            }

            base.ItemClick(item, user);
        }

        public void SpitItem(ItemObject item, Vector2 offset, int xVelocity = 50, int yVelocity = 50)
        {
            Queue.Add(0, () =>
            {
                WorldItem obj = new WorldItem(item.StackID() + item.RandomID, Position + offset, item);
                obj.Physics.AddVelocity(trueRandom.Next(-xVelocity, xVelocity), trueRandom.Next(-yVelocity, yVelocity));
                obj.Physics.Jump(300);

                World.AddObject(obj);
            });
        }
        public void EmptySlots(Vector2 offset)
        {
            int delay = 0;
            foreach (ItemSlot slots in Slots.Values)
            {
                if (slots != null && slots.Item != null)
                {
                    Queue.Add(delay, () =>
                    {
                        SpitItem(slots.Item, offset);
                        slots.Item = null;
                    });
                    delay += 150;
                }
            }
        }
    }
}
