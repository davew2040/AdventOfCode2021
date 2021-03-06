using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode.DaySix
{
    internal class Day6 : DailyChallenge
    {
        public const int DefaultTimerCycle = 6;
        public const int SpawnedFishTimerGap = 2;

        public async Task Process()
        {
            var fish = await ReadAndParse("Day6/Data/day_6_input.txt");

            var fishAges = InitializeFishAges(fish);

            var finalFishPool = GetFishAgesByDayCount(256, fishAges);

            var finalFishCount = finalFishPool.Sum(f => f.Value);

            Console.Write($"Final fish count = {finalFishCount}");
        }
        
        private async Task<IEnumerable<LanternFish>> ReadAndParse(string filename)
        {
            var fishCycles = await File.ReadAllLinesAsync(filename);

            var timerValues = fishCycles[0].Split(",", StringSplitOptions.RemoveEmptyEntries);

            return timerValues.Select(v => int.Parse(v)).Select(f => new LanternFish(f));
        }

        private Dictionary<int, long> InitializeFishAges(IEnumerable<LanternFish> fishes)
        {
            var fishAges = new Dictionary<int, long>();

            foreach (var fish in fishes)
            {
                AddToKey(fish.Timer, 1, fishAges);
            }

            return fishAges;
        }

        private Dictionary<int, long> GetNextIteration(Dictionary<int, long> initialFishAges)
        {
            var nextFishAges = new Dictionary<int, long>();

            foreach (var kvp in initialFishAges)
            {
                var fishAge = kvp.Key;
                var fishCount = kvp.Value;

                if (fishAge == 0)
                {
                    AddToKey(DefaultTimerCycle, fishCount, nextFishAges);
                    AddToKey(DefaultTimerCycle + SpawnedFishTimerGap, fishCount, nextFishAges);
                }
                else
                {
                    AddToKey(fishAge - 1, fishCount, nextFishAges);
                }
            }

            return nextFishAges;
        }

        private void AddToKey(int key, long value, Dictionary<int, long> dictionary)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, 0);
            }

            dictionary[key] = dictionary[key] + value;
        }

        private Dictionary<int, long> GetFishAgesByDayCount(int dayCount, Dictionary<int, long> initialFishes)
        {
            Dictionary<int, long> fish = initialFishes;

            for (int d = 1; d <= dayCount; d++)
            {
                fish = GetNextIteration(fish);
            }

            return fish;
        }

        private class LanternFish
        {
            public int Timer { get; set; }

            public LanternFish(int timer = DefaultTimerCycle + SpawnedFishTimerGap)
            {
                Timer = timer;
            }
        }
    }
}
