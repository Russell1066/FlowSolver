using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SolverCore
{
    public class SolverBase
    {
        protected FlowBoard Game { get; set; }
        protected CancellationToken Token { get; set; }

        internal class Node : IComparable
        {
            public Cell.Color Color;
            public int Index;
            public int DestinationIndex;
            public List<int> Path = new List<int>();
            public List<int> Moves = new List<int>();

            public Node() { }

            public Node(Node rhs)
            {
                Color = rhs.Color;
                Index = rhs.Index;
                DestinationIndex = rhs.DestinationIndex;
                Path = rhs.Path.ToList();
                Moves = rhs.Moves.ToList();
            }

            public int CompareTo(object obj)
            {
                if (obj == null)
                {
                    return 1;
                }
                var rhs = obj as Node;

                int retv = 0;

                if(retv == 0)
                {
                    var nextLhs = Moves.Contains(DestinationIndex) ? 1 : 0;
                    var nextRhs = rhs.Moves.Contains(rhs.DestinationIndex) ? 1 : 0;
                    retv = nextLhs - nextRhs;
                }

                if (retv == 0)
                {
                    retv = Moves.Count - rhs.Moves.Count;
                }

                if (retv == 0)
                {
                    retv = rhs.Path.Count - Path.Count;
                }

                if (retv == 0)
                {
                    retv = Index - rhs.Index;
                }

                return retv;
            }
        }

        internal static void CopySolutionToBoard(List<Cell> dest, List<Cell> source)
        {

            var endpoints = from cell in source
                            where cell.Parent == null
                            select cell;
            Parallel.ForEach(endpoints, (endpoint) =>
            {

                for (; endpoint.Child != null; endpoint = endpoint.Child)
                {
                    dest[endpoint.Child.Index].Parent = dest[endpoint.Index];
                    //Task.Delay(100).Wait();
                }
            });
        }

        internal static void SetNodeToSingleMove(List<int> moves, int move)
        {
            moves.Clear();
            moves.Add(move);
        }

        internal bool CanVisitAllCells(List<Node> nodes)
        {
            var cells = (from cell in Game.Cells
                         where cell.CellColor == Cell.Color.Empty
                         select cell.Index).ToList();

            List<int> foundCells = new List<int>();
            foreach (var cell in cells)
            {
                if (foundCells.Contains(cell))
                {
                    continue;
                }

                bool found = false;
                foreach (var n in nodes)
                {
                    int endPoint = n.Path.Last();
                    var color = n.Color;
                    var path = BreadthFirstSearch.Search(cell, endPoint, (int index) =>
                    {
                        var p = from c in Game.GetAdjacentCellIndicies(index, color)
                                where Game.Cells[c].CellColor != n.Color || c == endPoint || n.Moves.Contains(c)
                                select c;
                        return p.ToList();
                    });

                    if (path.Count() > 0)
                    {
                        found = true;
                        foundCells.AddRange(path);
                        break;
                    }
                }

                if (!found)
                {
                    return false;
                }
            }

            return true;
        }

        internal static List<int> GetMovesShortestFirst(Node node, List<int> nodePath)
        {
            if (node.Moves.Count == 1)
            {
                return node.Moves;
            }

            List<int> moves = new List<int>() { nodePath[1] };
            moves.AddRange(from move in node.Moves
                           where move != nodePath[1]
                           select move);

            return moves;
        }

        internal bool PathsExist(List<Node> nodes)
        {
            foreach (var node in nodes)
            {
                List<int> path = GetNodePath(node);

                if (path.Count == 0)
                {
                    return false;
                }
            }

            return true;
        }

        internal List<int> GetNodePath(Node node)
        {
            int pt1 = node.Path.Last();
            int pt2 = node.DestinationIndex;
            var color = node.Color;

            var path = BreadthFirstSearch.Search(pt1, pt2, (int index) =>
            {
                var p = from c in Game.GetAdjacentCellIndicies(index, color)
                        where Game.Cells[c].CellColor != color || c == pt2
                        select c;
                return p.ToList();
            });
            return path;
        }

        internal static List<Node> CopyNodes(List<Node> nodes)
        {
            return (from node in nodes
                    select new Node(node)).ToList();
        }

        internal void PopBoard(List<Cell.CellState> pushBoard)
        {
            for (int i = 0; i < pushBoard.Count; ++i)
            {
                Game.Cells[i].SetState(pushBoard[i]);
            }
        }

        internal List<Cell.CellState> PushBoard()
        {
            var pushBoard = new List<Cell.CellState>();
            foreach (var cell in Game.Cells)
            {
                pushBoard.Add(cell.GetCellState());
            }

            return pushBoard;
        }

        internal bool IsGameWon(List<Node> nodes)
        {
            bool allNodesDone = nodes.Count == 0;
            bool spaceFilled = (from cell in Game.Cells
                                where cell.CellColor == Cell.Color.Empty
                                select cell).Count() == 0;

            return allNodesDone && spaceFilled;
        }

        internal void SearchForcedPaths(List<Node> nodes)
        {
            nodes.Sort();

            while (nodes.Count > 0 && nodes[0].Moves.Count() == 1)
            {
                var node = nodes[0];
                var move = node.Moves[0];

                Game.Cells[move].Parent = Game.Cells[node.Path.Last()];
                node.Path.Add(move);

                ClearMove(nodes, node, move);

                if (move == node.DestinationIndex)
                {
                    nodes.Remove(node);
                }

                UpdateMoveList(node, move);
                nodes.Sort();
            }
        }

        internal static void ClearMove(List<Node> nodes, Node node, int move)
        {
            foreach (var n in nodes)
            {
                n.Moves.Remove(move);
            }
        }

        internal void UpdateMoveList(Node node, int move)
        {
            node.Moves = Game.GetAdjacentCellIndicies(move, node.Color);
            foreach (var n in node.Path)
            {
                node.Moves.Remove(n);
            }
        }

        internal List<Node> InitializeNodes()
        {
            List<Node> nodes = new List<Node>();
            foreach (var element in Game.Puzzle)
            {
                int pt1 = Game.PointToIndex(element.Pt1);
                int pt2 = Game.PointToIndex(element.Pt2);
                if (Game.GetAdjacentCellIndicies(pt1, element.FlowColor).Count() >
                    Game.GetAdjacentCellIndicies(pt2, element.FlowColor).Count())
                {
                    var temp = pt1;
                    pt1 = pt2;
                    pt2 = temp;
                }

                nodes.Add(CreateNode(element, pt1, pt2));
            }

            nodes.Sort();
            return nodes;
        }

        internal Node CreateNode(FlowBoard.Endpoints element, int pt1, int pt2)
        {
            return new Node()
            {
                Color = element.FlowColor,
                Index = pt1,
                DestinationIndex = pt2,
                Moves = Game.GetAdjacentCellIndicies(pt1, element.FlowColor),
                Path = new List<int>() { pt1 },
            };
        }
    }
}
