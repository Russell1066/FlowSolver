using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using static SolverCore.Cell;

namespace SolverCore
{
    public class FlowBoard : Board, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public BoardDefinition Board { get; private set; }
        public List<Endpoints> Puzzle { get; private set; } = new List<Endpoints>();
        public List<Cell> Cells { get; private set; } = new List<Cell>();
        private static List<Point> Adjacent = new List<Point>() { new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1) };

        public class Endpoints
        {
            public Color FlowColor { get; set; }
            public Point Pt1 { get; set; }
            public Point Pt2 { get; set; }
        };

        public FlowBoard()
        {
        }

        public void Reset()
        {
            InitializeBoard(Board);
        }

        public void Clear()
        {
            var board = new BoardDefinition()
            {
                BoardSize = Board.BoardSize,
                EndPointList = new List<Endpoints>()
            };

            InitializeBoard(board);
        }

        public void InitializeBoard(BoardDefinition boardDefinition)
        {
            Board = boardDefinition ?? Board;

            if (Board == null)
            {
                Width = 0;
                Height = 0;
                Puzzle = new List<Endpoints>();
                Cells = new List<Cell>();
                return;
            }

            Width = Board.BoardSize;
            Height = Width;
            Puzzle = Board.EndPointList;
            Cells = new List<Cell>();
            for (int i = 0; i < BoardSize; ++i)
            {
                Cells.Add(new Cell(i) { });
            }

            foreach (var endPoint in Puzzle)
            {
                Cells[PointToIndex(endPoint.Pt1)].MakeEndpoint(endPoint.FlowColor);
                Cells[PointToIndex(endPoint.Pt2)].MakeEndpoint(endPoint.FlowColor);
            }

            OnPropertyChanged("Initialized");
        }

        public void AddEndpoints(Endpoints endpoints)
        {
            Board.EndPointList.Add(endpoints);
            InitializeBoard(Board);
        }

        public IEnumerable<Cell> GetAllAdjacentCells(int cell)
        {
            var p = IndexToPoint(cell);
            var cells = from adj in Adjacent
                        let test = p + adj
                        where IsInBounds(test)
                        select Cells[PointToIndex(test)];

            return cells;
        }

        public List<int> GetAdjacentCellIndicies(int cell, Color allowColor = Color.Empty)
        {
            List<int> retv = new List<int>();
            Point p = IndexToPoint(cell);

            foreach (var adj in Adjacent)
            {
                var test = p + adj;
                if (!IsInBounds(test))
                {
                    continue;
                }

                var index = PointToIndex(test);
                var c = Cells[index];
                if (c.CellColor == Color.Empty || c.CellColor == allowColor)
                {
                    retv.Add(index);
                }
            }

            return retv;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public class BoardDefinition
        {
            public int BoardSize { get; set; }
            public List<Endpoints> EndPointList { get; set; }
            private const int MINENDPOINTS = 4;
            private const int MINBOARDSIZE = 5;
            private const int MAXBOARDSIZE = 14;

            public bool ValidBoardSize()
            {
                return BoardSize >= MINBOARDSIZE && BoardSize <= MAXBOARDSIZE;
            }

            public bool ValidEndPointList()
            {
                return EndPointList != null &&
                    EndPointList.Count >= MINENDPOINTS;
            }

            public bool ValidEndPoints()
            {
                var badElements = from endpoint in EndPointList
                                  let oob1 = IsOutofBounds(endpoint.Pt1)
                                  let oob2 = IsOutofBounds(endpoint.Pt2)
                                  let badColor = IsInvalidColor(endpoint.FlowColor)
                                  where oob1 || oob2 || badColor
                                  select new { endpoint, oob1, oob2, badColor };
                return badElements.Count() == 0;
            }

            public bool AllPointsUnique()
            {
                List<Point> points = GetAllPoints();
                Point init = new Point(-1, -1);
                bool allPointsUnique = true;

                foreach (var point in points)
                {
                    if (ArePointsColocated(init, point))
                    {
                        allPointsUnique = false;
                        break;
                    }
                }

                return allPointsUnique;

            }

            public bool IsValid()
            {
                return ValidBoardSize() && ValidEndPoints() && ValidEndPointList() && AllPointsUnique();
            }

            public string DescribeFailures()
            {
                if (IsValid())
                {
                    return string.Empty;
                }

                var sb = new StringBuilder();
                if (!ValidBoardSize())
                {
                    sb.AppendLine($"Invalid Board size ({BoardSize}) - " +
                        $"{MINBOARDSIZE} - {MAXBOARDSIZE} supported only");
                }

                if (!ValidEndPoints())
                {
                    sb.AppendLine($"Invalid Endpoints - all end points must have a valid color and be in bounds");
                }

                if (!ValidEndPointList())
                {
                    sb.AppendLine($"Invalid number of end points {EndPointList.Count} - {MINENDPOINTS}");
                }

                if (!AllPointsUnique())
                {
                    sb.AppendLine($"Invalid Endpoints - no end point can be colocated with another end point");
                }

                return sb.ToString();
            }

            private List<Point> GetAllPoints()
            {
                var points1 = from endpoint in EndPointList
                              select endpoint.Pt1;
                var points2 = from endpoint in EndPointList
                              select endpoint.Pt2;
                var points = points1.ToList();
                points.AddRange(points2);
                points.Sort();

                return points;
            }

            private bool IsOutofBounds(Point pt1)
            {
                return pt1.X < 0 || pt1.X >= BoardSize || pt1.Y < 0 || pt1.Y >= BoardSize;
            }

            private bool IsInvalidColor(Color flowColor)
            {
                return flowColor == Color.DontUse || flowColor == Color.Empty;
            }

            private bool ArePointsColocated(Point pt1, Point pt2)
            {
                return pt1.X == pt2.X && pt1.Y == pt2.Y;
            }
        };

        [Serializable]
        private class InvalidDefinitionException : Exception
        {
            public InvalidDefinitionException()
            {
            }

            public InvalidDefinitionException(string message) : base(message)
            {
            }

            public InvalidDefinitionException(string message, Exception innerException) : base(message, innerException)
            {
            }

            protected InvalidDefinitionException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }
    }
}
