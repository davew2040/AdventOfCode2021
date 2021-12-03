using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode.DayOne
{
    internal class DayOne : DailyChallenge
    {
        public async Task Process()
        {
            var data = await ReadAndParse("DayOne/Data/input.txt");
            Console.WriteLine(CountWindowsLinear(data, 3));
        }

        private int CountWindowsLinear(IList<int> data, int windowSize)
        {
            int index = 0;
            int increaseCount = 0;

            var valuesQueue = new Queue<int>();

            for (int i = 0; i < windowSize; i++)
            {
                valuesQueue.Enqueue(data[index++]);
            }

            int lastSum = valuesQueue.Sum();

            while (index < data.Count())
            {
                int nextValue = data[index++];
                int nextSum = lastSum + nextValue - valuesQueue.Dequeue();
                valuesQueue.Enqueue(nextValue);

                if (nextSum > lastSum)
                {
                    increaseCount++;
                }

                lastSum = nextSum;
            }

            return increaseCount;
        }

        private int CountWindows(IList<int> data, int windowSize)
        {
            var windows = GetWindows(data, windowSize);

            int increases = 0;

            for (int i = 1; i < windows.Count(); i++)
            {
                if (windows.ElementAt(i).Sum() > windows.ElementAt(i - 1).Sum())
                {
                    increases++;
                }
            }

            return increases;
        }

        private IList<IEnumerable<int>> GetWindows(IList<int> measurements, int windowSize)
        {
            var windows = new List<IEnumerable<int>>(); 

            for (int i = 0; i < measurements.Count-windowSize+1; i++)
            {
                var window = new List<int>();

                for (int j=0; j<windowSize; j++)
                {
                    window.Add(measurements[i+j]);
                }

                windows.Add(window);
            }

            return windows;
        }
        private int CountIncreases(IList<int> data)
        {
            int increases = 0;
            
            for (int i=1; i<data.Count(); i++)
            {
                if (data.ElementAt(i) > data.ElementAt(i - 1))
                {
                    increases++;
                }
            }

            return increases;
        }

        private async Task<IList<int>> ReadAndParse(string filename)
        {
            var lines = await File.ReadAllLinesAsync(filename);

            return lines.Select(l => int.Parse(l)).ToList();
        }
    }
}
