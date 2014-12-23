//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using System.Globalization;
//using System.Linq;
//using AiPathFinding.Model;
//using AiPathFinding.View;

//namespace AiPathFinding.Algorithm
//{
//    /// <summary>
//    /// Implementation of the Dijkstra-Algorithm.
//    /// </summary>
//    public sealed class DijkstraAlgorithm : AbstractPathFindAlgorithm
//    {
//        #region Fields

//        /// <summary>
//        /// Contains all visited nodes.
//        /// </summary>
//        private readonly List<Node> _visitedNodes = new List<Node>();
        
//        /// <summary>
//        /// Contains all unvisited nodes.
//        /// </summary>
//        private readonly List<Node> _unvisitedNodes = new List<Node>(); 

//        /// <summary>
//        /// Assigns a flag that indicates whether the node was visited and a cost to a node.
//        /// </summary>
//        private readonly Dictionary<Node, Tuple<bool, int>> _nodeDataMap = new Dictionary<Node, Tuple<bool, int>>();

//        #endregion

//        #region Constructor

//        public DijkstraAlgorithm() : base(PathFindName.Dijkstra)
//        {
//        }

//        #endregion

//        #region Main Methods

//        public override void ResetAlgorithm()
//        {
//            base.ResetAlgorithm();

//            _visitedNodes.Clear();
//            _unvisitedNodes.Clear();
//            _nodeDataMap.Clear();
//        }

//        protected override void PrepareData(Node playerNode, Node targetNode)
//        {
//            // add all nodes to the unvisited-node-list and tag nodes as unvisited with max cost
//            foreach (var n in Graph.Nodes.Where(n => n != null).SelectMany(nn => nn.Where(n => n != null)))
//                _nodeDataMap.Add(n, new Tuple<bool, int>(false, n == playerNode ? 0 : int.MaxValue));

//            // start with the first node
//            _unvisitedNodes.Add(playerNode);
//        }

//        protected override void FindShortestPath(Node playerNode, Node targetNode)
//        {
//            // loop while we have options an at least one of the options and we have not yet found a way
//            while (_unvisitedNodes.Count > 0 && _nodeDataMap[targetNode].Item2 == int.MaxValue)
//            {
//                // apply the algorithm to do the actual pathfinding
//                var currentNode = _unvisitedNodes[0];
//                ProcessNode(currentNode);

//                Steps.Add(GetAlgorithmStep(playerNode, currentNode));
//            }
//        }

//        protected override void FindAlternativePaths(Node playerNode, Node targetNode)
//        {
//            // look for other paths with same cost
//            // remove all costlier nodes playerNode the unvisited list
//            var stillOpen =
//                _unvisitedNodes.Where(n => n != targetNode && _nodeDataMap[n].Item2 < _nodeDataMap[targetNode].Item2).ToArray();
//            _unvisitedNodes.Clear();
//            _unvisitedNodes.AddRange(stillOpen);

//            // check all remaining nodes
//            for (var i = 0; i < _unvisitedNodes.Count; i++)
//            {
//                if (_unvisitedNodes[i] == targetNode || _nodeDataMap[_unvisitedNodes[i]].Item2 >= _nodeDataMap[targetNode].Item2)
//                    continue;
//                // check node
//                var currentNode = _unvisitedNodes[i];
//                i--;

//                ProcessNode(currentNode);

//                Steps.Add(GetAlgorithmStep(playerNode, currentNode));
//            }

//            Steps.Add(GetAlternativesStep(playerNode, targetNode));
//        }

//        /// <summary>
//        /// Gets a step in the algorithm, mostly to draw progress on map.
//        /// </summary>
//        /// <param name="from">Node to start playerNode</param>
//        /// <param name="currentNode">Node to which the path is being tried</param>
//        /// <returns>Step of the algorithm</returns>
//        private AlgorithmStep GetAlgorithmStep(Node from, Node currentNode)
//        {
//            // prepare data for printing cost
//            var costData = _nodeDataMap.Keys.Where(k => _nodeDataMap[k].Item2 != int.MaxValue).Select(n => new Tuple<string, Point, Brush, Font>(_nodeDataMap[n].Item2.ToString(CultureInfo.InvariantCulture), n.Location, n == currentNode ? Brushes.DarkRed : Brushes.Turquoise, new Font("Microsoft Sans Serif", _nodeDataMap[n].Item1 ? 10 : 14, _nodeDataMap[n].Item1 ? FontStyle.Regular : FontStyle.Bold))).ToList();

