using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdventOfCode.Day19
{
    internal class Day19 : DailyChallenge
    {
        private const string Filename = "Day19/Data/problem_input.txt";

        private enum Axis
        {
            X, Y, Z
        }

        public async Task Process()
        {
            var input = await ReadAndParse(Filename);

            var deltas = BuildDeltas(input);

            CompareDeltas(deltas);
        }

        private void CompareDeltas(Dictionary<int, Dictionary<Axis, IEnumerable<int>>> deltas)
        {
            List<ScannerComparison> comparisons = new List<ScannerComparison>();

            foreach (var kvp in deltas)
            {
                foreach (var kvp2 in deltas)
                {
                    if (kvp.Key == kvp2.Key)
                    {
                        continue;
                    }
                    var comparison = new ScannerComparison()
                    {
                        FirstScanner = kvp.Key,
                        SecondScanner = kvp2.Key,
                        X_X = FindOverlappingSpans(kvp.Value[Axis.X], kvp2.Value[Axis.X]),
                        X_Y = FindOverlappingSpans(kvp.Value[Axis.X], kvp2.Value[Axis.Y]),
                        X_Z = FindOverlappingSpans(kvp.Value[Axis.X], kvp2.Value[Axis.Z]),
                    };

                    comparisons.Add(comparison);
                }
            }

            var xXcomparisons = comparisons.SelectMany((x, i) => x.X_X.Select(xx => new {first = x.FirstScanner, second = x.SecondScanner, xx})).OrderByDescending(x => x.xx.Count());
            var xYcomparisons = comparisons.SelectMany((x, i) => x.X_Y).OrderByDescending(x => x.Count());
            var xZcomparisons = comparisons.SelectMany(x => x.X_Z).OrderByDescending(x => x.Count());
        }
            
        private IEnumerable<IEnumerable<int>> FindOverlappingSpans(IEnumerable<int> first, IEnumerable<int> second)
        {
            List<List<int>> spans = new List<List<int>>();

            Dictionary<int, int> valuePositionMap = new Dictionary<int, int>();

            for (int i = 0; i < second.Count(); i++)
            {
                valuePositionMap.Add(second.ElementAt(i), i);
            }

            for (int i=0; i<first.Count(); i++)
            {
                if (valuePositionMap.ContainsKey(first.ElementAt(i)))
                {
                    var a2position = valuePositionMap[first.ElementAt(i)];
                    List<int> newSpan = new List<int>();

                    while (a2position < second.Count() 
                        && i < first.Count() 
                        && second.ElementAt(a2position) == first.ElementAt(i))
                    {
                        newSpan.Add(first.ElementAt(i));
                        i++;
                        a2position++;
                    }

                    spans.Add(newSpan);
                }
            }

            return spans.Where(s => s.Count() > 1);
        }

        private Dictionary<int, Dictionary<Axis, IEnumerable<int>>> BuildDeltas(IEnumerable<ScannerData> scannerData)
        {
            var result = new Dictionary<int, Dictionary<Axis, IEnumerable<int>>>();

            foreach (var scanner in scannerData)
            {
                result.Add(scanner.ScannerIndicator, BuildSingleScannerDeltas(scanner));
            }

            return result;
        }

        private Dictionary<Axis, IEnumerable<int>> BuildSingleScannerDeltas(ScannerData scanner)
        {
            var result = new Dictionary<Axis, IEnumerable<int>>();

            var x = scanner.ScannerPositions.OrderBy(p => p.X).Select(p => p.X);
            result.Add(Axis.X, GetDeltas(x).ToHashSet());

            var y = scanner.ScannerPositions.OrderBy(p => p.Y).Select(p => p.Y);
            result.Add(Axis.Y, GetDeltas(y).ToHashSet());

            var z = scanner.ScannerPositions.OrderBy(p => p.Z).Select(p => p.Z);
            result.Add(Axis.Z, GetDeltas(z).ToHashSet());

            return result;
        }

        private IEnumerable<int> GetDeltas(IEnumerable<int> values)
        {
            var zipped = values.Zip(values.Skip(1));

            return zipped.Select(x => x.Second - x.First);
        }

        private async Task<IEnumerable<ScannerData>> ReadAndParse(string filename)
        {
            var lines = await File.ReadAllLinesAsync(filename);

            var datas = new List<ScannerData>();
            ScannerData currentScannerData = null;

            foreach (var line in lines)
            {
                if (line.Contains("---"))
                {
                    var scannerNumber = int.Parse(Regex.Match(line, @"\d+").Value);
                    currentScannerData = new ScannerData()
                    {
                        ScannerIndicator = scannerNumber
                    };
                    datas.Add(currentScannerData);
                }
                else if (line.Contains(","))
                {
                    var split = line.Split(",");
                    currentScannerData.ScannerPositions.Add(
                        new Position()
                        {
                            X = int.Parse(split[0]),
                            Y = int.Parse(split[1]),
                            Z = int.Parse(split[2])
                        }
                    );
                }
            }

            return datas;
        }

        private class ScannerComparison
        {
            public int FirstScanner { get; set; }
            public int SecondScanner { get; set; }
            public IEnumerable<IEnumerable<int>> X_X { get; set; }
            public IEnumerable<IEnumerable<int>> X_Y { get; set; }
            public IEnumerable<IEnumerable<int>> X_Z { get; set; }
        }

        private class ScannerData
        {
            public int ScannerIndicator { get; set; }
            public List<Position> ScannerPositions { get; set; } = new List<Position>();
        }

        private class Position
        {
            public int X;
            public int Y;
            public int Z;
        }
    }
}
