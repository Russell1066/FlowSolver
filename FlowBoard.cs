using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using static FlowSolver.Cell;

namespace FlowSolver
{
    public class FlowBoard : Board, INotifyPropertyChanged
    {
        public BoardDefinition Board { get; private set; }

        public class Endpoints
        {
            public Color FlowColor { get; set; }
            public Point Pt1 { get; set; }
            public Point Pt2 { get; set; }
        };

        public class BoardDefinition
        {
            public int BoardSize { get; set; }
            public List<Endpoints> EndPointList { get; set; }

            public bool Validate()
            {
                bool boardSize = BoardSize > 4;
                bool endpointList = EndPointList != null && EndPointList.Count > 3 && EndPointList.Count <= BoardSize;
                bool validEndPoints = (from endpoint in EndPointList
                                       where endpoint.Pt1.X < 0 || endpoint.Pt1.X >= BoardSize ||
                                       endpoint.Pt2.X < 0 || endpoint.Pt2.X >= BoardSize ||
                                       endpoint.Pt1.Y < 0 || endpoint.Pt1.Y >= BoardSize ||
                                       endpoint.Pt2.Y < 0 || endpoint.Pt2.Y >= BoardSize ||
                                       endpoint.FlowColor == Color.DontUse ||
                                       endpoint.FlowColor == Color.Empty
                                       select endpoint).Count() != 0;

                return boardSize && endpointList;
            }
        };

        internal void Reset()
        {
            InitializeBoard(Board);
        }

        internal void Clear()
        {
            var board = new BoardDefinition()
            {
                BoardSize = Board.BoardSize,
                EndPointList = new List<Endpoints>()
            };

            InitializeBoard(board);
        }

        internal void Load(string fileName)
        {
            throw new NotImplementedException();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public List<Endpoints> Puzzle = new List<Endpoints>();
        public List<Cell> Cells = new List<Cell>();
        static List<Point> Adjacent = new List<Point>() { new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1) };

        public FlowBoard()
        {
            InitializeBoard(GetGameSetup());
        }

        internal void InitializeBoard(BoardDefinition boardDefinition)
        {
            Board = boardDefinition;

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

            OnPropertyChanged(nameof(Cells));
        }

        internal void AddEndpoints(Endpoints endpoints)
        {
            Board.EndPointList.Add(endpoints);
            InitializeBoard(Board);
        }

        private BoardDefinition GetGameSetup()
        {
            BoardDefinition bd = new BoardDefinition()
            {
                BoardSize = 8,
                EndPointList = new List<Endpoints>
                {
                    new Endpoints() { FlowColor = Color.Yellow, Pt1 = new Point(4,0), Pt2 = new Point(7,1)},
                    new Endpoints() { FlowColor = Color.Cyan, Pt1 = new Point(1,1), Pt2 = new Point(2,5)},
                    new Endpoints() { FlowColor = Color.Red, Pt1 = new Point(1,2), Pt2 = new Point(4,4)},
                    new Endpoints() { FlowColor = Color.Orange, Pt1 = new Point(7,3), Pt2 = new Point(4,5)},
                    new Endpoints() { FlowColor = Color.Blue, Pt1 = new Point(5,4), Pt2 = new Point(6,6)},
                    new Endpoints() { FlowColor = Color.Green, Pt1 = new Point(1,6), Pt2 = new Point(5,6)},
                }
            };

            return bd;
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
    }
}
