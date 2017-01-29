using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowSolver
{
    public class Cell : INotifyPropertyChanged
    {
        public int Index { get; private set; }
        private Color _CellColor = Color.Empty;
        public Color CellColor
        {
            get { return _CellColor; }
            set
            {
                if (value != _CellColor)
                {
                    _CellColor = value;
                    OnPropertyChanged(nameof(CellColor));
                }
            }
        }

        public enum Color
        {
            Empty, // Used to denote an empty cell
            Red,
            Green,
            Blue,
            Yellow,
            Cyan,
            Orange,
            Magenta
        };

        public event PropertyChangedEventHandler PropertyChanged;

        public Cell(int index)
        {
            Index = index;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
