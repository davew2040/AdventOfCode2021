// See https://aka.ms/new-console-template for more information
using AdventOfCode.DayOne;
using AdventOfCode.DayThree;
using AdventOfCode.DayTwo;

try
{
    var day = new DayThree();

    await day.Process();

    Console.WriteLine("Done!");
}
catch (Exception ex)
{
    Console.WriteLine("An error has occurred!");
    Console.WriteLine(ex);
}