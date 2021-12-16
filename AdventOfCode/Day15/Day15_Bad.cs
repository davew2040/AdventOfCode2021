using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode.Day15
{
    internal class Day15_Bad : DailyChallenge
    {
        private const string Filename = "Day15/Data/problem_input.txt";

        public async Task Process()
        {
            var grid = await ReadAndParse(Filename);

            var shortestPath = FindShortestPath(grid);
        }

        private WeightedPath FindShortestPath(int[,] riskGrid)
        {
            var start = (0, 0);

            var context = new GraphTraversalContext()
            {
                CurrentPath = new WeightedPath()
                {
                    Path = new Stack<(int X, int Y)>(new[] { start }),
                    Cost = 0
                },
                ShortestPath = new WeightedPath()
                {
                    Cost = int.MaxValue
                },
                Start = start,
                End = (riskGrid.GetLength(0)-1, riskGrid.GetLength(1)-1),
                RiskGrid = riskGrid,
                Visited = new HashSet<(int X, int Y)>() { start }
            };

            FindShortestPath(start, context);

            return context.ShortestPath;
        }

        private void FindShortestPath((int X, int Y) currentPoint, GraphTraversalContext context)
        {
            if (context.CurrentPath.Cost > context.ShortestPath.Cost)
            {
                return;
            }

            if (currentPoint == context.End)
            {
                if (context.CurrentPath.Cost < context.ShortestPath.Cost)
                {
                    context.ShortestPath = new WeightedPath()
                    {
                        Path = new Stack<(int X, int Y)>(context.CurrentPath.Path.Select(x => x).Reverse()),
                        Cost = context.CurrentPath.Cost
                    };
                }

                return;
            }

            var nextPoints = GetAdjacentPoints(currentPoint)
                .Where(x => PointIsValid(x, context.RiskGrid))
                .Where(p => !context.Visited.Contains(p))
                .ToList();

            foreach (var nextPoint in nextPoints)
            {
                context.CurrentPath.Path.Push(nextPoint);
                context.CurrentPath.Cost += context.RiskGrid[nextPoint.X, nextPoint.Y];
                context.Visited.Add(nextPoint);

                FindShortestPath(nextPoint, context);

                context.Visited.Remove(nextPoint);
                context.CurrentPath.Cost -= context.RiskGrid[nextPoint.X, nextPoint.Y];
                context.CurrentPath.Path.Pop();
            }
        }

        private bool PointIsValid<T>((int X, int Y) point, T[,] grid)
        {
            return point.X >= 0 && point.X < grid.GetLength(0) 
                && point.Y >= 0 && point.Y < grid.GetLength(1);
        }

        private IEnumerable<(int X, int Y)> GetAdjacentPoints((int X, int Y) point)
        {
            return new (int X, int Y)[]
            {
                (point.X-1, point.Y),
                (point.X+1, point.Y),
                (point.X, point.Y-1),
                (point.X, point.Y+1)
            };
        }

        private class GraphTraversalContext
        {
            public WeightedPath ShortestPath { get; set; }
            public WeightedPath CurrentPath { get; set; }
            public (int X, int Y) Start { get; set; }
            public (int X, int Y) End { get; set; }
            public int[,] RiskGrid { get; set; }
            public HashSet<(int X, int Y)> Visited = new HashSet<(int X, int Y)>();
        }

        private class WeightedPath
        {
            public Stack<(int X, int Y)> Path { get; set; } = new Stack<(int X, int Y)>();
            public int Cost { get; set; }
        }

        private async Task<int[,]> ReadAndParse(string filename)
        {
            var lines = await File.ReadAllLinesAsync(filename);

            int[,] grid = new int[lines[0].Length, lines.Length];

            for (int y=0; y<lines.Length; y++)
            {
                for (int x=0; x<lines[0].Length; x++)
                {
                    grid[x, y] = lines[y][x] - '0';
                }
            }

            return grid;
        }
    }
}
