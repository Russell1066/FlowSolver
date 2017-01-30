using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FlowSolver
{
    public class Solver
    {
        FlowBoard game;
        bool VisualDelay = false;

        class Path
        {
            public List<int> parents = new List<int>();
        }

        class Node : IComparable
        {
            public Cell.Color Color;
            public int Index;
            public int DestinationIndex;
            public List<int> Path = new List<int>();
            public List<int> Moves = new List<int>();

            public int CompareTo(object obj)
            {
                var rhs = obj as Node;

                int retv = Moves.Count - rhs.Moves.Count;
                if (retv == 0)
                {
                    retv = (int)Color - (int)(rhs.Color);
                }

                if (retv == 0)
                {
                    retv = Index - rhs.Index;
                }

                return retv;
            }
        }

        public Solver(FlowBoard board)
        {
            game = board;

            List<Node> nodes = InitializeNodes();

            // Create the initial conditions
            SearchForcedPaths(nodes);

            Search(nodes);
        }

        private bool Search(List<Node> nodes)
        {
            if (IsGameWon(nodes))
            {
                return true;
            }

            if (nodes.Count == 0)
            {
                return false;
            }

            if (!PathsExist(nodes))
            {
                return false;
            }

            List<Cell.Color> pushBoard = PushBoard();

            var node = nodes[0];
            var nodePath = GetNodePath(node);
            nodePath.Reverse();
            List<int> moves = GetMovesShortestFirst(nodes, nodePath);

            foreach (var move in moves)
            {
                // restore the board
                PopBoard(pushBoard);

                var nodesCopy = NodeCopy(nodes);
                var nodeCopy = (from n in nodesCopy
                                where n.Index == node.Index
                                select n).First();

                nodeCopy.Moves.Clear();
                nodeCopy.Moves.Add(move);
                if (SearchForcedPaths(nodesCopy) == false)
                {
                    return false;
                }

                if(!CanVisitAllCells(nodes))
                {
                    return false;
                }

                if (Search(nodesCopy) == true)
                {
                    return true;
                }
            }


            return IsGameWon(nodes);
        }

        private bool CanVisitAllCells(List<Node> nodes)
        {
            var cells = (from cell in game.Cells
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
                        var p = from c in game.GetAdjacentCells(index, color)
                                where game.Cells[c].CellColor != n.Color || c == endPoint || n.Moves.Contains(c)
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

        private static List<int> GetMovesShortestFirst(List<Node> nodes, List<int> nodePath)
        {
            var tempMoves = nodes[0].Moves.ToList();
            tempMoves.Remove(nodePath[1]);
            var moves = new List<int>() { nodePath[1] };
            moves.AddRange(tempMoves);
            return moves;
        }

        private bool PathsExist(List<Node> nodes)
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

        private List<int> GetNodePath(Node node)
        {
            int pt1 = node.Path.Last();
            int pt2 = node.DestinationIndex;
            var color = node.Color;

            var path = BreadthFirstSearch.Search(pt1, pt2, (int index) =>
            {
                var p = from c in game.GetAdjacentCells(index, color)
                        where game.Cells[c].CellColor != color || c == pt2
                        select c;
                return p.ToList();
            });
            return path;
        }

        private static List<Node> NodeCopy(List<Node> nodes)
        {
            List<Node> retv = new List<Node>();
            foreach (var node in nodes)
            {
                retv.Add(new Node()
                {
                    Color = node.Color,
                    DestinationIndex = node.DestinationIndex,
                    Index = node.Index,
                    Moves = node.Moves.ToList(),
                    Path = node.Path.ToList()
                });
            }

            return retv;
        }

        private void PopBoard(List<Cell.Color> pushBoard)
        {
            for (int i = 0; i < pushBoard.Count; ++i)
            {
                game.Cells[i].CellColor = pushBoard[i];
            }

            if (VisualDelay)
            {
                Thread.Sleep(20);
            }
        }

        private List<Cell.Color> PushBoard()
        {
            List<Cell.Color> pushBoard = new List<Cell.Color>();
            foreach (var cell in game.Cells)
            {
                pushBoard.Add(cell.CellColor);
            }

            return pushBoard;
        }

        private bool IsGameWon(List<Node> nodes)
        {
            bool allNodesDone = nodes.Count == 0;
            bool spaceFilled = (from cell in game.Cells
                                where cell.CellColor == Cell.Color.Empty
                                select cell).Count() == 0;

            return allNodesDone && spaceFilled;
        }

        private bool SearchForcedPaths(List<Node> nodes)
        {
            while (nodes.Count > 0 && nodes[0].Moves.Count() == 1)
            {
                var node = nodes[0];
                var move = node.Moves[0];
                node.Path.Add(move);

                MarkCell(game.Cells[move], node.Color);

                ClearMove(nodes, node, move);

                if (move == node.DestinationIndex)
                {
                    nodes.Remove(node);
                }

                UpdateMoveList(node, move);
                nodes.Sort();
            }

            return true;
        }

        private void MarkCell(Cell cell, Cell.Color color)
        {
            if (VisualDelay)
            {
                cell.CellColor = Cell.Color.DontUse;
                Thread.Sleep(10);
                cell.CellColor = color;
                Thread.Sleep(10);
            }
            else
            {
                cell.CellColor = color;
            }
        }

        private static void ClearMove(List<Node> nodes, Node node, int move)
        {
            foreach (var n in nodes)
            {
                n.Moves.Remove(move);
            }
        }

        private void UpdateMoveList(Node node, int move)
        {
            node.Moves = game.GetAdjacentCells(move, node.Color);
            foreach (var n in node.Path)
            {
                node.Moves.Remove(n);
            }
        }

        private List<Node> InitializeNodes()
        {
            List<Node> nodes = new List<Node>();
            foreach (var element in game.Puzzle)
            {
                int pt1 = game.PointToIndex(element.Pt1);
                int pt2 = game.PointToIndex(element.Pt2);
                var node1 = new Node()
                {
                    Color = element.FlowColor,
                    Index = pt1,
                    DestinationIndex = pt2
                };
                node1.Moves = game.GetAdjacentCells(pt1, element.FlowColor);
                node1.Path.Add(pt1);
                nodes.Add(node1);
            }

            nodes.Sort();
            return nodes;
        }
    }
}
