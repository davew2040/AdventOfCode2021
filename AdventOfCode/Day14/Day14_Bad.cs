using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode.Day14
{
    internal class Day14_Bad : DailyChallenge
    {
        private const string Filename = "Day14/Data/problem_input.txt";
        private const int Steps = 40;

        public async Task Process()
        {
            var input = await ReadAndParse(Filename);

            var tracker = new Tracker(input.List);

            var leastCommon = tracker.TrackingMap.Min(m => m.Value.Count());
            var mostCommon = tracker.TrackingMap.Max(m => m.Value.Count());

            //Console.WriteLine(FormatList(input.List));

            Console.WriteLine($"least = {leastCommon}");
            Console.WriteLine($"most = {mostCommon}");

            Console.WriteLine($"most - least = {mostCommon-leastCommon}");
        } 

        private string FormatList<T>(DoublyLinkedList<T> list)
        {
            StringBuilder sb = new StringBuilder();

            var node = list.head;

            while (node != null)
            {
                sb.Append(node.value + " ");
                node = node.next;
            }

            return sb.ToString();
        }

        private void ProcessRules(IEnumerable<InsertionRule> rules, DoublyLinkedList<char> list, Tracker tracker)
        {
            var rulesToApply = GetDelayedRulesToApply(rules, list, tracker);

            foreach (var rule in rulesToApply)
            {
                ApplyRule(rule, tracker);
            }
        }

        private List<DelayedRule> GetDelayedRulesToApply(IEnumerable<InsertionRule> rules, DoublyLinkedList<char> list, Tracker tracker)
        {
            var delayedRules = new List<DelayedRule>();

            foreach (var rule in rules)
            {
                if (!tracker.TrackingMap.ContainsKey(rule.From[0]))
                {
                    continue;
                }
                var startNodes = tracker.TrackingMap[rule.From[0]].ToList();

                foreach (var node in startNodes)
                {
                    if (node.value == rule.From[0] && node.next?.value == rule.From[1])
                    {
                        delayedRules.Add(
                            new DelayedRule()
                            {
                                Node = node,
                                Rule = rule
                            }
                        );
                    }
                }
            }

            return delayedRules;
        }

        private void ApplyRule(DelayedRule delayedRule, Tracker tracker)
        {
            var node = delayedRule.Node;
            var newNode = new DoublyLinkedListNode<char>(delayedRule.Rule.To);
            var far = node.next;
            node.next = newNode;
            far.last = newNode;
            newNode.next = far;
            newNode.last = node;
            tracker.Add(newNode.value, newNode);
        }

        private async Task<Input> ReadAndParse(string filename)
        {
            var lines = await File.ReadAllLinesAsync(filename);
            var input = new Input();
            var rules = new List<InsertionRule>();

            foreach (var line in lines)
            {
                if (line.Contains("-"))
                {
                    var newRule = new InsertionRule(
                        new char[] { line[0], line[1] },
                        line[line.Length - 1]
                    );

                    rules.Add(newRule);
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    input.List = GetListFromString(line);
                }
            }

            input.InsertionRules = rules;

            return input;
        }

        private DoublyLinkedList<char> GetListFromString(string s)
        {
            var newList = new DoublyLinkedList<char>();

            foreach (char c in s)
            {
                newList.InsertTail(c);
            }

            return newList;
        }

        private class Input
        {
            public DoublyLinkedList<char> List { get; set; }
            public IEnumerable<InsertionRule> InsertionRules { get; set; }
        }

        private class DelayedRule
        {
            public InsertionRule Rule {get;set;}
            public DoublyLinkedListNode<char> Node { get; set; }
        }

        private class InsertionRule
        {
            public char[] From { get; set; }
            public char To { get; set; }

            public InsertionRule(char[] from, char to)
            {
                From = from;
                To = to;
            }
        }

        private class DoublyLinkedList<T>
        {
            public DoublyLinkedListNode<T> head;
            public DoublyLinkedListNode<T> tail;

            public DoublyLinkedList<T> Copy()
            {
                var newList = new DoublyLinkedList<T>();

                var node = head;
                while (node != null)
                {
                    newList.InsertTail(node.value);
                    node = node.next;
                }

                return newList;
            }

            public void InsertHead(T value)
            {
                if (head == null)
                {
                    head = new DoublyLinkedListNode<T>(value);
                    tail = head;
                }
                else
                {
                    var prevHead = head;
                    head = new DoublyLinkedListNode<T>(value);
                    head.next = prevHead;
                    prevHead.last = head;
                }
            }

            public void InsertTail(T value)
            {
                if (tail == null)
                {
                    head = new DoublyLinkedListNode<T>(value);
                    tail = head;
                }
                else
                {
                    var prevTail = tail;
                    tail = new DoublyLinkedListNode<T>(value);
                    tail.last = prevTail;
                    prevTail.next = tail;
                }
            }
        }

        private class DoublyLinkedListNode<T>
        {
            public T value;
            public DoublyLinkedListNode<T> next;
            public DoublyLinkedListNode<T> last;

            public DoublyLinkedListNode(T value)
            {
                this.value = value;
            }
        }

        private class Tracker
        {
            public Dictionary<char, HashSet<DoublyLinkedListNode<char>>> TrackingMap
                = new Dictionary<char, HashSet<DoublyLinkedListNode<char>>>();

            public Tracker(DoublyLinkedList<char> initial)
            {
                TrackingMap = new Dictionary<char, HashSet<DoublyLinkedListNode<char>>>();

                var node = initial.head;

                while (node != null)
                {
                    Add(node.value, node);
                    node = node.next;
                }
            }

            public void Add(char c, DoublyLinkedListNode<char> node)
            {
                if (!TrackingMap.ContainsKey(c))
                {
                    TrackingMap[c] = new HashSet<DoublyLinkedListNode<char>>();
                }

                TrackingMap[c].Add(node);
            }
        }
    }
}
