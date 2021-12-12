using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdventOfCode.Day12
{
    internal class Day12 : DailyChallenge
    {
        private const string Filename = "Day12/Data/day_12_input.txt";
        private const int SmallCaveMaxVisits = 2;

        public async Task Process()
        {
            var graph = await ReadAndParse(Filename);

            var routes = FindRoutes(graph, "start", "end");

            var printedRoutes = routes.Select(FormatRoute);

            Console.WriteLine($"Routes count = {printedRoutes.Count()}");
        }

        private async Task<Graph> ReadAndParse(string filename)
        {
            Graph graph = new Graph();

            var lines = await File.ReadAllLinesAsync(filename);

            foreach (var line in lines)
            {
                var split = line.Split('-');
                graph.AddEdge(split[0], split[1]);
            }

            return graph;
        }

        public IEnumerable<IEnumerable<GraphNode>> FindRoutes(Graph g, string start, string end)
        {
            var nodesVisited = new Dictionary<string, int>();
            var edgesVisited = new HashSet<string>();

            List<IEnumerable<GraphNode>> routes = new List<IEnumerable<GraphNode>>();
            Stack<GraphNode> currentRoute = new Stack<GraphNode>();
            AddVisit(start, nodesVisited);
            currentRoute.Push(g.Nodes[start]);
            FindRoutes(g.Nodes[start], g, start, end, nodesVisited, edgesVisited, currentRoute, routes);

            return routes;
        }

        private void FindRoutes(
            GraphNode currentNode, 
            Graph g,
            string start, 
            string end, 
            Dictionary<string, int> nodesVisited, 
            HashSet<string> edgesVisited, 
            Stack<GraphNode> currentRoute,
            List<IEnumerable<GraphNode>> routes
        )
        {
            if (currentNode.Label == end)
            {
                routes.Add(Reverse(currentRoute.Select(n => n).ToList()));
                return;
            }

            var connectedNodes = currentNode.Edges;

            foreach (var connectedNodeLabel in currentNode.Edges)
            {
                var nextNode = g.Nodes[connectedNodeLabel];
                var edgeKey = KeyifyEdge(currentNode.Label, nextNode.Label);

                if (ShouldExplore(g, start, end, connectedNodeLabel, nodesVisited, edgesVisited, edgeKey))
                {
                    AddVisit(nextNode.Label, nodesVisited);
                    //edgesVisited.Add(KeyifyEdge(currentNode.Label, nextNode.Label));
                    currentRoute.Push(nextNode);

                    FindRoutes(nextNode, g, start, end, nodesVisited, edgesVisited, currentRoute, routes);

                    currentRoute.Pop();
                    //edgesVisited.Remove(edgeKey);
                    nodesVisited[nextNode.Label] = nodesVisited[nextNode.Label] - 1;
                }
            }
        }

        private string FormatRoute(IEnumerable<GraphNode> route)
        {
            return String.Join(" ", route.Select(n => n.Label));
        }

        private bool ShouldExplore(
            Graph g,
            string start,
            string end,
            string nextNodeLabel, 
            Dictionary<string, int> nodesVisited, 
            HashSet<string> edgesVisited,
            string nextEdgeKey
        )
        {
            if (nextNodeLabel == start)
            {
                return false;
            }

            if (IsBigCave(nextNodeLabel))
            {
                return true;
            }
            else
            {
                bool hasDoubleVisit = nodesVisited.Any(pair => !IsBigCave(pair.Key) && pair.Value >= 2);
                if (!hasDoubleVisit)
                {
                    return true;
                }
                else
                {
                    return GetVisits(nextNodeLabel, nodesVisited) == 0;
                }
            }
        }

        private IEnumerable<T> Reverse<T>(IEnumerable<T> source)
        {
            Stack<T> ts = new Stack<T>();
            List<T> newList = new List<T>();

            foreach (var t in source)
            {
                ts.Push(t);
            }

            while (ts.Any())
            {
                newList.Add(ts.Pop());
            }

            return newList;
        }

        private void AddVisit(string nodeLabel, Dictionary<string, int> nodesVisited)
        {
            if (!nodesVisited.ContainsKey(nodeLabel))
            {
                nodesVisited.Add(nodeLabel, 0);
            }

            nodesVisited[nodeLabel] = nodesVisited[nodeLabel] + 1;
        }

        private int GetVisits(string nodeLabel, Dictionary<string, int> nodesVisited)
        {
            if (!nodesVisited.ContainsKey(nodeLabel))
            {
                nodesVisited.Add(nodeLabel, 0);
            }

            return nodesVisited[nodeLabel];
        }

        private bool IsBigCave(string label) => Char.IsUpper(label[0]);

        private string KeyifyEdge(string start, string end) => $"{start}_{end}";
        
        public class Graph
        {
            public Dictionary<string, GraphNode> Nodes { get; }

            public Graph()
            {
                Nodes = new Dictionary<string, GraphNode>();
            }

            // Inserts nodes if necessary
            public void AddEdge(string first, string second)
            {
                var firstNode = Get(first);
                var secondNode = Get(second);

                firstNode.Edges.Add(second);
                secondNode.Edges.Add(first);
            }

            private GraphNode Get(string label)
            {
                if (!Nodes.ContainsKey(label))
                {
                    Nodes.Add(label, new GraphNode(label));
                }

                return Nodes[label];
            }
        }

        public class GraphNode
        {
            public string Label { get; }

            public HashSet<string> Edges { get; }

            public GraphNode(string label)
            {
                Label = label;
                Edges = new HashSet<string>();
            }
        }
    }
}
