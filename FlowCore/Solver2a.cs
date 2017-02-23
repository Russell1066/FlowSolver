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
    public class Solver2a : SolverBase
    {
        public class SolverData
        {
            public FlowBoard.BoardDefinition BoardDefinition { get; set; }
            public int SolutionToTry { get; set; }
            public int MaxNodes { get; set; } = 200;

            public SolverData() { }
            public SolverData(SolverData rhs)
            {
                BoardDefinition = rhs.BoardDefinition;
                SolutionToTry = rhs.SolutionToTry;
                MaxNodes = rhs.MaxNodes;
            }
        }

        private CancellationTokenSource CompletedTokenSource { get; set; }
        private Object LockObj { get; set; }
        public int Index { get; private set; } = -1;
        public ConcurrentBag<int> ValidTests = new ConcurrentBag<int>();
        private const int THREADTESTTIMEms = 30 * 1000;
        public int MaxNodeListCount { get; set; } = int.MaxValue;


        private Solver2a(FlowBoard board)
        {
            Game = board;
            Game.Reset();
        }

        public static IEnumerable<int> GetSolutionList(FlowBoard board, int maxNodes = 200)
        {
            var solver = new Solver2a(board) { MaxNodeListCount = maxNodes, LockObj = new object() };
            var nodeSets = CreateNodeSets(solver.InitializeNodes(), solver.MaxNodeListCount);
            int count = 0;
            var solveList = from node in nodeSets
                            select count++;

            return solveList;
        }

        public static Task<bool> SolveIterationAsync(SolverData solverData, FlowBoard board, CancellationToken token)
        {
            var solver = new Solver2a(board) { MaxNodeListCount = solverData.MaxNodes, Token = token, LockObj = new object() };
            return Task.Run(() => solver.SolveIteration(solverData));
        }

        private bool SolveIteration(SolverData solverData)
        {
            var nodeSets = CreateNodeSets(InitializeNodes(), MaxNodeListCount);

            Trace.WriteLine($"Solving using {solverData.SolutionToTry,3}");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            bool solved = SearchNodes(nodeSets[solverData.SolutionToTry]);
            stopwatch.Stop();
            Trace.WriteLine($"Spent {stopwatch.Elapsed} on {solverData.SolutionToTry,3}");
            return solved;
        }

        public static Task<bool> Solve(FlowBoard board, CancellationToken ct)
        {
            var solver = new Solver2a(board) { Token = ct, LockObj = new object() };

            return Task.Run(() =>
            {
                var nodeSets = CreateNodeSets(solver.InitializeNodes(), solver.MaxNodeListCount);
                int count = 0;
                var solveList = from node in nodeSets
                                select count++;

                return solver.IterateSolutions(board, solveList.ToList());
            });
        }

        private bool IterateSolutions(FlowBoard board, IEnumerable<int> tests)
        {
            bool solved = false;
            foreach (int solutionToTry in tests)
            {
                if (solved)
                {
                    break;
                }
            }

            return solved;
        }

        private bool SolveBoard(FlowBoard board, int trySolution = -1)
        {
            List<Node> nodes = InitializeNodes();

            // Create sets of solvers based on starting conditions
            List<List<Node>> nodeSets = CreateNodeSets(nodes, MaxNodeListCount);
            CompletedTokenSource = new CancellationTokenSource();
            Token.Register(() => CompletedTokenSource.Cancel());
            var taskList = new List<Task>();
            var lockObj = new object();

            bool multipleItems = trySolution == -1;

            for (int index = 0; index < nodeSets.Count; ++index)
            {
                if (trySolution != -1 && index != trySolution)
                {
                    continue;
                }

                FlowBoard subBoard = new FlowBoard();
                subBoard.InitializeBoard(Game.Board);
                var subSolver = new Solver2a(subBoard)
                {
                    Index = index,
                    Token = CompletedTokenSource.Token
                };

                var nodeSet = nodeSets[index];

                taskList.Add(Task.Run(() =>
                {
                    SolveSubSet(subSolver, nodeSet, multipleItems);
                }));
            }

            try
            {
                Task.WaitAll(taskList.ToArray());
            }
            catch (AggregateException) when (Token.IsCancellationRequested)
            {
                Token.ThrowIfCancellationRequested();
            }
            catch (AggregateException ex) when (IsSolved(ex))
            {
            }

            if (Index != -1)
            {
                Trace.WriteLine($"Solution found {Index}");
            }
            else if (trySolution != -1)
            {
                return false;
            }
            else
            {
                if (ValidTests.Count > 0)
                {
                    Trace.WriteLine($"{ValidTests.Count} paths to try from {nodeSets.Count} = {(ValidTests.Count / (float)nodeSets.Count):P}");
                }
            }

            bool foundPaths = Index != -1;

            return foundPaths;
        }

        private bool IsSolved(AggregateException ex)
        {
            return ex.InnerException is OperationCanceledException && Index != -1;
        }

        // This solves the puzzle based on a single set of nodes
        private void SolveSubSet(Solver2a subSolver, List<Node> nodeSet, bool useTimeout)
        {
            if (useTimeout)
            {
                var timeout = new CancellationTokenSource(THREADTESTTIMEms);
                subSolver.Token.Register(() => timeout.Cancel());
                subSolver.Token = timeout.Token;
            }

            var task = Task.Run(() => { Solve(subSolver, nodeSet); }, subSolver.Token);

            try
            {
                task.Wait();
            }
            catch (AggregateException ex) when (ex.InnerException is OperationCanceledException && !Token.IsCancellationRequested)
            {
                ValidTests.Add(subSolver.Index);
            }
        }

        private void Solve(Solver2a subSolver, List<Node> nodeSet)
        {
            if (subSolver.SearchNodes(nodeSet))
            {
                if (UpdateSolverIndex(subSolver))
                {
                    CopySolutionToBoard(Game.Cells, subSolver.Game.Cells);
                }
            }
            else
            {
                Trace.WriteLine($"Exiting {subSolver.Index,3} with failure");
            }
        }

        private bool UpdateSolverIndex(Solver2a subSolver)
        {
            lock (LockObj)
            {
                if (!CompletedTokenSource.Token.IsCancellationRequested)
                {
                    CompletedTokenSource.Cancel();
                    Index = subSolver.Index;
                }
            }

            return Index == subSolver.Index;
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

                if (SearchNodes(nodesCopy, nodeCopy) == true)
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
