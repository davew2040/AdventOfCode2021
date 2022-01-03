using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AdventOfCode.Shared;

namespace AdventOfCode.Day22
{
    internal class Day22 : DailyChallenge
    {
        private static int SingleSplits = 0;
        private static int SplitAttempts = 0;

        private enum Include
        {
            Positive,
            Negative
        }

        private const string Filename = "Day22/Data/problem_input.txt";
        private const int QuadTreeDepth = 4;

        private readonly RangeCube TargetCube = new RangeCube()
        {
            X = new InclusiveRange(-50, 50),
            Y = new InclusiveRange(-50, 50),
            Z = new InclusiveRange(-50, 50),
        };

        public async Task Process()
        {
            var input = await ReadAndParse(Filename);

            var tree = new QuadTree(QuadTreeDepth, input.Bounds);

            for (int i = 0; i < input.Flips.Count(); i++)
            {
                var flip = input.Flips.ElementAt(i);

                var leafNodes = tree.FindIntersectingLeafNodes(flip.Cube);
                var allTouching = leafNodes.SelectMany(n => n.IntersectingCubes).ToHashSet();
                
                foreach (var touching in allTouching)
                {
                    int touchCount = allTouching.Count();
                    tree.Remove(touching);
                    var splits = SplitTarget(flip.Cube, touching);
                    foreach (var split in splits)
                    {
                        tree.Add(split);
                    }
                }

                if (flip.On)
                {
                    tree.Add(flip.Cube);
                }
            }

            var totalVolume = tree.Root.IntersectingCubes.Sum(v => v.Volume);

            Console.WriteLine($"Total Volume = {totalVolume}");
        }
        
        // Priority is given to edges in "one"
        private IEnumerable<RangeCube> SplitObjects(RangeCube one, RangeCube two)
        {
            //var xSplits = PlanarSplitX(two, one.X.Min - 1)
            //    .Union(PlanarSplitX(two, one.X.Max))
            //    .ToList();

            return new List<RangeCube>();
        }

        private void Test()
        {
            var target = new RangeCube()
            {
                X = new InclusiveRange(-2, 2),
                Y = new InclusiveRange(-2, 2),
                Z = new InclusiveRange(-2, 2)
            };

            var cutter = new RangeCube()
            {
                X = new InclusiveRange(0, 1),
                Y = new InclusiveRange(-4, 4),
                Z = new InclusiveRange(-4, 4)
            };

            var splitX = SplitTarget(
                cutter, target
            );
        }


        private IEnumerable<RangeCube> SplitTarget(RangeCube cutter, RangeCube target)
        {
            IEnumerable<RangeCube> resultingCubes = PlanarSplitX(target, cutter.X.Min);
            resultingCubes = resultingCubes.SelectMany(c => PlanarSplitX(c, cutter.X.Max + 1)).ToList();

            resultingCubes = resultingCubes.SelectMany(c => PlanarSplitY(c, cutter.Y.Min));
            resultingCubes = resultingCubes.SelectMany(c => PlanarSplitY(c, cutter.Y.Max + 1));

            resultingCubes = resultingCubes.SelectMany(c => PlanarSplitZ(c, cutter.Z.Min));
            resultingCubes = resultingCubes.SelectMany(c => PlanarSplitZ(c, cutter.Z.Max + 1));

            return resultingCubes.Where(c => !cutter.Contains(c.GetContainedPoint()));
        }

        // Includes values up to but not including "x"
        private IEnumerable<RangeCube> PlanarSplitX(RangeCube cube, int x)
        {
            var resultingCubes = new List<RangeCube>();

            var minRange = new InclusiveRange(cube.X.Min, Math.Min(x-1, cube.X.Max));
            var maxRange = new InclusiveRange(Math.Max(x, cube.X.Min), cube.X.Max);

            if (minRange.IsValid)
            {
                resultingCubes.Add(
                    new RangeCube()
                    {
                        X = minRange,
                        Y = cube.Y,
                        Z = cube.Z
                    });
            }

            if (maxRange.IsValid)
            {
                resultingCubes.Add(
                    new RangeCube()
                    {
                        X = maxRange,
                        Y = cube.Y,
                        Z = cube.Z
                    });
            }

            return resultingCubes;
        }

        private IEnumerable<RangeCube> PlanarSplitY(RangeCube cube, int y)
        {
            var resultingCubes = new List<RangeCube>();

            var minRange = new InclusiveRange(cube.Y.Min, Math.Min(y - 1, cube.Y.Max));
            var maxRange = new InclusiveRange(Math.Max(y, cube.Y.Min), cube.Y.Max);

            if (minRange.IsValid)
            {
                resultingCubes.Add(
                    new RangeCube()
                    {
                        X = cube.X,
                        Y = minRange,
                        Z = cube.Z
                    });
            }

            if (maxRange.IsValid)
            {
                resultingCubes.Add(
                    new RangeCube()
                    {
                        X = cube.X,
                        Y = maxRange,
                        Z = cube.Z
                    });
            }

            return resultingCubes;
        }

