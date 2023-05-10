using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using AetaLibrary.Elements.Images;
using ExtensionsLibrary.Extensions;
using EmberiumLibrary.Extensions;
using Merchantry.World.Tiles;
using System.IO;

namespace Merchantry.UI.Developer.TileEditor
{
    public class TilesUI : BaseUI
    {
        private Texture2D developerIcons;
        private int tilesetCount = 0;

        public StretchBoxElement buttonWell, setWell, tileWell;
        private ImageElement addSet, removeSet;
        private BoxElement setSelection, tileSelection;

        private event Action<TilesetData> onTilesetChange;
        private event Action<TileData> onTileChange;

        public event Action<TilesetData> OnTilesetChange { add { onTilesetChange += value; } remove { onTilesetChange -= value; } }
        public event Action<TileData> OnTileChange { add { onTileChange += value; } remove { onTileChange -= value; } }

        private TilesetData selectedTileset;
        public TilesetData SelectedTileset
        {
            get { return selectedTileset; }
            private set
            {
                //Remove interaction check from last set
                if (selectedTileset != null)
                    selectedTileset.SetActivity(false);

                selectedTileset = value;
                selectedTileset.SetActivity(true);
                TileIndex = 0;
                selectedTileset.RefreshPositions();
                SelectedTile = selectedTileset.Tiles.First();
                RefreshSelections();

                onTilesetChange?.Invoke(selectedTileset);
            }
        }
        private TileData selectedTile;
        public TileData SelectedTile
        {
            get { return selectedTile; }
            set
            {
                selectedTile = value;
                onTileChange?.Invoke(selectedTile);
            }
        }
        public void RefreshSelections(bool isForced = false)
        {
            RefreshSetSelect(isForced);
            RefreshTileSelect(isForced);
            CheckSelectionRender();
        }
        private void CheckSelectionRender()
        {
            if (SelectedTileset != null) setSelection.IsRendered = true;
            else setSelection.IsRendered = false;

            if (SelectedTile != null) tileSelection.IsRendered = true;
            else tileSelection.IsRendered = false;
        }
        private void RefreshSetSelect(bool isForced = false)
        {
            if (SelectedTileset != null)
            {
                int index = tilesets.IndexOf(SelectedTileset);
                if (isForced == false)
                    setSelection.POSITION_Loose(FromSet(index) - new Vector2(3, 3));
                else
                    setSelection.Position = FromSet(index) - new Vector2(3, 3);
            }
        }
        private void RefreshTileSelect(bool isForced = false)
        {
            if (SelectedTileset != null)
            {
                if (SelectedTile != null)
                {
                    if (isForced == false)
                        tileSelection.POSITION_Loose(SelectedTileset.FromIndex(SelectedTileset.Tiles.IndexOf(SelectedTile)) - new Vector2(3, 3));
                    else
                        tileSelection.Position = SelectedTileset.FromIndex(SelectedTileset.Tiles.IndexOf(SelectedTile)) - new Vector2(3, 3);
                }
            }
        }

        public TilesUI(GraphicsDevice Graphics, Vector2 Size)
            : base(Graphics, Size) { }

        private List<TilesetData> tilesets = new List<TilesetData>();

