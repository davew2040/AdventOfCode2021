using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode.Day20
{
    internal class Day20 : DailyChallenge
    {
        private const string Filename = "Day20/Data/problem_input.txt";
        private const int Steps = 50;

        public async Task Process()
        {
            var input = await ReadAndParse(Filename);

            //Print(input.BaseImage);

            var image = ProcessSteps(input, Steps);

            // Print(image);

            Console.WriteLine($"Final lit pixels = {image.LitPoints.Count()}");
        }

        private Image ProcessSteps(Input input, int steps)
        {
            var image = input.BaseImage;

            for (int i=0; i<steps; i++)
            {
                image = GetNextImage(image, input.ImageEnhancementAlgorithm, i % 2 == 1);
                //Print(image);
            }

            return image;
        }

        private void Print(Image image)
        {
            for (int y = image.TopLeft.Y; y < image.BottomRight.Y; y++)
            {
                for (int x = image.TopLeft.X; x < image.BottomRight.X; x++)
                {
                    Console.Write(image.LitPoints.Contains(new Point(x, y)) ? "#" : ".");
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        private Image GetNextImage(Image image, string algorithm, bool isOdd)
        {
            var newImage = new Image();
            newImage.TopLeft = new Point(image.TopLeft.X - 1, image.TopLeft.Y - 1);
            newImage.BottomRight = new Point(image.BottomRight.X + 1, image.BottomRight.Y + 1);

            for (int x = newImage.TopLeft.X; x < newImage.BottomRight.X; x++)
            {
                for (int y = newImage.TopLeft.Y;  y < newImage.BottomRight.Y; y++)
                {
                    var pixel = new Point(x, y);

                    var block = image.GetPixelBlock(pixel, algorithm, isOdd);
                    var binary = GetBinaryFromBlock(block);
                    var intValue = GetIntFromBinary(binary);
                    var lookup = algorithm.ElementAt(intValue);

                    if (lookup == '#')
                    {
                        newImage.LitPoints.Add(pixel);
                    }
                }
            }

            return newImage;
        }

        private string GetBinaryFromBlock(char[,] block)
        {
            var builder = new StringBuilder();

            for (int i = 0; i < block.GetLength(0); i++)
            {
                for (int j = 0; j < block.GetLength(1); j++)
                {
                    builder.Append(block[i,j]);
                }
            }

            return builder.ToString();
        }

        private int GetIntFromBinary(string binary)
        {
            int sum = 0;
            int shifter = 1;
            
            for (int b = binary.Length-1; b >= 0; b--)
            {
                if (binary[b] == '#')
                {
                    sum += shifter;
                }

                shifter <<= 1;
            }

            return sum;
        }

        private async Task<Input> ReadAndParse(string filename)
        {
            var lines = await File.ReadAllLinesAsync(filename);

            var input = new Input();

            var builder = new StringBuilder();

            var breakLine = lines.Select((x, i) => new { line = x, index = i }).First(l => string.IsNullOrWhiteSpace(l.line));

            for (int i=0; i<breakLine.index; i++)
            {
                builder.Append(lines[i]);
            }

            input.ImageEnhancementAlgorithm = builder.ToString();
            input.BaseImage.TopLeft = new Point(0, 0);
            input.BaseImage.BottomRight = new Point(
                lines.ElementAt(breakLine.index+1).Length, 
                lines.Length-(breakLine.index+1));

            for (int i=breakLine.index+1; i<lines.Length; i++)
            {
                int lineIndex = i - (breakLine.index + 1);

                for (int j=0; j<lines[i].Length; j++)
                {
                    var c = lines[i][j];

                    if (c == '#')
                    {
                        input.BaseImage.LitPoints.Add(new Point(j, lineIndex));
                    }
                }
            }

            return input;
        }

        private class Image
        {
            public HashSet<Point> LitPoints { get; set; } = new HashSet<Point>();
            public Point TopLeft { get; set; }
            public Point BottomRight { get; set; }

            public char[,] GetPixelBlock(Point p, string algorithm, bool isOdd)
            {
                char[,] pixelBlock = new char[3, 3];

                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        var testPoint = new Point(p.X + x, p.Y + y);
                        pixelBlock[y + 1, x + 1] = GetPixelValue(testPoint, algorithm, isOdd);
                    }
                }

                return pixelBlock;
            }

            private char GetPixelValue(Point p, string algorithm, bool isOdd)
            {
                // border - determined by whether the step count is odd/even, and the empty block fill
                //    value
                if (p.X < TopLeft.X || p.X >= BottomRight.X || p.Y < TopLeft.Y || p.Y >= BottomRight.Y)
                {
                    if (isOdd)
                    {
                        return algorithm[0];
                    }

                    return '.';
                }

                return LitPoints.Contains(p) ? '#' : '.';
            }

            public HashSet<Point> GetRelevantPixels()
            {
                var points = new HashSet<Point>();

                var xMin = TopLeft.X - 1;
                var xMax = BottomRight.X + 1;
                var yMin = TopLeft.Y - 1;
                var yMax = BottomRight.Y + 1;

                for (int x = xMin; x <= xMax; x++)
                {
                    for (int y = yMin; y <= yMax; y++)
                    {
                        if (x == xMin && y == yMin || x == xMin && y == yMax
                            || x == xMax && y == yMin || x == xMax && y == yMax)
                        {
                            continue;
                        }
                        points.Add(new Point(x, y));
                    }
                }

                return points;
            }
        }

        private class Point
        {
            public int X { get; set; }
            public int Y { get; set; }

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }

            public override int GetHashCode()
            {
                return X ^ Y;
            }

            public override bool Equals(Object obj)
            {
                if (!(obj is Point)) return false;

                Point p = (Point)obj;
                return X == p.X & Y == p.Y;
            }
        }

        private class Input
        {
            public string ImageEnhancementAlgorithm { get; set; }
            public Image BaseImage { get; set; } = new Image();
        }
    }
}