//            // prepare data for printing path
//            var pathData = new List<Tuple<Point, Point>>();
//            var node = currentNode;
//            while (node != from)
//            {
//                // find neighbor where you would "come playerNode"
//                Node[] minNode = {node.Edges.First(n => n != null).GetOtherNode(node)};
//                var node1 = node;
//                foreach (
//                    var e in
//                        node.Edges.Where(n => n != null)
//                            .Where(e => _nodeDataMap[e.GetOtherNode(node1)].Item2 < _nodeDataMap[minNode[0]].Item2))
//                    minNode[0] = e.GetOtherNode(node);

//                // add to list
//                var p1 = MainForm.MapPointToCanvasRectangle(node.Location);
//                var p2 = MainForm.MapPointToCanvasRectangle(minNode[0].Location);
//                pathData.Add(new Tuple<Point, Point>(new Point(p1.X + p1.Width/2, p1.Y + p1.Height/2),
//                    new Point(p2.X + p2.Width/2, p2.Y + p2.Height/2)));

//                node = minNode[0];
//            }

//            // create new step
//            var newStep = new AlgorithmStep(g =>
//            {
//                // draw cost of nodes
//                foreach (var d in costData)
//                    g.DrawString(d.Item1.ToString(CultureInfo.InvariantCulture), d.Item4, d.Item3, MainForm.MapPointToCanvasRectangle(d.Item2));

//                // draw path
//                foreach (var d in pathData)
//                    g.DrawLine(new Pen(Color.Yellow, 3), d.Item1, d.Item2);
//            }, _visitedNodes.Count, Graph.PassibleNodeCount);

//            return newStep;
//        }

//        /// <summary>
//        /// Finds all alternative paths to the target based on current information.
//        /// </summary>
//        /// <param name="from">Start node</param>
//        /// <param name="to">Target node</param>
//        /// <returns>AlgorithmStep with all alternative paths</returns>
//        private AlgorithmStep GetAlternativesStep(Node from, Node to)
//        {
//            // prepare data for drawing
//            var costData = _nodeDataMap.Keys.Where(k => _nodeDataMap[k].Item2 != int.MaxValue).Select(n => new Tuple<string, Point, Brush, Font>(_nodeDataMap[n].Item2.ToString(CultureInfo.InvariantCulture), n.Location, Brushes.Turquoise, new Font("Microsoft Sans Serif", _nodeDataMap[n].Item1 ? 10 : 14, _nodeDataMap[n].Item1 ? FontStyle.Regular : FontStyle.Bold))).ToList();

//            // prepare data for path
//            var pathData = new List<Tuple<Point, Point, Pen>>();
//            var openPaths = new List<List<Node>> {new List<Node> {to}};
//            var closedPaths = new List<List<Node>>();
//            do
//            {
//                for (var i = 0; i < openPaths.Count; i++)
//                {
//                    // find min cost
//                    var min = openPaths[i].Last().Edges.Where(n => n != null).Select(e => _nodeDataMap[e.GetOtherNode(openPaths[i].Last())].Item2).Concat(new[] {int.MaxValue}).Min();

//                    // find all neighbor (edges) with min cost
//                    var cheapestEdges =
//                        openPaths[i].Last()
//                            .Edges.Where(
//                                e => e != null && _nodeDataMap[e.GetOtherNode(openPaths[i].Last())].Item2 == min)
//                            .ToArray();

//                    // if only one exists, continue the current path
//                    if (cheapestEdges.Length == 1)
//                        openPaths[i].Add(cheapestEdges[0].GetOtherNode(openPaths[i].Last()));
//                    else
//                    {
//                        // copy current path to end of openpaths and add the different nodes
//                        for (var j = 1; j < cheapestEdges.Length; j++)
//                        {
//                            var newList = new Node[openPaths[i].Count];
//                            openPaths[i].CopyTo(newList);

//                            if (cheapestEdges[j].GetOtherNode(openPaths[i].Last()) == from)
//                            {
//                                closedPaths.Add(newList.ToList());
//                                closedPaths.Last().Add(cheapestEdges[j].GetOtherNode(openPaths[i].Last()));
//                            }
//                            else
//                            {
//                                openPaths.Add(newList.ToList());
//                                openPaths.Last().Add(cheapestEdges[j].GetOtherNode(openPaths[i].Last()));
//                            }
//                        }
//                        openPaths[i].Add(cheapestEdges[0].GetOtherNode(openPaths[i].Last()));
//                    }

