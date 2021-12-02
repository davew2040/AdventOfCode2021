using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode.DayTwo
{
    public enum Direction
    {
        Forward,
        Up,
        Down
    }

    public record MoveCommand(Direction Direction, int Distance);

    public record Position(int HorizontalPosition, int Depth, int Aim)
    {
        public Position ApplyCommand(MoveCommand command)
            =>
            command.Direction switch
            {
                Direction.Forward => this with { HorizontalPosition = HorizontalPosition + command.Distance },
                Direction.Up => this with { Depth = Depth - command.Distance },
                Direction.Down => this with { Depth = Depth + command.Distance },
                _ => throw new ArgumentException($"Encountered unrecognized Direction [{command.Direction}]")
            };

        public Position ApplyCommandWithAim(MoveCommand command)
            =>
            command.Direction switch
            {
                Direction.Forward => ForwardWithAim(command),
                Direction.Up => this with { Aim = Aim - command.Distance },
                Direction.Down => this with { Aim = Aim + command.Distance },
                _ => throw new ArgumentException($"Encountered unrecognized Direction [{command.Direction}]")
            };

        private Position ForwardWithAim(MoveCommand command)
            => new Position
            (
                HorizontalPosition + command.Distance,
                Depth + command.Distance * Aim,
                Aim
            );
    }

    internal class DayTwo
    {
        public async Task Process()
        {
            IEnumerable<MoveCommand> commands = null;

            try
            {
                commands = await ReadAndParse("DayTwo/Data/problem_3_input.txt");
                var position = new Position(0, 0, 0);

                foreach (var command in commands)
                {
                    position = position.ApplyCommandWithAim(command);
                }

                Console.WriteLine($"Final position: {position}");

                Console.WriteLine($"Multiplication = {position.HorizontalPosition * position.Depth}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while reading input!");
                Console.WriteLine(ex.Message);
            }
        }

        private async Task<IEnumerable<MoveCommand>> ReadAndParse(string filename)
        {
            var lines = await File.ReadAllLinesAsync(filename);

            var commands = lines.Select(ParseCommand);

            return commands;
        }

        private MoveCommand ParseCommand(string line)
        {
            var split = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if (split.Length != 2)
            {
                throw new ArgumentException($"Encountered line without two tokens: [{line}]");
            }

            var directionString = split[0];
            var distance = split[1];

            var direction = directionString.ToLower() switch
            {
                "forward" => Direction.Forward,
                "up" => Direction.Up,
                "down" => Direction.Down,
                _ => throw new ArgumentException($"Unrecognized direction {directionString}")
            };

            return new MoveCommand(direction, int.Parse(distance));
        }

        private Position ApplyCommand(Position position, MoveCommand command)
            => 
            command.Direction switch
            {
                Direction.Forward => position with { HorizontalPosition = position.HorizontalPosition + command.Distance },
                Direction.Up => position with { Depth = position.Depth - command.Distance },
                Direction.Down => position with { Depth = position.Depth + command.Distance },
                _ => throw new ArgumentException($"Encountered unrecognized Direction [{command.Direction}]")
            };
    }
}
