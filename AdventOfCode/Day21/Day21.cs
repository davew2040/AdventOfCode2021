using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode.Day21
{
    internal class Day21 : DailyChallenge
    {
        private const string Filename = "Day21/Data/puzzle_input.txt";
        private const int BoardSpots = 10;
        private const int DiceSides = 100;
        private const int PlayerCount = 2;
        private const int DiceRolls = 3;
        private const int WinningPoints = 21;

        public async Task Process()
        {
            var input = await ReadAndParse(Filename);

            var gameState = new GameState()
            {
                Player1Position = input.Player1Position-1,
                Player2Position = input.Player2Position-1
            };

            var knownOutcomes = new Dictionary<string, NonDeterministicGameOutcomes>();

            var endResult = TakeNdTurn(gameState, knownOutcomes);
        }

        private NonDeterministicGameOutcomes TakeNdTurn(GameState state, Dictionary<string, NonDeterministicGameOutcomes> knownOutcomes)
        {
            var stateKey = Keyify(state);

            if (knownOutcomes.TryGetValue(stateKey, out NonDeterministicGameOutcomes knownOutcome))
            {
                return knownOutcome;
            }

            var outcome = DoNdRolls(new Stack<int>(), state, knownOutcomes);

            if (!knownOutcomes.ContainsKey(stateKey))
            {
                knownOutcomes.Add(stateKey, outcome);
            }

            return outcome;
        }

        private string Keyify(GameState state)
            => $"{state.CurrentTurn}:{state.Player1Score}:{state.Player2Score}:{state.Player1Position}:{state.Player2Position}";

        private NonDeterministicGameOutcomes DoNdRollsComplete(IEnumerable<int> rolls, GameState state, Dictionary<string, NonDeterministicGameOutcomes> knownOutcomes)
        {
            var rollsSum = rolls.Sum();

            if (rollsSum == 9)
            {
                int x = 42;
            }

            int player1Score = state.Player1Score;
            int player2Score = state.Player2Score;
            int player1Position = state.Player1Position;
            int player2Position = state.Player2Position;

            if (state.CurrentTurn == 0)
            {
                player1Position = (player1Position + rollsSum) % BoardSpots;
                player1Score += (player1Position + 1);
            }
            else
            {
                player2Position = (player2Position + rollsSum) % BoardSpots;
                player2Score += (player2Position + 1);
            }

            var nextTurn = (state.CurrentTurn + 1) % PlayerCount;

            var nextState = new GameState()
            {
                CurrentTurn = nextTurn,
                Player1Score = player1Score,
                Player2Score = player2Score,
                Player1Position = player1Position,
                Player2Position = player2Position
            };

            var stateKey = Keyify(nextState);

            if (knownOutcomes.ContainsKey(stateKey))
            {
                return knownOutcomes[stateKey];
            }

            if (IsGameOver(nextState))
            {
                var winner = GetWinner(nextState);

                NonDeterministicGameOutcomes? outcome = null;

                if (winner.Player == 0)
                {
                    outcome = new NonDeterministicGameOutcomes()
                    {
                        Player1Wins = 1,
                        Player2Wins = 0
                    };
                }
                else
                {
                    outcome = new NonDeterministicGameOutcomes()
                    {
                        Player1Wins = 0,
                        Player2Wins = 1
                    };
                }

                knownOutcomes.Add(stateKey, outcome);

                return outcome;
            }
            else
            {
                return TakeNdTurn(nextState, knownOutcomes);
            }
        }

        private NonDeterministicGameOutcomes DoNdRolls(Stack<int> rolls, GameState state, 
            Dictionary<string, NonDeterministicGameOutcomes> knownOutcomes)
        {
            if (rolls.Count() == DiceRolls)
            {
                return DoNdRollsComplete(rolls, state, knownOutcomes);
            }
            else
            {
                var outcomes = new NonDeterministicGameOutcomes();

                rolls.Push(1);
                outcomes.Add(DoNdRolls(rolls, state, knownOutcomes));
                rolls.Pop();
                rolls.Push(2);
                outcomes.Add(DoNdRolls(rolls, state, knownOutcomes));
                rolls.Pop();
                rolls.Push(3);
                outcomes.Add(DoNdRolls(rolls, state, knownOutcomes));
                rolls.Pop();

                return outcomes;
            }
        }

        private GameState TakeTurn(GameState state, DeterministicDice dice)
        {
            var nextTurn = (state.CurrentTurn + 1) % PlayerCount;
            var rollsSum = GameRoll(dice).Sum();
            int player1Score = state.Player1Score;
            int player2Score = state.Player2Score;
            int player1Position = state.Player1Position;
            int player2Position = state.Player2Position;
            int rollCount = state.RollCount + DiceRolls;

            if (state.CurrentTurn == 0)
            {
                player1Position = (player1Position + rollsSum) % BoardSpots;
                player1Score += (player1Position + 1);
            }
            else
            {
                player2Position = (player2Position + rollsSum) % BoardSpots;
                player2Score += (player2Position + 1);
            }

            return new GameState()
            {
                CurrentTurn = nextTurn,
                Player1Score = player1Score,
                Player2Score = player2Score,
                Player1Position = player1Position,
                Player2Position = player2Position,
                RollCount = rollCount
            };
        }

        private bool IsGameOver(GameState state)
            => state.Player1Score >= WinningPoints || state.Player2Score >= WinningPoints;

        private (int Score, int Player) GetWinner(GameState state)
        {
            if (state.Player1Score > state.Player2Score)
            {
                return (state.Player1Score, 0);
            }
            else
            {
                return (state.Player2Score, 1);
            }
        }

        private (int Score, int Player) GetLoser(GameState state)
        {
            if (state.Player1Score < state.Player2Score)
            {
                return (state.Player1Score, 1);
            }
            else
            {
                return (state.Player2Score, 0);
            }
        }

        private List<int> GameRoll(DeterministicDice dice)
        {
            var rolls = new List<int>();
            for (int i=0; i<DiceRolls; i++)
            {
                rolls.Add(dice.Roll());
            }
            return rolls;
        }

        private async Task<Input> ReadAndParse(string filename)
        {
            var lines = await File.ReadAllLinesAsync(filename);
            var input = new Input();

            foreach (var line in lines)
            {
                var split = line.Split(":");
                int value = int.Parse(split[1]);

                if (split[0].Contains("1"))
                {
                    input.Player1Position = value;
                }
                else
                {
                    input.Player2Position = value;
                }
            }

            return input;
        }

        private class NonDeterministicGameOutcomes
        {
            public long Player1Wins;
            public long Player2Wins;

            public void Add(NonDeterministicGameOutcomes other)
            {
                Player1Wins += other.Player1Wins;
                Player2Wins += other.Player2Wins;
            }

            public static NonDeterministicGameOutcomes Add(
                NonDeterministicGameOutcomes first,
                NonDeterministicGameOutcomes second)
            {
                return new NonDeterministicGameOutcomes()
                {
                    Player1Wins = first.Player1Wins + second.Player1Wins,
                    Player2Wins = first.Player2Wins + second.Player2Wins
                };
            }
        }

        private class DeterministicDice
        {
            private int _nextRoll = 1;
            private readonly int _sides;

            public DeterministicDice(int sides)
            {
                _sides = sides;
            }

            public int Roll()
            {
                var returnValue = _nextRoll;

                _nextRoll += 1;

                if (_nextRoll > _sides)
                {
                    _nextRoll = 1;
                }

                return returnValue;
            }
        }

        private class Input
        {
            public int Player1Position { get; set; }
            public int Player2Position { get; set; }
        }

        private class GameState
        {
            public int Player1Position;
            public int Player2Position;
            public int Player1Score;
            public int Player2Score;
            public int CurrentTurn;
            public int RollCount;

            public GameState()
            {
            }

            public GameState(GameState state)
            {
                Player1Position = state.Player1Position;
                Player2Position = state.Player2Position;
                Player1Score = state.Player1Score;
                Player2Score = state.Player2Score;
                CurrentTurn = state.CurrentTurn;
                RollCount = state.RollCount;
            }
        }
    }
}
