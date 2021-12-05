using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode.DayFour
{
    internal class DayFour : DailyChallenge
    {
        public record WinningBoard(BingoBoard Board, int WinningNumber);

        public class BingoEntry
        {
            public int Number { get; set;}
            public bool Marked { get; private set; } = false;

            public void Mark()
            {
                Marked = true;
            }
        }

        public class BingoData
        {
            public IEnumerable<int> GameNumbers { get; }
            public IEnumerable<BingoBoard> Boards { get; }

            public BingoData(IEnumerable<int> gameNumbers, IEnumerable<BingoBoard> boards)
            {
                GameNumbers = gameNumbers;
                Boards = boards;
            }
        }

        public class BingoBoard
        {
            public int Width { get; }
            public int Height { get; }

            // Access by Numbers[row][column]
            public BingoEntry[,] Numbers { get; }

            public BingoBoard(int width, int height)
            {
                Width = width;
                Height = height;

                Numbers = new BingoEntry[Height, Width];

                for (int h = 0; h < Height; h++)
                {
                    for (int w = 0; w < Width; w++)
                    {
                        Numbers[h, w] = new BingoEntry();
                    }
                }
            }

            public void MarkNumber(int number)
            {
                for (int h = 0; h < Height; h++)
                {
                    for (int w = 0; w < Width; w++)
                    {
                        if (Numbers[h, w].Number == number)
                        {
                            Numbers[h, w].Mark();
                        }
                    }
                }
            }

            public int UnmarkedSum
            {
                get
                {
                    int sum = 0;

                    for (int h=0; h < Height; h++)
                    {
                        for (int w=0; w < Width; w++)
                        {
                            if (!Numbers[h, w].Marked)
                            {
                                sum += Numbers[h, w].Number;
                            }
                        }
                    }

                    return sum;
                }
            }

            public bool IsWon
            {
                get
                { 
                    // Check rows
                    for (int h = 0; h < Height; h++)
                    {
                        bool isWin = true;
                        for (int w = 0; w < Width; w++)
                        {
                            if (!Numbers[h, w].Marked)
                            {
                                isWin = false;
                            }
                        }
                        if (isWin)
                        {
                            return true;
                        }
                    }

                    // Check rows
                    for (int w = 0; w < Width; w++)
                    {
                        bool isWin = true;
                        for (int h = 0; h < Height; h++)
                        {
                            if (!Numbers[h, w].Marked)
                            {
                                isWin = false;
                            }
                        }
                        if (isWin)
                        {
                            return true;
                        }
                    }

                    return false;
                }
            }
        }

        public async Task Process()
        {
            var data = await ReadAndParse("DayFour/Data/day_4_input.txt", 5, 5);

            var winningBoards = FindWinningBoards(data);

            var lastWinner = winningBoards.LastOrDefault();

            if (lastWinner != null)
            {
                var winningNumber = lastWinner.WinningNumber;
                var unmarkedSum = lastWinner.Board.UnmarkedSum;

                Console.WriteLine($"{winningNumber} * {unmarkedSum} = {winningNumber * unmarkedSum}");
            }
        }

        private IEnumerable<WinningBoard> FindWinningBoards(BingoData data)
        {
            List<WinningBoard> winningBoards = new List<WinningBoard>();

            foreach (var n in data.GameNumbers)
            {
                foreach (var board in data.Boards)
                {
                    if (!board.IsWon)
                    {
                        board.MarkNumber(n);
                        if (board.IsWon)
                        {
                            winningBoards.Add(new WinningBoard(board, n));
                            if (data.Boards.All(b => b.IsWon))
                            {
                                return winningBoards;
                            }
                        }
                    }
                }
            }

            return winningBoards;
        }

        private WinningBoard? Play(BingoData data)
        {
            foreach (var n in data.GameNumbers)
            {
                foreach (var board in data.Boards)
                {
                    board.MarkNumber(n);
                    if (board.IsWon)
                    {
                        return new WinningBoard(board, n);
                    }
                }
            }

            return null;
        }

        private async Task<BingoData> ReadAndParse(string filename, int boardWidth, int boardHeight)
        {
            using (var stream = new FileStream(filename, FileMode.Open))
            using (var reader = new StreamReader(stream))
            {
                var numbersText = await reader.ReadLineAsync();

                IEnumerable<int> numbers = numbersText.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => int.Parse(x));

                var boards = new List<BingoBoard>();

                try
                {
                    while (true)
                    {
                        var board = await ReadBoard(reader, boardWidth, boardHeight);
                        if (board == null)
                        {
                            break;
                        }
                        boards.Add(board);
                    }
                }
                catch (Exception e)
                { 
                }

                return new BingoData(numbers, boards);
            }        
        }

        private async Task<BingoBoard?> ReadBoard(StreamReader reader, int boardWidth, int boardHeight)
        {
            var board = new BingoBoard(boardWidth, boardHeight);

            int h = 0;
            while (h < boardHeight)
            {
                if (reader.EndOfStream)
                {
                    return null;
                }

                var line = await reader.ReadLineAsync();

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var numbers = line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(n => int.Parse(n));

                if (numbers.Count() != boardWidth)
                {
                    throw new ArgumentException($"Encountered invalid number of columns = {numbers.Count()}");
                }

                for (int w = 0; w < boardWidth; w++)
                {
                    board.Numbers[h,w].Number = numbers.ElementAt(w);
                }

                h++;
            }

            return board;
        }
    }
}
