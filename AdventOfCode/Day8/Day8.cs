using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode.Day8
{
    public static class Extensions
    {
        public static HashSet<HashSet<char>> Subtract(this HashSet<HashSet<char>> source, HashSet<HashSet<char>> toSubtract)
        {
            HashSet<HashSet<char>> subtracted = new HashSet<HashSet<char>>();

            foreach (var c in source)
            {
                if (!toSubtract.Select(x => x.Stringify()).Contains(c.Stringify()))
                {
                    subtracted.Add(c);
                }
            }

            return subtracted;
        }

        public static HashSet<char> Subtract(this HashSet<char> source, HashSet<char> toSubtract)
        {
            HashSet<char> subtracted = new HashSet<char>();

            foreach (var c in source)
            {
                if (!toSubtract.Contains(c))
                {
                    subtracted.Add(c);
                }
            }

            return subtracted;
        }

        public static string Stringify(this HashSet<char> set)
        {
            return string.Join("", set.OrderBy(c => c));
        }
    }

    internal class Day8 : DailyChallenge
    {
        private readonly char[] digits = { 'a', 'b', 'c', 'd', 'e', 'f', 'g' };

        public async Task Process()
        {
            var allData = await ReadAndParse("Day8/Data/day_8_input.txt");

            var sum = allData.Select(Decode).Aggregate((sum, val) => sum + val);
        }

        private int Decode(DigitData data)
        {
            var table = GetTranslationTable(data.InputDigits);
            List<int> digits = new List<int>();
    
            foreach (var outputDigits in data.OutputDigits)
            {
                var key = table.Keys.Single(k => k == outputDigits.Stringify());

                var outputNumber = table[key];

                digits.Add(outputNumber);
            }

            int sum = 0;
            int multiplier = 1;

            for (int i= digits.Count-1; i>=0; i--)
            {
                sum += digits.ElementAt(i) * multiplier;
                multiplier *= 10;
            }

            return sum;
        }

        private Dictionary<string, int> GetTranslationTable(IEnumerable<HashSet<char>> data)
        {
            var testTable = InitializeTestTable();

            var setOfSets = data.ToHashSet();

            var bySize = setOfSets.GroupBy(x => x.Count(), x => x)
                .ToDictionary(x => x.Key, x => x.ToHashSet());
            var segmentTable = new Dictionary<char, HashSet<char>>();
            var translationTable = new Dictionary<int, HashSet<HashSet<char>>>();

            segmentTable['a'] = bySize[3].First().Subtract(bySize[2].First());

            translationTable[1] = bySize[2];
            translationTable[7] = bySize[3];
            translationTable[4] = bySize[4];
            translationTable[8] = bySize[7];

            translationTable[9] = bySize[6]
                .Where(six => six.Subtract(bySize[4].First()).Count() == 2)
                .ToHashSet();
            translationTable[2] = bySize[5]
                .Where(five => five.Subtract(bySize[4].First()).Count() == 3)
                .ToHashSet();
            translationTable[6] = bySize[6]
                .Where(five => five.Subtract(bySize[3].First()).Count() == 4)
                .ToHashSet();
            translationTable[0] = bySize[6]
                .Subtract(translationTable[9])
                .Subtract(translationTable[6]);

            segmentTable['e'] = translationTable[8].First()
                .Subtract(translationTable[9].First());
            segmentTable['g'] = translationTable[9].First()
                .Subtract(translationTable[4].First())
                .Subtract(segmentTable['a']);
            segmentTable['c'] = translationTable[9].First()
                .Subtract(translationTable[6].First())
                .Subtract(segmentTable['e']);

            translationTable[5] = new HashSet<HashSet<char>>()
            {
                translationTable[9].First()
                    .Subtract(segmentTable['c'])
            };

            translationTable[3] = bySize[5]
                .Subtract(translationTable[2])
                .Subtract(translationTable[5]);

            return translationTable.ToDictionary(x => x.Value.First().Stringify(), x => x.Key);
        }

        private Dictionary<char, HashSet<char>> InitializeTestTable()
        {
            var testTable = new Dictionary<char, HashSet<char>>();

            foreach (var digit in digits)
            {
                var newSet = new HashSet<char>();
                foreach (var digit2 in digits)
                {
                    newSet.Add(digit2);
                }

                testTable.Add(digit, newSet);
            }

            return testTable;
        }

        private int CountUniqueDigits(IEnumerable<DigitData> datas)
        {
            int[] uniqueDigitLengths = { 2, 3, 4, 7 };
            int total = 0;

            foreach (var data in datas)
            {
                foreach (var outputDigits in data.OutputDigits)
                {
                    if (uniqueDigitLengths.Contains(outputDigits.Count()))
                    {
                        total++;
                    }
                }
            }

            return total;
        }

        private async Task<IEnumerable<DigitData>> ReadAndParse(string filename)
        {
            var lines = await File.ReadAllLinesAsync(filename);
            var digitDatas = new List<DigitData>();

            foreach (var line in lines)
            {
                var pipeSplit = line.Split('|', StringSplitOptions.RemoveEmptyEntries);
                var inputDigitsSplit = pipeSplit[0].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var outputDigitsSplit = pipeSplit[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var newDigitData = new DigitData();

                newDigitData.InputDigits = inputDigitsSplit.Select(DigitsToHashSet);
                newDigitData.OutputDigits = outputDigitsSplit.Select(DigitsToHashSet);

                digitDatas.Add(newDigitData);
            }

            return digitDatas;
        }

        private HashSet<char> DigitsToHashSet(string s)
        {
            var set = new HashSet<char>();

            foreach (char c in s)
            {
                if (!set.Contains(c))
                {
                    set.Add(c);
                }
            }

            return set;
        }

        private class DigitData
        {
            public IEnumerable<HashSet<char>> InputDigits = new List<HashSet<char>>();
            public IEnumerable<HashSet<char>> OutputDigits = new List<HashSet<char>>();
        }
    }
}
