using System.Text;

namespace AdventOfCode.Day24
{
    internal class Day24 : DailyChallenge
    {
        private const string Filename = "Day24/Data/problem_input.txt";

        public enum InstructionType
        {
            Input,
            Add,
            Multiply,
            Divide,
            Mod,
            Equal
        }

        public static Dictionary<string, InstructionType> InstructionFromLabelMap = new Dictionary<string, InstructionType>()
        {
            { "inp", InstructionType.Input },
            { "add", InstructionType.Add },
            { "mul", InstructionType.Multiply },
            { "div", InstructionType.Divide },
            { "mod", InstructionType.Mod },
            { "eql", InstructionType.Equal },
        };

        public static Dictionary<InstructionType, string> InstructionToLabelMap = InstructionFromLabelMap.ToDictionary(x => x.Value, x => x.Key);

        public static HashSet<string> RegisterLabels = new HashSet<string>()
        {
            "w",
            "x",
            "y",
            "z",
        };
       
        public HashSet<int> ShowBlocks = new HashSet<int>()
        {
            0,
            1,
            2,
            3,
            4,
            5,
            6,
            7,
            8,
            9,
            10,
            11,
            12,
            13
        };

        public Dictionary<int, int> ReaderValues = new Dictionary<int, int>()
        {
            {  0, 9 },
            {  1, 9 },
            {  2, 9 },
            {  3, 9 },
            {  4, 9 },
            {  5, 4 },
            {  6, 9 },
            {  7, 9 },
            {  8, 9 },
            {  9, 9 },
            { 10, 1 },
            { 11, 5 },
            { 12, 3 },
            { 13, 9 },
        };

        private IEnumerable<int> RangeForSmallest;
        private IEnumerable<int> RangeForLargest;

        public Day24()
        {
            var rangeSmall = new List<int>();

            for (int i = 1; i <= 9; i++)
            {
                rangeSmall.Add(i);
            }

            RangeForSmallest = rangeSmall;

            var rangeLarge = new List<int>();

            for (int i = 9; i >= 9; i--)
            {
                rangeLarge.Add(i);
            }

            RangeForLargest = rangeLarge;
        }

        public async Task Process()
        {
            var program = await ReadAndParse(Filename);

            bool smallest = true;

            var final = FindLargestOrSmallest(program, smallest);

            var label = smallest ? "Smallest" : "Largest";

            Console.WriteLine($"{label} = {final.ToString()}");
        }

        private void RunInput(AocProgram program)
        {
            var reader = new InputReaderQueue();

            var state = new RegisterState();

            var chunks = ChunkInstructions(program);
            var analysis = new List<ChunkAnalysis>();

            for (int c = 0; c < chunks.Count(); c++)
            {
                var chunk = chunks.ElementAt(c);

                if (ShowBlocks.Contains(c))
                {
                    Console.WriteLine($"--- BLOCK {c} ---");
                    Console.WriteLine(analysis.Last().GetDescription());

                    if (analysis.Count > 1)
                    {
                        Console.WriteLine($"Next-W: z ({analysis.ElementAt(c).State.Values["z"]})%26 ({analysis.ElementAt(c).State.Values["z"] % 26}) + R ({analysis.ElementAt(c).R}) = "
                            + $"{analysis.ElementAt(c).State.Values["z"] % 26 + analysis.ElementAt(c).R}");
                    }

                    Console.WriteLine($"{state.Values["z"]} % 26 ({state.Values["z"] % 26}) + {analysis.Last().R} = " + (state.Values["z"] % 26 + analysis.Last().R));
                }

                for (int i = 0; i < chunk.Instructions.Count(); i++)
                {
                    var instruction = chunk.Instructions.ElementAt(i);

                    state = instruction.Execute(reader, state);

                    if (ShowBlocks.Contains(c))
                    {
                        Console.Write(instruction.GetDescription().PadLeft(10));
                        PrintState(state);
                        Console.WriteLine();
                    }
                }

                if (ShowBlocks.Contains(c))
                {
                    Console.WriteLine($"{state.Values["w"]} + {analysis.Last().P} = " + (state.Values["w"] + analysis.Last().P));
                }
            }
        }

        private void PrintState(RegisterState state)
        {
            foreach (var value in state.Values)
            {
                Console.Write($"{value.Key} = {value.Value}".PadLeft(15));
            }
        }

        private ChunkAnalysis AnalyzeChunk(IEnumerable<Instruction> chunk)
        {
            var analysis = new ChunkAnalysis();

            int m = chunk.ElementAt(4).SecondConstant;
            int r = chunk.ElementAt(5).SecondConstant;
            int p = chunk.ElementAt(15).SecondConstant;
            
            analysis.M = m;
            analysis.R = r;
            analysis.P = p;

            return analysis;
        }

