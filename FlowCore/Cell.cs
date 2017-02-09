using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolverCore
{
    public class Cell : INotifyPropertyChanged
    {
        public enum Color
        {
            DontUse = -1,
            Empty, // Used to denote an empty cell
            Red,
            Green,
            Blue,
            Yellow,
            Cyan,
            Orange,
            Magenta,
            Brown,
            DarkBlue,
            Gold,
            DarkCyan,
            Silver,
            White,
            DarkGreen,
            DarkPurple,
            LimeGreen,
        };

        public class CellState
        {
            public Color CellColor { get; private set; }
            public Cell Parent { get; private set; }

            public CellState(Color color, Cell parent)
            {
                CellColor = color;
                Parent = parent;
            }
        }

        public int Index { get; private set; }
        private Cell _Parent = null;
        public Cell Parent
        {
            get { return _Parent; }
            set
            {
                if (value != _Parent)
                {
                    if (_Parent != null)
                    {
                        _Parent.Child = null;
                        _Parent.OnPropertyChanged(nameof(Child));
                    }

                    _Parent = value;

                    if (_Parent != null)
                    {
                        _Parent.Child = this;
                        _Parent.OnPropertyChanged(nameof(Child));
                        CellColor = _Parent.CellColor;
                    }
                    else
                    {
                        CellColor = Color.Empty;
                    }
                }
            }
        }
        public Cell Child { get; private set; }
        private bool _IsEndpoint;
        public bool IsEndpoint
        {
            get { return _IsEndpoint; }
            set
            {
                if (value != _IsEndpoint)
                {
                    _IsEndpoint = value;
                    OnPropertyChanged(nameof(IsEndpoint));
                };
            }
        }
        private Color _CellColor = Color.Empty;

        public void Press()
        {
            OnPropertyChanged("Pressed");
        }

        public Color CellColor
        {
            get { return _CellColor; }
            private set
            {
                if (value != _CellColor)
                {
                    _CellColor = value;
                    OnPropertyChanged(nameof(CellColor));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public Cell(int index)
        {
            Index = index;
        }

        public void MakeEndpoint(Color color)
        {
            CellColor = color;
            IsEndpoint = true;
        }

        public void ClearEndpoint()
        {
            CellColor = Color.Empty;
            IsEndpoint = false;
        }

        public CellState GetCellState()
        {
            return new CellState(CellColor, Parent);
        }

        internal void SetState(CellState cellState)
        {
            Parent = cellState.Parent;
            CellColor = cellState.CellColor;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public override string ToString()
        {
            string endpoint = IsEndpoint ? "Endpoint" : string.Empty;
            return string.Format($"{Index,3} {CellColor} {endpoint}");
        }
    }
}
