using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode.Day9
{
    internal class Day9 : DailyChallenge
    {
        public async Task Process()
        {
            var data = await ReadAndParse("Day9/Data/day_9_input.txt");

            var lowPoints = GetLowPoints(data);

            var basins = lowPoints.Select(p => GetBasin(p, data));

            var orderedBasins = basins.OrderByDescending(b => b.Visited.Count());

            var topThree = orderedBasins.Take(3).Select(b => b.Visited.Count());

            var mult = topThree.Aggregate(1, (sum, next) => sum * next);
        }

        private Basin GetBasin(Coord lowPoint, Data data)
        {
            var basin = new Basin(lowPoint);

            var stack = new Stack<Coord>();

            stack.Push(lowPoint);

            while (stack.Count() != 0)
            {
                var next = stack.Pop();

                var testPoints = GetAdjacentPoints(next);

                var validTestPoints = testPoints.Where(p => data.IsValid(p));

                foreach (var testPoint in validTestPoints)
                {
                    if (data.GetValue(testPoint) != 9 && !basin.Contains(testPoint))
                    {
                        stack.Push(testPoint);
                        basin.Add(testPoint);
                    }
                }
            }

            return basin;

        }

        private IEnumerable<Coord> GetLowPoints(Data data)
        {
            var lowPoints = new List<Coord>();

            for (int y = 0; y < data.GetHeight(); y++)
            {
                for (int x = 0; x < data.GetWidth(); x++)
                {
                    var testPoint = new Coord(x, y);

                    if (IsLowPoint(data, testPoint))
                    {
                        lowPoints.Add(testPoint);
                    }
                }
            }

            return lowPoints;
        }

        private bool IsLowPoint(Data data, Coord coord)
        {
            var adjacentPoints = GetAdjacentPoints(coord);

            var validPoints = adjacentPoints.Where(p => data.IsValid(p));

            return validPoints.All(testPoint => data.GetValue(testPoint) > data.GetValue(coord));
        }

        private List<Coord> GetAdjacentPoints(Coord point)
        {
            List<Coord> testPoints = new List<Coord>();

            testPoints.Add(new Coord(point.X - 1, point.Y));
            testPoints.Add(new Coord(point.X + 1, point.Y));
            testPoints.Add(new Coord(point.X, point.Y - 1));
            testPoints.Add(new Coord(point.X, point.Y + 1));

            return testPoints;
        }

        private async Task<Data> ReadAndParse(string filename)
        {
            var lines = (await File.ReadAllLinesAsync(filename)).Select(l => l.Trim());

            var width = lines.ElementAt(0).Length;
            var height = lines.Count();

            var data = new Data(width, height);

            for (int i=0; i<lines.Count(); i++)
            {
                for (int c=0; c<lines.ElementAt(0).Length; c++)
                {
                    data.HeightMap[i, c] = lines.ElementAt(i).ElementAt(c) - '0';
                }
            }

            return data;
        }

        private record Coord(int X, int Y);

        private class Data
        {
            public int[,] HeightMap { get; set; }

            public Data(int width, int height)
            {
                HeightMap = new int[height, width];
            }

            public int GetHeight()
            {
                return HeightMap.GetLength(0);
            }

            public int GetWidth()
            {
                return HeightMap.GetLength(1);
            }

            public int GetValue(Coord coord)
            {
                return HeightMap[coord.Y, coord.X];
            }

            public int GetRiskLevel(Coord coord)
            {
                return HeightMap[coord.Y, coord.X] + 1;
            }

            public bool IsValid(Coord coord)
            {
                return (coord.X >= 0 && coord.X < GetWidth())
                    && (coord.Y >= 0 && coord.Y < GetHeight());
            }
        }

        private class Basin
        {
            public readonly Dictionary<string, Coord> Visited = new Dictionary<string, Coord>();

            public Basin(Coord point)
            {
                Add(point);
            }

            public bool Contains(Coord coord)
                => Visited.ContainsKey(Keyify(coord));

            public bool Add(Coord coord)
            {
                if (Visited.ContainsKey(Keyify(coord)))
                {
                    return false;
                }

                Visited[Keyify(coord)] = coord;

                return true;
            }

            private string Keyify(Coord coord)
                => $"{coord.X}:{coord.Y}";
        }
    }
}
