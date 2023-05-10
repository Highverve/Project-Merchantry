using Merchantry.World.Pathfinding;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Meta
{
    public class ObjectPath
    {
        public WorldObject Object { get; set; }

        #region Variables

        private List<Point> path = new List<Point>();
        private Dictionary<Point, Point> cameFrom = new Dictionary<Point, Point>();
        public Dictionary<Point, float> cost = new Dictionary<Point, float>();

        public List<Point> Path { get { return path; } }
        public Dictionary<Point, Point> CameFrom { get { return cameFrom; } }
        public Dictionary<Point, float> Cost { get { return cost; } }
        
        #endregion

        public void Find(Point start, Point end, bool ignoreTileCost = false)
        {
            path.Clear();
            cameFrom.Clear();
            cost.Clear();

            if (GameManager.References().World.NavGrid.IsExists(start) &&
                GameManager.References().World.NavGrid.IsExists(end))
            {
                Object.World.Pathfinder.Search(Object.World.NavGrid, start, end, ref cameFrom, ref cost, ignoreTileCost);
                Object.World.Pathfinder.ReconstructPath(start, end, cameFrom, ref path);

                onPathStart?.Invoke();
            }
        }
        public float Distance()
        {
            float result = 0;
            for (int i = 0; i < Path.Count - 1; i++)
                result += Vector2.Distance(ToVector(path[i]), ToVector(path[i + 1]));
            return result;
        }
        public Vector2 Next()
        {
            if (IsComplete() == false)
                return ToVector(path.First());
            else
                return Object.Position;
        }
        public void RemoveFirst()
        {
            if (IsComplete() == false)
            {
                path.RemoveAt(0);
                onNextTile?.Invoke(path[0]);
            }

            if (IsComplete() == true)
            {
                onPathComplete?.Invoke();
                onPathComplete = null;
            }
        }
        public bool IsComplete() { return Path.Count == 0; }

        public Vector2 ToVector(Point position)
        {
            return new Vector2((position.X * SquareGrid.TileWidth) + SquareGrid.TileWidth / 2,
                               (position.Y * SquareGrid.TileHeight) + SquareGrid.TileHeight / 2);
        }
        public Point ToPoint(Vector2 position)
        {
            return new Point((int)position.X / SquareGrid.TileWidth,
                             (int)position.Y / SquareGrid.TileHeight);
        }
        public Point Coords() { return ToPoint(Object.Position); }

        private event Action onPathStart, onPathComplete;
        private event Action<Point> onNextTile;
        public event Action OnPathStart { add { onPathStart += value; } remove { onPathStart -= value; } }
        /// <summary>
        /// Note: This event is cleared (set to null) after being called.
        /// </summary>
        public event Action OnPathComplete { add { onPathComplete += value; } remove { onPathComplete -= value; } }
        public event Action<Point> OnNextTile { add { onNextTile += value; } remove { onNextTile -= value; } }
    }
}