//                    // move completed (main) path to other list if it reaches the starting point
//                    if (openPaths[i].Last() != from) continue;
//                    closedPaths.Add(openPaths[i]);
//                    openPaths.RemoveAt(i);
//                    i--;
//                }
//            } while (openPaths.Count > 0);

//            // add path to list data
//            var minPath = closedPaths.Select(p => p.Count).Concat(new[] { int.MaxValue }).Min();
//            var maxPath = closedPaths.Select(p => p.Count).Concat(new[] { int.MinValue }).Max();
//            foreach (var path in closedPaths)
//                for (var i = 1; i < path.Count; i++)
//                {
//                    var p1 = MainForm.MapPointToCanvasRectangle(path[i - 1].Location);
//                    var p2 = MainForm.MapPointToCanvasRectangle(path[i].Location);
//                    var offset = 2 + 4 * closedPaths.IndexOf(path);
//                    var perc = maxPath == minPath ? 1 : (double)(path.Count - minPath) / (maxPath - minPath);
//                    var color = Color.FromArgb(255, (int)(perc * 255), (int)((1 - perc) * 255), 0);
//                    pathData.Add(
//                        new Tuple<Point, Point, Pen>(
//                            new Point(p1.X + offset, p1.Y + offset),
//                            new Point(p2.X + offset, p2.Y + offset), new Pen(color, 2)));
//                }

//            Console.WriteLine("* Found " + closedPaths.Count() + " distinct paths with cost of " + _nodeDataMap[to].Item2 + ", ranging playerNode " + minPath + " to " + maxPath + " cells long!");

//            // create new step
//            var newStep = new AlgorithmStep(g =>
//            {
//                // draw cost of nodes
//                foreach (var d in costData)
//                    g.DrawString(d.Item1.ToString(CultureInfo.InvariantCulture), d.Item4, d.Item3, MainForm.MapPointToCanvasRectangle(d.Item2));

//                // draw paths
//                foreach (var d in pathData)
//                    g.DrawLine(d.Item3, d.Item1, d.Item2);
//            }, _visitedNodes.Count, Graph.PassibleNodeCount);

//            return newStep;
//        }

//        /// <summary>
//        /// Processes one particular node and does algorithm stuff on it.
//        /// </summary>
//        /// <param name="node">Node that is processed</param>
//        private void ProcessNode(Node node)
//        {
//            // update distance to all neighbors (if shorter)
//            foreach (var e in node.Edges.Where(ee => ee != null && ee.GetOtherNode(node).Cost != int.MaxValue))
//            {
//                // get other node
//                var otherNode = e.GetOtherNode(node);

//                // insert node if it's not yet inserted (this way is cheaper to test that)
//                var insert = _nodeDataMap[otherNode].Item2 == int.MaxValue;

//                // if the current way is cheaper, update data
//                if (_nodeDataMap[otherNode].Item2 > _nodeDataMap[node].Item2 + otherNode.Cost)
//                    _nodeDataMap[otherNode] = new Tuple<bool, int>(_nodeDataMap[otherNode].Item1, _nodeDataMap[node].Item2 + otherNode.Cost);

//                if (!insert) continue;

//                // insert node in proper place
//                if (_unvisitedNodes.Count == 0)
//                    _unvisitedNodes.Add(otherNode);
//                for (var i = 0; i < _unvisitedNodes.Count; i++)
//                    if (_nodeDataMap[_unvisitedNodes[i]].Item2 >= _nodeDataMap[otherNode].Item2)
//                    {
//                        _unvisitedNodes.Insert(i, otherNode);
//                        insert = false;
//                        break;
//                    }

//                // couldn't insert, our node must be the most expensive one
//                if (insert)
//                    _unvisitedNodes.Add(otherNode);
//            }

//            // mark current node as visited and move it to the other list
//            _nodeDataMap[node] = new Tuple<bool, int>(true, _nodeDataMap[node].Item2);
//            _visitedNodes.Add(node);
//            _unvisitedNodes.Remove(node);
//        }

//        #endregion
//    }
//}
