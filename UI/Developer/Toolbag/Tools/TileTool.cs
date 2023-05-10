using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Merchantry.UI.Developer.General;
using Microsoft.Xna.Framework.Graphics;
using Merchantry.World.Tiles;
using Microsoft.Xna.Framework;

namespace Merchantry.UI.Developer.Toolbag.Tools
{
    public class TileTool : ToolData
    {
        private Point area;
        public Point Area
        {
            get { return area; }
            set
            {
                area.X = MathHelper.Max(value.X, 0);
                area.Y = MathHelper.Max(value.Y, 0);
            }
        }

        public TileTool()
        {
            area = new Point(2, 2);
        }

        public override void LeftClick()
        {
            if (References.Screens.Layers.IsMaximized && References.Screens.UI_IsMouseInsideUI() == false)
            {
                TileLayer selected = References.Screens.Layers.Selected;
                if (selected != null)
                {
                    if (References.Screens.Tiles.SelectedTileset != null && References.Screens.Tiles.SelectedTile != null)
                    {
                        if (References.Controls.IsMouseClicked(References.Controls.CurrentMS.LeftButton))
                        {
                            for (int y = 0; y < Area.Y; y++)
                            {
                                for (int x = 0; x < Area.X; x++)
                                {
                                    selected.AddTile(new Tile(References.Screens.Tiles.SelectedTileset.Tileset,
                                        References.Screens.Tiles.SelectedTile.Coords,
                                        selected.MouseCoords() + new Point(x, y)));
                                }
                            }
                        }
                    }
                }
            }
        }
        public override void MiddleClick() { }
        public override void RightClick()
        {
            if (References.Screens.Layers.IsMaximized && References.Screens.UI_IsMouseInsideUI() == false)
            {
                TileLayer selected = References.Screens.Layers.Selected;
                if (selected != null)
                {
                    if (References.Screens.Tiles.SelectedTileset != null && References.Screens.Tiles.SelectedTile != null)
                    {
                        if (References.Controls.IsMouseClicked(References.Controls.CurrentMS.RightButton))
                        {
                            for (int y = 0; y < Area.Y; y++)
                            {
                                for (int x = 0; x < Area.X; x++)
                                {
                                    selected.RemoveTile(selected.MousePosition() +
                                        new Vector2(x * World.WorldManager.TileWidth,
                                        y * World.WorldManager.TileHeight));
                                }
                            }
                        }
                    }
                }
            }
        }
        public override void Draw(SpriteBatch sb)
        {
            if (References.Screens.Layers.Selected != null)
            {
                for (int y = 0; y < Area.Y; y++)
                {
                    for (int x = 0; x < Area.X; x++)
                    {
                        TileLayer layer = References.Screens.Layers.Selected;
                        Point coords = layer.MouseCoords() + new Point(x, y);

                        References.Debugging.DrawWireframe(sb,
                            (coords.X * World.WorldManager.TileWidth) + (int)layer.Position.X,
                            (coords.Y * World.WorldManager.TileHeight) + (int)layer.Position.Y,
                            World.WorldManager.TileWidth,
                            World.WorldManager.TileHeight,
                            coords.X + ", " + coords.Y, "");
                    }
                }
            }
        }

        public override void NullifyProperties()
        {
        }
        public override void RefreshProperties()
        {
        }
        public override void SetProperties(PropertiesUI ui)
        {
        }
    }
}
