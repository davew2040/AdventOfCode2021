using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode
{
    public class Shared
    {
        public struct Point
        {
            public int X { get; }
            public int Y { get; }

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }

            public override int GetHashCode() => (X, Y).GetHashCode();
        }

        public struct Point3
        {
            public int X { get; }
            public int Y { get; }
            public int Z { get; }

            public Point3(int x, int y, int z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public override int GetHashCode() => (X, Y, Z).GetHashCode();
        }
    }
}
