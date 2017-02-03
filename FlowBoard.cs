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
        public class Endpoints
        {
            public Color FlowColor { get; set; }
            public Point Pt1 { get; set; }
            public Point Pt2 { get; set; }
        };

        class BoardDefinition
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

        public event PropertyChangedEventHandler PropertyChanged;
        public List<Endpoints> Puzzle = new List<Endpoints>();
        public List<Cell> Cells = new List<Cell>();
        static List<Point> Adjacent = new List<Point>() { new Point(-1, 0), new Point(1, 0), new Point(0, -1), new Point(0, 1) };

        public FlowBoard()
        {
            InitializeBoard();
        }

        private void InitializeBoard()
        {
            var board = GetGameSetup3();

            Width = board.BoardSize;
            Height = Width;
            Puzzle = board.EndPointList;

            for (int i = 0; i < BoardSize; ++i)
            {
                Cells.Add(new Cell(i) { });
            }

            foreach (var endPoint in Puzzle)
            {
                Cells[PointToIndex(endPoint.Pt1)].MakeEndpoint(endPoint.FlowColor);
                Cells[PointToIndex(endPoint.Pt2)].MakeEndpoint(endPoint.FlowColor);
            }
        }

        private BoardDefinition GetGameSetup0()
        {
            BoardDefinition bd = new BoardDefinition()
            {
                BoardSize = 5,
                EndPointList = new List<Endpoints>
                {
                    new Endpoints() { FlowColor = Color.Blue, Pt1 = new Point(0,0), Pt2 = new Point(1,1) },
                    new Endpoints() { FlowColor = Color.Green, Pt1 = new Point(1,0), Pt2 = new Point(2,2)},
                    new Endpoints() { FlowColor = Color.Yellow, Pt1 = new Point(3,0), Pt2 = new Point(1,2)},
                    new Endpoints() { FlowColor = Color.Red, Pt1 = new Point(3,1), Pt2 = new Point(1,3)},
                }
            };

            return bd;
        }

        private BoardDefinition GetGameSetup1()
        {
            string json = @"{""BoardSize"":8,""EndPointList"":[{""FlowColor"":2,""Pt1"":{""X"":0,""Y"":0},""Pt2"":{""X"":4,""Y"":7}},{""FlowColor"":5,""Pt1"":{""X"":1,""Y"":0},""Pt2"":{""X"":5,""Y"":1}},{""FlowColor"":6,""Pt1"":{""X"":1,""Y"":2},""Pt2"":{""X"":6,""Y"":6}},{""FlowColor"":3,""Pt1"":{""X"":2,""Y"":0},""Pt2"":{""X"":6,""Y"":0}},{""FlowColor"":4,""Pt1"":{""X"":5,""Y"":2},""Pt2"":{""X"":2,""Y"":4}},{""FlowColor"":7,""Pt1"":{""X"":5,""Y"":4},""Pt2"":{""X"":4,""Y"":6}},{""FlowColor"":1,""Pt1"":{""X"":7,""Y"":0},""Pt2"":{""X"":6,""Y"":1}}]}";

            return JsonConvert.DeserializeObject<BoardDefinition>(json);
        }

        private BoardDefinition GetGameSetup2()
        {
            BoardDefinition bd = new BoardDefinition()
            {
                BoardSize = 7,
                EndPointList = new List<Endpoints>
                {
                    new Endpoints() { FlowColor = Color.Orange, Pt1 = new Point(0,0), Pt2 = new Point(5,1) },
                    new Endpoints() { FlowColor = Color.Blue, Pt1 = new Point(5,0), Pt2 = new Point(2,2)},
                    new Endpoints() { FlowColor = Color.Red, Pt1 = new Point(0,1), Pt2 = new Point(1,5)},
                    new Endpoints() { FlowColor = Color.Green, Pt1 = new Point(5,3), Pt2 = new Point(3,6)},
                    new Endpoints() { FlowColor = Color.Yellow, Pt1 = new Point(6,3), Pt2 = new Point(4,6)},
                }
            };

            return bd;
        }

        private BoardDefinition GetGameSetup3()
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

        private BoardDefinition GetGameSetup4()
        {
            BoardDefinition bd = new BoardDefinition()
            {
                BoardSize = 8,
                EndPointList = new List<Endpoints>
                {
                    new Endpoints() { FlowColor = Color.Magenta, Pt1 = new Point(0,0), Pt2 = new Point(4,2)},
                    new Endpoints() { FlowColor = Color.Green, Pt1 = new Point(2,0), Pt2 = new Point(7,0)},
                    new Endpoints() { FlowColor = Color.Orange, Pt1 = new Point(0,1), Pt2 = new Point(7,3)},
                    new Endpoints() { FlowColor = Color.Yellow, Pt1 = new Point(6,1), Pt2 = new Point(5,5)},
                    new Endpoints() { FlowColor = Color.Red, Pt1 = new Point(7,1), Pt2 = new Point(4,3)},
                    new Endpoints() { FlowColor = Color.Cyan, Pt1 = new Point(3,2), Pt2 = new Point(3,5)},
                    new Endpoints() { FlowColor = Color.Blue, Pt1 = new Point(1,2), Pt2 = new Point(1,6)},
                    new Endpoints() { FlowColor = Color.Brown, Pt1 = new Point(2,2), Pt2 = new Point(3,6)},
                }
            };

            return bd;
        }

        private BoardDefinition GetGameSetup5()
        {
            BoardDefinition bd = new BoardDefinition()
            {
                BoardSize = 8,
                EndPointList = new List<Endpoints>
                {
                    new Endpoints() { FlowColor = Color.Red, Pt1 = new Point(2,0), Pt2 = new Point(3,3)},
                    new Endpoints() { FlowColor = Color.Blue, Pt1 = new Point(3,0), Pt2 = new Point(7,0)},
                    new Endpoints() { FlowColor = Color.Magenta, Pt1 = new Point(2,1), Pt2 = new Point(1,4)},
                    new Endpoints() { FlowColor = Color.Green, Pt1 = new Point(7,1), Pt2 = new Point(4,5)},
                    new Endpoints() { FlowColor = Color.Brown, Pt1 = new Point(4,2), Pt2 = new Point(6,7)},
                    new Endpoints() { FlowColor = Color.Cyan, Pt1 = new Point(7,2), Pt2 = new Point(7,7)},
                    new Endpoints() { FlowColor = Color.Orange, Pt1 = new Point(3,5), Pt2 = new Point(1,6)},
                    new Endpoints() { FlowColor = Color.Yellow, Pt1 = new Point(2,6), Pt2 = new Point(4,6)},
                }
            };

            return bd;
        }

        public List<int> GetAdjacentCells(Point cell, Color allowColor = Color.Empty)
        {
            return GetAdjacentCells(PointToIndex(cell), allowColor);
        }

        public List<int> GetAdjacentCells(int cell, Color allowColor = Color.Empty)
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
