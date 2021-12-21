using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode.Day18
{
    internal class Day18 : DailyChallenge
    {
        private const int ExplodeDepth = 4;
        private const int SplitMax = 10;
        private const string Filename = "Day18/Data/puzzle_input.txt";

        public async Task Process()
        {
            var nodes = await ReadAndParse(Filename);

            var added = Add(nodes);

            Console.WriteLine(added.Format);
            Console.WriteLine($"Magnitude = {added.Magnitude}");
            Console.WriteLine($"Greatest Magnitude = {FindLargestMagnitude(nodes)}");
        }

        private long FindLargestMagnitude(IEnumerable<TreeNode> nodeSet)
        {
            long greatestMagnitude = 0;
            for (int first = 0; first < nodeSet.Count()-1; first++)
            {
                for (int second = first+1; second < nodeSet.Count(); second++)
                {
                    var added = Add(nodeSet.ElementAt(first).Copy(), nodeSet.ElementAt(second).Copy());
                    var magnitude = added.Magnitude;

                    if (magnitude > greatestMagnitude)
                    {
                        greatestMagnitude = magnitude;
                    }
                }
            }

            return greatestMagnitude;
        }

        private TreeNode Add(IEnumerable<TreeNode> nodes)
        {
            var addedNode = nodes.First();

            for (int i=1; i<nodes.Count(); i++)
            {
                addedNode = Add(addedNode, nodes.ElementAt(i));
            }

            return addedNode;
        }

        private async Task<IEnumerable<TreeNode>> ReadAndParse(string filename)
        {
            var lines = await File.ReadAllLinesAsync(filename);

            return lines.Select(Parse);
        }

        private TreeNode Parse(string stringDefinition)
        {
            var root = new TreeNode(null);

            int index = 0;
            Parse(stringDefinition, ref index, root);

            return root;
        }

        private void Parse(string stringDefinition, ref int index, TreeNode currentNode)
        {
            if (char.IsDigit(stringDefinition.ElementAt(index)))
            {
                currentNode.Value = ParseValue(stringDefinition, ref index);
                return;
            }
            else if (stringDefinition.ElementAt(index) == '[')
            {
                var left = new TreeNode(currentNode);
                index++;
                Parse(stringDefinition, ref index, left);
                if (stringDefinition.ElementAt(index) != ',')
                {
                    throw new Exception($"Expected comma but found {stringDefinition.ElementAt(index)}");
                }
                index++;
                var right = new TreeNode(currentNode);
                Parse(stringDefinition, ref index, right);
                if (stringDefinition.ElementAt(index) != ']')
                {
                    throw new Exception($"Expected closing bracket but found {stringDefinition.ElementAt(index)}");
                }
                index++;
                currentNode.Left = left;
                currentNode.Right = right;
            }
            else
            {
                throw new Exception($"Expected opening bracket but found {stringDefinition.ElementAt(index)}");
            }
        }

        private int ParseValue(string stringDefinition, ref int index)
        {
            var builder = new StringBuilder();
            
            while (index < stringDefinition.Length && char.IsDigit(stringDefinition[index]))
            {
                builder.Append(stringDefinition[index]);
                index++;
            }

            return int.Parse(builder.ToString());
        }

        private TreeNode Add(TreeNode first, TreeNode second)
        {
            var addedNode = new TreeNode(null)
            {
                Left = first,
                Right = second
            };

            addedNode.Left.Parent = addedNode;
            addedNode.Right.Parent = addedNode;

            Reduce(addedNode);

            return addedNode;
        }

        private void Reduce(TreeNode node)
        {
            while (SingleReduceStep(node))
            {
                // do nothing
            }
        }

        private bool SingleReduceStep(TreeNode node)
        {
            var explodee = FindExplodee(0, node, ExplodeDepth);
            if (explodee != null)
            {
                Explode(explodee);
                return true;
            }
            var splitee = FindSplitee(node, SplitMax);
            if (splitee != null)
            {
                Split(splitee);
                return true;
            }

            return false;
        }

        private void Explode(TreeNode node)
        {
            var nextLeft = FindNextLeftNode(node);
            var nextRight = FindNextRightNode(node);

            if (nextLeft != null)
            {
                nextLeft.Value += node.Left.Value;
            }
            
            if (nextRight != null)
            {
                nextRight.Value += node.Right.Value;
            }

            node.Value = 0;
            node.Left = null;
            node.Right = null;
        }

        private void Split(TreeNode node)
        {
            var left = new TreeNode(node)
            {
                Value = (int)Math.Floor(node.Value / 2.0)
            };

            var right = new TreeNode(node)
            {
                Value = (int)Math.Ceiling(node.Value / 2.0)
            };

            node.Left = left;
            node.Right = right;
            node.Value = -1;
        }

        private TreeNode? FindNextLeftNode(TreeNode node)
        {
            TreeNode nodeWithLeftTree = node;

            while (nodeWithLeftTree != null)
            {
                if (nodeWithLeftTree.Parent == null)
                {
                    return null;
                }
                if (nodeWithLeftTree.Parent.Right == nodeWithLeftTree)
                {
                    break;
                }
                nodeWithLeftTree = nodeWithLeftTree.Parent;
            }

            var nextRight = nodeWithLeftTree.Parent.Left;
            while (!nextRight.IsLeaf)
            {
                nextRight = nextRight.Right;
            }

            return nextRight;
        }

        private TreeNode? FindNextRightNode(TreeNode node)
        {
            TreeNode nodeWithRightTree = node;

            while (nodeWithRightTree != null)
            {
                if (nodeWithRightTree.Parent == null)
                {
                    return null;
                }
                if (nodeWithRightTree.Parent.Left == nodeWithRightTree)
                {
                    break;
                }
                nodeWithRightTree = nodeWithRightTree.Parent;
            }

            var nextLeft = nodeWithRightTree.Parent.Right;
            while (!nextLeft.IsLeaf)
            {
                nextLeft = nextLeft.Left;
            }

            return nextLeft;
        }

        private TreeNode? FindExplodee(int currentDepth, TreeNode node, int explodeDepth)
        {
            if (node == null)
            {
                return null;
            }

            if (currentDepth >= explodeDepth)
            {
                return node.IsPair ? node : null;
            }

            var left = FindExplodee(currentDepth + 1, node.Left, explodeDepth);
            if (left != null)
            {
                return left;
            }

            return FindExplodee(currentDepth + 1, node.Right, explodeDepth);
        }

        private TreeNode? FindSplitee(TreeNode node, int maxValue)
        {
            if (node.IsLeaf)
            {
                return node.Value >= maxValue ? node : null;
            }

            var left = FindSplitee(node.Left, maxValue);
            if (left != null)
            {
                return left;
            }

            return FindSplitee(node.Right, maxValue);
        }

        private class TreeNode
        {
            public int Value { get; set; }
            public TreeNode Parent { get; set; }
            public TreeNode Left { get; set; }
            public TreeNode Right { get; set; }

            public TreeNode(TreeNode parent)
            {
                Parent = parent;
            }

            public TreeNode Copy()
            {
                return CopyNode(this);
            }

            public TreeNode CopyNode(TreeNode node)
            {
                if (node == null)
                {
                    return null;
                }

                var newNode = new TreeNode(null)
                {
                    Value = node.Value,
                    Left = CopyNode(node.Left),
                    Right = CopyNode(node.Right)
                };

                var left = CopyNode(node.Left);
                var right = CopyNode(node.Right);

                if (newNode.Left != null)
                {
                    newNode.Left.Parent = newNode;
                }
                if (newNode.Right != null)
                {
                    newNode.Right.Parent = newNode;
                }

                return newNode;
            }

            public string Format
            {
                get
                {
                    var builder = new StringBuilder();
                    FormatBuilder(builder);
                    return builder.ToString();
                }
            }

            private void FormatBuilder(StringBuilder builder)
            {
                if (IsLeaf)
                {
                    builder.Append(Value);
                }
                else
                {
                    builder.Append("[");
                    Left.FormatBuilder(builder);
                    builder.Append(",");
                    Right.FormatBuilder(builder);
                    builder.Append("]");
                }
            }
            
            public long Magnitude
            {
                get
                {
                    if (IsLeaf)
                    {
                        return Value;
                    }

                    return 3 * Left.Magnitude + 2 * Right.Magnitude;
                }
            }

            public bool IsPair
            {
                get
                {
                    // Right will also be non-null here
                    return Left != null;
                }
            }

            public bool IsLeaf
            {
                get
                {
                    // Right will also be non-null here
                    return !IsPair;
                }
            }
        }
    }
}
