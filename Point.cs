using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowSolver
{
    public class Point
    {
        public int X;
        public int Y;

        public Point() { }

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Point operator + (Point lhs, Point rhs)
        {
            return new Point(lhs.X + rhs.X, lhs.Y + rhs.Y);
        }
    };
}
