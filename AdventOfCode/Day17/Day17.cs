using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode.Day17
{
    internal class Day17 : DailyChallenge
    {
        private const string Filename = "Day17/Data/puzzle_input.txt";

        public async Task Process()
        {
            var input = await ReadAndParse(Filename);

            //var maxPath = FindMaximumY(input);
            //var maxY = maxPath.Max(p => p.Y);

            var velocities = FindValidVelocities(input);
            Console.WriteLine($"Count = {velocities.Count()}");
        }

        private async Task<Input> ReadAndParse(string filename)
        {
            var line = (await File.ReadAllLinesAsync(filename)).First();
            var commaSplit = line.Split(",");
            var equalSplit = commaSplit.Select(x => x.Split("="));
            var input = new Input();

            var xValues = line.Split(",")[0].Split("=")[1].Split(".", StringSplitOptions.RemoveEmptyEntries);
            var yValues = line.Split(",")[1].Split("=")[1].Split(".", StringSplitOptions.RemoveEmptyEntries);

            input.BoundsStart = (int.Parse(xValues[0]), int.Parse(yValues[0]));
            input.BoundsEnd = (int.Parse(xValues[1]), int.Parse(yValues[1]));

            return input;
        }

        public IEnumerable<(int X, int Y)> FindMaximumY(Input input)
        {
            IEnumerable<(int X, int Y)> maxPath = null;
            int currentMax = int.MinValue;

            for (int x=1; x<input.BoundsEnd.X; x++)
            {
                for (int y=0; y<input.BoundsStart.X*3; y++)
                {
                    var path = GetPath((x, y), input);
                    if (path != null)
                    {
                        var maxY = path.Select(x => x.Y).Max();
                        if (maxY > currentMax)
                        {
                            maxPath = path;
                            currentMax = maxY;
                        }
                    }
                }
            }

            return maxPath;
        }

        public IEnumerable<(int X, int Y)> FindValidVelocities(Input input)
        {
            List<(int X, int Y)> velocities = new List<(int X, int Y)>();
            for (int x = 1; x <= input.BoundsEnd.X; x++)
            {
                for (int y = -Math.Abs(input.BoundsStart.Y)-1; y < Math.Abs(input.BoundsStart.Y)+1; y++)
                {
                    var path = GetPath((x, y), input);
                    if (path != null)
                    {
                        velocities.Add((x, y));
                    }
                }
            }

            return velocities;
        }

        public IEnumerable<(int X, int Y)>? GetPath((int x, int y) velocity, Input input)
        {
            var points = new List<(int X, int Y)>();

            (int X, int Y) currentPosition = (0, 0);
            (int X, int Y) currentVelocity = velocity;
  
            while (true)
            {
                currentPosition = (
                    currentPosition.X + currentVelocity.X,
                    currentPosition.Y + currentVelocity.Y
                );
                currentVelocity = (
                    currentVelocity.X != 0 ? currentVelocity.X - 1 : 0,
                    currentVelocity.Y = currentVelocity.Y - 1
                );

                points.Add(currentPosition);

                if (IsWithin(currentPosition, input))
                {
                    return points;
                }
                if (currentVelocity.Y < 0 && currentPosition.Y < input.BoundsStart.Y)
                {
                    break;
                }
            }

            return null;
        }

        public bool IsWithin((int X, int Y) position, Input input)
        {
            return position.X >= input.BoundsStart.X && position.X <= input.BoundsEnd.X
                && position.Y >= input.BoundsStart.Y && position.Y <= input.BoundsEnd.Y;
        }

        public class Input
        {
            public (int X, int Y) BoundsStart { get; set; }
            public (int X, int Y) BoundsEnd { get; set; }
        }
    }
}
