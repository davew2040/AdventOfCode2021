using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode.Day10
{
    internal class Day10 : DailyChallenge
    {
        private readonly Dictionary<char, char> _openingClosingTokens = new Dictionary<char, char>
        {
            ['('] = ')',
            ['['] = ']',
            ['{'] = '}',
            ['<'] = '>',
        };

        private readonly Dictionary<char, int> _corruptionPointValues = new Dictionary<char, int>()
        {
            { ')', 3 },
            { ']', 57 },
            { '}', 1197 },
            { '>', 25137 }
        };

        private readonly Dictionary<char, int> _unclosedPointValues = new Dictionary<char, int>()
        {
            { ')', 1 },
            { ']', 2 },
            { '}', 3 },
            { '>', 4 }
        };

        public async Task Process()
        {
            var input = await ReadAndParse("Day10/Data/day_10_input.txt");
            var corrupted = input.Select(FindCorruption).Where(c => c.MismatchedCloser.HasValue);
            var uncorrupted = input.Select(FindCorruption).Where(c => !c.MismatchedCloser.HasValue);
            var uncorruptedScore = uncorrupted.Select(u => ScoreUnclosedValues(u.MissingClosers));

            var sorted = uncorruptedScore.OrderBy(x => x);
            var middle = sorted.ElementAt(sorted.Count() / 2);

            Console.WriteLine(middle);
        }

        private int ScoreCorruption(IEnumerable<char> corruptedChars)
        {
            return corruptedChars.Aggregate(0, (sum, value) => sum + _corruptionPointValues[value]);
        }

        private long ScoreUnclosedValues(IEnumerable<char> unclosedValues)
        {
            var shifter = 5;
            var sum = 0L;

            foreach (var c in unclosedValues)
            {
                sum *= shifter;
                sum += _unclosedPointValues[c];
            }

            return sum;
        }

        private async Task<IEnumerable<char[]>> ReadAndParse(string filename)
        {
            var lines = await File.ReadAllLinesAsync(filename);

            return lines.Select(l => l.ToCharArray());
        }

        private TokenParsingResult FindCorruption(char[] chars)
        {
            Stack<char> stack = new Stack<char>();

            foreach (char c in chars)
            {
                if (_openingClosingTokens.ContainsKey(c))
                {
                    stack.Push(c);
                }
                else
                {
                    if (!_openingClosingTokens.Values.Contains(c))
                    {
                        throw new ArgumentException($"Encountered invalid token [{c}]");
                    }

                    if (stack.Count == 0)
                    {
                        return new TokenParsingResult(c);
                    }

                    var opener = FindOpenerForCloser(c);

                    if (stack.Peek() != opener)
                    {
                        Console.WriteLine($"Expected opener [{opener}] but found [{stack.Peek()}]");
                        return new TokenParsingResult(c);
                    }

                    stack.Pop();
                }
            }

            List<char> requiredClosers = new List<char>();

            while (stack.Any())
            {
                var popped = stack.Pop();

                requiredClosers.Add(_openingClosingTokens[popped]);
            }

            return new TokenParsingResult(requiredClosers);
        }


        private char FindOpenerForCloser(char c)
            => _openingClosingTokens.FirstOrDefault(pair => pair.Value == c).Key;
        
        private class TokenParsingResult
        {
            public char? MismatchedCloser { get; }
            public IEnumerable<char> MissingClosers { get; }

            public TokenParsingResult(char? mismatchedCloser)
            {
                MismatchedCloser = mismatchedCloser;
                MissingClosers = new List<char>();
            }

            public TokenParsingResult(IEnumerable<char> missingClosers)
            {
                MismatchedCloser = null;
                MissingClosers = missingClosers;
            }
        }
    }
}
