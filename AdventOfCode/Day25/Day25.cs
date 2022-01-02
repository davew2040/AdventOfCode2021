using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode.Day25
{
    internal class Day25 : DailyChallenge
    {
        private const string Filename = "Day25/Data/problem_input.txt";

        private const char EastMarker = '>';
        private const char SouthMarker = 'v';
        private const char EmptyMarker = '.';

        public async Task Process()
        {
            var map = await ReadAndParse(Filename);

            var steps = StepsToEquilibirum(map);

            Console.WriteLine($"Steps = {steps}");
        }

        public int StepsToEquilibirum(Map map)
        {
            int steps = 0;

            while (true)
            {
                int moveCount = NextStep(map);
                if (moveCount == 0)
                {
                    break;
                }
                else
                {
                    steps++;
                }
            }

            return steps + 1;
        }

        public int NextStep(Map map)
        {
            int east = NextEast(map);
            int south = NextSouth(map);

            return east + south;
        }

        private int NextEast(Map map)
        {
            var shouldMove = new List<Point>();

            foreach (var east in map.Easties)
            {
                var eastAdjacent = map.GetEastAdjacent(east);
                if (map.GetValue(eastAdjacent) == EmptyMarker)
                {
                    shouldMove.Add(east);
                }
            }

            foreach (var move in shouldMove)
            {
                map.Move(move, map.GetEastAdjacent(move));
            }

            return shouldMove.Count();
        }

        private int NextSouth(Map map)
        {
            var shouldMove = new List<Point>();

            foreach (var south in map.Southies)
            {
                var southAdjacent = map.GetSouthAdjacent(south);
                if (map.GetValue(southAdjacent) == EmptyMarker)
                {
                    shouldMove.Add(south);
                }
            }

            foreach (var move in shouldMove)
            {
                map.Move(move, map.GetSouthAdjacent(move));
            }

            return shouldMove.Count();
        }

        private async Task<Map> ReadAndParse(string filename)
        {
            var lines = await File.ReadAllLinesAsync(filename);
            var map = new Map(lines[0].Length, lines.Length);

            for (int y=0; y<lines.Length; y++)
            {
                var line = lines[y];
                for (int x=0; x<line.Length; x++)
                {
                    char value = line[x];
                    map.Spots[x, y] = value;

                    if (value == EastMarker)
                    {
                        map.Easties.Add(new Point(x, y));
                    }
                    else if (value == SouthMarker)
                    {
                        map.Southies.Add(new Point(x, y));
                    }
                }
            }

            return map;
        }

        public class Map
        {
            public char[,] Spots;

            public HashSet<Point> Easties = new HashSet<Point>();
            public HashSet<Point> Southies = new HashSet<Point>();

            public Map(int width, int height)
            {
                Spots = new char[width, height];
            }

            public Point GetEastAdjacent(Point p)
            {
                int nextX = p.X + 1;
                if (nextX >= Spots.GetLength(0))
                {
                    nextX = 0;
                }
                return new Point(nextX, p.Y); ;
            }

            public Point GetSouthAdjacent(Point p)
            {
                int nextY = p.Y + 1;
                if (nextY >= Spots.GetLength(1))
                {
                    nextY = 0;
                }
                return new Point(p.X, nextY);
            }

            public void Print()
            {
                for (int y=0; y<Spots.GetLength(1); y++)
                {
                    for (int x=0; x<Spots.GetLength(0); x++)
                    {
                        Console.Write(Spots[x, y]);
                    }
                    Console.WriteLine();
                }
                Console.WriteLine();
            }

            public void Move(Point from, Point to)
            {
                var fromValue = GetValue(from);
                var toValue = GetValue(to);

                if (fromValue == EmptyMarker)
                {
                    throw new ArgumentException("Moving empty space!");
                }
                if (toValue != EmptyMarker)
                {
                    throw new ArgumentException("Attempted move to filled space.");
                }

                if (fromValue == EastMarker)
                {
                    Easties.Remove(from);
                    Easties.Add(to);
                }
                else if (fromValue == SouthMarker)
                {
                    Southies.Remove(from);
                    Southies.Add(to);
                }

                SetValue(to, fromValue);
                SetValue(from, EmptyMarker);
            }

            public char GetValue(Point p)
                => Spots[p.X, p.Y];

            public void SetValue(Point p, char value)
            {
                Spots[p.X, p.Y] = value;
            }
        }
       
        public struct Point
        {
            public int X { get; }
            public int Y { get; }

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }

            public override int GetHashCode() => (X, Y).GetHashCode();
        }
    }
}
