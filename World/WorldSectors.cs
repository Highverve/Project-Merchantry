using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World
{
    public class WorldSectors
    {
        public WorldManager World { get; set; }

        public const int AbsentSectorLifetime = 10000;
        public const int SectorSize = 256;
        public Dictionary<Point, Sector> Sectors { get; set; } = new Dictionary<Point, Sector>();
        public Dictionary<Point, Sector> Recycled { get; set; } = new Dictionary<Point, Sector>();

        public Sector Sector(Point coords)
        {
            if (Sectors.ContainsKey(coords) == false)
            {
                if (Recycled.ContainsKey(coords))
                {
                    Sector sector = Recycled[coords];

                    Sectors.Add(coords, sector);
                    Recycled.Remove(coords);
                    RepopulateNeighbours();
                }
                else
                {
                    Sector sector = new Sector();
                    sector.Key = coords;
                    sector.Sectors = this;

                    Sectors.Add(coords, sector);
                    RepopulateNeighbours();
                }
            }

            return Sectors[coords];
        }
        private Sector CreateSector(Point coords)
        {
            if (Recycled.ContainsKey(coords) == false && Sectors.ContainsKey(coords) == false)
                return new Sector() { Key = coords, Sectors = this };
            return null;
        }
        private Sector RecycleSector(Point coords)
        {
            if (Recycled.ContainsKey(coords))
                return Recycled[coords];
            return null;
        }

        public void RepopulateNeighbours()
        {
            foreach (Sector s in Sectors.Values)
                s.PopulateNeighbours();
        }

        public void Iterate(Point coords, Action<WorldObject> action)
        {
            if (Sectors.ContainsKey(coords))
            {
                foreach (WorldObject obj in Sectors[coords].Objects)
                    action?.Invoke(obj);
            }
        }
        public void Iterate(int x, int y, Action<WorldObject> action)
        {
            Iterate(new Point(x, y), action);
        }
        public void Iterate(Action<WorldObject> action, params Point[] sectors)
        {
            for (int i = 0; i < sectors.Length; i++)
                Iterate(sectors[i], action);
        }

        public Point[] Neighbours(Point center)
        {
            return new Point[8]
            {
                new Point(-1, -1) + center,
                new Point(0, -1) + center,
                new Point(1, -1) + center,
                new Point(-1, 0) + center,
                new Point(1, 0) + center,
                new Point(-1, 1) + center,
                new Point(0, 1) + center,
                new Point(1, 1) + center,
            };
        }
        public Point[] NeighboursPlusCenter(Point center)
        {
            return new Point[9]
            {
                new Point(-1, -1) + center,
                new Point(0, -1) + center,
                new Point(1, -1) + center,
                new Point(-1, 0) + center,
                center,
                new Point(1, 0) + center,
                new Point(-1, 1) + center,
                new Point(0, 1) + center,
                new Point(1, 1) + center,
            };
        }
        public Point[] Bounds(Point coords, Point size)
        {
            Point[] result = new Point[size.X * size.Y];

            for (int y = 0; y < size.Y; y++)
            {
                for (int x = 0; x < size.X; x++)
                {
                    result[x * y] = new Point(coords.X + x, coords.Y + y);
                }
            }

            return result;
        }

        public bool Contains(Point coords)
        {
            return Recycled.ContainsKey(coords) || Sectors.ContainsKey(coords);
        }
        public bool InSector(Vector2 position, Point coords)
        {
            return ToSector(position) == coords;
        }
        public bool InSector(Vector2 position, Sector sector)
        {
            return ToSector(position) == sector.Key;
        }
        public bool InSector(WorldObject obj, Sector sector)
        {
            return sector.Objects.Contains(obj);
        }
        public Point ToSector(Vector2 position)
        {
            return new Point((int)position.X / SectorSize, (int)position.Y / SectorSize);
        }

        public void Update(GameTime gt)
        {
            foreach (Sector sector in Sectors.Values)
            {
                sector.Update(gt);

                if (sector.Lifetime <= 0)
                {
                    World.Queue.Add(0, () =>
                    {
                        Recycled.Add(sector.Key, sector);
                        sector.Recycle();

                        Sectors.Remove(sector.Key);
                    });
                }
            }
        }
        public void DrawDebug(SpriteBatch sb)
        {
            foreach (Sector s in Sectors.Values)
                GameManager.References().Debugging.DrawWireframe(sb, (s.Key.X * SectorSize) - 5, (s.Key.Y * SectorSize) - 5, SectorSize - 10, SectorSize - 10, s.Key.X + "." + s.Key.Y, string.Empty);
        }

        public List<WorldObject> SectorObjects(params Point[] points)
        {
            List<WorldObject> result = new List<WorldObject>();
            for (int i = 0; i < points.Length; i++)
                result.AddRange(SectorObjects(points[i]));

            return result;
        }
        public List<WorldObject> SectorObjects(Point point)
        {
            if (Sectors.ContainsKey(point))
                return Sectors[point].Objects;
            return null;
        }
    }
    public class Sector
    {
        public WorldSectors Sectors { private get; set;}

        public Point Key { get; set; }
        public List<WorldObject> Objects { get; private set; } = new List<WorldObject>();

        public List<Sector> Neighbours { get; set; } = new List<Sector>();
        public void PopulateNeighbours()
        {
            Neighbours.Clear();

            SetNeighbour(Key + new Point(-1, -1));
            SetNeighbour(Key + new Point(0, -1));
            SetNeighbour(Key + new Point(1, -1));

            SetNeighbour(Key + new Point(-1, 0));
            //THIS
            SetNeighbour(Key + new Point(1, 0));

            SetNeighbour(Key + new Point(-1, 1));
            SetNeighbour(Key + new Point(0, 1));
            SetNeighbour(Key + new Point(1, 1));
        }
        private void SetNeighbour(Point coords)
        {
            if (Sectors.Sectors.ContainsKey(coords))
                Neighbours.Add(Sectors.Sectors[coords]);
            else if (Sectors.Recycled.ContainsKey(coords))
                Neighbours.Add(Sectors.Recycled[coords]);
        }

        public void Iterate(WorldObject sender, Action<WorldObject> action)
        {
            for (int i = 0; i < Neighbours.Count; i++)
            {
                for (int o = 0; o < Neighbours[i].Objects.Count; o++)
                    action?.Invoke(Neighbours[i].Objects[o]);
            }
            for (int i = 0; i < Objects.Count; i++)
            {
                if (Objects[i] != sender)
                    action?.Invoke(Objects[i]);
            }
        }

        public void AddObject(WorldObject obj)
        {
            if (!Objects.Contains(obj))
            {
                Objects.Add(obj);
                onAddObject?.Invoke(obj);
            }
        }
        public void RemoveObject(WorldObject obj)
        {
            if (Objects.Contains(obj))
            {
                Objects.Remove(obj);
                onRemoveObject?.Invoke(obj);
            }
        }

        private Action<WorldObject> onAddObject, onRemoveObject;
        public event Action<WorldObject> OnAddObject { add { onAddObject += value; } remove { onAddObject -= value; } }
        public event Action<WorldObject> OnRemoveObject { add { onRemoveObject += value; } remove { onRemoveObject -= value; } }

        public int Lifetime { get; set; }
        public void Update(GameTime gt)
        {
            if (Objects.Count == 0) Lifetime -= gt.ElapsedGameTime.Milliseconds;
            else Lifetime = WorldSectors.AbsentSectorLifetime;
        }

        public void Recycle()
        {
            Objects.Clear();

            onAddObject = null;
            onRemoveObject = null;

            Lifetime = WorldSectors.AbsentSectorLifetime;
        }

        public override string ToString()
        {
            return Key.X + "." + Key.Y;
        }
    }
}
