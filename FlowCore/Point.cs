using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolverCore
{
    public class Point : IComparable
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

        public override string ToString()
        {
            return string.Format($"X {X}, Y {Y}");
        }

        public int CompareTo(object obj)
        {
            var rhs = obj as Point;
            if(rhs == null)
            {
                return -1;
            }

            int retv = X - rhs.X;
            if(retv != 0)
            {
                retv = Y - rhs.Y;
            }

            return retv;
        }
    };
}
