using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode.DayOne
{
    internal class DayOne
    {
        public async Task Process()
        {
            Console.WriteLine(await CountWindows("DayOne/input.txt", 3));
        }

        public async Task<int> CountWindows(string filename, int windowSize)
        {
            var input = await ReadAndParse(filename);

            var windows = GetWindows(input, windowSize);

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

        public IList<IEnumerable<int>> GetWindows(IList<int> measurements, int windowSize)
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


        public async Task<int> CountIncreases(string filename)
        {
            var input = await ReadAndParse(filename);

            int increases = 0;
            
            for (int i=1; i<input.Count(); i++)
            {
                if (input.ElementAt(i) > input.ElementAt(i - 1))
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