        private void AddTileset(Texture2D texture, bool ignoreNotification = false)
        {
            if (ContainsTileset(texture.Name) == false)
            {
                TilesetData tileset;
                tilesets.Add(tileset = new TilesetData(this));

                ELEMENTS_Add(texture.Name, tileset.Icon = new ImageElement(2, FromSet(tilesets.Count - 1), texture, texture.Bounds,
                    Color.White, Vector2.Zero, new Vector2(64 / (float)texture.Width, 64 / (float)texture.Height), 0, SpriteEffects.None));
                tileset.Icon.Buttons.OnHoverEnter += () => Screens.Tooltip.LAYOUT_SetText("Path: " + tileset.Tileset.Name);
                tileset.Icon.Buttons.OnHoverExit += () => Screens.Tooltip.Minimize();
                tileset.Icon.Buttons.OnLeftClick += () =>
                {
                    if (SelectedTileset != tileset)
                        SelectedTileset = tileset;
                };
                tileset.Icon.POSITION_Speed = 15;
                tileset.PopulateTiles();

                RefreshPositions();
            }
            else if (ignoreNotification == false)
                References.Screens.HUD.SendNotification(References.Screens.Message.ICON_Gear, "Tileset " + texture.Name + " already added.");
        }
        private void RemoveTileset(TilesetData tileset)
        {
            int index = tilesets.IndexOf(SelectedTileset);
            SelectedTileset.RemoveAll();
            tilesets.Remove(SelectedTileset);

            if (tilesets.Count > 0)
            {
                SelectedTileset = tilesets[Math.Min(index, tilesets.Count - 1)];
                SelectedTile = SelectedTileset.Tiles.First();

                RefreshSelections();
                RefreshPositions();
            }
            else
            {
                SelectedTileset = null;
                SelectedTile = null;

                CheckSelectionRender();
            }
        }
        public bool ContainsTileset(string name)
        {
            for (int i = 0; i < tilesets.Count; i++)
            {
                if (tilesets[i].Tileset.Name == name)
                    return true;
            }

            return false;
        }
        public void LoadExisting()
        {
            foreach (TileLayer layer in References.World.Layers.Values)
            {
                for (int i = 0; i < layer.Tiles.Count; i++)
                    AddTileset(layer.Tiles[i].Texture, true);
            }

            if (tilesets.Count > 0)
                SelectedTileset = tilesets.First();
        }
        public void RefreshPositions(bool isForcing = false)
        {
            for (int i = 0; i < tilesets.Count; i++)
            {
                if (isForcing == false)
                    tilesets[i].Icon.POSITION_Loose(FromSet(i));
                else
                    tilesets[i].Icon.Position = FromSet(i);
            }
        }

        private void OpenTileset()
        {
            System.Windows.Forms.OpenFileDialog window = new System.Windows.Forms.OpenFileDialog();

            window.InitialDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\Content\\Assets\\Tiles";
            window.RestoreDirectory = true;
            window.Title = "Browse Tilesets (.xnb)";

            window.Filter = "XNB File (*.xnb)|*.xnb";
            window.Multiselect = true;
            window.ShowDialog();

            foreach (string file in window.FileNames)
            {
                string trimmed = "Assets/Tiles/" + Path.GetFileNameWithoutExtension(file);
                Texture2D tileset = null;
                try { tileset = Content.Load<Texture2D>(trimmed); }
                catch (Exception e)
                {
                    References.Screens.HUD.SendNotification(References.Screens.Message.ICON_Gear, "Error loading " + trimmed + " as tileset.");
                }

                if (tileset != null)
                    AddTileset(tileset);
            }
        }

        private int scrollIndex;
        public int TileIndex { get; private set; }
        public void ScrollSetView(int direction)
        {
            scrollIndex += direction;
            scrollIndex = MathHelper.Clamp(scrollIndex, Math.Min(-(tilesets.Count - 5), 0), 0);

            RefreshPositions();
            RefreshSetSelect();
        }
        public void ScrollTileView(int direction)
        {
            if (SelectedTileset != null)
            {
                TileIndex += direction;
                TileIndex = MathHelper.Clamp(TileIndex, Math.Min(-((SelectedTileset.Tiles.Count / 7) - 5), 0), 0);

                SelectedTileset.RefreshPositions();
                RefreshTileSelect();
            }
        }
        private Vector2 FromSet(int index)
        {
            return setWell.Position + new Vector2(5, 15 + ((index + scrollIndex) * 66));
        }

        public void ScrollSetIndex(int direction)
        {
            if (tilesets.Count > 0 && SelectedTileset != null)
            {
                int index = tilesets.IndexOf(SelectedTileset);
                index += direction;

                if (index > tilesets.Count - 1)
                    index = 0;
                else if (index < 0)
                    index = tilesets.Count - 1;

                SelectedTileset = tilesets[index];
            }
        }
        public void ScrollTileIndex(int direction)
        {
            if (tilesets.Count > 0 && SelectedTileset != null)
            {
                int index = SelectedTileset.Tiles.IndexOf(SelectedTile);
                index += direction;

                if (index > SelectedTileset.Tiles.Count - 1)
                    index = 0;
                else if (index < 0)
                    index = SelectedTileset.Tiles.Count - 1;

                SelectedTile = SelectedTileset.Tiles[index];
            }
        }

