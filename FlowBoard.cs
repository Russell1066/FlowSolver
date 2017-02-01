using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            GetGameSetup2();

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

        private void GetGameSetup0()
        {
            Width = 5;
            Height = 5;

            Puzzle = GetEndpoints0();
        }

        private void GetGameSetup1()
        {
            Width = 8;
            Height = 8;

            Puzzle = GetEndpoints1();
        }

        private void GetGameSetup2()
        {
            Width = 7;
            Height = 7;

            Puzzle = GetEndpoints2();
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
                if(!IsInBounds(test))
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

        private List<Endpoints> GetEndpoints0()
        {
            return new List<Endpoints>
            {
                new Endpoints() { FlowColor = Color.Blue, Pt1 = new Point(0,0), Pt2 = new Point(1,1) },
                new Endpoints() { FlowColor = Color.Green, Pt1 = new Point(1,0), Pt2 = new Point(2,2)},
                new Endpoints() { FlowColor = Color.Yellow, Pt1 = new Point(3,0), Pt2 = new Point(1,2)},
                new Endpoints() { FlowColor = Color.Red, Pt1 = new Point(3,1), Pt2 = new Point(1,3)},
            };
        }

        private List<Endpoints> GetEndpoints1()
        {
            return new List<Endpoints>
            {
                new Endpoints() { FlowColor = Color.Green, Pt1 = new Point(0,0), Pt2 = new Point(4,7) },
                new Endpoints() { FlowColor = Color.Cyan, Pt1 = new Point(1,0), Pt2 = new Point(5,1)},
                new Endpoints() { FlowColor = Color.Orange, Pt1 = new Point(1,2), Pt2 = new Point(6,6)},
                new Endpoints() { FlowColor = Color.Blue, Pt1 = new Point(2,0), Pt2 = new Point(6,0)},
                new Endpoints() { FlowColor = Color.Yellow, Pt1 = new Point(5,2), Pt2 = new Point(2,4)},
                new Endpoints() { FlowColor = Color.Magenta, Pt1 = new Point(5,4), Pt2 = new Point(4,6)},
                new Endpoints() { FlowColor = Color.Red, Pt1 = new Point(7,0), Pt2 = new Point(6,1)},
            };
        }

        private List<Endpoints> GetEndpoints2()
        {
            return new List<Endpoints>
            {
                new Endpoints() { FlowColor = Color.Orange, Pt1 = new Point(0,0), Pt2 = new Point(5,1) },
                new Endpoints() { FlowColor = Color.Blue, Pt1 = new Point(5,0), Pt2 = new Point(2,2)},
                new Endpoints() { FlowColor = Color.Red, Pt1 = new Point(0,1), Pt2 = new Point(1,5)},
                new Endpoints() { FlowColor = Color.Green, Pt1 = new Point(5,3), Pt2 = new Point(3,6)},
                new Endpoints() { FlowColor = Color.Yellow, Pt1 = new Point(6,3), Pt2 = new Point(4,6)},
            };
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
