using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode.Day11
{
    internal class Day11 : DailyChallenge
    {
        private const int StepCount = int.MaxValue;
        private const string Filename = "Day11/Data/day_11_input.txt";

        public async Task Process()
        {
            var map = await ReadAndParse(Filename);
            int flashCount = 0;

            for (int i=1; i<=StepCount; i++)
            {
                (map, int stepFlashCount) = GetNextStep(map);
                flashCount = flashCount + stepFlashCount;

                if (stepFlashCount == map.Map.GetLength(0)*map.Map.GetLength(1))
                {
                    Console.WriteLine($"Final flash step = {i}");
                    break;
                }
            }

            Console.WriteLine($"Flash count = {flashCount}");
        }

        private async Task<OctoMap> ReadAndParse(string filename)
        {
            var lines = (await File.ReadAllLinesAsync(filename)).Where(l => !string.IsNullOrWhiteSpace(l));

            var xSize = lines.ElementAt(0).Length;
            var ySize = lines.Count();

            var octoMap = new OctoMap(xSize, ySize);

            for (int y=0; y<ySize; y++)
            {
                var line = lines.ElementAt(y);

                for (int x=0; x<xSize; x++)
                {
                    octoMap.Map[x, y] = line[x] - '0';
                }
            }

            return octoMap;
        }
        
        private (OctoMap map, int flashCount) GetNextStep(OctoMap map)
        {
            var nextMap = map.Copy();
            bool[,] hasFlashed = new bool[nextMap.Map.GetLength(0), nextMap.Map.GetLength(1)];
            int flashCount = 0;

            for (int x=0; x<nextMap.Map.GetLength(0); x++)
            {
                for (int y=0; y<nextMap.Map.GetLength(1); y++)
                {
                    var currentPoint = new Point { X = x, Y = y };
                    RaiseEnergy(currentPoint, nextMap, hasFlashed, ref flashCount);
                }
            }

            for (int x = 0; x < nextMap.Map.GetLength(0); x++)
            {
                for (int y = 0; y < nextMap.Map.GetLength(1); y++)
                {
                    if (hasFlashed[x, y])
                    {
                        nextMap.Map[x, y] = 0;
                    }
                }
            }

            return (nextMap, flashCount);
        }

        private IEnumerable<Point> GetAdjacentPoints(Point p)
        {
            List<Point> points = new List<Point>();

            for (int x=-1; x<=1; x++)
            {
                for (int y=-1; y<=1; y++)
                {
                    if (!(x == 0 && y == 0))
                    {
                        points.Add(new Point() { X = p.X + x, Y = p.Y + y });
                    }
                }
            }

            return points;
        }

        private void RaiseEnergy(Point p, OctoMap map, bool[,] flashed, ref int flashCount)
        {
            if (flashed[p.X, p.Y])
            {
                return;
            }

            map.Map[p.X, p.Y] = map.Map[p.X, p.Y] + 1;

            if (map.Map[p.X, p.Y] > 9)
            {
                flashed[p.X, p.Y] = true;
                flashCount++;

                var nextPoints = GetValidAdjacentPoints(p, map.Map);
                foreach (var nextPoint in nextPoints)
                {
                    RaiseEnergy(nextPoint, map, flashed, ref flashCount);
                }
            }
        }

        private bool PointIsValid<T>(Point p, T[,] array)
            => p.X >= 0 && p.X < array.GetLength(0)
                && p.Y >= 0 && p.Y < array.GetLength(1);

        private IEnumerable<Point> GetValidAdjacentPoints<T>(Point p, T[,] array)
            => GetAdjacentPoints(p).Where(p => PointIsValid(p, array));

        private void PrintMap(OctoMap map)
        {
            for (int y=0; y<map.Map.GetLength(1); y++)
            {
                for (int x=0; x<map.Map.GetLength(0); x++)
                {
                    Console.Write(map.Map[x, y]);
                }
                Console.WriteLine();
            }

            Console.WriteLine();
        }

        private class OctoMap
        {
            public int [,] Map { get; private set; }

            public OctoMap(int xSize, int ySize)
            {
                Map = new int[xSize, ySize];
            }

            public OctoMap Copy()
            {
                var newMap = new OctoMap(Map.GetLength(0), Map.GetLength(1));

                newMap.Map = ArrayCopy(Map);

                return newMap;
            }
        }

        private static T[,] ArrayCopy<T>(T[,] source)
        {
            var newArray = new T[source.GetLength(0), source.GetLength(1)];

            for (int i=0; i<source.GetLength(0); i++)
            {
                for (int j=0; j<source.GetLength(1); j++)
                {
                    newArray[i,j] = source[i,j];
                }
            }

            return newArray;
        }

        private struct Point
        {
            public int X, Y;
        }
    }
}