        private IEnumerable<InstructionChunk> ChunkInstructions(AocProgram program)
        {
            List<Instruction> instructionBlock = new List<Instruction>();
            List<InstructionChunk> allBlocks = new List<InstructionChunk>();

            instructionBlock.Add(program.Instructions.First());

            foreach (var instruction in program.Instructions.Skip(1))
            {
                if (instruction is InputInstruction)
                {
                    allBlocks.Add(
                        new InstructionChunk()
                        {
                            Instructions = instructionBlock,
                            Analysis = AnalyzeChunk(instructionBlock)
                        });
                    instructionBlock = new List<Instruction>();
                }
                instructionBlock.Add(instruction);
            }

            allBlocks.Add(
                new InstructionChunk()
                {
                    Instructions = instructionBlock,
                    Analysis = AnalyzeChunk(instructionBlock)
                });

            return allBlocks;
        }

        private long? FindLargestOrSmallest(AocProgram program, bool smallest)
        {
            Dictionary<long, long> resultMap = new Dictionary<long, long>();
            var reader = new InputReaderQueue();
            var chunks = ChunkInstructions(program);
            var failedStates = new HashSet<string>();

            var registerStack = new Stack<RegisterState>();
            registerStack.Push(new RegisterState());
            long? final = null;

            FindSmallestLargestRecursiveFast(
                new Stack<int>(), 
                registerStack,
                chunks,
                14,
                reader,
                failedStates,
                smallest,
                (values, states) =>
                {
                    if (states.Peek().Values["z"] == 0)
                    {
                        final = ListToLong(values.Select(x => x).Reverse());
                        return true;
                    }

                    return false;
                });

            return final;
        }


        private bool FindSmallestLargestRecursive(
            Stack<int> values,
            Stack<RegisterState> states, 
            IEnumerable<InstructionChunk> chunks, 
            int digitCount, 
            InputReaderQueue input, 
            HashSet<string> failedStates,
            bool smallest,
            Func<Stack<int>, Stack<RegisterState>, bool> action)
        {
            int depth = values.Count();
            var hash = states.Peek().Hash(depth);

            if (depth == digitCount)
            {
                if (action(values, states) == true)
                {
                    return true;
                }
            }
            else if (failedStates.Contains(hash))
            {
                return false;
            }
            else
            {
                if (depth > 7 && states.Peek().Values["z"] > PowerCache.Get(1 + digitCount - depth))
                {
                    failedStates.Add(hash);
                    return false;
                }

                var currentChunk = chunks.ElementAt(depth);

                List<int> range = new List<int>();

                if (smallest)
                {
                    for (int i = 1; i <= 9; i++)
                    {
                        range.Add(i);
                    }
                }
                else
                {
                    for (int i = 9; i >= 1; i--)
                    {
                        range.Add(i);
                    }
                }

                foreach (var i in range)
                {
                    input.PushValue(i);
                    values.Push(i);
                    var nextState = ExecuteInstructions(
                        states.Peek(),
                        currentChunk.Instructions,
                        input);
                    states.Push(nextState);
                    if (FindSmallestLargestRecursive(values, states, chunks, digitCount, input, failedStates, smallest, action) == true)
                    {
                        return true;
                    }
                    values.Pop();
                    states.Pop();
                }

                if (!failedStates.Contains(hash))
                {
                    failedStates.Add(hash);
                }
            }

            return false;
        }

        private bool FindSmallestLargestRecursiveFast(
            Stack<int> values,
            Stack<RegisterState> states,
            IEnumerable<InstructionChunk> chunks,
            int digitCount,
            InputReaderQueue input,
            HashSet<string> failedStates,
            bool smallest,
            Func<Stack<int>, Stack<RegisterState>, bool> action)
        {
            int depth = values.Count();
            var hash = states.Peek().Hash(depth);

            if (depth == digitCount)
            {
                if (action(values, states) == true)
                {
                    return true;
                }
            }
            else if (failedStates.Contains(hash))
            {
                return false;
            }
            else
            {
                if (depth > 7 && states.Peek().Values["z"] > PowerCache.Get(1 + digitCount - depth))
                {
                    failedStates.Add(hash);
                    return false;
                }

                var currentChunk = chunks.ElementAt(depth);

                IEnumerable<int> range = smallest ? RangeForSmallest : RangeForLargest;

                foreach (var i in range)
                {
                    input.PushValue(i);
                    values.Push(i);
                    var nextState = ExecuteChunkFast(states.Peek(), currentChunk, i);
                    states.Push(nextState);
                    if (FindSmallestLargestRecursiveFast(values, states, chunks, digitCount, input, failedStates, smallest, action) == true)
                    {
                        return true;
                    }
                    values.Pop();
                    states.Pop();
                }

                if (!failedStates.Contains(hash))
                {
                    failedStates.Add(hash);
                }
            }

            return false;
        }

