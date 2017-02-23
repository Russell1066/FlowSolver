using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SolverCore
{
    public class Solver : SolverBase
    {
        private int Index { get; set; }

        private Solver(FlowBoard board)
        {
            Game = board;
            Game.Reset();
        }

        public static Task<bool> Solve(FlowBoard board, CancellationToken ct)
        {
            return Task.Run(() => SolveBoard(board, ct));
        }

        private static bool SolveBoard(FlowBoard board, CancellationToken ct)
        {
            var solver = new Solver(board) { Token = ct };

            List<Node> nodes = solver.InitializeNodes();

            solver.SearchForcedPaths(nodes);
            bool foundPaths = solver.Search(nodes);
            if (!foundPaths)
            {
                board.Reset();
            }

            return foundPaths;
        }

        private bool Search(List<Node> nodes, Node previous = null)
        {
            if (IsGameWon(nodes))
            {
                return true;
            }

            if (!TestPreconditions(nodes))
            {
                return false;
            }

            var pushBoard = PushBoard();

            Node node = GetStartingNode(nodes, previous);
            List<int> moves = GetMoveList(node);

            foreach (var move in moves)
            {
                // restore the board
                PopBoard(pushBoard);

                var nodesCopy = CopyNodes(nodes);
                Node nodeCopy = GetCopiedNode(nodesCopy, node);

                SetNodeToSingleMove(nodeCopy.Moves, move);

                // After the move is set, update all of the paths forced by this
                SearchForcedPaths(nodesCopy);

                if (Search(nodesCopy, nodeCopy) == true)
                {
                    return true;
                }
            }

            return IsGameWon(nodes);
        }

        private bool TestPreconditions(List<Node> nodes)
        {
            if (Token.IsCancellationRequested)
            {
                Token.ThrowIfCancellationRequested();
            }

            if (nodes.Count == 0)
            {
                return false;
            }

            if (!PathsExist(nodes))
            {
                return false;
            }

            if (!NoTrappedCells(nodes))
            {
                return false;
            }

            if (!CanVisitAllCells(nodes))
            {
                return false;
            }

            return true;
        }

        private bool NoTrappedCells(List<Node> nodes)
        {
            var empty = from cell in Game.Cells
                        where cell.CellColor == Cell.Color.Empty
                        select cell;

            var pathEnds = from node in nodes
                           select node.Path.Last();

            var endpoints = from node in nodes
                            select node.DestinationIndex;

            var exitPoints = endpoints.ToList();
            exitPoints.AddRange(pathEnds);

            var trapped = from s in empty
                          let adj = Game.GetAllAdjacentCells(s.Index)
                          let emptyNeighbor = from a in adj
                                              where a.CellColor == Cell.Color.Empty
                                              select a
                          let validEgress = from a in adj
                                            where exitPoints.Contains(a.Index)
                                            select a
                          let totalOutput = emptyNeighbor.Count() + validEgress.Count()
                          where totalOutput <= 1
                          select new { s, adj, validEgress, totalOutput };

            return trapped.Count() == 0;
        }

        private static Node GetCopiedNode(List<Node> nodesCopy, Node node)
        {
            return (from n in nodesCopy
                    where n.Index == node.Index
                    select n).First();
        }

        private List<int> GetMoveList(Node node)
        {
            var nodePath = GetNodePath(node);
            nodePath.Reverse();
            List<int> moves = GetMovesShortestFirst(node, nodePath);
            return moves;
        }

        private Node GetStartingNode(List<Node> nodes, Node previous)
        {
            var preferredNode = (from n in nodes
                                 where previous != null &&
                                 previous.Color == n.Color &&
                                 n.CompareTo(nodes[0]) == 0
                                 select n).FirstOrDefault();

            var node = preferredNode ?? nodes[0];
            return node;
        }
    }
}
