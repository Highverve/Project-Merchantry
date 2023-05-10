using Merchantry.World.Pathfinding.Tiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Pathfinding
{
    public class SquareGrid
    {
        public const int TileWidth = 32;
        public const int TileHeight = 32;

        public Dictionary<Point, PathTile> Tiles { get; private set; } = new Dictionary<Point, PathTile>();
        public SquareGrid() { }

        public void AddTile(PathTile tile)
        {
            if (!Tiles.ContainsKey(tile.Position))
                Tiles.Add(tile.Position, tile);
        }
        public void AddTile(int x, int y, int cost)
        {
            AddTile(new PathTile(new Point(x, y), cost));
        }
        public void RemoveTile(int x, int y)
        {
            Tiles.Remove(new Point(x, y));
        }
        public void RemoveChunk(int x, int y, int width, int height)
        {
            for (int xIndex = x; xIndex < x + width; xIndex++)
            {
                for (int yIndex = y; yIndex < y + height; yIndex++)
                {
                    RemoveTile(xIndex, yIndex);
                }
            }
        }
        public void ApplyTile(int x, int y, Action<PathTile> action)
        {
            action?.Invoke(Tiles[new Point(x, y)]);
        }
        public void ApplyChunk(int x, int y, int width, int height, Action<PathTile> action)
        {
            for (int xIndex = x; xIndex < x + width; xIndex++)
            {
                for (int yIndex = y; yIndex < y + height; yIndex++)
                {
                    ApplyTile(xIndex, yIndex, action);
                }
            }
        }
        public void ReplaceTile(PathTile tile)
        {
            if (tile != null)
            {
                if (IsExists(tile.Position))
                    RemoveTile(tile.Position.X, tile.Position.Y);
                AddTile(tile);
            }
        }
        public PathTile GetTile(Point position)
        {
            if (Tiles.ContainsKey(position))
                return Tiles[position];
            return null;
        }
        public PathTile GetTile(int x, int y)
        {
            return GetTile(new Point(x, y));
        }

        public bool IsExists(Point position) { return Tiles.ContainsKey(position); }
        public bool IsFlag(Point position, string flag)
        {
            if (IsExists(position))
            {
                PathTile tile = Tiles[position];
                if (tile is FlagTile)
                    return ((FlagTile)tile).Flag.ToUpper() == flag.ToUpper();
            }
            return false;
        }
        public float BaseCost(Point position)
        {
            if (IsExists(position))
                return Tiles[position].BaseCost;
            return -1;
        }
        public float Cost(PathTile current, PathTile next)
        {
            return next.Cost(current);
        }
        public float Cost(Point current, Point next)
        {
            return Tiles[next].Cost(Tiles[current]);
        }
        public int WallCount(Point current, Point next)
        {
            int wallCount = 0;

            if (current.X != next.X && current.Y != next.Y)
            {
                int dirX = next.X - current.X; //If 3x to 2x, value is 1x direction.
                int dirY = next.Y - current.Y; //If current is 3y  and next is 4y, value is -1y direction.

                if (!Tiles.ContainsKey(new Point(current.X + dirX, current.Y)))
                    wallCount++;
                if (!Tiles.ContainsKey(new Point(current.X, current.Y + dirY)))
                    wallCount++;
            }

            return wallCount;
        }

        public static readonly Point[] Directions = new Point[]
        {
            new Point(0, -1), //N
            new Point(1, -1), //NE
            new Point(1, 0), //E
            new Point(1, 1), //SE
            new Point(0, 1), //S
            new Point(-1, 1), //SW
            new Point(-1, 0), //W
            new Point(-1, -1) //NW
        };
        public IEnumerable<PathTile> Neighbours(Point position)
        {
            for (int i = 0; i < Directions.Length; i++)
            {
                Point next = position + Directions[i];
                if (IsExists(next) && Tiles[next].IsActive)
                    yield return Tiles[next];
            }
        }

        public void Draw(SpriteBatch sb)
        {
            foreach (PathTile tile in Tiles.Values)
            {
                Vector2 position = new Vector2((tile.Position.X * TileWidth) + TileWidth / 2, (tile.Position.Y * TileHeight) + TileHeight / 2);
                GameManager.References().Debugging.DrawCircle(sb, position, 4, tile.Color);
                sb.DrawString(GameManager.References().Debugging.TinyFont, tile.BaseCost.ToString(), position + new Vector2(-3, 8), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 1);
            }
        }
    }
}