        private RegisterState ExecuteChunkFast(RegisterState state, InstructionChunk chunk, long w)
        {
            var newState = new RegisterState(state);
            bool willIncrease = w != (state.Values["z"] % 26 + chunk.Analysis.R);
            if (chunk.Analysis.M > 1)
            {
                newState.Values["z"] = newState.Values["z"] / chunk.Analysis.M;
            }
            if (willIncrease)
            {
                newState.Values["z"] = newState.Values["z"] * 26 + chunk.Analysis.P + w;
            }

            return newState;
        } 

        private long ListToLong(IEnumerable<int> digits)
        {
            long shifter = 1;
            long result = 0;

            for (int i = digits.Count()-1; i >= 0; i--)
            {
                result += shifter * digits.ElementAt(i);
                shifter *= 10;
            }

            return result;
        }

        private RegisterState ExecuteInstructions(RegisterState state, IEnumerable<Instruction> instructions, InputReaderQueue input)
        {
            var currentState = state;

            foreach (var instruction in instructions)
            {
                currentState = instruction.Execute(input, currentState);
            }

            return currentState;
        }

        private object Group(Dictionary<long, long> values)
        {
            var grouped = values.GroupBy(c => c.Value)
                .Select(group => new
                {
                    Value = group.Key,
                    Count = group.Count()
                })
                .OrderByDescending(x => x.Count);

            return grouped;
        }

        private class InstructionChunk
        {
            public IEnumerable<Instruction> Instructions;
            public ChunkAnalysis Analysis;
        }

        public class InputReader
        {
            private int[] _values;
            private int index = 0;

            public InputReader(IEnumerable<int> values)
            {
                _values = values.ToArray();
            }

            public int GetValue()
            {
                if (index >= _values.Length)
                {
                    throw new Exception("Attempted to read beyond end of values list");
                }

                return _values[index++];
            }
        }

        public class InputReaderQueue
        {
            private Queue<long> _values = new Queue<long>();

            public InputReaderQueue()
            {
            }

            public void PushValue(long value)
            {
                _values.Enqueue(value);
            }

            public long GetValue()
            {
                if (!_values.Any())
                {
                    throw new Exception("Attempted to read beyond end of values list");
                }

                return _values.Dequeue();
            }
        }

        private async Task<AocProgram> ReadAndParse(string filename)
        {
            var lines = await File.ReadAllLinesAsync(filename);

            var instructions = new List<Instruction>();

            foreach (var line in lines)
            {
                var split = line.Split(" ");

                var type = InstructionFromLabelMap[split[0]];

                if (!RegisterLabels.Contains(split[1]))
                {
                    throw new Exception($"Unrecognized register {split[1]}");
                }

                Instruction newInstruction;

                if (type == InstructionType.Input)
                {
                    newInstruction = new InputInstruction();
                }
                else
                {
                    string secondRegister = null;
                    int secondConstant = 0;
                    if (RegisterLabels.Contains(split[2]))
                    {
                        secondRegister = split[2];
                    }
                    else
                    {
                        secondConstant = int.Parse(split[2]);
                    }

                    if (type == InstructionType.Add)
                    {
                        newInstruction = new AddInstruction();
                    }
                    else if (type == InstructionType.Multiply)
                    {
                        newInstruction = new MultiplyInstruction();
                    }
                    else if (type == InstructionType.Divide)
                    {
                        newInstruction = new DivideInstruction();
                    }
                    else if (type == InstructionType.Mod)
                    {
                        newInstruction = new ModInstruction();
                    }
                    else if (type == InstructionType.Equal)
                    {
                        newInstruction = new EqualInstruction();
                    }
                    else
                    {
                        throw new ArgumentException($"Recognized instruction type = {type}");
                    }

                    newInstruction.SecondRegister = secondRegister;
                    newInstruction.SecondConstant = secondConstant;
                }

                newInstruction.FirstRegister = split[1];

                instructions.Add(newInstruction);
            }

            return new AocProgram()
            {
                Instructions = instructions
            };
        }