        public override void Load(ContentManager cm)
        {
            base.Load(cm);

            developerIcons = cm.Load<Texture2D>("UI/Icons/developerIcons");

            ELEMENTS_Add("SetsWell", setWell = new StretchBoxElement(1, TopLeft + new Vector2(25, -5), wellBlack, Color.White, new Point(74, (int)Size.Y + 10), 16));
            ELEMENTS_Add("TilesWell", tileWell = new StretchBoxElement(1, TopLeft + new Vector2(107, -5), wellBlack, Color.White, new Point((int)Size.X - 130, (int)Size.Y + 10), 16));
            setWell.Buttons.OnHover += () =>
            {
                if (IsMaximized == true)
                {
                    if (Controls.ScrollDirection() > 0)
                        ScrollSetView(1);
                    if (Controls.ScrollDirection() < 0)
                        ScrollSetView(-1);
                }
            };
            tileWell.Buttons.OnHover += () =>
            {
                if (IsMaximized == true)
                {
                    if (Controls.ScrollDirection() > 0)
                        ScrollTileView(1);
                    if (Controls.ScrollDirection() < 0)
                        ScrollTileView(-1);
                }
            };

            ELEMENTS_Add("TilesetBox", setSelection = new BoxElement(3, Vector2.Zero, pixel, Color.Transparent, Color.White, new Vector2(70, 70), 2));
            ELEMENTS_Add("TileBox", tileSelection = new BoxElement(3, Vector2.Zero, pixel, Color.Transparent, Color.White, new Vector2(70, 70), 2));
            setSelection.POSITION_Speed = 15;
            tileSelection.POSITION_Speed = 15;

            AddWellButtons();
        }
        private void AddWellButtons()
        {
            //Black well to contain buttons
            ELEMENTS_Add("ButtonWell", buttonWell = new StretchBoxElement(1, TopLeft + new Vector2(-38, 16), wellBlack, Color.White, new Point(48, 48 * 4), 16));
            CLICKBOX_Add("ButtonWell", buttonWell.Position.ToPoint(), buttonWell.Size);
            buttonWell.TargetRender = "EXTERNAL";

            ELEMENTS_Add("AddLayerButton", addSet = new ImageElement(1, buttonWell.Position + new Vector2(24, 24), developerIcons, new Rectangle(0, 0, 48, 48),
                Color.Lerp(Color.Green, Color.White, .4f), Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            addSet.SetOrigin(.5f, .5f);
            addSet.SCALE_Speed = 10;
            addSet.TargetRender = "EXTERNAL";
            addSet.Buttons.OnHoverEnter += () =>
            {
                addSet.SCALE_Loose(1.25f);
                Screens.Tooltip.LAYOUT_SetText("Add Tileset");
            };
            addSet.Buttons.OnHoverExit += () =>
            {
                addSet.SCALE_Loose(1);
                Screens.Tooltip.Minimize();
            };
            addSet.Buttons.OnLeftClick += () =>
            {
                Queue.Add(0, () =>
                {
                    OpenTileset();
                });
            };

            ELEMENTS_Add("RemoveLayerButton", removeSet = new ImageElement(1, buttonWell.Position + new Vector2(24, 24 + 48), developerIcons, new Rectangle(48, 0, 48, 48),
                ColorExt.Raspberry, Vector2.Zero, Vector2.One, 0, SpriteEffects.None));
            removeSet.SetOrigin(.5f, .5f);
            removeSet.SCALE_Speed = 10;
            removeSet.TargetRender = "EXTERNAL";
            removeSet.Buttons.OnHoverEnter += () =>
            {
                removeSet.SCALE_Loose(1.25f);
                Screens.Tooltip.LAYOUT_SetText("Delete Tileset");
            };
            removeSet.Buttons.OnHoverExit += () =>
            {
                removeSet.SCALE_Loose(1);
                Screens.Tooltip.Minimize();
            };
            removeSet.Buttons.OnLeftClick += () =>
            {
                RemoveTileset(SelectedTileset);
            };
        }
        public override void PostInitialize()
        {
            base.PostInitialize();

            IsTitlebarEnabled = true;
            Title = "Tile Viewer";
            TitlebarWidth = 180;

            Minimize();
        }

        protected override void DrawInterior(SpriteBatch sb)
        {
            DrawElements(sb);
        }
        protected override void DrawExterior(SpriteBatch sb)
        {
            buttonWell.Draw(sb);
            addSet.Draw(sb);
            removeSet.Draw(sb);
        }
    }
    public class TilesetData
    {
        private TilesUI ui;

        public Texture2D Tileset
        {
            get { return Icon.Texture; }
            set { Icon.Texture = value; }
        }
        public ImageElement Icon { get; set; }
        public List<TileData> Tiles { get; private set; } = new List<TileData>();

        public Vector2 FromIndex(int index)
        {
            int x = index % 7;
            int y = (index / 7) + ui.TileIndex;//Tiles.Count % 5;

            return ui.tileWell.Position + new Vector2(5 + (x * 66), 40 + (y * 66));
        }
        public void RefreshPositions(bool isForcing = false)
        {
            for (int i = 0; i < Tiles.Count; i++)
            {
                if (isForcing == false)
                    Tiles[i].Icon.POSITION_Loose(FromIndex(i));
                else
                    Tiles[i].Icon.Position = FromIndex(i);
            }
        }
        public void PopulateTiles()
        {
            //64x64, 1 spacing
            int width = Tileset.Width / 66;
            int height = Tileset.Height / 66;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Rectangle source = new Rectangle(1 + (x * 66), 1 + (y * 66), 64, 64);
                    if (Tileset.IsEmpty(source) == false)
                    {
                        TileData tile = new TileData(new Point(x, y));
                        ui.ELEMENTS_Add(Tileset.Name + x + "." + y, tile.Icon = new ImageElement(2, Vector2.Zero, Tileset,
                            source, Color.White, Vector2.Zero, Vector2.Zero, 0, SpriteEffects.None));

                        tile.Icon.POSITION_Speed = 15;
                        tile.Icon.SCALE_Speed = 15;
                        tile.Icon.IsInteract = false;

                        tile.Icon.Buttons.OnHoverEnter += () => ui.Screens.Tooltip.LAYOUT_SetText("Coords: " + tile.Coords.X + ", " + tile.Coords.Y);
                        tile.Icon.Buttons.OnHoverExit += () => ui.Screens.Tooltip.Minimize();
                        tile.Icon.Buttons.OnLeftClick += () =>
                        {
                            ui.SelectedTile = tile;
                            ui.RefreshSelections();
                        };

                        Tiles.Add(tile);
                    }
                }
            }

            RefreshPositions();
        }
        public void SetActivity(bool isVisible)
        {
            for (int i = 0; i < Tiles.Count; i++)
            {
                if (isVisible == true)
                {
                    Tiles[i].Icon.SCALE_Loose(1);
                    Tiles[i].Icon.IsInteract = true;
                }
                else
                {
                    Tiles[i].Icon.SCALE_Loose(0);
                    Tiles[i].Icon.IsInteract = false;
                }
            }
        }
        public void RemoveAll()
        {
            ui.Queue.Add(0, () =>
            {
                ui.ELEMENTS_Remove(Icon);
                for (int i = 0; i < Tiles.Count; i++)
                    ui.ELEMENTS_Remove(Tiles[i].Icon);
            });
        }

        public TilesetData(TilesUI UI)
        {
            ui = UI;
        }
    }
    [System.Diagnostics.DebuggerDisplay("{Coords}")]
    public class TileData
    {
        public ImageElement Icon { get; set; }
        public Point Coords { get; set; }

        public TileData(Point Coords) { this.Coords = Coords; }
    }
}