        private IEnumerable<RangeCube> PlanarSplitZ(RangeCube cube, int z)
        {
            var resultingCubes = new List<RangeCube>();

            var minRange = new InclusiveRange(cube.Z.Min, Math.Min(z - 1, cube.Z.Max));
            var maxRange = new InclusiveRange(Math.Max(z, cube.Z.Min), cube.Z.Max);

            if (minRange.IsValid)
            {
                resultingCubes.Add(
                    new RangeCube()
                    {
                        X = cube.X,
                        Y = cube.Y,
                        Z = minRange
                    });
            }

            if (maxRange.IsValid)
            {
                resultingCubes.Add(
                    new RangeCube()
                    {
                        X = cube.X,
                        Y = cube.Y,
                        Z = maxRange
                    });
            }

            return resultingCubes;
        }

        private void AddIfMissing<T>(T item, HashSet<T> set)
        {
            if (!set.Contains(item))
            {
                set.Add(item);
            }
        }

        private void RemoveIfPresent<T>(T item, HashSet<T> set)
        {
            if (set.Contains(item))
            {
                set.Remove(item);
            }
        }

        private IEnumerable<Point3> GetCubePoints(RangeCube cube)
        {
            for (int x = cube.X.Min; x <= cube.X.Max; x++)
            {
                for (int y = cube.Y.Min; y <= cube.Y.Max; y++)
                {
                    for (int z = cube.Z.Min; z <= cube.Z.Max; z++)
                    {
                        yield return new Point3(x, y, z);
                    }
                }
            }
        }

        private async Task<Input> ReadAndParse(string filename)
        {
            var lines = await File.ReadAllLinesAsync(filename);
            var flips = new List<CubeFlip>();
            var input = new Input();

            foreach (var line in lines)
            {
                var newFlip = new CubeFlip();

                newFlip.On = line.Contains("on");
                newFlip.Cube = new RangeCube();

                var ranges = line.Split(',').Select(c => c.Split('=')[1]);

                int count = 0;
                foreach (var range in ranges)
                {
                    var numbers = range.Split("..");

                    var newRange = new InclusiveRange(int.Parse(numbers[0]), int.Parse(numbers[1]));

                    if (count == 0)
                    {
                        newFlip.Cube.X = newRange;
                    }
                    else if (count == 1)
                    {
                        newFlip.Cube.Y = newRange;
                    }
                    else
                    {
                        newFlip.Cube.Z = newRange;
                    }

                    input.Bounds = ExpandRange(input.Bounds, newFlip.Cube);

                    count++;
                }

                flips.Add(newFlip);
            }

            input.Flips = flips;

            return input;
        }

        private InclusiveRange ExpandRange(InclusiveRange lastRange, InclusiveRange newRange)
        {
            return new InclusiveRange(
                Math.Min(lastRange.Min, newRange.Min),
                Math.Max(lastRange.Max, newRange.Max)
                );
        }


        private RangeCube ExpandRange(RangeCube lastRange, RangeCube newRange)
        {
            return new RangeCube()
            {
                X = ExpandRange(lastRange.X, newRange.X),
                Y = ExpandRange(lastRange.Y, newRange.Y),
                Z = ExpandRange(lastRange.Z, newRange.Z)
            };
        }

        private class Input
        {
            public IEnumerable<CubeFlip> Flips;
            public RangeCube Bounds = new RangeCube()
            {
                X = new InclusiveRange(int.MaxValue, int.MinValue),
                Y = new InclusiveRange(int.MaxValue, int.MinValue),
                Z = new InclusiveRange(int.MaxValue, int.MinValue),
            };
        }

        private class CubeFlip
        {
            public bool On;
            public RangeCube Cube;
        }

        private class BitCube
        {
            public HashSet<Point3> OnPoints = new HashSet<Point3>();
        }

        private struct RangeCube
        {
            public InclusiveRange X { get; set; }
            public InclusiveRange Y { get; set; }
            public InclusiveRange Z { get; set; }

            public Point3 GetContainedPoint()
                => new Point3(X.Min, Y.Min, Z.Min);

            public long Volume
            {
                get => X.Size * Y.Size * Z.Size;
            }

            public bool IsValid
            {
                get => X.IsValid && Y.IsValid && Z.IsValid;
            }

            public bool Contains(Point3 point)
                => X.Contains(point.X) && Y.Contains(point.Y) && Z.Contains(point.Z);

