// See https://aka.ms/new-console-template for more information
using AdventOfCode.Day10;
using AdventOfCode.Day11;
using AdventOfCode.Day12;
using AdventOfCode.Day13;
using AdventOfCode.Day14;
using AdventOfCode.Day15;
using AdventOfCode.Day16;
using AdventOfCode.Day17;
using AdventOfCode.Day7;
using AdventOfCode.Day8;
using AdventOfCode.Day9;
using AdventOfCode.DayFive;
using AdventOfCode.DayFour;
using AdventOfCode.DayOne;
using AdventOfCode.DaySix;
using AdventOfCode.DayThree;
using AdventOfCode.DayTwo;

try
{
    var day = new Day17();

    await day.Process();

    Console.WriteLine("Done!");
}
catch (Exception ex)
{
    Console.WriteLine("An error has occurred!");
    Console.WriteLine(ex);
}