using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode.Day15
{
    internal class Day15 : DailyChallenge
    {
        private const string Filename = "Day15/Data/problem_input.txt";

        public async Task Process()
        {
            var baseGrid = await ReadAndParse(Filename);

            Console.WriteLine();

            var grid = SplatGrid(baseGrid, 5, 5);

            var shortestPaths = new Dictionary<(int X, int Y), Path>();
            //var nextNodesQueue = new SortedSet<WeightedNode>(Comparer<WeightedNode>.Create(
            //    (n1, n2) => n1.Weight.CompareTo(n2.Weight)));

            var nextNodesQueue = new HashSet<WeightedNode>();
            (int X, int Y) currentNode = (0,0);

            for (int x=0; x<grid.GetLength(0); x++)
            {
                for (int y=0; y<grid.GetLength(1); y++)
                {
                    shortestPaths.Add(
                        (x, y),
                        new Path()
                        {
                            To = (x, y),
                            From = null,
                            Distance = int.MaxValue
                        });
                }
            }

            var startingPath = shortestPaths[currentNode];
            startingPath.Distance = 0;
            nextNodesQueue.Add(new WeightedNode()
            {
                Point = currentNode,
                Weight = 0
            });

            while (nextNodesQueue.Count > 0)
            {
                var min = nextNodesQueue.Min(n => n.Weight);
                var minElement = nextNodesQueue.First(p => p.Weight == min);
                nextNodesQueue.Remove(minElement);
                currentNode = minElement.Point;
                //nextNodesQueue.Remove(nextNodesQueue.First());

                if (currentNode.X == grid.GetLength(0)-1 && currentNode.Y == grid.GetLength(1)-1)
                {
                    break;
                }

                var currentPath = shortestPaths[currentNode];
                currentPath.Completed = true;

                var adjacentPoints = GetAdjacentPoints(currentNode)
                    .Where(p => PointIsValid(p, grid))
                    .Where(p => !shortestPaths[p].Completed);

                foreach (var adjacentPoint in adjacentPoints)
                {
                    var nextPointPath = shortestPaths[adjacentPoint];
                    var edgeDistance = grid[adjacentPoint.X, adjacentPoint.Y];
                    var totalDistanceToNode = currentPath.Distance + edgeDistance;

                    if (totalDistanceToNode < nextPointPath.Distance)
                    {
                        nextPointPath.Distance = totalDistanceToNode;
                        nextPointPath.From = currentNode;

                        nextNodesQueue.RemoveWhere(x => x.Point == adjacentPoint);
                        nextNodesQueue.Add(new WeightedNode()
                        {
                            Point = adjacentPoint,
                            Weight = totalDistanceToNode
                        });
                    }
                }
            }

            var path = GetPath((grid.GetLength(0) - 1, grid.GetLength(1) - 1), shortestPaths);
            var weightedPath = path.Select(p => grid[p.X, p.Y]);
            var sum = weightedPath.Sum() - grid[0,0];
        }

        private int[,] SplatGrid(int[,] grid, int xTimes, int yTimes)
        {
            int[,] newGrid = new int[grid.GetLength(0)*xTimes, grid.GetLength(1)*yTimes];

            for (int xCopy=0; xCopy<xTimes; xCopy++)
            {
                for (int yCopy=0; yCopy<yTimes; yCopy++)
                {
                    for (int x=0; x<grid.GetLength(0); x++)
                    {
                        for (int y=0; y<grid.GetLength(1); y++)
                        {
                            int shift = xCopy + yCopy;
                            newGrid[x + xCopy * grid.GetLength(0), y + yCopy * grid.GetLength(1)] = Wrap(grid[x, y] + shift);
                        }
                    }
                }
            }

            return newGrid;
        }

        private int Wrap(int x)
        {
            while (x > 9)
            {
                x -= 9;
            }

            return x;
        }

        private void PrintGrid(int[,] grid)
        {
            for (int y=0; y<grid.GetLength(1); y++)
            {
                for (int x=0; x<grid.GetLength(0); x++)
                {
                    Console.Write(grid[x, y]);
                }
                Console.WriteLine();
            }
        }

        private IEnumerable<(int X, int Y)> GetPath((int X, int Y) end, Dictionary<(int X, int Y), Path> shortestPaths)
        {
            var path = new List<(int X, int Y)>();
            var currentNode = end;
            path.Add(end);

            while (shortestPaths[currentNode].From != null)
            {
                path.Add(shortestPaths[currentNode].From.Value);
                currentNode = shortestPaths[currentNode].From.Value;       
            }

            path.Reverse();

            return path;
        }
        
        private IEnumerable<(int X, int Y)> GetAdjacentPoints((int X, int Y) point)
        {
            return new (int X, int Y)[]
            {
                (point.X, point.Y-1),
                (point.X, point.Y+1),
                (point.X-1, point.Y),
                (point.X+1, point.Y)
            };
        }

        private bool PointIsValid<T>((int X, int Y) point, T[,] grid)
        {
            return point.X >= 0 && point.X < grid.GetLength(0)
                && point.Y >= 0 && point.Y < grid.GetLength(1);
        }


        private class Path
        {
            public (int X, int Y) To { get; set; }
            public (int X, int Y)? From { get; set; }
            public int Distance { get; set; }
            public bool Completed { get; set; } = false;
        }

        private struct WeightedNode
        {
            public (int X, int Y) Point { get; set; }
            public int Weight { get; set; }

            public override int GetHashCode()
            {
                return Point.GetHashCode();
            }
        }

        private struct Edge
        {
            public (int X, int Y) To { get; set; }
            public (int X, int Y)? From { get; set; }
        }

        private async Task<int[,]> ReadAndParse(string filename)
        {
            var lines = await File.ReadAllLinesAsync(filename);

            int[,] grid = new int[lines[0].Length, lines.Length];

            for (int y = 0; y < lines.Length; y++)
            {
                for (int x = 0; x < lines[0].Length; x++)
                {
                    grid[x, y] = lines[y][x] - '0';
                }
            }

            return grid;
        }
    }
}