            public RangeCube Intersect(RangeCube other)
                => new RangeCube()
                {
                    X = X.Intersect(other.X),
                    Y = Y.Intersect(other.Y),
                    Z = Z.Intersect(other.Z)
                };

            public override int GetHashCode()
            {
                return (X, Y, Z).GetHashCode();
            }
        }

        private struct InclusiveRange
        {
            public int Min { get; set; }
            public int Max { get; set; }
            public long Size { get => (Max - Min) + 1; }

            public bool IsValid => Min <= Max;

            public bool Contains(int value) => value >= Min && value <= Max;

            public InclusiveRange(int min, int max)
            {
                Min = min;
                Max = max;
            }

            public InclusiveRange Intersect(InclusiveRange other)
            {
                int largerMin = Math.Max(Min, other.Min);
                int smallerMax = Math.Min(Max, other.Max);

                return new InclusiveRange(largerMin, smallerMax);
            }

            public override int GetHashCode()
            {
                return (Min, Max).GetHashCode();
            }
        }

        private class QuadTree
        {
            public QuadTreeNode Root { get; set; }

            public QuadTree(int maxDepth, RangeCube bounds)
            {
                if (maxDepth <= 0)
                {
                    throw new ArgumentException($"{nameof(maxDepth)} cannot be null");
                }

                Root = CreateSubtree("A1", bounds, 1, maxDepth);
            }

            private QuadTreeNode CreateSubtree(string label, RangeCube bounds, int currentDepth, int maxDepth)
            {
                char tag = (char)(currentDepth + (int)'A');

                var newNode = new QuadTreeNode()
                {
                    Label = label,
                    Bounds = bounds
                };

                if (currentDepth < maxDepth)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        for (int y = 0; y < 2; y++)
                        {
                            for (int z = 0; z < 2; z++)
                            {
                                var nextLabel = $"{label}:{tag}{1 + (x*4) + (y*2) + z}";
                                var nextCube = new RangeCube()
                                {
                                    X = (x == 0) ? GetFirstHalf(bounds.X) : GetSecondHalf(bounds.X),
                                    Y = (y == 0) ? GetFirstHalf(bounds.Y) : GetSecondHalf(bounds.Y),
                                    Z = (z == 0) ? GetFirstHalf(bounds.Z) : GetSecondHalf(bounds.Z)
                                };

                                newNode.Children.Add(CreateSubtree(nextLabel, nextCube, currentDepth + 1, maxDepth));
                            }
                        }
                    }
                }

                return newNode;
            }

            public InclusiveRange GetFirstHalf(InclusiveRange range)
            {
                return new InclusiveRange(range.Min, (range.Max-range.Min) / 2 + range.Min);
            }

            public InclusiveRange GetSecondHalf(InclusiveRange range)
            {
                return new InclusiveRange((range.Max - range.Min) / 2 + 1 + range.Min, range.Max);
            }

            public void Add(RangeCube toAdd)
            {
                AddRecursive(toAdd, Root);
            }

            private void AddRecursive(RangeCube toAdd, QuadTreeNode node)
            {
                if (node.Bounds.Intersect(toAdd).IsValid)
                {
                    node.IntersectingCubes.Add(toAdd);
                }
                else
                {
                    int x = 42;
                }

                foreach (var child in node.Children)
                {
                    AddRecursive(toAdd, child);
                }
            }

            public void Remove(RangeCube toRemove)
            {
                RemoveRecursive(toRemove, Root);
            }

            private void RemoveRecursive(RangeCube toRemove, QuadTreeNode node)
            {
                if (node.IntersectingCubes.Contains(toRemove))
                {
                    node.IntersectingCubes.Remove(toRemove);

                    foreach (var child in node.Children)
                    {
                        RemoveRecursive(toRemove, child);
                    }
                }
            }

            public IEnumerable<QuadTreeNode> FindIntersectingLeafNodes(RangeCube target)
            {
                var containing = new List<QuadTreeNode>();

                FindIntersectingLeafNodesRecursive(target, Root, containing);

                return containing;
            }

            private void FindIntersectingLeafNodesRecursive(RangeCube target, QuadTreeNode node, List<QuadTreeNode> containing)
            {
                if (node.Bounds.Intersect(target).IsValid)
                {
                    if (node.IsLeaf)
                    {
                        containing.Add(node);
                    }
                    else
                    {
                        foreach (var child in node.Children)
                        {
                            FindIntersectingLeafNodesRecursive(target, child, containing);
                        }
                    }
                }
            }

            public class QuadTreeNode
            {
                public RangeCube Bounds { get; set; }
                public string Label { get; set; }
                public bool IsLeaf { get => !Children.Any(); }

                public List<QuadTreeNode> Children = new List<QuadTreeNode>();
                public HashSet<RangeCube> IntersectingCubes = new HashSet<RangeCube>();
            }
        }
    }
}