        private class AocProgram
        {
            public List<Instruction> Instructions { get; set; }

            public RegisterState Execute(InputReaderQueue input)
            {
                var state = new RegisterState();

                foreach (var instruction in Instructions)
                {
                    instruction.Execute(input, state);
                }

                return state;
            }
        }

        public abstract class Instruction
        {
            public InstructionType Type { get; set; }
            public string FirstRegister { get; set; }
            public string? SecondRegister { get; set; }
            public int SecondConstant { get; set; }

            public abstract RegisterState Execute(InputReaderQueue input, RegisterState state);

            public string GetDescription()
            {
                return $"{InstructionToLabelMap[Type]} {FirstRegister} {SecondRegister ?? SecondConstant.ToString()}";
            }

            protected long GetSecondValue(RegisterState registers)
            {
                if (SecondRegister != null)
                {
                    return registers.Values[SecondRegister];
                }

                return SecondConstant;
            }
        }

        public class InputInstruction : Instruction
        {
            public InputInstruction()
            {
                Type = InstructionType.Input;
            }

            public override RegisterState Execute(InputReaderQueue input, RegisterState state)
            {
                var newState = new RegisterState(state);
                newState.Values[FirstRegister] = input.GetValue();
                return newState;
            }
        }

        public class AddInstruction : Instruction
        {
            public AddInstruction()
            {
                Type = InstructionType.Add;
            }

            public override RegisterState Execute(InputReaderQueue input, RegisterState state)
            {
                var newState = new RegisterState(state);
                newState.Values[FirstRegister] = state.Values[FirstRegister] + GetSecondValue(state);
                return newState;
            }
        }

        public class MultiplyInstruction : Instruction
        {
            public MultiplyInstruction()
            {
                Type = InstructionType.Multiply;
            }

            public override RegisterState Execute(InputReaderQueue input, RegisterState state)
            {
                var newState = new RegisterState(state);
                newState.Values[FirstRegister] = state.Values[FirstRegister] * GetSecondValue(state);
                return newState;
            }
        }


        public class DivideInstruction : Instruction
        {
            public DivideInstruction()
            {
                Type = InstructionType.Divide;
            }

            public override RegisterState Execute(InputReaderQueue input, RegisterState state)
            {
                var newState = new RegisterState(state);
                newState.Values[FirstRegister] = state.Values[FirstRegister] / GetSecondValue(state);
                return newState;
            }
        }

        public class ModInstruction : Instruction
        {
            public ModInstruction()
            {
                Type = InstructionType.Mod;
            }

            public override RegisterState Execute(InputReaderQueue input, RegisterState state)
            {
                var newState = new RegisterState(state);
                newState.Values[FirstRegister] = state.Values[FirstRegister] % GetSecondValue(state);
                return newState;
            }
        }


        public class EqualInstruction : Instruction
        {
            public EqualInstruction()
            {
                Type = InstructionType.Equal;
            }

            public override RegisterState Execute(InputReaderQueue input, RegisterState state)
            {
                var newState = new RegisterState(state);
                newState.Values[FirstRegister] =
                    state.Values[FirstRegister] == GetSecondValue(state)
                    ? 1
                    : 0;
                return newState;
            }
        }

        private class ChunkAnalysis
        {
            public int R;
            public int P;
            public int M;
            public RegisterState State;

            public string GetDescription()
            { 
                var builder = new StringBuilder();

                builder.Append($"M = {M.ToString()}  ".PadLeft(8));
                builder.Append($"R = {R.ToString()}  ".PadLeft(8));
                builder.Append($"P = {P.ToString()}  ".PadLeft(8));

                return builder.ToString();
            }
        }

        private static class PowerCache
        {
            private static Dictionary<int, long> _cache = new Dictionary<int, long>();

            public static long Get(int depth)
            {
                if (_cache.TryGetValue(depth, out var result))
                {
                    return result;
                }
                else 
                {
                    long power = (long)Math.Pow(26, depth);
                    _cache[depth] = power;
                    return power;
                }
            }
        }

        public class RegisterState
        {
            public Dictionary<string, long> Values { get; set; } = new Dictionary<string, long>();

            public RegisterState()
            {
                foreach (var label in RegisterLabels)
                {
                    Values[label] = 0;
                }
            }

            public RegisterState(RegisterState other)
            {
                foreach (var label in RegisterLabels)
                {
                    Values[label] = other.Values[label];
                }
            }

            public string Hash(int depth)
            {
                return $"{depth}:{Values["x"]}:{Values["y"]}:{Values["z"]}:{Values["w"]}";
            }
        }
    }
}
