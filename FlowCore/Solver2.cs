using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using SolverCore.
using FlowBoard = SolverCore.FlowBoard;
using Cell = SolverCore.Cell;
using BreadthFirstSearch = SolverCore.BreadthFirstSearch;

namespace SolverCore
{
    public class Solver2 : SolverBase
    {
        public class SolverData
        {
            public FlowBoard.BoardDefinition BoardDefinition { get; set; }
            public int SolutionIndex { get; set; }
            public int MaxNodes { get; set; } = 200;
            public int SolverCount { get; set; }

            public SolverData() { }
            public SolverData(SolverData rhs)
            {
                BoardDefinition = rhs.BoardDefinition;
                SolutionIndex = rhs.SolutionIndex;
                MaxNodes = rhs.MaxNodes;
            }
        }
        private int RecursionLevel { get; set; }
        private int MaxRecursionLevel { get; set; }

        public int Index { get; private set; } = -1;
        public int MaxNodeListCount { get; set; } = int.MaxValue;


        private Solver2(FlowBoard board)
        {
            Game = board;
            Game.Reset();
        }

        public static IEnumerable<int> GetSolutionList(FlowBoard board, int maxNodes = 200)
        {
            var solver = new Solver2(board) { MaxNodeListCount = maxNodes };
            var nodeSets = CreateNodeSets(solver.InitializeNodes(), solver.MaxNodeListCount);
            int count = 0;
            var solveList = from node in nodeSets
                            select count++;

            // If you don't resolve this, it will keep increasing!
            return solveList.ToList();
        }

        // This is only thread safe if you use different boards
        // If the boards are the same, they will conflict with one another
        public static Task<bool> SolveIterationAsync(SolverData solverData, FlowBoard board, CancellationToken token)
        {
            return Task.Run(() => SolveIteration(solverData, board, token));
        }

        public static bool SolveIteration(SolverData solverData, FlowBoard board, CancellationToken token)
        {
            var solver = new Solver2(board)
            {
                MaxNodeListCount = solverData.MaxNodes,
                Token = token,
            };

            return solver.SolveIteration(solverData);
        }

        private bool SolveIteration(SolverData solverData)
        {
            var nodeSets = CreateNodeSets(InitializeNodes(), MaxNodeListCount);

            Trace.WriteLine($"Solving using {solverData.SolutionIndex,3}");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var nodes = nodeSets[solverData.SolutionIndex];
            SearchForcedPaths(nodes);
            bool solved = SearchNodes(nodes);
            stopwatch.Stop();
            Trace.WriteLine($"Spent {stopwatch.Elapsed} on {solverData.SolutionIndex,3}");
            return solved;
        }

        private static List<List<Node>> CreateNodeSets(List<Node> nodes, int max = int.MaxValue)
        {
            var retv = new List<List<Node>>();
            if (nodes.Count == 0)
            {
                return retv;
            }

            retv.AddRange(from move in NodeToListofSingleMoves(nodes[0])
                          select new List<Node>() { move });

            foreach (var node in nodes.Skip(1))
            {
                if (node.Moves.Count * retv.Count > max)
                {
                    break;
                }
                else
                {
                    retv = AddNode(retv, node);
                }
            }

            if (retv[0].Count < nodes.Count)
            {
                var remainingNodes = nodes.Skip(retv[0].Count);
                foreach (var list in retv)
                {
                    list.AddRange(remainingNodes);
                }
            }

            Debug.Assert((from r in retv select r.Count == nodes.Count ? 1 : 0).Sum() == retv.Count);

            return retv;
        }

        private static List<List<Node>> AddNode(List<List<Node>> retv, Node node)
        {
            return (from list in retv
                    from move in NodeToListofSingleMoves(node)
                    select new List<Node>(list) { move }).ToList();
        }

        private static List<Node> NodeToListofSingleMoves(Node node)
        {
            return (from move in node.Moves
                    select new Node(node)
                    {
                        Moves = new List<int>() { move }
                    }).ToList();
        }

        private bool SearchNodes(List<Node> nodes, Node previous = null)
        {
            RecursionLevel++;

            if(RecursionLevel < 10)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"RecursionLevel: {RecursionLevel}");
                foreach(var n in nodes)
                {
                    sb.AppendLine($"    {n.Color,12} is {n.Path.Count,3} cell(s) long and has {n.Moves.Count} moves");
                }
                Trace.WriteLine(sb.ToString());
            }
            else if(MaxRecursionLevel < RecursionLevel)
            {
                MaxRecursionLevel = RecursionLevel;
                Trace.WriteLine($"Deepening to {RecursionLevel,3}");
            }

            if (IsGameWon(nodes))
            {
                RecursionLevel--;
                return true;
            }

            if (!TestPreconditions(nodes))
            {
                RecursionLevel--;
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

                if (SearchNodes(nodesCopy, nodeCopy) == true)
                {
                    RecursionLevel--;
                    return true;
                }
            }

            RecursionLevel--;
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

            var trapped = from emptyCell in empty
                          let adjacentCells = Game.GetAllAdjacentCells(emptyCell.Index)
                          let emptyNeighbor = from adjacent in adjacentCells
                                              where adjacent.CellColor == Cell.Color.Empty
                                              select adjacent
                          let validEgress = from egress in adjacentCells
                                            where exitPoints.Contains(egress.Index)
                                            select egress
                          let totalOutput = emptyNeighbor.Count() + validEgress.Count()
                          where totalOutput <= 1
                          select new { emptyCell, adjacentCells, validEgress, totalOutput };

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
