using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
            X, Y, Z, XNeg, YNeg, ZNeg
        }

        public async Task Process()
        {
            var input = await ReadAndParse(Filename);

            FindBeacons(input);
        }

        private void FindBeacons(Input input)
        {
            var finishedScanners = new Dictionary<int, ScannerData>();
            finishedScanners.Add(0, input.Scanners[0]);
            var remainingScanners = input.Scanners.ToDictionary(s => s.Key, s => s.Value);
            remainingScanners.Remove(0);

            while (remainingScanners.Any())
            {
                var hasOverlaps = GetFirstOverlapByThreshold(finishedScanners.Values, remainingScanners.Values, 12);

                var lastScanner = input.Scanners[hasOverlaps.ScannerOne];
                var nextScanner = input.Scanners[hasOverlaps.ScannerTwo];
                nextScanner.CorrectRotation = hasOverlaps.Rotation;
                nextScanner.Position = hasOverlaps.CommonOffset;
                nextScanner.AdjustedPositions = nextScanner.Rotations[nextScanner.CorrectRotation]
                    .Select(p => (p.original, p.rotated, Add(p.rotated, nextScanner.Position.Value)));

                var intersect = lastScanner.AdjustedPositions.Select(p => p.adjusted)
                    .Intersect(nextScanner.AdjustedPositions.Select(p => p.adjusted));

                var original = nextScanner.AdjustedPositions.Where(p => intersect.Contains(p.adjusted)).Select(p => p.original);
                var adjusted = nextScanner.AdjustedPositions.Where(p => intersect.Contains(p.adjusted)).Select(p => p.adjusted);

                remainingScanners.Remove(nextScanner.ScannerNumber);
                finishedScanners.Add(nextScanner.ScannerNumber, nextScanner);
            }

            var totalBeacons = input.Scanners
                .SelectMany(p => p.Value.AdjustedPositions.Select(a => a.adjusted))
                .OrderBy(b => b.X)
                .Distinct()
                .ToList();

            Console.WriteLine($"Count = {totalBeacons.Count()}");

            int largestManhattan = int.MinValue;

            foreach (var scanner1 in input.Scanners.Select(s => s.Value.Position.Value))
            {
                foreach (var scanner2 in input.Scanners.Select(s => s.Value.Position.Value))
                {
                    int manhattanDistance = 
                        Math.Abs(scanner2.X - scanner1.X) 
                        + Math.Abs(scanner2.Y - scanner1.Y) 
                        + Math.Abs(scanner2.Z - scanner1.Z);

                    if (manhattanDistance > largestManhattan)
                    {
                        largestManhattan = manhattanDistance;
                    }
                }
            }

            Console.WriteLine($"Manhattan = {largestManhattan}");
        }

        private OverlapResult GetOverlapResult(ScannerData scannerOne, ScannerData scannerTwo)
        {
            var pair = new UnorderedPair(scannerOne.ScannerNumber, scannerTwo.ScannerNumber);
            if (cachedOverlapResults.ContainsKey(pair))
            {
                return cachedOverlapResults[pair];
            }

            var result = GetOverlaps(scannerOne, scannerTwo);

            cachedOverlapResults[pair] = new OverlapResult()
            {
                CommonOffset = result.commonOffset,
                Rotation = result.rotation,
                ScannerOne = scannerOne.ScannerNumber,
                ScannerTwo = scannerTwo.ScannerNumber,
                Overlaps = result.overlaps
            };

            return cachedOverlapResults[pair];
        }

        private OverlapResult GetFirstOverlapByThreshold(
            IEnumerable<ScannerData> completedSet, IEnumerable<ScannerData> remainingSet, int threshold)
        {
            foreach (var completed in completedSet)
            {
                foreach (var remaining in remainingSet)
                {
                    var overlaps = GetOverlapResult(completed, remaining);
                    if (overlaps.Overlaps >= threshold)
                    {
                        return overlaps;
                    }
                }
            }

            throw new Exception("No overlaps found!");
        }

        private (int overlaps, Rotation rotation, Position3 commonOffset) GetOverlaps(ScannerData one, ScannerData two)
        {
            var rotationOverlaps = two.Rotations.ToDictionary(
                kvp => kvp.Key, 
                kvp => GetOverlaps(one.AdjustedPositions.Select(p => p.adjusted), kvp.Value.Select(p => p.rotated)));

            var maxOverlaps = rotationOverlaps.Max(kvp2 => kvp2.Value.overlaps);
            var maxKvp = rotationOverlaps.First(kvp => kvp.Value.overlaps == maxOverlaps);
                
            return (maxKvp.Value.overlaps, maxKvp.Key, maxKvp.Value.commonOffset);
        }

        private (int overlaps, Position3 commonOffset) GetOverlaps(IEnumerable<Position3> one, IEnumerable<Position3> two)
        {
            var diffTrack = new Dictionary<Position3, int>();

            foreach (var onePoint in one)
            {
                foreach (var twoPoint in two)
                {
                    var diff = Subtract(twoPoint, onePoint);
                    if (!diffTrack.ContainsKey(diff))
                    {
                        diffTrack[diff] = 0;
                    }
                    diffTrack[diff] = diffTrack[diff] + 1;
                }
            }

            var maxValue = diffTrack.Max(kvp => kvp.Value);
            var maxKvp = diffTrack.First(kvp => kvp.Value == maxValue);

            return (maxKvp.Value, maxKvp.Key);
        }

        //private static IEnumerable<Rotation> BuildRotations()
        //{
        //    var rotations = new List<Rotation>();

        //    var stack = new Stack<Axis>();

        //    MakeRotations(stack, rotations);

        //    return rotations;
        //}

        //private static void MakeRotations(Stack<Axis> values, List<Rotation> rotations)
        //{
        //    IEnumerable<Axis> all = new List<Axis>()
        //    {
        //        Axis.X, Axis.Y, Axis.Z
        //    };

        //    if (values.Count() == all.Count())
        //    {
        //        foreach (var value in values)
        //        {
        //            Console.Write(value + " ");
        //        }
        //        Console.WriteLine();

        //        rotations.Add(
        //            new Rotation(
        //                values.ElementAt(0),
        //                values.ElementAt(1),
        //                values.ElementAt(2)
        //            ));
        //    }
        //    else
        //    {
        //        foreach (var available in all.Where(a => !values.Contains(a)).ToList())
        //        {
        //            values.Push(available);
        //            MakeRotations(values, rotations);
        //            values.Pop();
        //            values.Push(Neg(available, true));
        //            MakeRotations(values, rotations);
        //            values.Pop();
        //        }
        //    }
        //}

        private static Axis Neg(Axis axis)
        {
            return axis switch
            {
                Axis.X => Axis.XNeg,
                Axis.Y => Axis.YNeg,
                Axis.Z => Axis.ZNeg,
                Axis.XNeg => Axis.X,
                Axis.YNeg => Axis.Y,
                Axis.ZNeg => Axis.Z,
            };
        }

        //private Dictionary<AbsoluteDistance, List<ByDistanceData>> BuildDistanceMap(Input input)
        //{
        //    var distanceAggregator = new Dictionary<AbsoluteDistance, List<ByDistanceData>>();

        //    foreach (var scanner in input.Scanners)
        //    {
        //        for (int i=0; i<scanner.Value.BeaconPositions.Count(); i++)
        //        {
        //            var fromPosition = scanner.Value.BeaconPositions[i];
        //            var from = new BeaconAddress(scanner.Value.ScannerIndicator, i);

        //            for (int j=0; j<scanner.Value.BeaconPositions.Count(); j++)
        //            {
        //                var toPosition = scanner.Value.BeaconPositions[j];
        //                var to = new BeaconAddress(scanner.Value.ScannerIndicator, j);

        //                if (i == j)
        //                {
        //                    continue;
        //                }

        //                var offset = Subtract(fromPosition, toPosition);
        //                var absolute = new AbsoluteDistance(offset.X, offset.Y, offset.Z);

        //                AddToDictList(distanceAggregator, absolute, new ByDistanceData()
        //                {
        //                    From = from,
        //                    To = to,
        //                    FromPosition = fromPosition,
        //                    ToPosition = toPosition,
        //                    Offset = offset
        //                });
        //            }
        //        }
        //    }

        //    return distanceAggregator;
        //}

        private Position3 Subtract(Position3 one, Position3 two)
        {
            return new Position3(two.X - one.X, two.Y - one.Y, two.Z - one.Z);
        }

        private Position3 Add(Position3 one, Position3 two)
        {
            return new Position3(two.X + one.X, two.Y + one.Y, two.Z + one.Z);
        }

        private void AddToDictList<T, R>(Dictionary<T, List<R>> dictionary, T key, R value)
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary[key] = new List<R>();
            }

            dictionary[key].Add(value);
        }


        private class ByDistanceData
        {
            public BeaconAddress From { get; set; }
            public BeaconAddress To { get; set; }
            public Position3 FromPosition { get; set; }
            public Position3 ToPosition { get; set; }
            public Position3 Offset { get; set; }
        }

        //private void Test(Input input)
        //{
        //    var context = BuildContext(input);

        //    var sharedSpans = new List<(int Scanner, Axis Axis, List<IndexPointer> Indices)>();
        //    for (int i=0; i<1; i++)
        //    {
        //        var sourceDeltas = BuildDeltaList(Axis.Z, context.ScannerPointsByScanner[i].SortedByAxis[Axis.Z], input.Scanners);

        //        for (int j=0; j<context.ScannerPointsByScanner.Count(); j++)
        //        {
        //            if (i == j)
        //            {
        //                continue;
        //            }

        //            var target = context.ScannerPointsByScanner[j];

        //            foreach (var axis in AllAxes)
        //            {
        //                var newAxisDeltas = BuildDeltaList(axis, target.SortedByAxis[axis], input.Scanners);
        //                var largestSpan = FindLargestSharedSpan(sourceDeltas, newAxisDeltas);
        //                sharedSpans.Add((i, axis, largestSpan));
        //            }
        //        }
        //    }

        //    var sorted = sharedSpans.OrderByDescending(s => s.Indices.Count());
        //}

        //private List<IndexPointer> FindLargestSharedSpan(List<DeltaListEntry> major, List<DeltaListEntry> minor)
        //{
        //    List<IndexPointer> largestSpan = new List<IndexPointer>();

        //    for (int i=0; i<minor.Count(); i++)
        //    {
        //        for (int j = 0; j < major.Count(); j++)
        //        {
        //            var testSpan = new List<IndexPointer>();

        //            int k = 0;
        //            while (i+k < minor.Count() && j+k < major.Count() 
        //                && minor[i+k].Distance == major[j+k].Distance)
        //            {
        //                testSpan.Add(new IndexPointer() { Minor = i+k, Major = j+k });
        //                k++;
        //            }

        //            if (testSpan.Count() > largestSpan.Count())
        //            {
        //                if (testSpan.Count() >= 3)
        //                {
        //                    var x = 42;
        //                }
        //                largestSpan = testSpan;
        //            }
        //        }
        //    }

        //    return largestSpan;
        //}



        private static Dictionary<UnorderedPair, OverlapResult> cachedOverlapResults = new Dictionary<UnorderedPair, OverlapResult>();


        private class AbsoluteDistance
        {
            public int One;
            public int Two;
            public int Three;

            public AbsoluteDistance(int x, int y, int z)
            {
                var ordered = new int[] { Math.Abs(x), Math.Abs(y), Math.Abs(z) }.OrderBy(x => x);

                One = ordered.ElementAt(0);
                Two = ordered.ElementAt(1);
                Three = ordered.ElementAt(2);
            }

            public override int GetHashCode() => (One, Two, Three).GetHashCode();


            public override bool Equals([NotNullWhen(true)] object? other)
            {
                if (!(other is AbsoluteDistance))
                {
                    return false;
                }

                var otherPosition = (AbsoluteDistance)other;

                return otherPosition.One == One && otherPosition.Two == Two && otherPosition.Three == Three;
            }
        }

        private ProblemContext BuildContext(Input input)
        {
            var context = new ProblemContext();

            foreach (var scanner in input.Scanners)
            {
                context.ScannerPointsByScanner[scanner.Key] = GetPointsByAxis(scanner.Value);
            }

            return context;
        }

        private PointsByAllAxes GetPointsByAxis(ScannerData scanner)
        {
            var points = new PointsByAllAxes();

            points.SortedByAxis[Axis.X] = scanner.BeaconPositions
                .Select((p, i) => new BeaconAddress(scanner.ScannerNumber, i))
                .OrderBy(x => scanner.BeaconPositions.ElementAt(x.PointAddress).X);
            points.SortedByAxis[Axis.Y] = scanner.BeaconPositions
                .Select((p, i) => new BeaconAddress(scanner.ScannerNumber, i))
                .OrderBy(x => scanner.BeaconPositions.ElementAt(x.PointAddress).Y);
            points.SortedByAxis[Axis.Z] = scanner.BeaconPositions
                .Select((p, i) => new BeaconAddress(scanner.ScannerNumber, i))
                .OrderBy(x => scanner.BeaconPositions.ElementAt(x.PointAddress).Z);
            points.SortedByAxis[Axis.XNeg] = scanner.BeaconPositions
                .Select((p, i) => new BeaconAddress(scanner.ScannerNumber, i))
                .OrderByDescending(x => scanner.BeaconPositions.ElementAt(x.PointAddress).X);
            points.SortedByAxis[Axis.YNeg] = scanner.BeaconPositions
                .Select((p, i) => new BeaconAddress(scanner.ScannerNumber, i))
                .OrderByDescending(x => scanner.BeaconPositions.ElementAt(x.PointAddress).Y);
            points.SortedByAxis[Axis.ZNeg] = scanner.BeaconPositions
                .Select((p, i) => new BeaconAddress(scanner.ScannerNumber, i))
                .OrderByDescending(x => scanner.BeaconPositions.ElementAt(x.PointAddress).Z);

            return points;
        }

        private IEnumerable<int> GetDeltas(IEnumerable<int> values)
        {
            var zipped = values.Zip(values.Skip(1));

            return zipped.Select(x => x.Second - x.First);
        }

        private List<DeltaListEntry> BuildDeltaList(
            Axis axis, 
            IEnumerable<BeaconAddress> points,
            Dictionary<int, ScannerData> scannerData)
        {
            var deltas = new List<DeltaListEntry>();

            for (int i = 0; i < points.Count() - 2; i++)
            {
                var p1 = points.ElementAt(i);
                var p2 = points.ElementAt(i+1);
                var p1Value = PointAtAddress(p1, scannerData);
                var p2Value = PointAtAddress(p2, scannerData);

                var distance = ValueByAxis(axis, p2Value) - ValueByAxis(axis, p1Value);

                deltas.Add(new DeltaListEntry()
                {
                    Distance = distance,
                    PointPairs = new List<PointPair>() { new PointPair(p1, p2, p1Value, p2Value) { AddressFrom = p1, AddressTo = p2 } }
                });
            }

            return deltas;
        }

        private Position3 PointAtAddress(BeaconAddress address, Dictionary<int, ScannerData> scanners)
        {
            return scanners[address.Scanner].BeaconPositions.ElementAt(address.PointAddress);
        }

        private int ValueByAxis(Axis axis, Position3 point)
            => axis switch
            {
                Axis.X => point.X,
                Axis.XNeg => point.X,
                Axis.Y => point.Y,
                Axis.YNeg => point.Y,
                Axis.Z => point.Z,
                Axis.ZNeg => point.Z,
                _ => throw new ArgumentException($"Unrecognized axis value {axis}")
            };

        private async Task<Input> ReadAndParse(string filename)
        {
            var lines = await File.ReadAllLinesAsync(filename);

            var datas = new List<ScannerData>();
            ScannerData currentScannerData = null;
            int lastScannerNumber = -1;
            var beaconPositions = new List<Position3>();

            foreach (var line in lines)
            {
                if (line.Contains("---"))
                {
                    if (beaconPositions.Any())
                    {
                        datas.Add(new ScannerData(lastScannerNumber, beaconPositions));
                    }
                    beaconPositions = new List<Position3>();
                    lastScannerNumber = int.Parse(Regex.Match(line, @"\d+").Value);
                }
                else if (line.Contains(","))
                {
                    var split = line.Split(",");
                    beaconPositions.Add(
                        new Position3()
                        {
                            X = int.Parse(split[0]),
                            Y = int.Parse(split[1]),
                            Z = int.Parse(split[2])
                        }
                    );
                }
            }

            datas.Add(new ScannerData(lastScannerNumber, beaconPositions));

            var input = new Input()
            {
                Scanners = datas.ToDictionary(d => d.ScannerNumber, d => d)
            };

            input.Scanners[0].Position = new Position3(0, 0, 0);
            input.Scanners[0].CorrectRotation = new Rotation(0, 0, 0);
            input.Scanners[0].AdjustedPositions = input.Scanners[0].BeaconPositions
                .Select(p => (p, p, p));

            return input;
        }

        private class Input
        {
            public Dictionary<int, ScannerData> Scanners { get; set; }
                = new Dictionary<int, ScannerData>();
        }

        private class PointPair
        {
            public BeaconAddress AddressFrom { get; set; }
            public BeaconAddress AddressTo { get; set; }
            public Position3 PositionFrom { get; set; }
            public Position3 PositionTo { get; set; }

            public PointPair(BeaconAddress addressFrom, BeaconAddress addressTo, Position3 positionFrom, Position3 positionTo)
            {
                AddressFrom = addressFrom;
                AddressTo = addressFrom;
                PositionFrom = positionFrom;
                PositionTo = positionTo;
            }
        }

        private class ProblemContext
        {
            public Dictionary<int, PointsByAllAxes> ScannerPointsByScanner { get; set; } 
                = new Dictionary<int, PointsByAllAxes>();

            public List<DeltaListEntry> Deltas { get; set; } = new List<DeltaListEntry>();
        }

        private class DeltaListEntry
        {
            public int Distance { get; set; }
            public List<PointPair> PointPairs { get; set; } = new List<PointPair>();
        }

        private class PointsByAllAxes
        {
            public Dictionary<Axis, IEnumerable<BeaconAddress>> SortedByAxis { get; set; }
                = new Dictionary<Axis, IEnumerable<BeaconAddress>>();
        }

        private class BeaconAddress
        {
            public int Scanner { get; set; }
            public int PointAddress { get; set; }

            public BeaconAddress(int scanner, int point)
            {
                Scanner = scanner;
                PointAddress = point;
            }
        }

        private class ScannerData
        {
            public int ScannerNumber { get; set; }
            public IEnumerable<Position3> BeaconPositions { get; set; }
            public IEnumerable<(Position3 original, Position3 rotated, Position3 adjusted)> AdjustedPositions { get; set; }
            public Position3? Position { get; set; }
            public Rotation? CorrectRotation { get; set; }

            public Dictionary<Rotation, IEnumerable<(Position3 rotated, Position3 original)>> Rotations { get; set; } 
                = new Dictionary<Rotation, IEnumerable<(Position3 rotated, Position3 original)>>();

            public ScannerData(int scannerNumber, IEnumerable<Position3> beaconPositions)
            {
                ScannerNumber = scannerNumber;
                BeaconPositions = beaconPositions;

                for (int x = 0; x <= 270; x += 90)
                {
                    for (int y = 0; y <= 270; y += 90)
                    {
                        for (int z = 0; z <= 270; z += 90)
                        {
                            var rotation = new Rotation(x, y, z);
                            Rotations.Add(rotation, beaconPositions.Select(p => (rotation.Rotate(p), p)));
                        }
                    }
                }
            } 
        }

        private class Rotation
        {
            public int XDeg;
            public int YDeg;
            public int ZDeg;

            public Rotation(int x, int y, int z)
            {
                XDeg = x;
                YDeg = y;
                ZDeg = z;
            }

            public Position3 Rotate(Position3 input)
            {
                var newPos = new Position3(input.X, input.Y, input.Z);

                newPos = RotateX(newPos, XDeg);
                newPos = RotateY(newPos, YDeg);
                newPos = RotateZ(newPos, ZDeg);

                return newPos;
            }
            
            private Position3 RotateX(Position3 pos, int deg)
            {
                var rad = DegToRad(deg);
                return new Position3(
                    pos.X, 
                    (int)Math.Round((double)pos.Y * Math.Cos(rad) - pos.Z * Math.Sin(rad)),
                    (int)Math.Round((double)pos.Y * Math.Sin(rad) + pos.Z * Math.Cos(rad))
                );
            }

            private Position3 RotateY(Position3 pos, int deg)
            {
                var rad = DegToRad(deg);
                return new Position3(
                    (int)Math.Round((double)pos.X * Math.Cos(rad) + pos.Z * Math.Sin(rad)),
                    pos.Y,
                    (int)Math.Round((double)(-pos.X) * Math.Sin(rad) + pos.Z * Math.Cos(rad))
                );
            }

            private Position3 RotateZ(Position3 pos, int deg)
            {
                var rad = DegToRad(deg);
                return new Position3(
                    (int)Math.Round((double)pos.X * Math.Cos(rad) - pos.Y * Math.Sin(rad)),
                    (int)Math.Round((double)pos.X * Math.Sin(rad) + pos.Y * Math.Cos(rad)),
                    pos.Z
                );
            }

            private double DegToRad(double degrees)
                => (degrees / 180.0) * Math.PI;

            public override int GetHashCode()
            {
                return (XDeg, YDeg, ZDeg).GetHashCode();
            }

            public override bool Equals(object? obj)
            {
                if (!(obj is Rotation))
                {
                    return false;
                }
                var rot = obj as Rotation;
                return rot.XDeg == XDeg && rot.YDeg == YDeg && rot.ZDeg == ZDeg;
            }
        }

        private struct Position3
        {
            public int X;
            public int Y;
            public int Z;

            public Position3(int x, int y, int z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public override int GetHashCode() => (X, Y, Z).GetHashCode();

            public override bool Equals([NotNullWhen(true)] object? other)
            {
                if (!(other is Position3))
                {
                    return false;
                }

                var otherPosition = (Position3)other;

                return otherPosition.X == X && otherPosition.Y == Y && otherPosition.Z == Z;
            }
        }

        private struct UnorderedPair
        {
            public int First { get; }
            public int Second { get; }

            public UnorderedPair(int first, int second)
            {
                First = Math.Min(first, second);
                Second = Math.Max(first, second);
            }

            public override int GetHashCode()
            {
                return (First, Second).GetHashCode();
            }
        }

        private class OverlapResult
        {
            public int ScannerOne;
            public int ScannerTwo;
            public Rotation Rotation;
            public Position3 CommonOffset;
            public int Overlaps;
        }

    }
}
