using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SolverCore
{
    public class Solver2
    {
        class Node
        {
            public Cell.Color Color;
            public int Index;
            public FlowBoard Game;
            public int DestinationIndex;
            public List<int> Path = new List<int>();
            public List<int> Moves = new List<int>();
            public List<int> ShortestPath = new List<int>();
        }

        public static Task<bool> Solve(FlowBoard game, CancellationToken Token)
        {
            return Task.Run(() =>
            {
                var nodes = (from ep in game.Puzzle
                             let index1 = game.PointToIndex(ep.Pt1)
                             let index2 = game.PointToIndex(ep.Pt2)
                             let color = ep.FlowColor
                             select new Node()
                             {
                                 Game = game,
                                 Color = color,
                                 Index = index1,
                                 DestinationIndex = index2,
                                 Path = new List<int>() { index1 },
                                 Moves = game.GetAdjacentCellIndicies(index1, color),
                                 ShortestPath = BreadthFirstSearch.Search(index2, index1,
                                  index => game.GetAdjacentCellIndicies(index, color))
                             }).ToList();

                // Now find all of the shortest paths without conflicts
                List<int> paths = GetDuplicatePathIndicies(nodes);
                var display = from node in nodes
                              where node.ShortestPath.Intersect(paths).Count() == 0
                              select node;

                foreach (var node in display)
                {
                    var shortestPath = node.ShortestPath;
                    var cells = node.Game.Cells;
                    for (int i = 1; i < shortestPath.Count; ++i)
                    {
                        cells[shortestPath[i]].Parent = cells[shortestPath[i - 1]];
                    }
                }

                return false;
            });
        }

        private static List<int> GetDuplicatePathIndicies(List<Node> nodes)
        {
            var paths = (from node in nodes
                         from cell in node.ShortestPath
                         group cell by cell into g
                         where g.Count() > 1
                         select g.Key)
                        .ToList();

            //var pathNodes = (from node in nodes
            //                 from cell in node.ShortestPath
            //                 group cell by cell into g
            //                 where g.Count() > 1
            //                 select g.Key)
            //                 .ToList();

            return paths;
        }
    }
}
