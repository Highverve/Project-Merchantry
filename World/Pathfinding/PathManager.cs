using Merchantry.World.Pathfinding.Tiles;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchantry.World.Pathfinding
{
    public class PathManager
    {
        public float Euclidean(Point goal, Point current, float cost)
        {
            int dx = Math.Abs(current.X - goal.X);
            int dy = Math.Abs(current.Y - goal.Y);

            return cost * (float)Math.Sqrt(dx * dx + dy * dy);
        }
        public float Manhattan(Point next, Point goal)
        {
            int dx = Math.Abs(next.X - goal.X);
            int dy = Math.Abs(next.Y - goal.Y);

            return 5 * (dx + dy);
        }
        public float DiagonalHeuristic(Point next, Point goal, float minimumCost, float d2)
        {
            int dx = Math.Abs(next.X - goal.X);
            int dy = Math.Abs(next.Y - goal.Y);

            return minimumCost * (dx + dy) + (d2 - 2 * minimumCost) * Math.Min(dx, dy);
        }
        public float Heuristic(Point a, Point b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }

        public void Search(SquareGrid grid, Point start, Point end,
                           ref Dictionary<Point, Point> cameFrom,
                           ref Dictionary<Point, float> cost, bool isIgnoreTileCost = false)
        {
            PriorityQueue<Point> frontier = new PriorityQueue<Point>();
            frontier.Enqueue(start, 0);

            cameFrom.Add(start, start);
            cost.Add(start, 0);

            while (frontier.Count > 0)
            {
                Point current = frontier.Dequeue();
                if (current == end)
                    break;

                foreach (PathTile next in grid.Neighbours(current))
                {
                    float newCost = cost[current];
                    if (isIgnoreTileCost == false)
                        newCost += next.Cost(grid.Tiles[current]);
                    else
                        newCost += next.DiagonalCost(grid.Tiles[current]);

                    if (grid.WallCount(current, next.Position) == 0)
                    {
                        if (cost.ContainsKey(next.Position) == false)
                        {
                            cost[next.Position] = newCost;
                            cameFrom.Add(next.Position, current);
                            frontier.Enqueue(next.Position, newCost + Heuristic(end, next.Position));
                        }
                        else if (newCost < cost[next.Position])
                        {
                            cost[next.Position] = newCost;
                            cameFrom[next.Position] = current;
                            frontier.Enqueue(next.Position, newCost + Heuristic(end, next.Position));
                        }
                    }
                }
            }
        }
        public List<Point> ReconstructPath(Point start, Point end, Dictionary<Point, Point> cameFrom)
        {
            List<Point> path = new List<Point>();
            ReconstructPath(start, end, cameFrom, ref path);
            return path;
        }
        public void ReconstructPath(Point start, Point end, Dictionary<Point, Point> cameFrom, ref List<Point> path)
        {
            Point current = end;

            while (current != start)
            {
                path.Add(current);
                current = cameFrom[current];
            }
            path.Add(start);
            path.Reverse();
        }
    }
}
