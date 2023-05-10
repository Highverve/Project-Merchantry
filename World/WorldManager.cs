using AetaLibrary;
using ExtensionsLibrary._2D;
using ExtensionsLibrary.Extensions;
using ExtensionsLibrary.Input;
using ExtensionsLibrary.Support;
using Merchantry.Extensions;
using Merchantry.Particles.Types;
using Merchantry.UI.Items;
using Merchantry.World.Meta.Goals;
using Merchantry.World.Meta.Shapes;
using Merchantry.World.Objects;
using Merchantry.World.Objects.Characters;
using Merchantry.World.Objects.Characters.Creatures;
using Merchantry.World.Objects.Characters.Sentient;
using Merchantry.World.Objects.Flora;
using Merchantry.World.Objects.UIs;
using Merchantry.World.Objects.UIs.Elements;
using Merchantry.World.Objects.Various;
using Merchantry.World.Pathfinding;
using Merchantry.World.Pathfinding.Tiles;
using Merchantry.World.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World
{
    public class WorldManager
    {
        private GraphicsDevice graphics;
        public ContentManager Content { get; private set; }

        private Effect outline;

        public RenderTarget2D FlatTarget, ObjectTarget;

        public Controls Controls { get; private set; }
        public ExtensionsLibrary._2D.Camera Camera { get; private set; }
        public TimeQueue Queue { get; private set; }
        public WorldSectors Sectors { get; private set; }
        public PathManager Pathfinder { get; private set; }
        public SquareGrid NavGrid { get; set; } = new SquareGrid();

        private References references;

        public Random LevelRandom { get; private set; }
        public string RandomStamp(int length = 8) { return LevelRandom.NextString("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890", length); }

        public Dictionary<string, WorldObject> Objects { get; set; }

        #region Layers

        public Dictionary<string, TileLayer> Layers { get; set; }

        public void AddLayer(string key, TileLayer layer)
        {
            if (Layers.ContainsKey(key))
                throw new Exception("Layers already contain a tile layer with \"" + key + "\" key.");

            layer.Name = key;
            layer.World = this;

            Layers.Add(key, layer);
            references.Screens.Layers.AddLayer(layer);
        }
        public void RemoveLayer(string key)
        {
            if (Layers.ContainsKey(key))
            {
                references.Screens.Layers.RemoveLayer(Layers[key]);
                Layers.Remove(key);
            }
        }
        public void RemoveLayer(TileLayer layer) { RemoveLayer(layer.Name); }
        public void RenameLayer(string key, string newKey)
        {
            if (Layers.ContainsKey(key))
            {
                TileLayer layer = Layers[key];
                RemoveLayer(key);
                AddLayer(key, layer);
            }
        }

        #endregion

        public const int TileWidth = 64;
        public const int TileHeight = 64;
        public Point MapSize { get; private set; }
        public float CalculateDepth(float y) { return (y / MapSize.Y); }
        public float PixelDepth() { return CalculateDepth(1);; }
        public Vector2 FromTile(int x, int y) { return new Vector2(TileWidth * x, TileHeight * y); }
        public Vector2 FromTile(Point coords) { return FromTile(coords.X, coords.Y); }

        public bool IsOpenSpaceMouse() { return selected == null; }
        public bool IsOpenSpace(Vector2 position)
        {
            foreach (WorldObject obj in Objects.Values)
            {
                if (obj.IsContains(position))
                    return false;
            }
            return true;
        }

        public WorldObject Controlled { get; private set; }
        private event Action<WorldObject> onControlledChange;
        public event Action<WorldObject> OnControlledChange { add { onControlledChange += value; } remove { onControlledChange -= value; } }
        public void SetControlled(WorldObject controlled)
        {
            if (Controlled != controlled)
            {
                Controlled = controlled;
                onControlledChange?.Invoke(Controlled);

                references.Screens.Backpack.SetStorage(Controlled.Storage);
                //references.Screens.Inventory.SetStorage(Controlled.Storage);
                references.Screens.HUD.SetStorage(Controlled.Storage);
            }
        }

        private WorldObject focused;
        public WorldObject Focused
        {
            get { return focused; }
            set
            {
                if (focused != value)
                {
                    focused = value;

                    if (focused != null)
                        onFocusedChange?.Invoke(focused);
                }
            }
        }
        private event Action<WorldObject> onFocusedChange;
        public event Action<WorldObject> OnFocusedChange { add { onFocusedChange += value; } remove { onFocusedChange -= value; } }

        private WorldObject selected;
        public WorldObject Selected
        {
            get { return selected; }
            set
            {
                if (selected != value)
                {
                    //Old
                    if (selected != null)
                        selected.HoverExit(Controlled);

                    //New
                    selected = value;
                    if (Selected != null)
                        selected.HoverEnter(Controlled);

                    onSelectedChange?.Invoke(selected);
                }
            }
        }
        private event Action<WorldObject> onSelectedChange;
        public event Action<WorldObject> OnSelectedChange { add { onSelectedChange += value; } remove { onSelectedChange -= value; } }
        private List<WorldObject> selectList = new List<WorldObject>();
        public void CheckSelected()
        {
            selectList.Clear();
            foreach (WorldObject obj in Objects.Values)
            {
                if (obj.IsEnabled && obj.IsSelectable && obj.IsContains(Camera.ToWorld(Controls.MouseVector())))
                    selectList.Add(obj);
            }

            if (selectList.Count > 1)
            {
                selectList.Sort((a, b) => a.Depth.CompareTo(b.Depth));
                Selected = selectList.LastOrDefault();
            }
            else if (selectList.Count == 1)
                Selected = selectList.FirstOrDefault();
            else if (selectList.Count == 0)
                Selected = null;
        }
        public bool IsAllowSelecting { get; set; } = true;

        //Temporary (probably)
        private SpriteFont font, smallFont;
        private Texture2D vignette;

        public WorldManager(GraphicsDevice Graphics)
        {
            graphics = Graphics;

            Objects = new Dictionary<string, WorldObject>(StringComparer.OrdinalIgnoreCase);
            Layers = new Dictionary<string, TileLayer>(StringComparer.OrdinalIgnoreCase);

            //Move to level load.
            LevelRandom = new Random(Guid.NewGuid().GetHashCode());
        }

        public void Initialize()
        {
            references = GameManager.References();

            ObjectTarget = new RenderTarget2D(graphics, (int)references.Settings.Resolution.X, (int)references.Settings.Resolution.Y);
            FlatTarget = new RenderTarget2D(graphics, (int)references.Settings.Resolution.X, (int)references.Settings.Resolution.Y);
            Controls = references.Controls;

            Camera = new Camera(references.Settings.Resolution);
            Camera.MoveSpeed = 15f;
            Camera.ScaleSpeed = 5f;

            Queue = new TimeQueue();

            Sectors = new WorldSectors();
            Sectors.World = this;
        }
        public void Load(ContentManager cm)
        {
            Content = cm;

            //outline = cm.Load<Effect>("Assets/Effects/Shaders/Outline");

            //Temporary layers
            AddGround();
            AddHouse("LeftHouse");
            AddHouse("RightHouse");
            AddHouse("FrontHouse");

            AddObject(new LayerObject("LeftHouseLayer", FromTile(16, 15) - new Vector2(32, 16), Layers["LeftHouse"]));
            AddObject(new LayerObject("RightHouseLayer", FromTile(26, 14) - new Vector2(32, 16), Layers["RightHouse"]));
            AddObject(new LayerObject("FrontHouseLayer", FromTile(20, 24) - new Vector2(32, 16), Layers["FrontHouse"]));

            Layers["LeftHouse"].AddTile(new Tile(cottage, new Point(5, 0), new Point(2, -4)) { Position = FromTile(2, -4) + new Vector2(32, 48), Rotation = -.02f });
            Layers["RightHouse"].AddTile(new Tile(cottage, new Point(5, 0), new Point(1, -1)) { Position = FromTile(1, -1) + new Vector2(32, 16), Rotation = .025f });
            Layers["RightHouse"].AddTile(new Tile(cottage, new Point(5, 0), new Point(3, -1)) { Position = FromTile(3, -1) + new Vector2(32, 24), Rotation = -.05f });

            font = cm.Load<SpriteFont>("UI/Fonts/ArchwayKnights_19px");
            smallFont = cm.Load<SpriteFont>("UI/Fonts/ArchwayKnights_12px");
            vignette = cm.Load<Texture2D>("Assets/Effects/vignette");

            //Temporary
            AddObject(new PlayerObject("Player", FromTile(19, 13) + new Vector2(32, 24), cm.Load<Texture2D>("Assets/Characters/fodderAdultYellow")));
            Objects["Player"].Stats.Mass = 100;
            Objects["Player"].Origin = new Vector2(35, 108);
            Objects["Player"].Shape.Add("Circle", new Circle(new Vector2(0, 4), 15) { Group = WorldObject.SHAPE_Collision });
            Objects["Player"].Storage.AddItem("Stone", 5);
            Objects["Player"].Storage.AddItem("Stick", 5);
            Objects["Player"].Storage.AddItem("IronPickaxe", 1);
            Objects["Player"].Storage.AddItem("GorgerChow", 3);
            Objects["Player"].Storage.AddItem("Currency", 15);
            Objects["Player"].IsSelectable = false;
            Camera.ForceFocus(Objects["Player"].Position);
            Focused = Objects["Player"];

            SetControlled(Objects["Player"]);

            CraftingObject ui = new CraftingObject("Crafting", FromTile(19, 12) + new Vector2(32, 32), cm.Load<Texture2D>("Assets/Objects/furniture_CraftingTable"), new Vector2(-6, -45));
            ui.Origin = new Vector2(ui.Texture.Width / 2, ui.Texture.Height - 8);
            ui.DisplayName = "Crafting Table";
            ui.Shape.Add("Box", new Box(new Vector2(-24, -20), new Vector2(46, 32)) { Group = WorldObject.SHAPE_Collision });
            ui.Stats.IsImmovable = true;
            AddObject(ui);
            ui.OnLeftClick += (obj) =>
            {
                obj.AI_FollowPath(37, 25, 24);
                obj.Path.OnPathComplete += () =>
                {
                    if (obj.SmoothScale.X.Result > 0)
                        Queue.Add(1000, () => ((CharacterObject)obj).FaceToward(-1));
                };
            };

            AddObject(new RenderObject("Bench", FromTile(20, 12) + new Vector2(32, 32), cm.Load<Texture2D>("Assets/Objects/furniture_bench")));
            Objects["Bench"].Origin = new Vector2(30, 40);
            Objects["Bench"].Shape.Add("Main", new Circle(Vector2.Zero, 16) { Group = WorldObject.SHAPE_Collision });
            Objects["Bench"].Stats.Mass = 100;
            Objects["Bench"].Stats.IsImmovable = true;
            Objects["Bench"].IsSelectable = false;

            AddBook(cm);

            AddObject(new StorageObject("PotStorage", FromTile(19, 10) + new Vector2(22, 32), cm.Load<Texture2D>("Assets/Objects/furniture_PotBlueSmall"),
                new Point(2, 1), new Vector2(-2, -10), new Vector2(-5, -10), Vector2.Zero, new Color(40, 40, 40, 255), new Color(80, 80, 80, 255),
                Color.Lerp(Color.LightSkyBlue, Color.Black, .25f), Color.LightSkyBlue, font));
            ((StorageObject)Objects["PotStorage"]).Origin = new Vector2(17, 32);
            ((StorageObject)Objects["PotStorage"]).IsVisibleOnlyActive = true;
            ((StorageObject)Objects["PotStorage"]).DisplayName = "Pot";
            ItemObject itemChow = references.Assets.Items.Items["GorgerChow"].Copy();
            itemChow.Quantity = 1;
            ((StorageObject)Objects["PotStorage"]).Slots.First().Value.Item = itemChow;
            ItemObject itemStick = references.Assets.Items.Items["Stick"].Copy();
            itemStick.Quantity = 3;
            ((StorageObject)Objects["PotStorage"]).Slots.Last().Value.Item = itemStick;

            AddObject(new StorageObject("BarrelStorage", FromTile(18, 12) + new Vector2(32, 32), cm.Load<Texture2D>("Assets/Objects/furniture_BarrelLarge"), new Point(3, 3), new Vector2(-6, -52),
                      new Vector2(-10, -40), new Vector2(0, 50), new Color(40, 40, 40, 255), new Color(80, 80, 80, 255), Color.Lerp(Color.IndianRed, Color.Black, .25f),
                      Color.Lerp(Color.IndianRed, Color.White, 0), cm.Load<SpriteFont>("UI/Fonts/ArchwayKnights_12px")));
            ((RenderObject)Objects["BarrelStorage"]).Origin = new Vector2(((RenderObject)Objects["BarrelStorage"]).Texture.Width / 2, ((RenderObject)Objects["BarrelStorage"]).Texture.Height - 15);
            Objects["BarrelStorage"].DisplayName = "Barrel";
            Objects["BarrelStorage"].Shape.Add("Main", new Circle(Vector2.Zero, 20) { Group = WorldObject.SHAPE_Collision });
            Objects["BarrelStorage"].Stats.IsImmovable = true;
            Objects["BarrelStorage"].OnLeftClick += (obj) =>
            {
                //obj.Path.Find(obj.Path.ToPoint(obj.Position), new Point(38, 25));
                if (obj.Position.X >= obj.Path.ToVector(new Point(38, 0)).X)
                    obj.AI_FollowPath(37, 25, 32);
                else
                    obj.AI_FollowPath(38, 25, 32);

                obj.Path.OnPathComplete += () =>
                {
                    if (obj.SmoothScale.X.Result < 0)
                        Queue.Add(1000, () => ((CharacterObject)obj).FaceToward(1));
                };
            };

            AddObject(new MarketObject("MarketStall", FromTile(19, 14) + new Vector2(32, 0), cm.Load<Texture2D>("Assets/Objects/furniture_SalesTable"), new Point(4, 3), new Vector2(-8, -38),
                      new Vector2(-15, -30), new Vector2(0, 50), new Color(40, 40, 40, 255), new Color(80, 80, 80, 255), Color.Lerp(Color.Green, Color.Gray, .65f),
                      Color.Lerp(Color.Lerp(Color.Green, Color.Gray, .65f), Color.White, .25f), cm.Load<SpriteFont>("UI/Fonts/ArchwayKnights_12px")));
            Objects["MarketStall"].Origin = new Vector2(35, 48);
            Objects["MarketStall"].DisplayName = "Market Stall";
            Objects["MarketStall"].Shape.Add("Box", new Box(new Vector2(-24, -16), new Vector2(46, 32)) { Group = WorldObject.SHAPE_Collision });
            Objects["MarketStall"].Stats.IsImmovable = true;
            Objects["MarketStall"].OnLeftClick += (obj) =>
            {
                obj.AI_FollowPath(37, 27, 16);
                obj.AI.Goals["Pathfind"].OnSuccess += () => Queue.Add(1000, () => ((CharacterObject)obj).FaceToward(-1));
            };
            ((MarketObject)Objects["MarketStall"]).OnPurchase += (obj, i, q) =>
            {
                references.Screens.HUD.SendNotification(references.Screens.Message.ICON_Craft, obj.DisplayName + " bought " + q + "x " + i.Name);
                Objects["MarketStall"].Physics.Jump(150);
                references.Particles.AddCircleExpand("WORLD", q, .2f, 80, 150, () =>
                {
                    return new Firework(Objects["MarketStall"].Physics.AltitudePosition() - new Vector2(0, 24))
                    { Depth = Objects["MarketStall"].Depth + PixelDepth() * 50 };
                });
            };
            ((MarketObject)Objects["MarketStall"]).Merchant = Objects["Player"];

            AddObject(new RenderObject("Pot1", FromTile(18, 13) + new Vector2(16, 0), cm.Load<Texture2D>("Assets/Objects/furniture_PotBlueLarge")));
            ((RenderObject)Objects["Pot1"]).Origin = new Vector2(22, 43);
            Objects["Pot1"].IsSelectable = false;

            AddObject(new RenderObject("Pot2", FromTile(21, 13) + new Vector2(6, -8), cm.Load<Texture2D>("Assets/Objects/furniture_PotBlueSmall")));
            ((RenderObject)Objects["Pot2"]).Origin = new Vector2(17, 32);
            Objects["Pot2"].IsSelectable = false;

            AddObject(new SignObject("Sign", FromTile(21, 15) + new Vector2(48, 56), cm.Load<Texture2D>("Assets/Objects/sign_woodenPost"), 8, new Vector2(0, -45),
                          new TextElement(new Vector2(-5, -50), smallFont, "Hamlet\nof Oro", new Color(48, 48, 48, 255), new Vector2(2), -.075f, smallFont.MeasureString("Hamlet\nof Oro") / 2)));
            ((RenderObject)Objects["Sign"]).Origin = new Vector2(32, 67);

            AddObject(new SignObject("LeftSign", FromTile(21, 15) + new Vector2(32, 64), cm.Load<Texture2D>("Assets/Objects/sign_hanging"), 8, new Vector2(0, 30),
              new TextElement(new Vector2(-2, 38), smallFont, "House\n Brae", new Color(48, 48, 48, 255), new Vector2(2), -.075f, smallFont.MeasureString("House\n Brae") / 2)));
            ((RenderObject)Objects["LeftSign"]).Origin = new Vector2(28, 3);
            Objects["LeftSign"].Attach(Objects["LeftHouseLayer"], new Vector2(100, -128));

            //AddObject(new Doggo("Doggo", FromTile(21, 15), cm.Load<Texture2D>("Assets/Characters/doggoBrown"), "Scruffy"));

            AddLayer("Fences", new TileLayer());
            Layers["Fences"].Name = "Fences";
            Layers["Fences"].Origin = new Vector2(32, 52);
            Layers["Fences"].IsObjectLayer = true;
            Texture2D fence = cm.Load<Texture2D>("Assets/Tiles/fenceWooden");

            Layers["Fences"].AddTile(new DepthTile(fence, new Point(2, 2), new Point(18, 11), 25));
            Layers["Fences"].AddTile(new DepthTile(fence, new Point(1, 2), new Point(19, 11), 25));
            Layers["Fences"].AddTile(new DepthTile(fence, new Point(2, 2), new Point(20, 11), 25));
            Layers["Fences"].AddTile(new DepthTile(fence, new Point(2, 3), new Point(21, 11), 25));
            Layers["Fences"].AddTile(new DepthTile(fence, new Point(1, 4), new Point(21, 12), 25));
            Layers["Fences"].AddTile(new DepthTile(fence, new Point(1, 1), new Point(21, 13), 25) { Position = FromTile(21, 13) + new Vector2(16, 32) });
            Layers["Fences"].AddTile(new DepthTile(fence, new Point(0, 4), new Point(21, 10), 25));

            Layers["Fences"].AddTile(new DepthTile(fence, new Point(3, 0), new Point(16, 18), 25));
            Layers["Fences"].AddTile(new DepthTile(fence, new Point(0, 1), new Point(16, 19), 25));
            Layers["Fences"].AddTile(new DepthTile(fence, new Point(0, 2), new Point(16, 20), 25));
            Layers["Fences"].AddTile(new DepthTile(fence, new Point(2, 2), new Point(15, 18), 25));
            Layers["Fences"].AddTile(new DepthTile(fence, new Point(1, 2), new Point(14, 18), 25));
            Layers["Fences"].AddTile(new DepthTile(fence, new Point(2, 0), new Point(13, 18), 25));

            stalk1 = cm.Load<Texture2D>("Assets/Objects/flora_grassStalk1");
            stalk1.Tag = "stalk1";
            stalk2 = cm.Load<Texture2D>("Assets/Objects/flora_grassStalk2");
            stalk2.Tag = "stalk2";
            stalk3 = cm.Load<Texture2D>("Assets/Objects/flora_grassStalk3");
            stalk3.Tag = "stalk3";

            //Above fence
            AddFlower(new Vector2(1351, 776));
            AddFlower(new Vector2(1295, 736));
            AddFlower(new Vector2(1347, 698));
            AddFlower(new Vector2(1165, 682));
            AddFlower(new Vector2(1166, 718));
            AddFlower(new Vector2(1211, 730));
            AddGrass(new Vector2(1239, 722), stalk1);
            AddGrass(new Vector2(1320, 687), stalk1);
            AddGrass(new Vector2(1172, 637), stalk1);
            AddGrass(new Vector2(1134, 605), stalk2);
            AddGrass(new Vector2(1318, 728), stalk3);

            //Right of fence, between houses
            AddGrass(new Vector2(1393, 736), stalk2);
            AddGrass(new Vector2(1425, 743), stalk1);
            AddGrass(new Vector2(1391, 790), stalk3);
            AddGrass(new Vector2(1441, 777), stalk3);

            //Right field
            AddGrass(new Vector2(1516, 951), stalk2);
            AddGrass(new Vector2(1485, 976), stalk2);
            AddGrass(new Vector2(1448, 1005), stalk3);
            AddGrass(new Vector2(1484, 1000), stalk1);
            AddGrass(new Vector2(1456, 1035), stalk1);
            AddGrass(new Vector2(1503, 1045), stalk3);
            AddGrass(new Vector2(1483, 1014), stalk2);
            AddGrass(new Vector2(1521, 1089), stalk1);
            AddGrass(new Vector2(1482, 1044), stalk2);
            AddGrass(new Vector2(1514, 968), stalk3);
            AddGrass(new Vector2(1541, 980), stalk3);
            AddGrass(new Vector2(1623, 913), stalk3);
            AddGrass(new Vector2(1618, 953), stalk2);
            AddGrass(new Vector2(1677, 917), stalk2);
            AddGrass(new Vector2(1652, 931), stalk2);
            AddGrass(new Vector2(1664, 960), stalk3);
            AddGrass(new Vector2(1587, 978), stalk1);
            AddGrass(new Vector2(1578, 993), stalk2);
            AddGrass(new Vector2(1627, 985), stalk1);
            AddGrass(new Vector2(1566, 942), stalk3);
            AddGrass(new Vector2(1601, 1015), stalk1);
            AddGrass(new Vector2(1593, 1065), stalk1);
            AddGrass(new Vector2(1679, 1041), stalk1);
            AddGrass(new Vector2(1654, 1091), stalk1);
            AddGrass(new Vector2(1572, 1098), stalk2);
            AddGrass(new Vector2(1739, 967), stalk2);

            AddFlower(new Vector2(709, 759));
            AddFlower(new Vector2(751, 694));
            AddFlower(new Vector2(799, 758));
            AddFlower(new Vector2(804, 807));
            AddFlower(new Vector2(786, 888));
            AddFlower(new Vector2(720, 841));
            AddFlower(new Vector2(787, 604));
            AddFlower(new Vector2(879, 538));
            AddFlower(new Vector2(1152, 618));
            AddFlower(new Vector2(1107, 527));
            AddFlower(new Vector2(1601, 917));
            AddFlower(new Vector2(1668, 1000));
            AddFlower(new Vector2(1608, 986));
            AddFlower(new Vector2(1708, 929));
            AddFlower(new Vector2(1551, 1058));

            AddObject(new SpringFlower("Springflower1", FromTile(25, 16) + new Vector2(32, 16), cm.Load<Texture2D>("Assets/Objects/flora_springflowerRed"), .1f));
            AddObject(new SpringFlower("Springflower2", FromTile(12, 11) + new Vector2(0, 32), cm.Load<Texture2D>("Assets/Objects/flora_springflowerRed"), .1f));
            AddObject(new SpringFlower("Springflower4", FromTile(19, 11) + new Vector2(48, 24), cm.Load<Texture2D>("Assets/Objects/flora_springflowerRed"), .1f));
            AddObject(new SpringFlower("Springflower5", FromTile(18, 10) + new Vector2(48, 32), cm.Load<Texture2D>("Assets/Objects/flora_springflowerRed"), .1f));
            AddObject(new SpringFlower("Springflower6", FromTile(22, 11) + new Vector2(0, 64), cm.Load<Texture2D>("Assets/Objects/flora_springflowerRed"), .1f));

            //Left-house field
            AddGrass(new Vector2(815, 898), stalk1);
            AddGrass(new Vector2(767, 876), stalk1);
            AddGrass(new Vector2(798, 849), stalk2);
            AddGrass(new Vector2(734, 825), stalk3);
            AddGrass(new Vector2(766, 800), stalk1);
            AddGrass(new Vector2(745, 757), stalk3);
            AddGrass(new Vector2(760, 814), stalk2);
            AddGrass(new Vector2(748, 840), stalk1);
            AddGrass(new Vector2(745, 866), stalk3);
            AddGrass(new Vector2(774, 827), stalk3);
            AddGrass(new Vector2(831, 645), stalk1);
            AddGrass(new Vector2(767, 656), stalk2);
            AddGrass(new Vector2(805, 676), stalk1);
            AddGrass(new Vector2(810, 726), stalk3);
            AddGrass(new Vector2(791, 690), stalk1);

            AddObject(new RenderObject("BirchStump1", FromTile(19, 10) + new Vector2(48, 48), cm.Load<Texture2D>("Assets/Objects/flora_treeBirchStump")) { Origin = new Vector2(25, 38) } );
            AddObject(new RenderObject("BirchStump2", FromTile(24, 16) + new Vector2(16, 0), cm.Load<Texture2D>("Assets/Objects/flora_treeBirchStump")) { Origin = new Vector2(25, 38) });
            Objects["BirchStump1"].IsSelectable = false;
            Objects["BirchStump2"].IsSelectable = false;

            //Objects["BuildingLayer"].Shape.Add("Main", new Box(new Vector2(-160, -320), new Vector2(320, 320)));

            /*AddObject(new Berd("Berd2", FromTile(15, 16), cm.Load<Texture2D>("Assets/Characters/birdCardinal"), "Cardinal"));
            AddObject(new Berd("Berd3", FromTile(15, 16), cm.Load<Texture2D>("Assets/Characters/birdCardinal"), "Cardinal"));

            Objects["Berd2"].Memory.AddMemory("Owner", Objects["Berd1"]);
            Objects["Berd3"].Memory.AddMemory("Owner", Objects["Berd2"]);*/

            AddObject(new Gorger("Gorger1", FromTile(23, 15) + new Vector2(32, 48), cm.Load<Texture2D>("Assets/Characters/pigPinkFemale"), "Gorger"));
            AddObject(new Gorger("Gorger2", FromTile(24, 14) + new Vector2(0, 40), cm.Load<Texture2D>("Assets/Characters/pigPinkMale"), "Gorger"));
            AddObject(new Gorger("Gorger3", FromTile(24, 14) + new Vector2(64, 24), cm.Load<Texture2D>("Assets/Characters/pigPinkFemale"), "Gorger"));
            AddObject(new Gorger("Gorger4", FromTile(18, 9) + new Vector2(32, 32), cm.Load<Texture2D>("Assets/Characters/pigPinkMale"), "Gorger"));
            AddObject(new Gorger("Gorger5", FromTile(20, 10) + new Vector2(16, 16), cm.Load<Texture2D>("Assets/Characters/pigPinkFemale"), "Gorger"));

            ((Gorger)Objects["Gorger1"]).SetNormalBehaviour();
            ((Gorger)Objects["Gorger2"]).SetNormalBehaviour();
            ((Gorger)Objects["Gorger3"]).SetNormalBehaviour();
            ((Gorger)Objects["Gorger4"]).SetNormalBehaviour();
            ((Gorger)Objects["Gorger5"]).SetNormalBehaviour();

#if DEBUG
            //SetControlled(Focused = Objects["Gorger4"]);
#endif
            //Pathfind test
            Pathfinder = new PathManager();
            NavGrid = new SquareGrid();
            for (int y = 0; y < 64; y++)
            {
                for (int x = 0; x < 64; x++)
                {
                    NavGrid.AddTile(x, y, 10);
                }
            }
            NavGrid.RemoveChunk(26, 21, 10, 9);
            NavGrid.RemoveChunk(27, 20, 8, 1);
            NavGrid.RemoveChunk(28, 19, 6, 1);
            NavGrid.RemoveChunk(29, 18, 4, 1);
            NavGrid.RemoveChunk(30, 17, 2, 1);

            NavGrid.RemoveChunk(46, 19, 10, 9);
            NavGrid.RemoveChunk(47, 18, 8, 1);
            NavGrid.RemoveChunk(48, 17, 6, 1);
            NavGrid.RemoveChunk(49, 16, 4, 1);
            NavGrid.RemoveChunk(50, 15, 2, 1);

            NavGrid.RemoveChunk(34, 39, 10, 9);
            NavGrid.RemoveChunk(35, 38, 8, 1);
            NavGrid.RemoveChunk(36, 37, 6, 1);
            NavGrid.RemoveChunk(37, 36, 4, 1);
            NavGrid.RemoveChunk(38, 35, 2, 1);

            NavGrid.ApplyChunk(44, 16, 2, 13, (t) => t.BaseCost = 1);
            NavGrid.ApplyChunk(34, 30, 5, 4, (t) => t.BaseCost = 1);
            NavGrid.ApplyChunk(39, 30, 4, 4, (t) => t.BaseCost = 1);
            NavGrid.ApplyChunk(38, 18, 6, 2, (t) => t.BaseCost = 1);
            NavGrid.ApplyChunk(42, 28, 3, 2, (t) => t.BaseCost = 1);
            NavGrid.ApplyTile(43, 30, (t) => t.BaseCost = 1);
            NavGrid.ApplyChunk(22, 32, 7, 3, (t) => t.BaseCost = 1);
            NavGrid.ApplyChunk(29, 33, 6, 3, (t) => t.BaseCost = 1);
            NavGrid.ApplyTile(27, 35, (t) => t.BaseCost = 1);
            NavGrid.ApplyTile(28, 35, (t) => t.BaseCost = 1);
            NavGrid.ApplyTile(32, 32, (t) => t.BaseCost = 1);
            NavGrid.ApplyTile(33, 32, (t) => t.BaseCost = 1);
            NavGrid.ApplyTile(35, 34, (t) => t.BaseCost = 1);
            NavGrid.ApplyTile(43, 33, (t) => t.BaseCost = 1);
            NavGrid.ApplyTile(42, 34, (t) => t.BaseCost = 1);
            NavGrid.ApplyChunk(20, 32, 2, 2, (t) => t.BaseCost = 1);
            NavGrid.ApplyChunk(43, 34, 3, 2, (t) => t.BaseCost = 1);
            NavGrid.ApplyTile(42, 28, (t) => t.BaseCost = 3);

            NavGrid.ReplaceTile(new DirectionalTile(new Point(22, 32), 5, -4, -4, -4, 5));
            NavGrid.ReplaceTile(new DirectionalTile(new Point(23, 32), 5, -4, -4, -4, 5));
            NavGrid.ReplaceTile(new DirectionalTile(new Point(24, 32), 5, -4, -4, -4, 5));
            NavGrid.ReplaceTile(new DirectionalTile(new Point(25, 32), 5, -4, -4, -4, 5));
            NavGrid.ReplaceTile(new DirectionalTile(new Point(26, 32), 5, -4, -4, -4, 5));

            NavGrid.ReplaceTile(new DirectionalTile(new Point(22, 33), 5, -4, -4, 5, -4));
            NavGrid.ReplaceTile(new DirectionalTile(new Point(23, 33), 5, -4, -4, 5, -4));
            NavGrid.ReplaceTile(new DirectionalTile(new Point(24, 33), 5, -4, -4, 5, -4));
            NavGrid.ReplaceTile(new DirectionalTile(new Point(25, 33), 5, -4, -4, 5, -4));
            NavGrid.ReplaceTile(new DirectionalTile(new Point(26, 33), 5, -4, -4, 5, -4));

            NavGrid.ApplyChunk(34, 30, 6, 1, (t) => t.BaseCost = 10);
            NavGrid.ApplyChunk(34, 31, 4, 1, (t) => t.BaseCost = 10);

            //Stump and pot
            NavGrid.RemoveTile(38, 20);
            NavGrid.RemoveTile(39, 21);

            //Lone fence post
            NavGrid.RemoveTile(42, 27);

            //Market Stall
            NavGrid.RemoveTile(38, 27);
            NavGrid.RemoveTile(39, 27);

            //Town Sign
            NavGrid.RemoveTile(43, 31);

            //Tree stump (pigs)
            NavGrid.RemoveTile(48, 31);

            //Player's stall
            NavGrid.RemoveChunk(36, 23, 7, 2);
            NavGrid.RemoveTile(36, 25);
            NavGrid.RemoveTile(42, 25);
            NavGrid.ApplyChunk(42, 26, 2, 1, (p) => p.BaseCost = 1);
            NavGrid.ApplyTile(44, 26, (p) => p.BaseCost = 0);
            NavGrid.RemoveTile(42, 22);
            NavGrid.RemoveTile(42, 21);

            NavGrid.RemoveChunk(0, 0, 19, 64);
            NavGrid.RemoveChunk(19, 0, 45, 13);
            NavGrid.RemoveChunk(19, 39, 45, 24);
            NavGrid.RemoveChunk(59, 13, 5, 26);

            //Cutting off sides
            NavGrid.RemoveChunk(56, 23, 3, 1);
            NavGrid.RemoveChunk(19, 22, 7, 1);

            AddCharacters();
            SetBerdEvent(180000);
            SetStampedeEvent();

            //Temporary
            //references.Screens.Layers.FillLayers(Layers);
            references.Screens.Tiles.LoadExisting();

            references.Screens.Properties.SetSelected(Objects["Sproule"]);
        }

        private void AddGround()
        {
            AddLayer("Ground", new TileLayer());
            Layers["Ground"].IsObjectLayer = false;
            Layers["Ground"].Origin = new Vector2(TileWidth * 32 / 2, TileHeight * 32 / 2);
            Texture2D ground = Content.Load<Texture2D>("Assets/Tiles/ground");

            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 10, 7));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 11, 7));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 12, 7));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 13, 7));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 14, 7));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 15, 7));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 16, 7));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 17, 7));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 18, 7));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 19, 7));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 20, 7));
            Layers["Ground"].AddTile(new Tile(ground, 3, 1, 21, 7));
            Layers["Ground"].AddTile(new Tile(ground, 4, 1, 22, 7));
            Layers["Ground"].AddTile(new Tile(ground, 4, 1, 23, 7));
            Layers["Ground"].AddTile(new Tile(ground, 4, 1, 24, 7));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 26, 7));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 27, 7));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 28, 7));

            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 10, 8));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 11, 8));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 12, 8));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 13, 8));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 14, 8));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 15, 8));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 16, 8));
            Layers["Ground"].AddTile(new Tile(ground, 3, 1, 17, 8));
            Layers["Ground"].AddTile(new Tile(ground, 4, 1, 18, 8));
            Layers["Ground"].AddTile(new Tile(ground, 4, 1, 19, 8));
            Layers["Ground"].AddTile(new Tile(ground, 4, 1, 20, 8));
            Layers["Ground"].AddTile(new Tile(ground, 3, 5, 21, 8));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 22, 8));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 23, 8));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 27, 8));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 28, 8));

            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 10, 9));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 11, 9));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 12, 9));
            Layers["Ground"].AddTile(new Tile(ground, 1, 0, 13, 9));
            Layers["Ground"].AddTile(new Tile(ground, 3, 3, 17, 9));
            Layers["Ground"].AddTile(new Tile(ground, 3, 4, 18, 9));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 19, 9));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 20, 9));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 21, 9));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 22, 9));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 28, 9));

            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 10, 10));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 11, 10));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 12, 10));
            Layers["Ground"].AddTile(new Tile(ground, 3, 3, 18, 10));
            Layers["Ground"].AddTile(new Tile(ground, 4, 3, 19, 10));
            Layers["Ground"].AddTile(new Tile(ground, 4, 3, 20, 10));
            Layers["Ground"].AddTile(new Tile(ground, 3, 4, 21, 10));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 22, 10));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 28, 10));

            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 10, 11));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 11, 11));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 12, 11));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 18, 11));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 19, 11));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 20, 11));
            Layers["Ground"].AddTile(new Tile(ground, 3, 3, 21, 11));
            Layers["Ground"].AddTile(new Tile(ground, 3, 4, 22, 11));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 28, 11));

            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 10, 12));
            Layers["Ground"].AddTile(new Tile(ground, 2, 0, 11, 12));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 12, 12));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 18, 12));
            Layers["Ground"].AddTile(new Tile(ground, 2, 0, 19, 12));
            Layers["Ground"].AddTile(new Tile(ground, 3, 1, 20, 12));
            Layers["Ground"].AddTile(new Tile(ground, 4, 1, 21, 12));
            Layers["Ground"].AddTile(new Tile(ground, 3, 5, 22, 12));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 28, 12));

            Layers["Ground"].AddTile(new Tile(ground, 4, 1, 10, 13));
            Layers["Ground"].AddTile(new Tile(ground, 5, 1, 11, 13));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 12, 13));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 18, 13));
            Layers["Ground"].AddTile(new Tile(ground, 5, 0, 19, 13));
            Layers["Ground"].AddTile(new Tile(ground, 3, 2, 20, 13));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 21, 13));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 22, 13));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 28, 13));

            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 10, 14));
            Layers["Ground"].AddTile(new Tile(ground, 2, 5, 11, 14));
            Layers["Ground"].AddTile(new Tile(ground, 5, 1, 12, 14));
            Layers["Ground"].AddTile(new Tile(ground, 3, 1, 18, 14));
            Layers["Ground"].AddTile(new Tile(ground, 4, 1, 19, 14));
            Layers["Ground"].AddTile(new Tile(ground, 3, 5, 20, 14));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 21, 14));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 22, 14));
            Layers["Ground"].AddTile(new Tile(ground, 5, 2, 23, 14));
            Layers["Ground"].AddTile(new Tile(ground, 1, 0, 24, 14));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 25, 14));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 26, 14));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 27, 14));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 28, 14));

            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 10, 15));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 11, 15));
            Layers["Ground"].AddTile(new Tile(ground, 2, 5, 12, 15));
            Layers["Ground"].AddTile(new Tile(ground, 4, 1, 13, 15));
            Layers["Ground"].AddTile(new Tile(ground, 4, 1, 14, 15));
            Layers["Ground"].AddTile(new Tile(ground, 4, 1, 15, 15));
            Layers["Ground"].AddTile(new Tile(ground, 4, 1, 16, 15));
            Layers["Ground"].AddTile(new Tile(ground, 4, 1, 17, 15));
            Layers["Ground"].AddTile(new Tile(ground, 3, 5, 18, 15));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 19, 15));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 20, 15));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 21, 15));
            Layers["Ground"].AddTile(new Tile(ground, 2, 4, 22, 15));
            Layers["Ground"].AddTile(new Tile(ground, 5, 3, 23, 15));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 24, 15));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 25, 15));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 26, 15));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 27, 15));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 28, 15));

            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 10, 16));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 11, 16));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 12, 16));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 13, 16));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 14, 16));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 15, 16));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 16, 16));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 17, 16));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 18, 16));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 19, 16));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 20, 16));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 21, 16));
            Layers["Ground"].AddTile(new Tile(ground, 2, 5, 22, 16));
            Layers["Ground"].AddTile(new Tile(ground, 5, 1, 23, 16));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 24, 16));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 25, 16));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 26, 16));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 27, 16));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 28, 16));

            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 10, 17));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 11, 17));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 12, 17));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 13, 17));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 14, 17));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 15, 17));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 16, 17));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 17, 17));
            Layers["Ground"].AddTile(new Tile(ground, 2, 4, 18, 17));
            Layers["Ground"].AddTile(new Tile(ground, 3, 4, 20, 17));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 21, 17));
            Layers["Ground"].AddTile(new Tile(ground, 3, 0, 22, 17));
            Layers["Ground"].AddTile(new Tile(ground, 2, 5, 23, 17));
            Layers["Ground"].AddTile(new Tile(ground, 5, 1, 24, 17));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 25, 17));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 26, 17));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 27, 17));
            Layers["Ground"].AddTile(new Tile(ground, 0, 0, 28, 17));

            //Layers["Ground"].AddTile(new Tile(ground, 3, 0, 10, 18));

            MapSize = new Point(32 * TileWidth, 32 * TileHeight);

            //Path
            Texture2D path = Content.Load<Texture2D>("Assets/Tiles/pathStone");
            Layers["Ground"].AddTile(new Tile(path, new Point(0, 0), new Point(23, 8)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(0, 2), new Point(22, 8)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(3, 0), new Point(22, 9)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(2, 0), new Point(21, 9)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(0, 0), new Point(20, 9)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(1, 4), new Point(19, 9)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(0, 1), new Point(22, 10)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(0, 1), new Point(22, 13)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(3, 3), new Point(22, 14)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(2, 2), new Point(21, 14)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(3, 3), new Point(21, 15)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(2, 2), new Point(20, 15)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(4, 1), new Point(19, 15)) { DepthOffset = PixelDepth() });
            //Layers["Ground"].AddTile(new Tile(path, new Point(0, 1), new Point(18, 15)) { DepthOffset = PixelDepth() });
            //Layers["Ground"].AddTile(new Tile(path, new Point(2, 2), new Point(17, 15)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(3, 0), new Point(20, 16)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(2, 0), new Point(19, 16)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(3, 0), new Point(18, 16)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(2, 0), new Point(17, 16)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(3, 2), new Point(21, 16)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(2, 3), new Point(21, 17)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(3, 2), new Point(22, 17)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(3, 3), new Point(17, 17)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(3, 1), new Point(16, 17)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(2, 0), new Point(15, 17)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(3, 0), new Point(14, 17)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(2, 3), new Point(13, 17)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(4, 0), new Point(12, 17)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(4, 0), new Point(11, 17)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(2, 2), new Point(16, 16)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(4, 1), new Point(15, 16)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(3, 2), new Point(14, 16)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(3, 0), new Point(13, 16)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(3, 0), new Point(12, 16)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(2, 0), new Point(11, 16)) { DepthOffset = PixelDepth() });
            Layers["Ground"].AddTile(new Tile(path, new Point(2, 2), new Point(10, 16)) { DepthOffset = PixelDepth() });

            Layers["Ground"].AddTile(new Tile(path, new Point(0, 4), new Point(21, 13)) { DepthOffset = PixelDepth(), Position = FromTile(21, 13) + new Vector2(48, 24) });
        }
        Texture2D cottage;
        private void AddHouse(string ID)
        {
            AddLayer(ID, new TileLayer());
            Layers[ID].Name = ID;
            Layers[ID].Origin = new Vector2(160, 48);
            if (cottage == null)
                cottage = Content.Load<Texture2D>("Assets/Tiles/structuresCottage");

            //Base frame
            Layers[ID].AddTile(new Tile(cottage, 1, 7, 0, 0));
            Layers[ID].AddTile(new Tile(cottage, new Point(3, 7), new Point(1, 0)));
            Layers[ID].AddTile(new Tile(cottage, new Point(3, 7), new Point(2, 0)));
            Layers[ID].AddTile(new Tile(cottage, new Point(3, 7), new Point(3, 0)));
            Layers[ID].AddTile(new Tile(cottage, new Point(5, 7), new Point(4, 0)));

            //Side frames
            Layers[ID].AddTile(new Tile(cottage, new Point(5, 6), new Point(0, -1)));
            Layers[ID].AddTile(new Tile(cottage, new Point(5, 6), new Point(4, -1)));
            Layers[ID].AddTile(new Tile(cottage, new Point(1, 5), new Point(0, -2)));
            Layers[ID].AddTile(new Tile(cottage, new Point(1, 6), new Point(4, -2)));

            //Ledge frame
            Layers[ID].AddTile(new Tile(cottage, new Point(3, 5), new Point(0, -2)) { DepthOffset = PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(3, 5), new Point(1, -2)) { DepthOffset = PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(3, 5), new Point(2, -2)) { DepthOffset = PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(3, 5), new Point(3, -2)) { DepthOffset = PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(3, 5), new Point(4, -2)) { DepthOffset = PixelDepth() });

            //Roof frame (front)
            Layers[ID].AddTile(new Tile(cottage, new Point(2, 4), new Point(0, -3)));
            Layers[ID].AddTile(new Tile(cottage, new Point(5, 3), new Point(1, -4)));
            Layers[ID].AddTile(new Tile(cottage, new Point(3, 3), new Point(2, -5)));
            Layers[ID].AddTile(new Tile(cottage, new Point(5, 4), new Point(3, -4)));
            Layers[ID].AddTile(new Tile(cottage, new Point(4, 4), new Point(4, -3)));

            Layers[ID].AddTile(new Tile(cottage, new Point(3, 4), new Point(3, -3)));
            Layers[ID].AddTile(new Tile(cottage, new Point(3, 4), new Point(1, -3)));

            //Roof frame (back)
            Layers[ID].AddTile(new Tile(cottage, new Point(2, 4), new Point(0, -5)));
            Layers[ID].AddTile(new Tile(cottage, new Point(2, 4), new Point(1, -6)));
            Layers[ID].AddTile(new Tile(cottage, new Point(3, 4), new Point(2, -6)));
            Layers[ID].AddTile(new Tile(cottage, new Point(4, 3), new Point(2, -7)));
            Layers[ID].AddTile(new Tile(cottage, new Point(4, 4), new Point(3, -6)));
            Layers[ID].AddTile(new Tile(cottage, new Point(4, 4), new Point(4, -5)));

            //Roof corners (right)
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 4), new Point(3, -5)));
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 4), new Point(2, -6)) { DepthOffset = PixelDepth() * 2 });
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 4), new Point(3, -3)));
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 4), new Point(2, -4)) { DepthOffset = PixelDepth() * 2 });
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 4), new Point(4, -4)));
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 4), new Point(4, -2)) { DepthOffset = PixelDepth() * 2 });

            Layers[ID].AddTile(new Tile(cottage, new Point(1, 3), new Point(5, -4)) { DepthOffset = PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(1, 3), new Point(5, -2)) { DepthOffset = PixelDepth() });

            //Roof corners (left)
            Layers[ID].AddTile(new Tile(cottage, new Point(1, 4), new Point(1, -5)));
            Layers[ID].AddTile(new Tile(cottage, new Point(1, 4), new Point(2, -6)));
            Layers[ID].AddTile(new Tile(cottage, new Point(1, 4), new Point(1, -3)));
            Layers[ID].AddTile(new Tile(cottage, new Point(1, 4), new Point(2, -4)));
            Layers[ID].AddTile(new Tile(cottage, new Point(1, 4), new Point(0, -4)));
            Layers[ID].AddTile(new Tile(cottage, new Point(1, 4), new Point(0, -2)) { DepthOffset = PixelDepth() * 2 });

            Layers[ID].AddTile(new Tile(cottage, new Point(0, 3), new Point(-1, -4)) { DepthOffset = PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 3), new Point(-1, -2)) { DepthOffset = PixelDepth() });


            //Roof edges
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 2), new Point(-1, -4)));
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 2), new Point(-1, -3)));
            Layers[ID].AddTile(new Tile(cottage, new Point(4, 2), new Point(5, -4)));
            Layers[ID].AddTile(new Tile(cottage, new Point(4, 2), new Point(5, -3)));

            //Roof left
            Layers[ID].AddTile(new Tile(cottage, new Point(1, 2), new Point(0, -3)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(1, 1), new Point(0, -4)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(1, 0), new Point(0, -5)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(1, 2), new Point(1, -4)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(1, 1), new Point(1, -5)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(1, 0), new Point(1, -6)) { DepthOffset = -PixelDepth() });

            //Roof middle
            Layers[ID].AddTile(new Tile(cottage, new Point(2, 2), new Point(2, -5)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(2, 1), new Point(2, -6)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(2, 0), new Point(2, -7)) { DepthOffset = -PixelDepth() });

            //Roof right
            Layers[ID].AddTile(new Tile(cottage, new Point(3, 2), new Point(4, -3)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(3, 1), new Point(4, -4)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(3, 0), new Point(4, -5)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(3, 2), new Point(3, -4)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(3, 1), new Point(3, -5)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(3, 0), new Point(3, -6)) { DepthOffset = -PixelDepth() });

            //Front accents
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 7), new Point(0, 0)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 7), new Point(1, 0)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 7), new Point(2, 0)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 7), new Point(3, 0)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 7), new Point(4, 0)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 6), new Point(0, -1)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 6), new Point(1, -1)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 6), new Point(2, -1)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 6), new Point(3, -1)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 6), new Point(4, -1)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 5), new Point(0, -2)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 5), new Point(1, -2)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 5), new Point(2, -2)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 5), new Point(3, -2)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 5), new Point(4, -2)) { DepthOffset = -PixelDepth() });
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 5), new Point(0, -3)) { DepthOffset = -PixelDepth() * 2 });
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 5), new Point(1, -3)) { DepthOffset = -PixelDepth() * 2 });
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 5), new Point(2, -3)) { DepthOffset = -PixelDepth() * 2 });
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 5), new Point(3, -3)) { DepthOffset = -PixelDepth() * 2 });
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 5), new Point(4, -3)) { DepthOffset = -PixelDepth() * 2 });
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 5), new Point(1, -4)) { DepthOffset = -PixelDepth() * 2 });
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 5), new Point(2, -4)) { DepthOffset = -PixelDepth() * 2 });
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 5), new Point(3, -4)) { DepthOffset = -PixelDepth() * 2 });
            Layers[ID].AddTile(new Tile(cottage, new Point(0, 5), new Point(2, -5)) { DepthOffset = -PixelDepth() * 2 });
        }
        private void AddFlower(Vector2 position)
        {
            Flower f = new Flower("Flower" + LevelRandom.NextString("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890", 8),
                position, Content.Load<Texture2D>("Assets/Objects/flora_tulip" + LevelRandom.NextString("Blue", "Orange")));
            f.Origin = new Vector2(11, 45);
            f.IsSelectable = false;
            AddObject(f);
            Console.WriteLine("AddFlower(new Vector2(" + (int)position.X + ", " + (int)position.Y + "));");
        }
        private Texture2D stalk1, stalk2, stalk3;
        private void AddGrass(Vector2 position, Texture2D texture)
        {
            Flower f = new Flower("Grass" + LevelRandom.NextString("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890", 8), position, texture);
            if (texture.Tag.ToString() == "stalk3")
                f.Origin = new Vector2(6, 26);
            else
                f.Origin = new Vector2(11, 33);
            f.IsSelectable = false;
            AddObject(f);
            Console.WriteLine("AddGrass(new Vector2(" + (int)position.X + ", " + (int)position.Y + "), " + texture.Tag + ");");
        }
        private void AddBook(ContentManager cm)
        {
            BookObject CurrentBook = new BookObject("RecipeBook", Vector2.Zero, cm.Load<Texture2D>("UI/World/libraryBookRed"), 24, new Vector2(-10, 0));
            CurrentBook.Attach(Objects["Bench"], new Vector2(10, -26));
            AddObject(CurrentBook);

            CurrentBook.OnLeftClick += (obj) =>
            {
                obj.AI_FollowPath(39, 25, 24);
                obj.Path.OnPathComplete += () =>
                {
                    if (obj.SmoothScale.X.Result > 0)
                        Queue.Add(1000, () => ((CharacterObject)obj).FaceToward(-1));
                };
                //obj.Path.Find(obj.Path.ToPoint(obj.Position), new Point(39, 25));
            };

            Texture2D grid1 = cm.Load<Texture2D>("UI/World/book_craftGrid");
            Texture2D grid2 = cm.Load<Texture2D>("UI/World/book_craftGridAlt");
            Texture2D mark1 = cm.Load<Texture2D>("UI/World/book_mark1");
            Texture2D mark2 = cm.Load<Texture2D>("UI/World/book_mark2");
            Texture2D mark3 = cm.Load<Texture2D>("UI/World/book_mark3");
            Color darkGray = new Color(48, 48, 48, 255);

            BookData data = new BookData("Basic Crafting");
            data.LeftArrowOffset = new Vector2(-25, 3);
            data.RightArrowOffset = new Vector2(5, 6);
            data.LeftNumberOffset = new Vector2(-24, 4);
            data.RightNumberOffset = new Vector2(4, 4);
            data.AddPage(0, (p) =>
            {
                p.Content.Add(new TextElement(new Vector2(-22, -10), font, "Novice's Guide", Color.DarkRed));
                p.Content.Add(new TextElement(new Vector2(-20.5f, -8.5f), smallFont, "To Crafting", Color.Black));
                p.Content.Add(new TextElement(new Vector2(-26, -6), smallFont, "Basic Tools ... 3-10", Color.Black));
                p.Content.Add(new TextElement(new Vector2(-26, -4), smallFont, "Iron Tools ... 11-18", Color.Black));
            });
            data.AddPage(1, null);
            data.AddPage(2, (p) =>
            {
                p.Content.Add(new TextElement(new Vector2(-22, -8), font, "Stone Pickaxe", Color.DarkSlateGray));
                p.Content.Add(new TextElement(new Vector2(-22, -6.5f), smallFont, "Stone, Stick", darkGray));

                p.Content.Add(new ImageElement(new Vector2(-22, -4), grid1) { IsShadowed = false });

                p.Content.Add(new ImageElement(new Vector2(-22, -4), mark1) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-20, -4), mark2) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-18, -4), mark3) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-20, -2), mark1) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-20, 0), mark3) { IsShadowed = false, Color = darkGray });
            });
            data.AddPage(3, (p) =>
            {
                p.Content.Add(new TextElement(new Vector2(-5, -8), font, "Stone Axe", Color.DarkSlateGray));
                p.Content.Add(new TextElement(new Vector2(-5, -6.5f), smallFont, "Stone, Stick", darkGray));

                p.Content.Add(new ImageElement(new Vector2(-5, -4), grid2) { IsShadowed = false });

                p.Content.Add(new ImageElement(new Vector2(-5, -4), mark2) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-3, -4), mark1) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-5, -2), mark2) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-3, -2), mark3) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-3, 0), mark1) { IsShadowed = false, Color = darkGray });
            });
            data.AddPage(4, (p) =>
            {
                p.Content.Add(new TextElement(new Vector2(-22, -7), font, "Stone Hammer", Color.DarkSlateGray));
                p.Content.Add(new TextElement(new Vector2(-22, -5f), smallFont, "Stone, Stick", darkGray));

                p.Content.Add(new ImageElement(new Vector2(-22, -3), grid2) { IsShadowed = false });

                p.Content.Add(new ImageElement(new Vector2(-22, -3), mark1) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-20, -3), mark3) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-18, -3), mark2) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-22, -1), mark2) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-20, -1), mark2) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-18, -1), mark1) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-20, 1), mark3) { IsShadowed = false, Color = darkGray });
            });
            data.AddPage(5, (p) =>
            {
                p.Content.Add(new TextElement(new Vector2(-4, -8), font, "Stone Spade", Color.DarkSlateGray));
                p.Content.Add(new TextElement(new Vector2(-4, -6.5f), smallFont, "Stone, Stick", darkGray));

                p.Content.Add(new ImageElement(new Vector2(-4, -4), grid1) { IsShadowed = false });

                p.Content.Add(new ImageElement(new Vector2(-2, -4), mark1) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-2, -2), mark3) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-2, 0), mark2) { IsShadowed = false, Color = darkGray });
            });
            data.AddPage(6, (p) =>
            {
                p.Content.Add(new TextElement(new Vector2(-22, -8), font, "Stone Cudgel", Color.DarkSlateGray));
                p.Content.Add(new TextElement(new Vector2(-22, -6f), smallFont, "Stone, Stick", darkGray));

                p.Content.Add(new ImageElement(new Vector2(-22, -4), grid1) { IsShadowed = false });

                p.Content.Add(new ImageElement(new Vector2(-22, -4), mark1) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-20, -4), mark3) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-22, -2), mark2) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-20, -2), mark2) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-20, 0), mark3) { IsShadowed = false, Color = darkGray });
            });
            data.AddPage(7, (p) =>
            {
                p.Content.Add(new TextElement(new Vector2(-5, -7), font, "Wooden Bow", Color.DarkSlateGray));
                p.Content.Add(new TextElement(new Vector2(-5, -5.5f), smallFont, "String, Stick", darkGray));

                p.Content.Add(new ImageElement(new Vector2(-5, -4), grid1) { IsShadowed = false });

                p.Content.Add(new ImageElement(new Vector2(-3, -4), mark1) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-5, -2), mark3) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-3, 0), mark3) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-1, -4), mark2) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-1, -2), mark1) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-1, 0), mark2) { IsShadowed = false, Color = darkGray });
            });
            data.AddPage(8, (p) =>
            {
                p.Content.Add(new TextElement(new Vector2(-21, -8), font, "Stone Arrow", Color.DarkSlateGray));
                p.Content.Add(new TextElement(new Vector2(-22, -6.5f), smallFont, "Stone, Stick, Feather", darkGray));

                p.Content.Add(new ImageElement(new Vector2(-21, -4), grid2) { IsShadowed = false });

                p.Content.Add(new ImageElement(new Vector2(-19, -4), mark3) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-19, -2), mark1) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-19, 0), mark2) { IsShadowed = false, Color = darkGray });
            });
            data.AddPage(9, (p) =>
            {
                p.Content.Add(new TextElement(new Vector2(-5, -8.5f), font, "Fishing Rod", Color.DarkSlateGray));
                p.Content.Add(new TextElement(new Vector2(-7, -7f), smallFont, "Stick, String, Iron Ingot", darkGray));

                p.Content.Add(new ImageElement(new Vector2(-5, -5), grid2) { IsShadowed = false });

                p.Content.Add(new ImageElement(new Vector2(-5, -5), mark2) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-5, -3), mark3) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-5, -1), mark1) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-3, -3), mark2) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-1, -1), mark3) { IsShadowed = false, Color = darkGray });
            });
            //Iron tools
            data.AddPage(10, null);
            data.AddPage(10, (p) =>
            {
                p.Content.Add(new TextElement(new Vector2(-5, -8), font, "Iron Pickaxe", Color.MidnightBlue));
                p.Content.Add(new TextElement(new Vector2(-5, -6.5f), smallFont, "Iron Ingot, Stick", darkGray));

                p.Content.Add(new ImageElement(new Vector2(-5, -5), grid1) { IsShadowed = false });

                p.Content.Add(new ImageElement(new Vector2(-5, -5), mark3) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-3, -5), mark3) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-1, -5), mark1) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-3, -3), mark2) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-3, -1), mark1) { IsShadowed = false, Color = darkGray });
            });
            data.AddPage(11, (p) =>
            {
                p.Content.Add(new TextElement(new Vector2(-22, -9), font, "Iron Axe", Color.MidnightBlue));
                p.Content.Add(new TextElement(new Vector2(-22, -7.5f), smallFont, "Iron Ingot, Stick", darkGray));

                p.Content.Add(new ImageElement(new Vector2(-22, -6), grid2) { IsShadowed = false });

                p.Content.Add(new ImageElement(new Vector2(-22, -6), mark1) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-20, -6), mark3) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-22, -4), mark1) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-20, -4), mark1) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-20, -2), mark2) { IsShadowed = false, Color = darkGray });
            });
            data.AddPage(12, (p) =>
            {
                p.Content.Add(new TextElement(new Vector2(-7, -8), font, "Iron Hammer", Color.MidnightBlue));
                p.Content.Add(new TextElement(new Vector2(-6, -6.5f), smallFont, "Iron Ingot, Stick", darkGray));

                p.Content.Add(new ImageElement(new Vector2(-6, -5), grid1) { IsShadowed = false });

                p.Content.Add(new ImageElement(new Vector2(-6, -5), mark2) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-4, -5), mark1) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-2, -5), mark2) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-6, -3), mark3) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-4, -3), mark1) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-2, -3), mark2) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-4, -1), mark1) { IsShadowed = false, Color = darkGray });
            });
            data.AddPage(13, (p) =>
            {
                p.Content.Add(new TextElement(new Vector2(-22, -8), font, "Iron Spade", Color.MidnightBlue));
                p.Content.Add(new TextElement(new Vector2(-23, -6.5f), smallFont, "Iron Ingot, Stick", darkGray));

                p.Content.Add(new ImageElement(new Vector2(-22, -5), grid1) { IsShadowed = false });

                p.Content.Add(new ImageElement(new Vector2(-20, -5), mark2) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-20, -3), mark1) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-20, -1), mark3) { IsShadowed = false, Color = darkGray });
            });
            data.AddPage(14, (p) =>
            {
                p.Content.Add(new TextElement(new Vector2(-5, -8), font, "Iron Longsword", Color.MidnightBlue));
                p.Content.Add(new TextElement(new Vector2(-5, -6.5f), smallFont, "Iron Ingot, Stick", darkGray));

                p.Content.Add(new ImageElement(new Vector2(-5, -5), grid2) { IsShadowed = false });

                p.Content.Add(new ImageElement(new Vector2(-3, -5), mark3) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-3, -3), mark2) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-3, -1), mark1) { IsShadowed = false, Color = darkGray });
            });
            data.AddPage(15, (p) =>
            {
                p.Content.Add(new TextElement(new Vector2(-23, -8), font, "Iron Dagger", Color.MidnightBlue));
                p.Content.Add(new TextElement(new Vector2(-22.5f, -6.5f), smallFont, "Iron Ingot, Stick", darkGray));

                p.Content.Add(new ImageElement(new Vector2(-22, -4), grid2) { IsShadowed = false });

                p.Content.Add(new ImageElement(new Vector2(-20, -2), mark1) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-20, 0), mark2) { IsShadowed = false, Color = darkGray });
            });
            data.AddPage(16, (p) =>
            {
                p.Content.Add(new TextElement(new Vector2(-5, -7), font, "Iron Bow", Color.MidnightBlue));
                p.Content.Add(new TextElement(new Vector2(-6, -5.5f), smallFont, "Iron Ingot, String, Stick", darkGray));

                p.Content.Add(new ImageElement(new Vector2(-5, -4), grid1) { IsShadowed = false });

                //p.Content.Add(new ImageElement(new Vector2(-5, -5), mark2) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-3, -4), mark1) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-5, -2), mark3) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-3, 0), mark3) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-1, -4), mark2) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-1, -2), mark1) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-1, 0), mark2) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-3, -2), mark2) { IsShadowed = false, Color = darkGray });
            });
            data.AddPage(17, (p) =>
            {
                p.Content.Add(new TextElement(new Vector2(-22, -8), font, "Iron Arrow", Color.MidnightBlue));
                p.Content.Add(new TextElement(new Vector2(-22, -6f), smallFont, "Iron Ingot, Stick,\nFeather", darkGray));

                p.Content.Add(new ImageElement(new Vector2(-22, -3), grid1) { IsShadowed = false });

                p.Content.Add(new ImageElement(new Vector2(-20, -3), mark3) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-20, -1), mark2) { IsShadowed = false, Color = darkGray });
                p.Content.Add(new ImageElement(new Vector2(-20, 1), mark3) { IsShadowed = false, Color = darkGray });
            });

            CurrentBook.SetData(data);
            CurrentBook.Animation.FrameSize = new Point(40, 23);
            CurrentBook.Origin = new Vector2(30, 12);
            CurrentBook.Animation.AddState("Closed", () => CurrentBook.Animation.CurrentFrame = new Point(1, 0), null);
            CurrentBook.Animation.AddState("Open", () => CurrentBook.Animation.CurrentFrame = new Point(0, 0), null);
            CurrentBook.Animation.FrameState = "Closed";
        }
        private void AddGrid(BookPage page, Texture2D grid, Texture2D mark1, Texture2D mark2, Texture2D mark3)
        {

        }
        private void AddCharacters()
        {
            AddObject(new Stranger("Stranger", FromTile(17, 15) + new Vector2(32, 32), Content.Load<Texture2D>("Assets/Characters/fodderAdultBlue"), "Hooded Stranger"));

            /*AddObject(new Merchant("Lizard", FromTile(18, 15) + new Vector2(32, 32), Content.Load<Texture2D>("Assets/Characters/salamanderMerchant"), "Wizard Lizard",
                new Color(40, 46, 51), new Color(71, 81, 89), new Color(153, 130, 114), new Color(204, 174, 153)));
            Objects["Lizard"].Origin = new Vector2(55, 84);*/

            Objects.Characters.Sentient.Merchants.Sproule salamander;
            AddObject(salamander = new Objects.Characters.Sentient.Merchants.Sproule("Sproule", FromTile(10, 7), 
                Content.Load<Texture2D>("Assets/Characters/" + LevelRandom.NextString("lizardWizard", "salamanderMerchant")), "Odd Fellow", new Color(63, 41, 41), new Color(114, 74, 74),
                new Color(153, 130, 114), new Color(204, 174, 153)));
            salamander.Origin = new Vector2(55, 84);
        }
        public void ExitCutscene()
        {
            references.Screens.Message.Minimize();
            references.Screens.Transitions.BoxVertical(2.5f, 1);
            references.Screens.HUD.Fade(Color.White, 5f);

            Focused = Controlled;
            Camera.TargetScale = new Vector2(2);
            IsAllowSelecting = true;
        }

        public void Update(GameTime gt)
        {
            if (Controlled != null && references.Settings.IsPaused == false) Controlled.UpdateControls(gt);
            if (Focused != null) Camera.Focus(Focused.FocusPosition());
            Camera.Update(gt);

            //Temporary

            UpdateDebugCamera(gt);
            if (references.Settings.IsPaused == false)
                UpdateCustomerRate(gt);
            // ---

            if (references.Settings.IsPaused == false)
            {
                Queue.Update(gt);
                Sectors.Update(gt);

                if (references.Screens.UI_IsMouseInsideUI() == false && IsAllowSelecting == true)
                {
                    CheckSelected();
                    UpdateSelected();
                    if (IsOpenSpaceMouse()) UpdateDragAdd();
                }
                else
                    Selected = null;

                foreach (WorldObject obj in Objects.Values)
                {
                    if (obj.IsEnabled)
                        obj.Update(gt);
                }

                foreach (TileLayer layer in Layers.Values)
                    layer.Update(gt);
            }

            /*if (references.Screens.Inventory.IsMaximized == true &&
                Controls.IsMouseClicked(Controls.CurrentMS.LeftButton) &&
                references.Screens.UI_IsMouseInsideUI() == false)
            {
                Queue.Add(0, () =>
                {
                    references.Screens.UI_SetActive("Inventory");
                    //References.Screens.Inventory.IsInteract = true;
                    //References.Screens.Inventory.CheckActivity();
                });
            }*/
        }

        private void UpdateDragAdd()
        {
            if (Controls.IsMouseClicked(Controls.CurrentMS.LeftButton))
            {
                //If true, mouse is holding item.
                if (references.Screens.Drag.IsHolding())
                {
                    //If false, clicking on game world.
                    if (references.Screens.UI_IsMouseInsideUI() == false)
                    {
                        Queue.Add(50, () =>
                        {
                            ItemObject item = references.Screens.Drag.TakeItem();

                            WorldItem obj = new WorldItem(item.StackID() + item.RandomID, Camera.ToWorld(Controls.MouseVector()), item);
                            obj.Physics.SetAltitude(50);
                            AddObject(obj);

                            Vector2 velocity = Controls.MouseVector() - Controls.LastMS.Position.ToVector2();
                            obj.Physics.AddVelocity(velocity * 20);
                        });
                    }
                }
            }
        }
        private void UpdateSelected()
        {
            if (selected != null && references.Screens.UI_IsMouseInsideUI() == false)
            {
                if (references.Screens.Drag.IsHolding() == false)
                {
                    if (Controls.IsMouseClicked(Controls.CurrentMS.LeftButton))
                        selected.LeftClick(Controlled);
                    if (Controls.IsMouseClicked(Controls.CurrentMS.RightButton))
                        selected.RightClick(Controlled);
                }
                else if (Controls.IsMouseClicked(Controls.CurrentMS.LeftButton))
                {
                    if (selected.IsAcceptingItems == true)
                        selected.ItemClick(references.Screens.Drag.TakeItem(), Controlled);
                }
            }
        }

        public void Draw(SpriteBatch sb)
        {
            RenderFlat(sb);
            RenderObjects(sb);

            graphics.SetRenderTarget(null);
            graphics.Clear(Color.Transparent);

            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

            sb.Draw(FlatTarget, FlatTarget.Bounds, Color.White);
            sb.Draw(ObjectTarget, ObjectTarget.Bounds, Color.White);

            sb.Draw(vignette, new Rectangle(0, 0, (int)references.Settings.Resolution.X, (int)references.Settings.Resolution.Y), new Color(255, 255, 255, 128));

            sb.End();
        }
        private void RenderFlat(SpriteBatch sb)
        {
            graphics.SetRenderTarget(FlatTarget);
            graphics.Clear(Color.Transparent);

            sb.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, s, null, Camera.View());

            foreach (TileLayer layer in Layers.Values)
                if (layer.IsObjectLayer == false)
                    layer.Draw(sb);

            //Sectors.DrawDebug(sb);

            sb.End();
        }
        private void RenderObjects(SpriteBatch sb)
        {
            graphics.SetRenderTarget(ObjectTarget);
            graphics.Clear(Color.Transparent);

            sb.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, s, null, Camera.View());

            foreach (TileLayer layer in Layers.Values)
                if (layer.IsObjectLayer == true)
                    layer.Draw(sb);

            foreach (WorldObject obj in Objects.Values)
            {
                if (obj.IsVisible)
                    obj.Draw(sb);
            }

            references.Particles.DrawGroup(sb, "WORLD");

            if (references.Debugging.IsTileMouseRender)
                DrawTool(sb);
            if (references.Debugging.IsNavGridRender)
                DrawNavGrid(sb);
            if (references.Debugging.IsSelectShapeRender)
                DrawShapes(sb);

            sb.End();
        }
        RasterizerState s = new RasterizerState { CullMode = CullMode.None };

        #region Debugging

        private void UpdateDebugCamera(GameTime gt)
        {
            if (references.Debugging.IsDebugCamera == true)
            {
                if (references.Screens.UI_IsMouseInsideUI() == false)
                {
                    if (Controls.ScrollDirection() > 0)
                        Camera.TargetScale += new Vector2(Camera.TargetScale.X < 4 ? .25f : 1);
                    if (Controls.ScrollDirection() < 0)
                        Camera.TargetScale -= new Vector2(Camera.TargetScale.X < 4 ? .25f : 1);
                    if (Controls.IsKeyPressed(Keys.D0))
                        Camera.TargetScale = new Vector2(2);
                }

                if (Controls.IsKeyDown(Keys.LeftAlt))
                {
                    Vector2 direction = Camera.ToWorld(Controls.MouseVector()) - (Camera.Position + Camera.Origin);
                    float distance = direction.Length() * .5f;
                    if (direction != Vector2.Zero)
                        direction.Normalize();

                    Vector2 pos = (Camera.Position + Camera.Origin) + (direction * distance);

                    Focused = null;
                    Camera.Focus(pos);
                }
            }
        }
        private void DrawTool(SpriteBatch sb)
        {
            if (references.Screens.Developer.IsMaximized == true && references.Screens.Tools.SelectedTool != null)
                references.Screens.Tools.SelectedTool.Draw(sb);
        }
        private void DrawNavGrid(SpriteBatch sb)
        {
            int x = (int)Camera.ToWorld(Controls.MouseVector()).X / SquareGrid.TileWidth;
            int y = (int)Camera.ToWorld(Controls.MouseVector()).Y / SquareGrid.TileHeight;
            references.Debugging.DrawWireframe(sb, x * SquareGrid.TileWidth, y * SquareGrid.TileHeight, SquareGrid.TileWidth, SquareGrid.TileHeight, x + ", " + y, "");
            NavGrid.Draw(sb);
        }
        private void DrawShapes(SpriteBatch sb)
        {
            foreach (WorldObject obj in Objects.Values)
            {
                if (obj.IsSelectable == true)
                    obj.Shape.DrawDebug(sb, references.Debugging.Pixel, WorldObject.SHAPE_Select);
            }
        }

        #endregion

        public void AddObject(WorldObject obj)
        {
            if (obj == null)
                throw new Exception("The WorldObject cannot be null.");
            if (Objects.ContainsKey(obj.ID))
                throw new Exception("An object with key \"" + obj.ID + "\" already exists.");

            obj.World = this;
            obj.Camera = Camera;
            obj.Controls = Controls;
            obj.Queue = Queue;

            Objects.Add(obj.ID, obj);

            obj.Initialize();
            obj.Load(Content);
            obj.Intro();
        }
        public void RemoveObject(string id)
        {
            if (Objects.ContainsKey(id))
                Objects.Remove(id);
        }
        public void RemoveObject(WorldObject obj)
        {
            RemoveObject(obj.ID);
        }
        public void Apply<T>(Action<WorldObject> action) where T : WorldObject
        {
            foreach (T t in Objects.Values.OfType<T>())
                action?.Invoke(t);
        }

        public void SpawnItem(ItemObject item, Vector2 position, int xVelocity = 50, int yVelocity = 50)
        {
            Queue.Add(0, () =>
            {
                WorldItem obj = new WorldItem(item.StackID() + RandomStamp(), position, item);
                SpitItem(obj, xVelocity, yVelocity);
            });
        }
        public void SpitItem(WorldObject obj, int xVelocity = 50, int yVelocity = 50, float jumpHeight = 300)
        {
            Queue.Add(0, () =>
            {
                obj.Physics.AddVelocity(obj.TrueRandom.Next(-xVelocity, xVelocity), obj.TrueRandom.Next(-yVelocity, yVelocity));
                obj.Physics.Jump(jumpHeight);
                obj.Intro();

                AddObject(obj);
            });
        }

        public void SetBerdEvent(int timeDelay)
        {
            Queue.Add(timeDelay, () =>
            {
                if (Controlled.Memory.Contains("Event", "BerdFlock") == false)
                {
                    references.Screens.HUD.SendNotification(references.Screens.Message.ICON_Note, "A familiar noise draws near...");
                    Controlled.Memory.AddMemory(true, "Event", "BerdFlock");
                }
                else
                    references.Screens.HUD.SendNotification(references.Screens.Message.ICON_Note, LevelRandom.NextString("A bird flock approaches...", "Birds!"));

                Queue.Add(5000, () =>
                {
                    SpawnBerdFlock(LevelRandom.Next(5, 13), 150, 800, LevelRandom.Next(0, 1));
                    SetBerdEvent(LevelRandom.Next(180, 480) * 1000);
                });
            });
        }
        public void SpawnBerdFlock(int quantity, int minDelay, int maxDelay, int pathIndex = 0)
        {
            int time = LevelRandom.Next(minDelay, maxDelay);
            for (int i = 0; i < quantity; i++)
            {
                Queue.Add(time, () => SpawnResourceBerd(pathIndex));
                time += LevelRandom.Next(minDelay, maxDelay);
            }
        }
        public void SpawnResourceBerd(int pathIndex)
        {
            Berd b;
            AddObject(b = new Berd("Berd" + LevelRandom.NextString("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890", 8), Vector2.Zero,
                Content.Load<Texture2D>("Assets/Characters/birdCardinal"), "Cardinal"));

            b.IsResourceBird = true;
            b.SetBirdPath(pathIndex);

            if (LevelRandom.Next(0, 4) == 0)
                b.Storage.AddItem("Stick", 1);
            if (LevelRandom.Next(0, 2) == 0)
                b.Storage.AddItem("Feather", LevelRandom.Next(1, 3));
        }

        private int delay = 300000;
        public void SetStampedeEvent()
        {
            Queue.Add(delay, () =>
            {
                if (Controlled.Memory.Contains("Event", "GorgerStampede") == false)
                {
                    references.Screens.HUD.SendNotification(references.Screens.Message.ICON_Note, "A low rumble approaches...");
                    Controlled.Memory.AddMemory(true, "Event", "GorgerStampede");
                }
                else
                    references.Screens.HUD.SendNotification(references.Screens.Message.ICON_Note, LevelRandom.NextString("A gorger horde is coming...", "Gorgers approach!"));
                Queue.Add(5000, () =>
                {
                    delay = LevelRandom.Next(180000, 300000, 420000);
                    SpawnStampede(30, 100, 500);
                    SetStampedeEvent();
                });
            });
        }
        public void SpawnStampede(int quantity, int minDelay, int maxDelay)
        {
            int time = LevelRandom.Next(minDelay, maxDelay);
            for (int i = 0; i < quantity; i++)
            {
                Queue.Add(time, () => SpawnStampedingGorger(new Point(LevelRandom.Next(18, 24), 7),
                    LevelRandom.NextDouble() > .5f ? new Point(LevelRandom.Next(45, 57), 38) : new Point(LevelRandom.Next(22, 32), 38)));
                time += LevelRandom.Next(minDelay, maxDelay);
            }
        }
        public void SpawnStampedingGorger(Point start, Point end)
        {
            Gorger g = null;
            AddObject(g = new Gorger("Gorger" + LevelRandom.NextString("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890", 8),
                FromTile(start) + new Vector2(32, 32), Content.Load<Texture2D>("Assets/Characters/" + LevelRandom.NextString("pigPinkFemale", "pigPinkMale")),
                "Gorger"));
            g.SetStampedeBehaviour(end);
        }

        private int customerRate = 0, customerRateTimer = 0, delayTimer, minDelayTimer = 30000, maxDelayTimer = 60000;
        private void UpdateCustomerRate(GameTime gt)
        {
            customerRateTimer += gt.ElapsedGameTime.Milliseconds;
            if (customerRateTimer >= 60000 && customerRateTimer < 120000 && customerRate != 1)
                SetCustomerRate(customerRate = 1);
            if (customerRateTimer >= 120000 && customerRateTimer < 180000 && customerRate != 2)
                SetCustomerRate(customerRate = 2);
            if (customerRateTimer >= 180000 && customerRateTimer < 300000 && customerRate != 3)
                SetCustomerRate(customerRate = 3);
            if (customerRateTimer >= 300000 && customerRateTimer < 480000 && customerRate != 4)
                SetCustomerRate(customerRate = 4);
            if (customerRateTimer >= 480000 && customerRateTimer < 600000 && customerRate != 5)
                SetCustomerRate(customerRate = 5);

            UpdateCustomerSpawning(gt);
        }
        private void UpdateCustomerSpawning(GameTime gt)
        {
            if (customerRate >= 0)
            {
                delayTimer -= gt.ElapsedGameTime.Milliseconds;

                if (delayTimer <= 0)
                {
                    if (LevelRandom.NextDouble() > .25f)
                        SpawnCustomer();
                    else
                        SpawnPasserby();
                    delayTimer = LevelRandom.Next(minDelayTimer, maxDelayTimer);
                }
            }
        }
        public void SetCustomerRate(int index)
        {
            if (index == 1)
            {
                customerRateTimer = 60000;
                minDelayTimer = 15000;
                maxDelayTimer = 25000;
            }
            if (index == 2)
            {
                customerRateTimer = 120000;
                minDelayTimer = 12000;
                maxDelayTimer = 18000;
            }
            if (index == 3)
            {
                customerRateTimer = 180000;
                minDelayTimer = 8000;
                maxDelayTimer = 15000;
            }
            if (index == 4)
            {
                customerRateTimer = 300000;
                minDelayTimer = 5000;
                maxDelayTimer = 10000;
            }
            if (index == 5)
            {
                customerRateTimer = 480000;
                minDelayTimer = 3000;
                maxDelayTimer = 6000;
            }
        }
        private void SpawnCustomer()
        {
            Customer c = null;

            Point start = LevelRandom.NextObject(new Point(10, 7), new Point(28, 7), new Point(28, 18), new Point(10, 18));
            int min = LevelRandom.NextObject(500, 750, 1000, 1250, 1500, 2000);
            int max = min + LevelRandom.NextObject(500, 1000, 1500);

            AddObject(c = new Customer("Customer" + LevelRandom.NextString("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890", 8), FromTile(start),
                Content.Load<Texture2D>("Assets/Characters/fodderAdult" + LevelRandom.NextString("Blue", "Red", "White", "Yellow")), LevelRandom.NextFloat(.5f, .8f), min, max));
            c.LookingFor = LevelRandom.NextFunction(() => c.FarmerItems(), () => c.ConstructorItems(), () => c.WoodsmanItems(),
                () => c.TravellerItems(), () => c.KnightItems(), () => c.ArtisanItems(), () => c.MinerItems(), () => c.FisherItems());
            c.Font = smallFont;
        }
        private List<Point> destinations = new List<Point>();
        private void SpawnPasserby()
        {
            destinations.Clear();
            destinations.Add(new Point(28, 7));
            destinations.Add(new Point(28, 18));
            destinations.Add(new Point(10, 18));

            Point start = destinations[LevelRandom.Next(0, destinations.Count)];
            destinations.Remove(start);

            AddObject(new Passerby("Passerby" + LevelRandom.NextString("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890", 8),
                FromTile(start), Content.Load<Texture2D>("Assets/Characters/fodderAdult" + LevelRandom.NextString("Blue", "Red", "White", "Yellow")),
                "Passerby", destinations[LevelRandom.Next(0, destinations.Count)]));
        }

        public List<WorldObject> Temp()
        {
            return Objects.Values.ToList();
        }
    }
}
