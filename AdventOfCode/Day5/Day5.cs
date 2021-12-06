using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdventOfCode.DayFive
{ 

    internal class Day5 : DailyChallenge
    {
        public async Task Process()
        {
            var input = await ReadAndParse("DayFive/Data/day_5_input.txt");

            var markedMap = MarkLinesWithDiagonal(input);

            var multiMarkedPoints = markedMap.Encountered.Values.Where(v => v > 1);

            Console.WriteLine($"Count = {multiMarkedPoints.Count()}");
        }

        private async Task<IEnumerable<Line>> ReadAndParse(string filename)
        {
            var textLines = await File.ReadAllLinesAsync(filename);
            var result = new List<Line>();

            foreach (var textLine in textLines)
            {
                var matches = Regex.Matches(textLine, @"\d+");

                var numbers = matches.Select(x => int.Parse(x.Value)).ToArray();

                result.Add(
                    new Line
                    (
                        new Point
                        (
                            numbers[0], numbers[1]
                        ),
                        new Point
                        (
                            numbers[2], numbers[3]
                        )
                    )
                );
            }

            return result;
        }

        private LineMap MarkLines(IEnumerable<Line> lines)
        {
            var lineMap = new LineMap();

            foreach (var line in lines.Where(l => l.IsVertical || l.IsHorizontal))
            {
                var span = line.GetSpan();

                foreach (var p in span)
                {
                    lineMap.MarkPoint(p);
                } 
            }

            return lineMap;
        }

        private LineMap MarkLinesWithDiagonal(IEnumerable<Line> lines)
        {
            var lineMap = new LineMap();

            foreach (var line in lines.Where(l => l.IsVertical || l.IsHorizontal || l.IsDiagonal))
            {
                var span = line.GetSpan();

                foreach (var p in span)
                {
                    lineMap.MarkPoint(p);
                }
            }

            return lineMap;
        }

        private void PrintLineMap(int width, int height, LineMap map)
        {
            for (int y=0; y<height; y++)
            {
                for (int x=0; x<width; x++)
                {
                    var testPoint = new Point(x, y);
                    if (!map.Encountered.ContainsKey(testPoint))
                    {
                        Console.Write(".");
                    }
                    else
                    {
                        Console.Write(map.Encountered[testPoint]);
                    }
                }
                Console.Write("\n");
            }
        }

        private record Line(Point Start, Point End)
        {
            public bool IsHorizontal { get => Start.Y == End.Y; }
            public bool IsVertical { get => Start.X == End.X; }
            public bool IsDiagonal
            {
                get
                {
                    return Math.Abs(Start.X - End.X) == Math.Abs(Start.Y - End.Y);
                }
            }

            public IEnumerable<Point> GetSpan()
            {
                if (IsHorizontal)
                {
                    int min = Math.Min(Start.X, End.X);
                    int max = Math.Max(Start.X, End.X);

                    return Enumerable.Range(min, max-min+1).Select(x => new Point(x, Start.Y));
                }
                else if (IsVertical)
                {
                    int min = Math.Min(Start.Y, End.Y);
                    int max = Math.Max(Start.Y, End.Y);

                    return Enumerable.Range(min, max-min+1).Select(y => new Point(Start.X, y));
                }
                else if (IsDiagonal)
                {
                    var xSpan = GetIntSpan(Start.X, End.X);
                    var ySpan = GetIntSpan(Start.Y, End.Y);

                    return xSpan.Zip(ySpan).Select(z => new Point(z.First, z.Second));
                }

                throw new Exception("Can only be used on horizontal, vertical, or diagonal lines!");
            }
        }

        private static IEnumerable<int> GetIntSpan(int start, int end)
        {
            bool isPositive = end > start;
            var values = new List<int>();

            if (isPositive)
            {
                for (int i = start; i <= end; i++)
                {
                    values.Add(i);
                }
            }
            else
            {
                for (int i = start; i >= end; i--)
                {
                    values.Add(i);
                }
            }

            return values;
        }

        private record Point(int X, int Y);

        private class LineMap
        { 
            public Dictionary<Point, int> Encountered { get; } = new Dictionary<Point, int>(); 

            public void MarkPoint(Point p)
            {
                if (!Encountered.ContainsKey(p))
                {
                    Encountered[p] = 0;
                }

                Encountered[p] = Encountered[p] + 1;
            }
        }
    }
}
