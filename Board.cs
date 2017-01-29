using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowSolver
{
    public class Board
    {
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public int BoardSize => Width * Height;

        public int PointToIndex(Point p)
        {
            Debug.Assert(IsInBounds(p));
            return p.X + p.Y * Width;
        }

        public Point IndexToPoint(int index)
        {
            Debug.Assert(index >= 0 && index < Width * Height);
            return new Point() { X = index % Width, Y = index / Width };
        }

        public bool IsInBounds(Point p)
        {
            return p.X >= 0 && p.X < Width && p.Y >= 0 && p.Y < Height;
        }
    }
}
