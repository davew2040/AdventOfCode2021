using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode.Day23
{
    internal class Day23 : DailyChallenge
    {
        private const string Filename = "Day23/Data/problem_input.txt";

        private const int HallwayRow = 1;
        //private HashSet<int> RoomRows = new HashSet<int> { NearHallwayRoom, FarHallwayRoom };
        private HashSet<char> SnailMarkers = new HashSet<char>()
        {
            'A', 'B', 'C', 'D'
        };
        private static Dictionary<char, int> SnailHomesLetterToColumn = new Dictionary<char, int>()
        {
            { 'A', 3 },
            { 'B', 5 },
            { 'C', 7 },
            { 'D', 9 }
        };
        private static Dictionary<int, char> SnailHomesColumnToLetter = SnailHomesLetterToColumn
            .ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

        private Dictionary<char, int> SnailWeights = new Dictionary<char, int>()
        {
            { 'A', 1 },
            { 'B', 10 },    
            { 'C', 100 },
            { 'D', 1000 }
        };

        private static HashSet<Point> SnailHallwaySpots = new HashSet<Point>()
        {
            new Point(1, 1),
            new Point(2, 1),
            new Point(4, 1),
            new Point(6, 1),
            new Point(8, 1),
            new Point(10, 1),
            new Point(11, 1),
        };

        public async Task Process()
        {
            var input = await ReadAndParse(Filename);
            
            Print(input.Map);

            var start = DateTime.Now;

            var shortest = BuildShortestPathContext(input.Map);

            PrintMoveSet(shortest);

            Console.WriteLine($"Finished in {(DateTime.Now - start).TotalMilliseconds}");

            Console.WriteLine($"cheapest = {shortest}");
        }

        private ShortestPathContext BuildShortestPathContext(Map map)
        {
            var context = new ShortestPathContext();

            var rootEntry = new ShortestPathContextEntry()
            {
                DistanceFromStart = 0,
                NodeHash = map.Hash,
                From = null,
                Map = map,
                IsComplete = true
            };
            context.ShortestPathsByHash.Add(map.Hash, rootEntry);
            context.AddByDistance(rootEntry);

            while (true)
            {
                if (context.ShortestPathsSoFarByDistance.Count == 0)
                {
                    throw new Exception("Could not find valid node to process");
                }

                var currentNode = context.PopFirst();
                currentNode.IsComplete = true;

                if (currentNode.Map.IsComplete)
                {
                    context.FinalHash = currentNode.NodeHash;
                    break;
                }

                //var candidates = FindMoveCandidates(currentNode.Map);
                //var moves = candidates.SelectMany(c => FindValidMoves(c, currentNode.Map));
                var moves = FindValidMoves(currentNode.Map);

                foreach (var move in moves)
                {
                    var movedMap = new Map(currentNode.Map, move.Move);

                    if (!context.ShortestPathsByHash.ContainsKey(movedMap.Hash))
                    {
                        var newEntry = new ShortestPathContextEntry()
                        {
                            DistanceFromStart = currentNode.DistanceFromStart + move.Cost,
                            Map = movedMap,
                            From = currentNode.Map.Hash,
                            NodeHash = movedMap.Hash,
                            IsComplete = false
                        };
                        context.ShortestPathsByHash.Add(movedMap.Hash, newEntry);
                        context.AddByDistance(newEntry);
                    }
                    else
                    {
                        var newMapNode = context.ShortestPathsByHash[movedMap.Hash];
                        if (!newMapNode.IsComplete)
                        {
                            var currentDistance = currentNode.DistanceFromStart + move.Cost;
                            if (newMapNode.DistanceFromStart > currentDistance)
                            {
                                newMapNode.From = currentNode.Map.Hash;
                                context.UpdateByDistance(currentDistance, newMapNode);
                            }
                        }
                    }
                }
            }

            return context;
        }

        private IEnumerable<string> GetMoveSet(ShortestPathContext context)
        {
            var moveSet = new List<string>();
            var currentHash = context.FinalHash;

            while (currentHash != null)
            {
                var entry = context.ShortestPathsByHash[currentHash];
                moveSet.Add(entry.NodeHash);
                currentHash = entry.From;
            };

            return moveSet;
        }

        private void PrintMoveSet(ShortestPathContext context)
        {
            var hashes = GetMoveSet(context);

            foreach (var hash in hashes.Reverse())
            {
                var map = context.ShortestPathsByHash[hash].Map;
                Print(map);
            }
        }

        private IEnumerable<MoveSet> FindMoveSets(Map map)
        {
            var validMoveSets = new List<MoveSet>();
            var currentMoveSet = new Stack<WeightedMove>();
            int cheapestCost = int.MaxValue;
            var encounteredStates = new HashSet<string>();

            FindMoveSetsRecursive(currentMoveSet, encounteredStates, validMoveSets, ref cheapestCost, map, 0);

            return validMoveSets;
        }

        private void FindMoveSetsRecursive(
            Stack<WeightedMove> currentMoveSet,
            HashSet<string> encounteredStates,
            List<MoveSet> validMoveSets,
            ref int cheapestCost,
            Map map,
            int depth
        )
        {
            var currentCost = currentMoveSet.Sum(m => m.Cost);
            if (currentCost > cheapestCost)
            {
                return;
            }

            if (map.IsComplete)
            {
                if (currentCost < cheapestCost)
                {
                    validMoveSets.Add(new MoveSet(currentMoveSet.ToList().AsEnumerable().Reverse()));
                    cheapestCost = currentCost;
                }
                return;
            }

            var hash = map.Hash;
            if (encounteredStates.Contains(hash))
            {
                return;
            }
            else
            {
                encounteredStates.Add(hash);
            }

            var candidates = FindMoveCandidates(map);
            var moves = candidates.SelectMany(c => FindValidMoves(c, map));
            moves = FilterMoves(moves).OrderBy(x => x.Cost).ToList();

            int count = 0;
            foreach (var move in moves)
            {
                map.MakeMove(move.Move);
                if (depth == 0)
                {
                    int x = 24;
                    Console.WriteLine(count++);
                    Print(map);
                }
                //Print(map);
                currentMoveSet.Push(move);
                FindMoveSetsRecursive(currentMoveSet, encounteredStates, validMoveSets, ref cheapestCost, map, depth+1);
                currentMoveSet.Pop();
                map.UndoMove(move.Move);
                //Print(map);
            }
        }

        private IEnumerable<WeightedMove> FilterMoves(IEnumerable<WeightedMove> moves)
        {
            var yMax = moves.Max(m => m.Move.To.Y);
            return moves.Where(m => m.Move.To.Y == yMax);
        }

        private IEnumerable<Point> FindMoveCandidates(Map map)
        {
            var candidates = new List<Point>();

            var snailPoints = map.Values
                .Where(kvp => SnailMarkers.Contains(kvp.Value))
                .Select(kvp => kvp.Key)
                .Where(s => CanMove(s, map));
            
            return snailPoints;
        }


        private IEnumerable<WeightedMove> FindValidMoves(Map map)
        {
            var validMoves = new List<WeightedMove>();

            foreach (var value in map.Values)
            {
                if (!SnailMarkers.Contains(value.Value))
                {
                    continue;
                }

                var snailValue = map.Values[value.Key];
                var cost = SnailWeights[snailValue];

                if (value.Key.Y == HallwayRow)
                {
                    var homeColumn = SnailHomesLetterToColumn[value.Value];
                    if (!HomeIsComplete(homeColumn, map))
                    {
                        continue;
                    }
                    var xDistance = DistanceToColumn(value.Key, homeColumn, map);
                    if (xDistance == -1)
                    {
                        continue;
                    }
                    var columnSnailPosition = ColumnSnailPosition(homeColumn, map);
                    if (!columnSnailPosition.HasValue)
                    {
                        continue;
                    }
                    var yDistance = columnSnailPosition.Value.Y - HallwayRow;

                    validMoves.Add(new WeightedMove()
                    {
                        Cost = (xDistance + yDistance) * cost,
                        Move = new Move(value.Key, columnSnailPosition.Value)
                    });
                }
                else
                {
                    if (HomeIsComplete(value.Key.X, map))
                    {
                        continue;
                    }
                    var homeTop = HomeTop(value.Key.X, map);
                    if (!value.Key.Equals(homeTop))
                    {
                        continue;
                    }
                    var distanceY = homeTop.Y - HallwayRow;
                    var hallwayPoints = ReachableHallwayPoints(value.Key.X, map);

                    foreach (var hallwayPoint in hallwayPoints)
                    {
                        var distanceX = Math.Abs(value.Key.X - hallwayPoint);
                        validMoves.Add(
                            new WeightedMove()
                            {
                                Cost = cost * (distanceX + distanceY),
                                Move = new Move(value.Key, new Point(hallwayPoint, HallwayRow))
                            }
                        );
                    }
                }
            }

            return validMoves;
        }

        private List<int> ReachableHallwayPoints(int xValue, Map map)
        {
            var xSpots = new List<int>();

            for (int x=xValue-1; x >= 1; x--)
            {
                var testPoint = new Point(x, 1);
                if (!SnailHallwaySpots.Contains(testPoint))
                {
                    continue;
                }
                if (map.Values[testPoint] != '.')
                {
                    break;
                }
                xSpots.Add(x);
            }

            for (int x = xValue + 1; x <= 11; x++)
            {
                var testPoint = new Point(x, 1);
                if (!SnailHallwaySpots.Contains(testPoint))
                {
                    continue;
                }
                if (map.Values[testPoint] != '.')
                {
                    break;
                }
                xSpots.Add(x);
            }

            return xSpots;
        }

        private int DistanceToColumn(Point p, int targetX, Map map)
        {
            var first = p.X;
            var second = targetX;
            var min = Math.Min(first, second);
            var max = Math.Max(first, second);

            for (int x = min+1; x < max; x++)
            {
                if (map.Values[new Point(x, HallwayRow)] != '.')
                {
                    return -1;
                }
            }

            return max - min;
        }

        private bool HomeIsComplete(int targetX, Map map)
        {
            int expectedHomeValue = SnailHomesColumnToLetter[targetX];
            for (int y=HallwayRow+map.RoomSize; y>HallwayRow; y--)
            {
                var value = map.Values[new Point(targetX, y)];
                if (value == '.')
                {
                    return true;
                }
                else if (value != expectedHomeValue)
                {
                    return false;
                }
            }

            return true;
        }


        private Point HomeTop(int targetX, Map map)
        {
            for (int y = HallwayRow + 1; y <= HallwayRow + map.RoomSize; y++)
            {
                var testPoint = new Point(targetX, y);
                var value = map.Values[testPoint];
                if (value != '.')
                {
                    return testPoint;
                }
            }

            throw new Exception("Accidentally reached bottom without finding target!");
        }

        private Point? ColumnSnailPosition(int columnX, Map map)
        {
            if (map.Values[new Point(columnX, HallwayRow+1)] != '.')
            {
                return null;
            }

            var expectedValue = SnailHomesColumnToLetter[columnX];

            for (int y=HallwayRow+map.RoomSize; y > HallwayRow; y--)
            {
                var testPoint = new Point(columnX, y);
                if (map.Values[testPoint] == '.')
                {
                    return testPoint;
                }

                if (map.Values[testPoint] != expectedValue)
                {
                    return null;
                }
            }

            return null;
        }


        private IEnumerable<WeightedMove> FindValidMoves(Point p, Map map)
        {
            var validMoves = new List<WeightedMove>();
            var possibleDestinations = FindDestinations(p, map);

            if (IsInHallway(p))
            {
                var homeDestinations = possibleDestinations
                    .Where(d => d.Move.To.Y >= 2 
                        && d.Move.To.X == SnailHomesLetterToColumn[map.Values[p]]);
                if (!homeDestinations.Any())
                {
                    return validMoves;
                }

                var yMax = homeDestinations.Max(d => d.Move.To.Y);
                var targetHomeDestination = homeDestinations.FirstOrDefault(d => d.Move.To.Y == yMax);

                if (targetHomeDestination != null && map.IsBlocking(p))
                {
                    validMoves.Add(targetHomeDestination);
                }
            }
            else 
            {
                var hallwaySpots = possibleDestinations.Where(d => SnailHallwaySpots.Contains(d.Move.To));
                validMoves.AddRange(hallwaySpots);
            }

            return validMoves;
        }

        // Has the ability to move, not considering whether move is desirable
        private bool CanMove(Point p, Map map)
        {
            if (IsInHallway(p))
            {
                return true;
            }

            if (!map.IsInFinalPosition(p))
            {
                return !map.IsBlocked(p);
            }

            return false;
        }

        private bool IsInHallway(Point p) => HallwayRow == p.Y;

        private HashSet<Point> FindDestinationsStack(Point start, Dictionary<Point, char> map)
        {
            var destinations = new HashSet<Point>();
            var visited = new HashSet<Point>();

            var nextPoints = new Stack<Point>();
            nextPoints.Push(start);

            while (nextPoints.Count() > 0)
            {
                var nextPoint = nextPoints.Pop();
                visited.Add(nextPoint);

                if (!nextPoint.Equals(start))
                {
                    destinations.Add(nextPoint);
                }

                var adjacentPoints = GetAdjacentPoints(nextPoint)
                    .Where(p => !visited.Contains(p))
                    .Where(p => map[p] == '.');

                foreach (var adjacentPoint in adjacentPoints)
                {
                    nextPoints.Push(adjacentPoint);
                }
            }

            return destinations;
        }

        private IEnumerable<WeightedMove> FindDestinations(Point start, Map map)
        {
            var destinations = new List<WeightedMove>();
            var pathSoFar = new Stack<Point>();
            var visited = new HashSet<Point>();
            pathSoFar.Push(start);

            FindDestinationsRecursive(start, pathSoFar, visited, destinations, map);

            return destinations;
        }

        private void FindDestinationsRecursive(
            Point start,
            Stack<Point> pathSoFar, 
            HashSet<Point> visited, 
            List<WeightedMove> destinations,
            Map map)
        {
            visited.Add(pathSoFar.Peek());

            if (pathSoFar.Count() > 1)
            {
                destinations.Add(
                    new WeightedMove()
                    {
                        Move = new Move(pathSoFar.Last(), pathSoFar.First()),
                        Cost = (pathSoFar.Count()-1) * SnailWeights[map.Values[start]]
                    }
                );
            }

            var adjacentPoints = GetAdjacentPoints(pathSoFar.Peek())
                .Where(p => map.Values[p] == '.')
                .Where(p => !visited.Contains(p))
                .ToList();

            foreach (var adjacentPoint in adjacentPoints)
            {
                pathSoFar.Push(adjacentPoint);
                FindDestinationsRecursive(start, pathSoFar, visited, destinations, map);
                pathSoFar.Pop();
            }
        }

        private IEnumerable<Point> GetAdjacentPoints(Point p)
        {
            return new List<Point>()
            {
                new Point(p.X-1, p.Y),
                new Point(p.X+1, p.Y),
                new Point(p.X, p.Y-1),
                new Point(p.X, p.Y+1),
            };
        }

        private void Print(Map map)
        {
            var xMax = map.Values.Keys.Max(p => p.X);
            var yMax = map.Values.Keys.Max(p => p.Y);

            for (int y = 0; y <= yMax; y++)
            {
                for (int x = 0; x <= xMax; x++)
                {
                    var p = new Point(x, y);

                    if (map.Values.ContainsKey(p))
                    {
                        Console.Write(map.Values[p]);
                    }
                    else
                    {
                        Console.Write(' ');
                    }
                }
                Console.WriteLine();
            }

            Console.WriteLine();
        }

        private class Move
        {
            public Point From;
            public Point To;

            public Move(Point from, Point to)
            {
                From = from;
                To = to;
            }
        }

        private class WeightedMove
        {
            public Move Move;
            public int Cost;
        }

        private class MoveSet
        {
            public IEnumerable<WeightedMove> Moves;

            public int Cost
            {
                get
                {
                    return Moves.Sum(m => m.Cost);
                }
            }

            public MoveSet(IEnumerable<WeightedMove> moves)
            {
                Moves = moves;
            }
        }

        private async Task<Input> ReadAndParse(string filename)
        {
            var lines = await File.ReadAllLinesAsync(filename);
            var input = new Input();
            input.Map = new Map(lines.Length - 3);

            for (int y = 0; y < lines.Count(); y++)
            {
                for (int x =0; x<lines[y].Length; x++)
                {
                    input.Map.Values.Add(new Point(x, y), lines[y][x]);
                }
            }

            return input;
        }

        private struct Point
        {
            public int X;
            public int Y;

            public Point(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

        private class Map
        {
            public Dictionary<Point, char> Values = new Dictionary<Point, char>();
            public int RoomSize;

            public HashSet<Point> SnailRoomSpots = new HashSet<Point>();
            public HashSet<Point> AllSnailSpots;

            private string _hash = null;

            public Map(int roomSize)
            {
                RoomSize = roomSize;

                for (int i=1; i<=roomSize; i++)
                {
                    foreach (int home in SnailHomesLetterToColumn.Values)
                    {
                        SnailRoomSpots.Add(new Point(home, HallwayRow + i));
                    }
                }

                AllSnailSpots = SnailRoomSpots.Union(SnailHallwaySpots).ToHashSet();
            }

            public Map(Map map, Move move)
            {
                Values = map.Values.ToDictionary(x => x.Key, x => x.Value);
                RoomSize = map.RoomSize;

                MakeMove(move);

                SnailRoomSpots = map.SnailRoomSpots;

                AllSnailSpots = map.AllSnailSpots;
            }

            public void MakeMove(Move move)
            {
                Values[move.To] = Values[move.From];
                Values[move.From] = '.';
            }

            public void UndoMove(Move move)
            {
                Values[move.From] = Values[move.To];
                Values[move.To] = '.';
            }

            private bool IsInHome(Point p)
                => SnailRoomSpots.Contains(p) && p.X == SnailHomesLetterToColumn[Values[p]];

            public bool IsBlocking(Point p)
            {
                for (int y=p.Y+1; y<=RoomSize+2; y++)
                {
                    if (!IsInHome(new Point(p.X, y)))
                    {
                        return true;
                    }
                }

                return false;
            }

            public bool IsBlocked(Point p)
            {
                for (int y = p.Y - 1; y < 1; y--)
                {
                    if (Values[new Point(p.X, p.Y)] != '.')
                    {
                        return true;
                    }
                }

                return false;
            }

            public bool IsInFinalPosition(Point p)
            {
                for (int y = p.Y; y <= RoomSize + 2; y++)
                {
                    if (!IsInHome(new Point(p.X, y)))
                    {
                        return false;
                    }
                }

                return false;
            }

            public string Hash
            {
                get
                {
                    if (_hash == null)
                    {
                        StringBuilder builder = new StringBuilder();

                        for (int x = 1; x <= 11; x++)
                        {
                            builder.Append(Values[new Point(x, 1)]);
                        }

                        for (int y = 2; y < 2 + RoomSize; y++)
                        {
                            foreach (var x in SnailHomesColumnToLetter.Keys)
                            {
                                builder.Append(Values[new Point(x, y)]);
                            }
                        }

                        _hash = builder.ToString();
                    }

                    return _hash;
                }
            }

            public bool IsComplete
            {
                get
                {
                    foreach (var spot in SnailRoomSpots)
                    {
                        var value = Values[spot];
                        if (value != SnailHomesColumnToLetter[spot.X])
                        {
                            return false;
                        }
                    }

                    return true;
                }
            }
        }

        private class RoomInfo
        {

        }

        private class ShortestPathContext
        {
            public SortedList<int, HashSet<ShortestPathContextEntry>> ShortestPathsSoFarByDistance 
                = new SortedList<int, HashSet<ShortestPathContextEntry>>();
            public Dictionary<string, ShortestPathContextEntry> ShortestPathsByHash
                = new Dictionary<string, ShortestPathContextEntry>();
            public string FinalHash = null;
            
            public ShortestPathContextEntry PopFirst()
            {
                var firstList = ShortestPathsSoFarByDistance.First().Value;
                if (firstList.Count == 1)
                {
                    var toReturn = firstList.First();
                    ShortestPathsSoFarByDistance.RemoveAt(0);
                    return toReturn;
                }
                else
                {
                    var toReturn = firstList.First();
                    firstList.Remove(firstList.First());
                    return toReturn;
                }
            }

            public void AddByDistance(ShortestPathContextEntry toAdd)
            {
                if (!ShortestPathsSoFarByDistance.ContainsKey(toAdd.DistanceFromStart))
                {
                    ShortestPathsSoFarByDistance.Add(toAdd.DistanceFromStart, 
                        new HashSet<ShortestPathContextEntry>());
                }
                var list = ShortestPathsSoFarByDistance[toAdd.DistanceFromStart];
                list.Add(toAdd);
            }

            public void UpdateByDistance(int newDistance, ShortestPathContextEntry update)
            {
                var list = ShortestPathsSoFarByDistance[update.DistanceFromStart];

                if (list.Count > 1)
                {
                    list.Remove(list.Single(x => x.NodeHash == update.NodeHash));
                }
                else
                {
                    ShortestPathsSoFarByDistance.Remove(update.DistanceFromStart);
                }

                update.DistanceFromStart = newDistance;

                AddByDistance(update);
            }
        }

        private class ShortestPathContextEntry
        {
            public string NodeHash { get; set; }
            public int DistanceFromStart { get; set; }
            public string? From { get; set; }
            public Map Map { get; set; }
            public bool IsComplete { get; set; }

            public override bool Equals(object obj)
            {
                return Equals(obj as ShortestPathContextEntry);
            }

            public bool Equals(ShortestPathContextEntry other)
            {
                return other != null &&
                       NodeHash == other.NodeHash;
            }

            public override int GetHashCode()
            {
                return NodeHash.GetHashCode();
            }
        }

        private class Input
        {
            public Map Map;
        }
    }
}
