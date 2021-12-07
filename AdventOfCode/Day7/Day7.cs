using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode.Day7
{
    internal class Day7 : DailyChallenge
    {
        public async Task Process()
        {
            var positions = await ReadAndParse("Day7/Data/day_7_input.txt");

            var fuel = FindLeastFuelSummation(positions);

            Console.WriteLine($"Minimum fuel = {fuel}");
        }

        private int TotalFuelToMoveToPosition(int targetPosition, IEnumerable<int> positions, int maxFuel)
        {
            int allCrabsSum = 0;

            foreach (var position in positions)
            {
                allCrabsSum += Math.Abs(position - targetPosition);

                if (allCrabsSum > maxFuel)
                {
                    return int.MaxValue;
                }
            }

            return allCrabsSum;
        }

        private int TotalFuelToMoveToPositionSummation(int targetPosition, IEnumerable<int> positions, int maxFuel)
        {
            int allCrabsSum = 0;

            foreach (var position in positions)
            {
                var distance = Math.Abs(position - targetPosition);
                var distanceFuel = Summation.GetSum(distance);
                allCrabsSum += distanceFuel;

                if (allCrabsSum > maxFuel)
                {
                    return int.MaxValue;
                }
            }

            return allCrabsSum;
        }

        private int FindLeastFuel(IEnumerable<int> positions)
        {
            var boundaries = FindBoundaries(positions);
            int leastFuel = int.MaxValue;

            for (int i = boundaries.least; i <= boundaries.most; i++)
            {
                int fuel = TotalFuelToMoveToPosition(i, positions, leastFuel);
                if (fuel < leastFuel)
                {
                    leastFuel = fuel;
                }
            }

            return leastFuel;
        }

        private int FindLeastFuelSummation(IEnumerable<int> positions)
        {
            var boundaries = FindBoundaries(positions);
            int leastFuel = int.MaxValue;

            for (int i = boundaries.least; i <= boundaries.most; i++)
            {
                int fuel = TotalFuelToMoveToPositionSummation(i, positions, leastFuel);
                if (fuel < leastFuel)
                {
                    leastFuel = fuel;
                }
            }

            return leastFuel;
        }

        private (int least, int most) FindBoundaries(IEnumerable<int> positions)
        {
            int least = int.MaxValue;
            int most = int.MinValue;

            foreach (var position in positions)
            {
                if (position < least)
                {
                    least = position;
                }

                if (position > most)
                {
                    most = position;
                } 
            }

            return (least, most);
        }

        private async Task<IEnumerable<int>> ReadAndParse(string filename)
        {
            string firstLine = (await File.ReadAllLinesAsync(filename)).First();
            return firstLine.Split(",").Select(x => int.Parse(x));
        }

        private static class Summation 
        {
            private static readonly Dictionary<int, int> _aggregation = new Dictionary<int, int>()
            {
                [0] = 0,
                [1] = 1
            };

            public static int GetSum(int n)
            {
                if (n < 0)
                {
                    throw new ArgumentException($"{n} must be >= 0");
                }

                if (_aggregation.ContainsKey(n))
                {
                    return _aggregation[n];
                }
                else
                {
                    var summation = n + GetSum(n - 1);
                    _aggregation[n] = summation;
                    return summation;
                }
            }
        }
    }
}
