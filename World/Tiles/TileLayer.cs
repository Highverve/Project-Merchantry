using Merchantry.UI.Developer.General;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Tiles
{
    public class TileLayer : IProperties
    {
        private Vector2 origin, scale;
        private float rotation;

        public string Name { get; set; }
        public Vector2 Position { get; set; }
        public float Depth { get; set; }

        public Vector2 Origin
        {
            get { return origin; }
            set
            {
                if (origin != value)
                {
                    origin = value;
                    CalculateView();
                }
            }
        }
        public Vector2 Scale
        {
            get { return scale; }
            set
            {
                if (scale != value)
                {
                    scale = value;
                    CalculateView();
                }
            }
        }
        public float Rotation
        {
            get { return rotation; }
            set
            {
                if (rotation != value)
                {
                    rotation = value;
                    CalculateView();
                }
            }
        }
        public Objects.Various.LayerObject Attached { get; set; }
        public WorldManager World { get; set; }

        public List<Tile> Tiles { get; private set; } = new List<Tile>();
        public bool IsVisible { get; set; } = true;
        public bool IsObjectLayer { get; set; } = true;

        public void AddTile(Tile tile)
        {
            tile.Layer = this;
            tile.Initialize();
            Tiles.Add(tile);
        }
        private List<Tile> removal = new List<Tile>();
        public void RemoveTile(Vector2 position)
        {
            removal.Clear();
            for (int i = 0; i < Tiles.Count; i++)
            {
                if (Tiles[i].Contains(position))
                    removal.Add(Tiles[i]);
            }

            if (removal.Count > 0)
            {
                removal.Sort((a, b) => b.DepthOffset.CompareTo(a.DepthOffset));
                Tiles.Remove(removal.First());
            }
        }
        public Tile SelectTile(Vector2 position)
        {
            for (int i = 0; i < Tiles.Count; i++)
            {
                if (Tiles[i].Contains(position))
                    return Tiles[i];
            }
            return null;
        }

        private Matrix view;
        public Matrix View() { return view; }
        public void CalculateView()
        {
            view = Matrix.CreateTranslation(new Vector3(-Origin, 0.0f)) *
                   Matrix.CreateRotationZ(Rotation) *
                   Matrix.CreateScale(Scale.X, Scale.Y, 1) *
                   Matrix.CreateTranslation(new Vector3(Origin, 0.0f));
        }
        public Vector2 ToScreen(Vector2 position) { return Vector2.Transform(position, View()); }
        public Vector2 ToWorld(Vector2 position) { return Vector2.Transform(position, Matrix.Invert(View())); }

        public Vector2 MousePosition()
        {
            return World.Camera.ToWorld(World.Controls.MouseVector()) - Position;
        }
        public Point MouseCoords()
        {
            Vector2 mouse = MousePosition();

            if (mouse.X < 0) mouse.X -= WorldManager.TileWidth;
            if (mouse.Y < 0) mouse.Y -= WorldManager.TileHeight;

            return new Point((int)mouse.X / WorldManager.TileWidth, (int)mouse.Y / WorldManager.TileHeight);
        }

        public TileLayer()
        {
            origin = Vector2.Zero;
            scale = Vector2.One;
            rotation = 0;

            CalculateView();
        }
        public void Update(GameTime gt)
        {
            for (int i = 0; i < Tiles.Count; i++)
                Tiles[i].Update(gt);
        }
        public void Draw(SpriteBatch sb)
        {
            for (int i = 0; i < Tiles.Count; i++)
            {
                if (IsVisible == true)
                    Tiles[i].Draw(sb);
            }
        }

        private UI.Elements.NumberElement posXNumber, posYNumber, originXNumber, originYNumber, scaleXNumber, scaleYNumber, rotationNumber, depthNumber;
        private AetaLibrary.Elements.Text.TextElement attachText, visibleText;
        public void SetProperties(PropertiesUI ui)
        {
            ui.PROPERTY_AddHeader(Name);
            posXNumber = ui.PROPERTY_AddNumber("Position.X: ", Position.X, null, () => Position += new Vector2(10, 0), () => Position -= new Vector2(10, 0));
            posYNumber = ui.PROPERTY_AddNumber("Position.Y: ", Position.Y, null, () => Position += new Vector2(0, 10), () => Position -= new Vector2(0, 10));
            originXNumber = ui.PROPERTY_AddNumber("Origin.X: ", Origin.X, null, () => Origin += new Vector2(10, 0), () => Origin -= new Vector2(10, 0));
            originYNumber = ui.PROPERTY_AddNumber("Origin.Y: ", Origin.Y, null, () => Origin += new Vector2(0, 10), () => Origin -= new Vector2(0, 10));
            scaleXNumber = ui.PROPERTY_AddNumber("Scale.X: ", Scale.X, null, () => Scale += new Vector2(.1f, 0), () => Scale -= new Vector2(.1f, 0));
            scaleYNumber = ui.PROPERTY_AddNumber("Scale.Y: ", Scale.Y, null, () => Scale += new Vector2(0, .1f), () => Scale -= new Vector2(0, .1f));
            rotationNumber = ui.PROPERTY_AddNumber("Rotation: ", Rotation, null, () => Rotation += MathHelper.TwoPi * .01f, () => Rotation -= MathHelper.TwoPi * .01f);
            depthNumber = ui.PROPERTY_AddNumber("Depth: ", Depth, null, () => Depth += .01f, () => Depth -= .01f);
            ui.PROPERTY_AddSpacer(20);
            visibleText = ui.PROPERTY_AddButton((IsVisible ? "Visible" : "Hidden"), () => IsVisible = !IsVisible);
            attachText = ui.PROPERTY_AddLabelButton("Attached: " + (Attached != null ? Attached.ID : "None"), () => { if (Attached != null) ui.SetSelected(Attached); });
        }
        public void RefreshProperties()
        {
            posXNumber.Number = Position.X;
            posYNumber.Number = Position.Y;
            originXNumber.Number = Origin.X;
            originYNumber.Number = Origin.Y;
            scaleXNumber.Number = Scale.X;
            scaleYNumber.Number = Scale.Y;
            rotationNumber.Number = Rotation;
            visibleText.Text = (IsVisible ? "Visible" : "Hidden");
            attachText.Text = "Attached: " + (Attached != null ? Attached.ID : "None");
        }
        public void NullifyProperties()
        {
            posXNumber = null;
            posYNumber = null;
            depthNumber = null;
            rotationNumber = null;
            attachText = null;
        }
    }
}
