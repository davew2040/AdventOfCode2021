// See https://aka.ms/new-console-template for more information
using AdventOfCode.Day7;
using AdventOfCode.DayFive;
using AdventOfCode.DayFour;
using AdventOfCode.DayOne;
using AdventOfCode.DaySix;
using AdventOfCode.DayThree;
using AdventOfCode.DayTwo;

try
{
    var day = new Day7();

    await day.Process();

    Console.WriteLine("Done!");
}
catch (Exception ex)
{
    Console.WriteLine("An error has occurred!");
    Console.WriteLine(ex);
}