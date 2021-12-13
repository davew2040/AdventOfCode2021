using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AdventOfCode.Day13
{
    internal class Day13 : DailyChallenge
    {
        private const string Filename = "Day13/Data/problem_input.txt";

        public async Task Process()
        {
            var input = await ReadAndParse(Filename);

            var finalMarkers = DoAllFolds(input.Markers, input.Folds);

            PrintMarkers(finalMarkers);

            Console.WriteLine($"Final count = {finalMarkers.MarkedPoints.Count()}");
        }
        
        private GridMarkers DoAllFolds(GridMarkers sourceMarkers, IEnumerable<Fold> folds)
        {
            var markers = sourceMarkers;

            foreach (var fold in folds)
            {
                markers = DoFold(markers, fold);
                //PrintMarkers(markers);
                Console.WriteLine();
            }

            return markers;
        }

        private GridMarkers DoFold(GridMarkers sourceMarkers, Fold fold)
        {
            if (fold.Type == FoldType.X)
            {
                return FoldX(fold.Axis, sourceMarkers);
            }
            else if (fold.Type == FoldType.Y)
            {
                return FoldY(fold.Axis, sourceMarkers);
            }

            throw new ArgumentException($"Unrecognized type {fold.Type}");
        }

        private GridMarkers FoldX(int xLine, GridMarkers sourceMarkers)
        {
            var newPoints = new HashSet<Point>();

            foreach (var point in sourceMarkers.MarkedPoints)
            {
                if (point.X < xLine)
                {
                    newPoints.Add(point);
                }
                else
                {
                    var xDist = point.X - xLine;
                    var newX = xLine - xDist;
                    newPoints.Add(new Point(newX, point.Y));
                }
            }

            return new GridMarkers()
            {
                MarkedPoints = newPoints
            };
        }

        private GridMarkers FoldY(int yLine, GridMarkers sourceMarkers)
        {
            var newPoints = new HashSet<Point>();

            foreach (var point in sourceMarkers.MarkedPoints)
            {
                if (point.Y < yLine)
                {
                    newPoints.Add(point);
                }
                else
                {
                    var yDist = point.Y - yLine;
                    var newY = yLine - yDist;
                    newPoints.Add(new Point(point.X, newY));
                }
            }

            return new GridMarkers()
            {
                MarkedPoints = newPoints
            };
        }

        private async Task<Input> ReadAndParse(string filename)
        {
            var points = new List<Point>();
            var folds = new List<Fold>();

            var lines = await File.ReadAllLinesAsync(filename);

            foreach (var line in lines)
            {
                if (line.Contains(','))
                {
                    var split = line.Split(",");

                    var newPoint = new Point(int.Parse(split[0]), int.Parse(split[1]));

                    points.Add(newPoint);
                }
                else if (line.Contains("fold"))
                {
                    if (Regex.IsMatch(line, @"x=\d"))
                    {
                        int value = int.Parse(line.Split("=")[1]);
                        folds.Add(new Fold()
                        {
                            Type = FoldType.X,
                            Axis = value
                        });
                    }
                    else if (Regex.IsMatch(line, @"y=\d"))
                    {
                        int value = int.Parse(line.Split("=")[1]);
                        folds.Add(new Fold()
                        {
                            Type = FoldType.Y,
                            Axis = value
                        });
                    }
                }
            }

            return new Input()
            {
                Markers = new GridMarkers()
                {
                    MarkedPoints = points.ToHashSet()
                },
                Folds = folds
            };
        }

        private void PrintMarkers(GridMarkers markers)
        {
            var xWidth = markers.MarkedPoints.Max(p => p.X);
            var yHeight = markers.MarkedPoints.Max(p => p.Y);

            for (int y=0; y<yHeight+1; y++)
            {
                for (int x=0; x<xWidth+1; x++)
                {
                    if (markers.MarkedPoints.Contains(new Point(x, y)))
                    {
                        Console.Write('#');
                    }
                    else
                    {
                        Console.Write('.');
                    }
                }

                Console.WriteLine();
            }
        }

        private class GridMarkers
        {
            public HashSet<Point> MarkedPoints { get; set; }
        }

        private class Input
        {
            public GridMarkers Markers { get; set; }
            public IEnumerable<Fold> Folds { get; set; }
        }

        private class Fold
        {
            public FoldType Type { get; set; }
            public int Axis { get; set; }
        }

        private enum FoldType
        {
            X, Y
        }

        private struct Point
        {
            public int X { get; }
            public int Y { get; }

            public Point(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public override bool Equals(Object obj)
            {
                if (!(obj is Point)) return false;

                Point p = (Point)obj;
                return X == p.X & Y == p.Y;
            }

            public override int GetHashCode()
            {
                return X ^ Y;
            }
        }
    }
}
