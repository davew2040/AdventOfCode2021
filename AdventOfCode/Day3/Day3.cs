using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode.DayThree
{
    internal class Day3 : DailyChallenge
    { 
        public async Task Process()
        {
            var lines = await ReadAndParseAsync("DayThree/Data/problem_5_input.txt");

            var columnTally = TallyDiagnosticLines(lines);

            var gammaDigits = GetGammaRateDigits(columnTally);
            var epsilonDigits = GetEpsilonRateDigits(columnTally);

            var gammaInt = BinaryDigitsToInt(gammaDigits);
            var epsInt = BinaryDigitsToInt(epsilonDigits);

            var oxygenGeneratorRating = GetOxygenGeneratorRating(lines);
            var co2ScrubberRating = GetCo2ScrubberRating(lines);

            var oxygenGeneratorInt = BinaryDigitsToInt(oxygenGeneratorRating);
            var co2ScrubberInt = BinaryDigitsToInt(co2ScrubberRating);

            Console.WriteLine($"Multiplication = {oxygenGeneratorInt * co2ScrubberInt}");
        }

        private int BinaryDigitsToInt(char[] digits)
        {
            var sum = 0;
            var digitShifter = 1;

            foreach (var digit in digits.Reverse())
            {
                if (digit == '1')
                {
                    sum += digitShifter;
                }
                digitShifter <<= 1;
            }

            return sum;
        }

        private char[] GetOxygenGeneratorRating(IEnumerable<DiagnosticLine> lines)
        {
            return IterativePrune(lines, GetGammaRateDigits);
        }

        private char[] GetCo2ScrubberRating(IEnumerable<DiagnosticLine> lines)
        {
            return IterativePrune(lines, GetEpsilonRateDigits);
        }

        private char[] IterativePrune(IEnumerable<DiagnosticLine> lines, Func<AllColumnsTally, char[]> ratingGetter)
        {
            var digitCount = lines.First().Digits.Length;

            for (int i=0; i<digitCount; i++)
            {
                var tally = TallyDiagnosticLines(lines);
                var rating = ratingGetter(tally);

                lines = lines.Where(line => line.Digits[i] == rating[i]).ToList();

                if (lines.Count() == 1)
                {
                    return lines.First().Digits;
                }
            }

            throw new Exception("Ended process with more than one matching value.");
        }

        private char[] AssessDigits(AllColumnsTally allColumns, Func<int, int, char> digitAssessor)
        {
            var builder = new StringBuilder();

            foreach (var key in allColumns.AllColumns.Keys.OrderBy(x => x))
            {
                var digitCounts = allColumns.AllColumns[key];

                var zeroes = digitCounts.GetCount('0');
                var ones = digitCounts.GetCount('1');

                var digit = digitAssessor(zeroes, ones);

                builder.Append(digit);
            }

            return builder.ToString().ToCharArray();
        }

        private char[] GetGammaRateDigits(AllColumnsTally allColumns)
        {
            return AssessDigits(allColumns, (zeroes, ones) => ones >= zeroes ? '1' : '0');
        }

        private char[] GetEpsilonRateDigits(AllColumnsTally allColumns)
        {
            return AssessDigits(allColumns, (zeroes, ones) => zeroes <= ones ? '0' : '1');
        }

        private async Task<IEnumerable<DiagnosticLine>> ReadAndParseAsync(string filename)
        {
            var lines = await File.ReadAllLinesAsync(filename);

            var commands = lines.Select(l => new DiagnosticLine(l.ToCharArray()));

            return commands;
        }

        private AllColumnsTally TallyDiagnosticLines(IEnumerable<DiagnosticLine> lines)
        {
            var columnsTally = new AllColumnsTally();

            foreach (var line in lines)
            {
                var digits = line.Digits;

                for (int i = 0; i < digits.Length; i++)
                {
                    var digit = digits[i];

                    var column = columnsTally.GetColumn(i);

                    column.CountDigit(digit);
                }
            }

            return columnsTally;
        }

        private class DiagnosticLine
        {
            public char[] Digits { get; }

            public DiagnosticLine(char[] digits)
            {
                Digits = digits;
            }
        }

        // Tallies up all digits encountered in a single column
        private class ColumnDigitTally
        {
            public Dictionary<char, int> Tally { get; } = new Dictionary<char, int>();

            public int GetCount(char digit)
            {
                if (!Tally.ContainsKey(digit))
                {
                    Tally.Add(digit, 0);
                }

                return Tally[digit];
            }

            public void CountDigit(char digit)
            {
                if (!Tally.ContainsKey(digit))
                {
                    Tally.Add(digit, 0);
                }

                Tally[digit] = Tally[digit] + 1;
            }
        }

        // Collects per-column information about digit values
        private class AllColumnsTally
        {
            public Dictionary<int, ColumnDigitTally> AllColumns = new Dictionary<int, ColumnDigitTally>();

            public ColumnDigitTally GetColumn(int index)
            {
                if (!AllColumns.ContainsKey(index))
                {
                    AllColumns.Add(index, new ColumnDigitTally());
                }

                return AllColumns[index];
            }
        }
    }


}
