// See https://aka.ms/new-console-template for more information
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
    var day = new Day9();

    await day.Process();

    Console.WriteLine("Done!");
}
catch (Exception ex)
{
    Console.WriteLine("An error has occurred!");
    Console.WriteLine(ex);
}