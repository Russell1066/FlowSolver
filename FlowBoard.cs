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
            Width = 8;
            Height = 8;

            InitializeBoard();
        }

        private void InitializeBoard()
        {
            for (int i = 0; i < BoardSize; ++i)
            {
                Cells.Add(new Cell(i) { });
            }

            Puzzle = GetEndpoints();

            foreach (var endPoint in Puzzle)
            {
                Cells[PointToIndex(endPoint.Pt1)].CellColor = endPoint.FlowColor;
                Cells[PointToIndex(endPoint.Pt2)].CellColor = endPoint.FlowColor;
            }
        }

        public List<int> GetAdjacentCells(int cell)
        {
            List<int> retv = new List<int>();
            Point p = IndexToPoint(cell);

            foreach (var adj in Adjacent)
            {
                var test = p + adj;
                var index = PointToIndex(test);
                if (IsInBounds(test) && Cells[index].CellColor == Color.Empty)
                {
                    retv.Add(index);
                }
            }

            return retv;
        }

        private List<Endpoints> GetEndpoints()
        {
            return new List<Endpoints>
            {
                new Endpoints() { FlowColor = Color.Green, Pt1 = new Point(0,0), Pt2 = new Point(4,7) },
                new Endpoints() { FlowColor = Color.Cyan, Pt1 = new Point(1,0), Pt2 = new Point(5,1)},
                new Endpoints() { FlowColor = Color.Blue, Pt1 = new Point(2,0), Pt2 = new Point(6,0)},
                new Endpoints() { FlowColor = Color.Red, Pt1 = new Point(7,0), Pt2 = new Point(6,1)},
                new Endpoints() { FlowColor = Color.Orange, Pt1 = new Point(1,2), Pt2 = new Point(6,6)},
                new Endpoints() { FlowColor = Color.Yellow, Pt1 = new Point(5,2), Pt2 = new Point(2,4)},
                new Endpoints() { FlowColor = Color.Magenta, Pt1 = new Point(6,4), Pt2 = new Point(4,5)},
            };
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
