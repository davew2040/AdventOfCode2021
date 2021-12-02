// See https://aka.ms/new-console-template for more information
using AdventOfCode.DayOne;
using AdventOfCode.DayTwo;

try
{
    var day = new DayTwo();

    await day.Process();

    Console.WriteLine("Done!");
}
catch (Exception ex)
{
    Console.WriteLine("An error has occurred!");
    Console.WriteLine(ex);
}