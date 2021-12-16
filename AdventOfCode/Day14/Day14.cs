using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode.Day14
{
    internal class Day14 : DailyChallenge
    {
        private const string Filename = "Day14/Data/problem_input.txt";
        private const int Steps = 40;

        public async Task Process()
        {
            var input = await ReadAndParse(Filename);
            var pairMapResult = input.PairMap;
            var elementMapResult = input.ElementCount;

            for (int i=0; i<Steps; i++)
            {
                (pairMapResult, elementMapResult) = GetNextStep(pairMapResult, elementMapResult, input.InsertionRules);
            }

            var leastCommon = elementMapResult.Min(m => m.Value);
            var mostCommon = elementMapResult.Max(m => m.Value);

            Console.WriteLine($"least = {leastCommon}");
            Console.WriteLine($"most = {mostCommon}");

            Console.WriteLine($"most - least = {mostCommon - leastCommon}");
        }

        private (Dictionary<(char first, char second), long>, Dictionary<char, long>) GetNextStep(
            Dictionary<(char first, char second), long> pairMap,
            Dictionary<char, long> elementMap,
            IEnumerable<InsertionRule> rules
            )
        {
            var nextPairMap = new Dictionary<(char first, char second), long>();
            var nextElementMap = pairMap.Select(kvp => kvp).ToDictionary(k => k.Key, k => k.Value);

            foreach (var rule in rules)
            {
                if (pairMap.ContainsKey(rule.From))
                {
                    MapAdd(pairMap[rule.From], (rule.From.first, rule.To), nextPairMap);
                    MapAdd(pairMap[rule.From], (rule.To, rule.From.second), nextPairMap);
                    MapAdd(pairMap[rule.From], rule.To, elementMap);
                }
            }

            return (nextPairMap, elementMap);
        }

        private async Task<Input> ReadAndParse(string filename)
        {
            var lines = await File.ReadAllLinesAsync(filename);
            var input = new Input();
            var rules = new List<InsertionRule>();

            foreach (var line in lines)
            {
                if (line.Contains("-"))
                {
                    var newRule = new InsertionRule(
                        ( line[0], line[1] ),
                        line[line.Length - 1]
                    );

                    rules.Add(newRule);
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    input.PairMap = GetPairMapFromString(line);
                    input.ElementCount = GetElementMapFromString(line);
                }
            }

            input.InsertionRules = rules;

            return input;
        }

        private Dictionary<(char first, char second), long> GetPairMapFromString(string s)
        {
            var pairMap = new Dictionary<(char first, char second), long>();

            for (int i = 0; i < s.Length - 1; i++)
            {
                MapAdd(1, (s[i], s[i + 1]), pairMap);
            }

            return pairMap;
        }

        private Dictionary<char, long> GetElementMapFromString(string s)
        {
            var map = new Dictionary<char, long>();

            for (int i = 0; i < s.Length; i++)
            {
                MapAdd(1, s[i], map);
            }

            return map;
        }


        private void MapAdd<T>(long count, T value, Dictionary<T, long> map)
        {
            if (!map.ContainsKey(value))
            {
                map[value] = 0;
            }

            map[value] = map[value] + count;
        }

        private class InsertionRule
        {
            public (char first, char second) From { get; set; }
            public char To { get; set; }

            public InsertionRule((char first, char second) from, char to)
            {
                From = from;
                To = to;
            }
        }

        private class Input
        {
            public Dictionary<(char first, char second), long> PairMap { get; set; }
            public Dictionary<char, long> ElementCount { get; set; }
            public IEnumerable<InsertionRule> InsertionRules { get; set; }
        }
    }
}
