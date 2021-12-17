using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode.Day16
{
    internal class Day16 : DailyChallenge
    {
        private const string Filename = "Day16/Data/problem_input.txt";
        
        public enum PacketType
        {
            Operator,
            Literal
        }
        
        public enum OperatorType
        {
            Sum,
            Product,
            Minimum,
            Maximum,
            GreaterThan,
            LessThan,
            EqualTo
        }

        private static readonly Dictionary<int, OperatorType> _operatorTypeTable = new Dictionary<int, OperatorType>()
        {
            [0] = OperatorType.Sum,
            [1] = OperatorType.Product,
            [2] = OperatorType.Minimum,
            [3] = OperatorType.Maximum,
            [5] = OperatorType.GreaterThan,
            [6] = OperatorType.LessThan,
            [7] = OperatorType.EqualTo
        };

        private static readonly Dictionary<char, string> _hexTable = new Dictionary<char, string>()
        {
            ['0'] = "0000",
            ['1'] = "0001",
            ['2'] = "0010",
            ['3'] = "0011",
            ['4'] = "0100",
            ['5'] = "0101",
            ['6'] = "0110",
            ['7'] = "0111",
            ['8'] = "1000",
            ['9'] = "1001",
            ['A'] = "1010",
            ['B'] = "1011",
            ['C'] = "1100",
            ['D'] = "1101",
            ['E'] = "1110",
            ['F'] = "1111",
        };

        public async Task Process()
        {
            var hex = await ReadAndParse(Filename);
            var binary = ConvertToBinary(hex);

            int position = 0;
            var packet = Packet.ReadPacket(binary, ref position);

            long value = packet.GetValue();

            Console.WriteLine(value);
        }

        public static long BinaryToLong(string binary)
        {
            long sum = 0;
            long shifter = 1;
            for (int i=binary.Length-1;  i >=0; i--, shifter <<= 1)
            {
                if (binary[i] == '1')
                {
                    sum += shifter;
                }
            }
            return sum;
        } 

        private async Task<string> ReadAndParse(string filename)
        {
            return (await File.ReadAllLinesAsync(filename)).First();
        }

        private int CountAllVersionInstances(Packet packet)
        {
            if (packet is LiteralPacket literal)
            {
                return literal.Version;
            }
            else if (packet is OperatorPacket op)
            {
                return op.Version + op.Subpackets.Sum(p => CountAllVersionInstances(p));
            }

            throw new ArgumentException("Unrecognized packet type");
        }

        private static string ConvertToBinary(string hex)
        {
            var builder = new StringBuilder();

            foreach (var c in hex)
            {
                builder.Append(_hexTable[c]);
            }

            return builder.ToString();
        }

        public class OperatorPacket : Packet
        {
            public char LengthTypeId { get; set; }
            public IEnumerable<Packet> Subpackets { get; set; }
            public OperatorType OperatorType => _operatorTypeTable[PacketTypeCode];

            public OperatorPacket()
            {
                Type = PacketType.Operator;
                Subpackets = new List<Packet>();
            }

            public override long GetValue()
            {
                if (PacketTypeCode == 0)
                {
                    return Subpackets.Sum(x => x.GetValue());
                }
                else if (PacketTypeCode == 1)
                {
                    var product = 1L;
                    foreach (var val in Subpackets.Select(v => v.GetValue()))
                    {
                        product *= val;
                    }
                    return product;
                }
                else if (PacketTypeCode == 2)
                {
                    return Subpackets.Min(x => x.GetValue());
                }
                else if (PacketTypeCode == 3)
                {
                    return Subpackets.Max(x => x.GetValue());
                }
                else if (PacketTypeCode == 5)
                {
                    if (Subpackets.Count() != 2)
                    {
                        throw new ArgumentException("Must have two subpackets");
                    }
                    return Subpackets.ElementAt(0).GetValue() > Subpackets.ElementAt(1).GetValue() 
                        ? 1 : 0;
                }
                else if (PacketTypeCode == 6)
                {
                    if (Subpackets.Count() != 2)
                    {
                        throw new ArgumentException("Must have two subpackets");
                    }
                    return Subpackets.ElementAt(0).GetValue() < Subpackets.ElementAt(1).GetValue()
                        ? 1 : 0;
                }
                else if (PacketTypeCode == 7)
                {
                    if (Subpackets.Count() != 2)
                    {
                        throw new ArgumentException("Must have two subpackets");
                    }
                    return Subpackets.ElementAt(0).GetValue() == Subpackets.ElementAt(1).GetValue()
                        ? 1 : 0;
                }

                throw new ArgumentException($"Unrecognized packet type = {PacketTypeCode}");
            }

            public static OperatorPacket ReadOperatorPacket(string input, ref int position)
            {
                var newPacket = new OperatorPacket();

                newPacket.Version = (int)BinaryToLong(input.Substring(position, 3));
                newPacket.PacketTypeCode = (int)BinaryToLong(input.Substring(position + 3, 3));
                position += 6;

                newPacket.LengthTypeId = input.ElementAt(position++);

                if (newPacket.LengthTypeId == '0')
                {
                    var length = (int)BinaryToLong(input.Substring(position, 15));
                    position += 15;
                    newPacket.Subpackets = ReadSubpacketsByLength(input, ref position, length);
                }
                else if (newPacket.LengthTypeId == '1')
                {
                    var numberOfSubpackets = (int)BinaryToLong(input.Substring(position, 11));
                    position += 11;
                    newPacket.Subpackets = ReadSubpacketsByCount(input, ref position, numberOfSubpackets);
                }

                return newPacket;
            }

            private static IEnumerable<Packet> ReadSubpacketsByLength(string input, ref int position, int totalLength)
            {
                var startingPosition = position;
                List<Packet> packets = new List<Packet>();

                while (position - startingPosition < totalLength)
                {
                    var newPacket = Packet.ReadPacket(input, ref position);
                    packets.Add(newPacket);
                }

                return packets;
            }

            private static IEnumerable<Packet> ReadSubpacketsByCount(string input, ref int position, int count)
            {
                List<Packet> packets = new List<Packet>();

                for (int i=0; i<count; i++)
                {
                    var newPacket = Packet.ReadPacket(input, ref position);
                    packets.Add(newPacket);
                }

                return packets;
            }
        }

        public class LiteralPacket : Packet
        {
            public long Value { get; set; }

            public LiteralPacket()
            {
                this.Type = PacketType.Literal;
            }

            public override long GetValue()
            {
                return Value;
            }

            public static LiteralPacket ReadLiteralPacket(string input, ref int position, int? totalBitCount = null)
            {
                var newPacket = new LiteralPacket();

                newPacket.Version = (int)BinaryToLong(input.Substring(position, 3));
                newPacket.PacketTypeCode = (int)BinaryToLong(input.Substring(position + 3, 3));
                position += 6;

                var builder = new StringBuilder();
                int bitsRead = 6;

                while (true)
                {
                    var nextBitGroup = input.Substring(position, 5);
                    position += 5;
                    bitsRead += 5;
                    
                    builder.Append(nextBitGroup.Substring(1, 4));

                    if (nextBitGroup[0] == '0')
                    {
                        break;
                    }
                }

                // Move past any trailing content
                if (totalBitCount.HasValue)
                {
                    position += totalBitCount.Value - bitsRead;
                }

                newPacket.Value = BinaryToLong(builder.ToString());

                return newPacket;
            }
        }

        public abstract class Packet
        {
            public PacketType Type { get; set; }
            public int PacketTypeCode { get; set; }
            public int Version { get; set; }
            public abstract long GetValue();

            public static Packet ReadPacket(string input, ref int position)
            {
                var typeCode = input.Substring(position, 6).Substring(3);
                var packetType = BinaryToLong(typeCode);

                if (packetType == 4)
                {
                    return LiteralPacket.ReadLiteralPacket(input, ref position);
                }
                else
                {
                    return OperatorPacket.ReadOperatorPacket(input, ref position);
                }

                throw new Exception($"Unrecognized type = {packetType}");
            }
        }
    }
}
